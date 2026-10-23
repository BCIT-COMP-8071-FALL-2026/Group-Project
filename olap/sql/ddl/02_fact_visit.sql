-- FactVisit per docs/olap/ETL-Mapping.md. One row per OLTP visit.
-- Run after 01_dimensions.sql. Safe to rerun.
-- Timestamps are datetime2 (as in OLTP) so cutoffs compare fractions of a second (Query1-Contract.md).
-- Missed and not yet due are not stored: the query procedures compute them at run time.

USE OlapDW;
GO

DROP TABLE IF EXISTS dbo.FactVisit;
GO

CREATE TABLE dbo.FactVisit (
    VisitFactID      int       NOT NULL CONSTRAINT PK_FactVisit PRIMARY KEY,   -- OLTP Visit.VisitID
    DateKey          int       NOT NULL CONSTRAINT FK_FactVisit_DimDate REFERENCES dbo.DimDate (DateKey),
    SeniorKey        int       NOT NULL CONSTRAINT FK_FactVisit_DimSenior REFERENCES dbo.DimSenior (SeniorKey),
    ProfessionalKey  int       NOT NULL CONSTRAINT FK_FactVisit_DimProfessional REFERENCES dbo.DimProfessional (ProfessionalKey),
    ServiceKey       int       NOT NULL CONSTRAINT FK_FactVisit_DimService REFERENCES dbo.DimService (ServiceKey),
    OrganizationKey  int       NOT NULL CONSTRAINT FK_FactVisit_DimOrganization REFERENCES dbo.DimOrganization (OrganizationKey),
    ScheduledStart   datetime2 NOT NULL,
    ActualCheckIn    datetime2 NULL,
    ActualCheckOut   datetime2 NULL,
    DurationMinutes  int       NULL,
    LateMinutes      int       NULL,
    CompletedCount   int       NOT NULL,
    CancelledCount   int       NOT NULL,
    LateCheckInCount int       NOT NULL,
    CONSTRAINT CK_FactVisit_Counts CHECK (
        CompletedCount IN (0, 1) AND CancelledCount IN (0, 1) AND LateCheckInCount IN (0, 1)
        AND CompletedCount + CancelledCount <= 1
    ),
    CONSTRAINT CK_FactVisit_CheckOutAfterCheckIn CHECK (ActualCheckOut >= ActualCheckIn),
    CONSTRAINT CK_FactVisit_Minutes CHECK (DurationMinutes >= 0 AND LateMinutes >= 0),
    CONSTRAINT CK_FactVisit_LateNeedsCheckIn CHECK (LateCheckInCount = 0 OR ActualCheckIn IS NOT NULL)
);
GO
