-- A8a + A8b (combined): full reload of the OlapDW star schema from OLTPAppDb.
-- Per docs/olap/ETL-Mapping.md, docs/OLAP_Queries.md and docs/Query1-Contract.md.
--
-- Option A: one atomic "full reload" so the star holds OLTPAppDb data only.
--   1. TRUNCATE FactVisit (child; releases every FK so the dims can be cleared).
--   2. DELETE each dimension and reseed its surrogate identity so keys regenerate
--      deterministically (1..N in natural-key order) on every run.
--   3. INSERT the dimensions, ordered by natural key for stable surrogate keys.
--   4. INSERT FactVisit after the dimensions (FKs), one row per OLTP visit.
--
-- Missed / not-yet-due are NOT stored here: the reporting procedures
-- (dbo.usp_MissedVisitsByMonth, ...) compute MissedCount and the eligible
-- VisitCount at run time from ScheduledStart + ActualCheckIn using
-- SYSUTCDATETIME() (docs/Query1-Contract.md). The stored measures are the
-- deterministic ones: DurationMinutes, LateMinutes, CompletedCount,
-- CancelledCount, LateCheckInCount.
--
-- Rerunnable: each run produces an identical star (full reload, no duplicates).
-- Assumption: OLTPAppDb enforces ActualCheckOut >= ActualCheckIn (the DDL CHECK
-- constraint CK_FactVisit_CheckOutAfterCheckIn otherwise rejects the row).

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

        -- 2) DimDate: a complete calendar from the earliest to the latest scheduled visit.
        --    The two-table cross join yields ~tens of thousands of numbers, ample for the
        --    reporting horizon; extend it only if a horizon longer than ~170 years is needed.
        DECLARE @StartDate DATE, @EndDate DATE;
        SELECT
            @StartDate = MIN(CAST(ScheduledStart AS DATE)),
            @EndDate   = MAX(CAST(ScheduledStart AS DATE))
        FROM OLTPAppDb.dbo.Visit;

        IF @StartDate IS NOT NULL
        BEGIN
            ;WITH Numbers AS (
                SELECT TOP (DATEDIFF(DAY, @StartDate, @EndDate) + 1)
                    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
                FROM sys.all_objects a
                CROSS JOIN sys.all_objects b
            ),
            CalendarDates AS (
                SELECT DATEADD(DAY, n, @StartDate) AS FullDate
                FROM Numbers
            )
            INSERT INTO dbo.DimDate (DateKey, FullDate, [Day], [Month], [Year], [Quarter], [DayOfWeek])
            SELECT
                YEAR(FullDate) * 10000 + MONTH(FullDate) * 100 + DAY(FullDate),
                FullDate,
                DAY(FullDate),
                MONTH(FullDate),
                YEAR(FullDate),
                DATEPART(QUARTER, FullDate),
                DATENAME(WEEKDAY, FullDate)
            FROM CalendarDates;
        END;

        -- 3) The four entity dimensions, ordered by natural key so the surrogate
        --    key each row receives is stable across reloads.
        INSERT INTO dbo.DimSenior (SeniorID, FirstName, LastName, DateOfBirth, IsLivingAlone, City)
        SELECT s.SeniorID, p.FirstName, p.LastName, s.DateOfBirth, s.IsLivingAlone, p.City
        FROM OLTPAppDb.dbo.Senior s
        JOIN OLTPAppDb.dbo.Person p ON p.PersonID = s.SeniorID
        ORDER BY s.SeniorID;

        INSERT INTO dbo.DimProfessional (ProfessionalID, FirstName, LastName)
        SELECT c.CaregiverID, p.FirstName, p.LastName
        FROM OLTPAppDb.dbo.Caregiver c
        JOIN OLTPAppDb.dbo.Person p ON p.PersonID = c.CaregiverID
        ORDER BY c.CaregiverID;

        INSERT INTO dbo.DimService (ServiceID, ServiceName)
        SELECT ServiceID, ServiceName
        FROM OLTPAppDb.dbo.Service
        ORDER BY ServiceID;

        INSERT INTO dbo.DimOrganization (OrganizationID, OrganizationName)
        SELECT AgencyID, Name
        FROM OLTPAppDb.dbo.Agency
        ORDER BY AgencyID;

        -- 4) Facts, one row per OLTP visit, loaded after the dimensions (FKs).
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
            v.ActualCheckOut,
            -- DurationMinutes: whole minutes check-in -> check-out, when both exist.
            CASE WHEN v.ActualCheckIn IS NOT NULL AND v.ActualCheckOut IS NOT NULL
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
