
-- =====================================================
-- Senior Care Management - OLAP Star Schema
-- File: 01_CreateStarSchema.sql
--
-- Grain: One row per OLTP Visit.
-- Dates and timestamps are interpreted as UTC.
-- =====================================================

-- 1. CREATE DATABASE
IF DB_ID('SeniorCareOLAP') IS NULL
BEGIN
    CREATE DATABASE SeniorCareOLAP;
END;
GO

USE SeniorCareOLAP;
GO

-- =====================================================
-- 2. DIMENSION: DATE
-- Source: Visit.ScheduledStart
-- DateKey format: YYYYMMDD
-- =====================================================
CREATE TABLE dbo.DimDate (
    DateKey INT NOT NULL PRIMARY KEY,
    FullDate DATE NOT NULL UNIQUE,
    [Year] INT NOT NULL,
    [Month] INT NOT NULL,
    [Day] INT NOT NULL,
    MonthName VARCHAR(20) NOT NULL,
    [Quarter] INT NOT NULL,

    CONSTRAINT CK_DimDate_Month
        CHECK ([Month] BETWEEN 1 AND 12),

    CONSTRAINT CK_DimDate_Day
        CHECK ([Day] BETWEEN 1 AND 31),

    CONSTRAINT CK_DimDate_Quarter
        CHECK ([Quarter] BETWEEN 1 AND 4)
);
GO

-- =====================================================
-- 3. DIMENSION: SENIOR
-- Source: OLTP Senior
-- SeniorID is the source system ID.
-- =====================================================
CREATE TABLE dbo.DimSenior (
    SeniorKey INT IDENTITY(1,1) PRIMARY KEY,
    SeniorID INT NOT NULL UNIQUE,
    AgeGroup VARCHAR(30) NULL
);
GO

-- =====================================================
-- 4. DIMENSION: CAREGIVER
-- Source: OLTP Caregiver JOIN Person
-- CaregiverID is shared with Person.
-- =====================================================
CREATE TABLE dbo.DimCaregiver (
    CaregiverKey INT IDENTITY(1,1) PRIMARY KEY,
    CaregiverID INT NOT NULL UNIQUE,
    CaregiverName NVARCHAR(150) NULL
);
GO

-- =====================================================
-- 5. DIMENSION: AGENCY
-- Source: OLTP Agency
-- Use Visit.AgencyID for historical visit mapping.
-- =====================================================
CREATE TABLE dbo.DimAgency (
    AgencyKey INT IDENTITY(1,1) PRIMARY KEY,
    AgencyID INT NOT NULL UNIQUE,
    AgencyName NVARCHAR(150) NULL
);
GO

-- =====================================================
-- 6. DIMENSION: TIME OF DAY
-- Source: UTC hour from Visit.ScheduledStart
-- 24 rows, one for each hour (0-23).
-- =====================================================
CREATE TABLE dbo.DimTimeOfDay (
    TimeOfDayKey INT IDENTITY(1,1) PRIMARY KEY,
    HourOfDay INT NOT NULL UNIQUE,
    TimePeriod VARCHAR(20) NOT NULL,

    CONSTRAINT CK_DimTimeOfDay_Hour
        CHECK (HourOfDay BETWEEN 0 AND 23)
);
GO

-- =====================================================
-- 7. FACT TABLE: VISITS
--
-- Grain: One row per OLTP Visit.
--
-- Query 1 calculates missed visits dynamically
-- using ScheduledStart and ActualCheckIn.
-- =====================================================
CREATE TABLE dbo.FactVisits (

    -- Surrogate primary key
    VisitKey INT IDENTITY(1,1) PRIMARY KEY,

    -- Original OLTP Visit identifier
    VisitID INT NOT NULL UNIQUE,

    -- Dimension foreign keys
    DateKey INT NOT NULL,
    SeniorKey INT NOT NULL,
    CaregiverKey INT NOT NULL,
    AgencyKey INT NOT NULL,
    TimeOfDayKey INT NOT NULL,

    -- Source data needed for reporting (UTC)
    ScheduledStart DATETIME2(7) NOT NULL,
    ActualCheckIn DATETIME2(7) NULL,
    ActualCheckOut DATETIME2(7) NULL,

    -- Original OLTP status stored as a string
    VisitStatus NVARCHAR(20) NOT NULL,

    -- =================================================
    -- MEASURES
    -- =================================================

    -- One fact row = one scheduled visit.
    -- Query 1 applies eligibility rules separately.
    VisitCount INT NOT NULL DEFAULT 1,

    -- Optional ETL-derived measures.
    -- Query 1 MUST NOT use MissedCount because its
    -- missed calculation depends on evaluation time.
    MissedCount INT NOT NULL DEFAULT 0,
    LateCount INT NOT NULL DEFAULT 0,
    CancelledCount INT NOT NULL DEFAULT 0,

    -- Actual visit duration, when available.
    DurationMinutes INT NULL,

    -- =================================================
    -- FOREIGN KEYS
    -- =================================================
    CONSTRAINT FK_FactVisits_Date
        FOREIGN KEY (DateKey)
        REFERENCES dbo.DimDate(DateKey),

    CONSTRAINT FK_FactVisits_Senior
        FOREIGN KEY (SeniorKey)
        REFERENCES dbo.DimSenior(SeniorKey),

    CONSTRAINT FK_FactVisits_Caregiver
        FOREIGN KEY (CaregiverKey)
        REFERENCES dbo.DimCaregiver(CaregiverKey),

    CONSTRAINT FK_FactVisits_Agency
        FOREIGN KEY (AgencyKey)
        REFERENCES dbo.DimAgency(AgencyKey),

    CONSTRAINT FK_FactVisits_TimeOfDay
        FOREIGN KEY (TimeOfDayKey)
        REFERENCES dbo.DimTimeOfDay(TimeOfDayKey),

    -- =================================================
    -- VALIDATION CONSTRAINTS
    -- =================================================
    CONSTRAINT CK_FactVisits_Status
        CHECK (VisitStatus IN (
            'Scheduled',
            'InProgress',
            'Completed',
            'Missed',
            'Cancelled'
        )),

    CONSTRAINT CK_FactVisits_VisitCount
        CHECK (VisitCount = 1),

    CONSTRAINT CK_FactVisits_MissedCount
        CHECK (MissedCount IN (0, 1)),

    CONSTRAINT CK_FactVisits_LateCount
        CHECK (LateCount IN (0, 1)),

    CONSTRAINT CK_FactVisits_CancelledCount
        CHECK (CancelledCount IN (0, 1)),

    CONSTRAINT CK_FactVisits_Duration
        CHECK (
            DurationMinutes IS NULL
            OR DurationMinutes >= 0
        )
);
GO

-- =====================================================
-- 8. VERIFY THE CREATED TABLES
-- =====================================================
SELECT
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;
GO
