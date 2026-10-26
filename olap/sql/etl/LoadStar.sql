-- A8a (dimensions, PR #98) + A8b (facts): full reload of the OlapDW star from OLTPAppDb.
-- Per docs/olap/ETL-Mapping.md, docs/OLAP_Queries.md and docs/Query1-Contract.md.
--
-- Division of labor (agreed with A8a):
--   * dbo.usp_LoadDimensions (A8a) loads all five dimensions. It does NOT clear the
--     tables, does NOT reseed identities, and does NOT start a transaction; it requires
--     a caller transaction and expects the tables already cleared + reseeded.
--   * dbo.usp_LoadStar (A8b) owns the full reload in a single transaction:
--       1. TRUNCATE FactVisit (child; releases every FK so the dims can be cleared).
--       2. DELETE each dimension and reseed its surrogate identity so the A8a inserts
--          restart the surrogate keys at 1.
--       3. EXEC dbo.usp_LoadDimensions  (loads DimDate + the four entity dimensions).
--       4. INSERT FactVisit after the dimensions (FKs), one row per OLTP visit.
--
-- The C# ODBC runner keeps calling usp_LoadStar (unchanged); it just now delegates the
-- dimension inserts to usp_LoadDimensions.
--
-- Missed / not-yet-due are NOT stored here: the reporting procedures
-- (dbo.usp_MissedVisitsByMonth, ...) compute MissedCount and the eligible
-- VisitCount at run time from ScheduledStart + ActualCheckIn using
-- SYSUTCDATETIME() (docs/Query1-Contract.md). The stored measures are the
-- deterministic ones: DurationMinutes, LateMinutes, CompletedCount,
-- CancelledCount, LateCheckInCount.
--
-- Rerunnable: each run is a full reload -- the star holds OLTPAppDb data only, no
-- duplicates. OLTPAppDb does not enforce ActualCheckOut >= ActualCheckIn, so a check-out
-- before check-in loads as NULL instead of failing CK_FactVisit_CheckOutAfterCheckIn.

USE OlapDW;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LoadStar
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1) Clear the star. FactVisit is the referencing (child) table, so TRUNCATE
        --    is allowed and releases all FK dependencies on the dimensions.
        TRUNCATE TABLE dbo.FactVisit;

        --    Dimensions are referenced by FactVisit FKs, so TRUNCATE is blocked; DELETE
        --    them and reseed the surrogate identity to 0 so the next insert restarts at 1.
        DELETE FROM dbo.DimDate;                       -- DateKey is computed (YYYYMMDD), not identity.
        DELETE FROM dbo.DimSenior;
        DBCC CHECKIDENT ('dbo.DimSenior', RESEED, 0) WITH NO_INFOMSGS;
        DELETE FROM dbo.DimProfessional;
        DBCC CHECKIDENT ('dbo.DimProfessional', RESEED, 0) WITH NO_INFOMSGS;
        DELETE FROM dbo.DimService;
        DBCC CHECKIDENT ('dbo.DimService', RESEED, 0) WITH NO_INFOMSGS;
        DELETE FROM dbo.DimOrganization;
        DBCC CHECKIDENT ('dbo.DimOrganization', RESEED, 0) WITH NO_INFOMSGS;

        -- 2) Load all five dimensions via the A8a procedure. It inserts DimDate
        --    (calendar from the OLTP scheduled-visit date range) and DimSenior /
        --    DimProfessional / DimService / DimOrganization from OLTPAppDb, each row
        --    keyed on the OLTP natural key (SeniorID, CaregiverID, ServiceID, AgencyID).
        --    It does not clear or reseed (the clear above did) and runs inside this
        --    transaction, so the surrogate identities restart at 1.
        EXEC dbo.usp_LoadDimensions;

        -- 3) Facts, one row per OLTP visit, loaded after the dimensions (FKs).
        INSERT INTO dbo.FactVisit
            (VisitFactID, DateKey, SeniorKey, ProfessionalKey, ServiceKey, OrganizationKey,
             ScheduledStart, ActualCheckIn, ActualCheckOut,
             DurationMinutes, LateMinutes, CompletedCount, CancelledCount, LateCheckInCount)
        SELECT
            v.VisitID,
            dd.DateKey,
            ds.SeniorKey,
            dp.ProfessionalKey,
            dserv.ServiceKey,
            doz.OrganizationKey,
            v.ScheduledStart,
            v.ActualCheckIn,
            -- ActualCheckOut: NULL when before check-in, so one bad row cannot fail the reload.
            CASE WHEN v.ActualCheckOut >= v.ActualCheckIn THEN v.ActualCheckOut
                 ELSE NULL END,
            -- DurationMinutes: whole minutes check-in -> check-out, when both exist and in order.
            CASE WHEN v.ActualCheckOut >= v.ActualCheckIn
                 THEN DATEDIFF(MINUTE, v.ActualCheckIn, v.ActualCheckOut)
                 ELSE NULL END,
            -- LateMinutes: NULL if no check-in; 0 if early/on time; else minutes past start.
            CASE WHEN v.ActualCheckIn IS NULL THEN NULL
                 WHEN v.ActualCheckIn <= v.ScheduledStart THEN 0
                 ELSE DATEDIFF(MINUTE, v.ScheduledStart, v.ActualCheckIn) END,
            -- CompletedCount: check-in + check-out present, not cancelled, checked in within
            -- the 60-minute window (the deterministic "not missed by timestamp" test).
            CASE WHEN v.Status <> N'Cancelled'
                      AND v.ActualCheckIn IS NOT NULL AND v.ActualCheckOut IS NOT NULL
                      AND v.ActualCheckIn <= DATEADD(MINUTE, 60, v.ScheduledStart)
                 THEN 1 ELSE 0 END,
            -- CancelledCount: from Status.
            CASE WHEN v.Status = N'Cancelled' THEN 1 ELSE 0 END,
            -- LateCheckInCount: >10 min late and within the 60-minute window, not cancelled.
            CASE WHEN v.Status <> N'Cancelled'
                      AND v.ActualCheckIn IS NOT NULL
                      AND v.ActualCheckIn > DATEADD(MINUTE, 10, v.ScheduledStart)
                      AND v.ActualCheckIn <= DATEADD(MINUTE, 60, v.ScheduledStart)
                 THEN 1 ELSE 0 END
        FROM OLTPAppDb.dbo.Visit v
        JOIN dbo.DimDate         dd    ON dd.FullDate      = CAST(v.ScheduledStart AS DATE)
        JOIN dbo.DimSenior       ds    ON ds.SeniorID      = v.SeniorID
        JOIN dbo.DimProfessional dp    ON dp.ProfessionalID = v.CaregiverID
        JOIN dbo.DimService      dserv ON dserv.ServiceID  = v.ServiceID
        JOIN dbo.DimOrganization doz   ON doz.OrganizationID = v.AgencyID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH

    -- 5) Summary of the reload, returned to the runner/endpoint.
    SELECT
        (SELECT COUNT(*) FROM dbo.DimDate)         AS DimDate,
        (SELECT COUNT(*) FROM dbo.DimSenior)       AS DimSenior,
        (SELECT COUNT(*) FROM dbo.DimProfessional) AS DimProfessional,
        (SELECT COUNT(*) FROM dbo.DimService)      AS DimService,
        (SELECT COUNT(*) FROM dbo.DimOrganization) AS DimOrganization,
        (SELECT COUNT(*) FROM dbo.FactVisit)       AS FactVisit;
END;
GO
