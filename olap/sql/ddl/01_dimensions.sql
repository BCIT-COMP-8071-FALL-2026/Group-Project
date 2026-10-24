-- Dimension tables per docs/olap/ETL-Mapping.md.
-- Safe to rerun: drops FactVisit first (it references every dimension), then recreates the dimensions.
-- String lengths match the OLTP source columns.

USE OlapDW;
GO

DROP TABLE IF EXISTS dbo.FactVisit;
DROP TABLE IF EXISTS dbo.DimDate;
DROP TABLE IF EXISTS dbo.DimSenior;
DROP TABLE IF EXISTS dbo.DimProfessional;
DROP TABLE IF EXISTS dbo.DimService;
DROP TABLE IF EXISTS dbo.DimOrganization;
GO

CREATE TABLE dbo.DimDate (
    DateKey   int          NOT NULL CONSTRAINT PK_DimDate PRIMARY KEY,   -- YYYYMMDD
    FullDate  date         NOT NULL CONSTRAINT UQ_DimDate_FullDate UNIQUE,
    [Day]     int          NOT NULL,
    [Month]   int          NOT NULL,
    [Year]    int          NOT NULL,
    [Quarter] int          NOT NULL,
    [DayOfWeek] nvarchar(10) NOT NULL
);

CREATE TABLE dbo.DimSenior (
    SeniorKey     int           IDENTITY(1,1) NOT NULL CONSTRAINT PK_DimSenior PRIMARY KEY,
    SeniorID      int           NOT NULL CONSTRAINT UQ_DimSenior_SeniorID UNIQUE,
    FirstName     nvarchar(100) NOT NULL,
    LastName      nvarchar(100) NOT NULL,
    DateOfBirth   date          NOT NULL,
    IsLivingAlone bit           NOT NULL,
    City          nvarchar(100) NOT NULL
);

CREATE TABLE dbo.DimProfessional (
    ProfessionalKey int           IDENTITY(1,1) NOT NULL CONSTRAINT PK_DimProfessional PRIMARY KEY,
    ProfessionalID  int           NOT NULL CONSTRAINT UQ_DimProfessional_ProfessionalID UNIQUE,   -- Caregiver.CaregiverID
    FirstName       nvarchar(100) NOT NULL,
    LastName        nvarchar(100) NOT NULL
);

CREATE TABLE dbo.DimService (
    ServiceKey  int           IDENTITY(1,1) NOT NULL CONSTRAINT PK_DimService PRIMARY KEY,
    ServiceID   int           NOT NULL CONSTRAINT UQ_DimService_ServiceID UNIQUE,
    ServiceName nvarchar(100) NOT NULL
);

CREATE TABLE dbo.DimOrganization (
    OrganizationKey  int           IDENTITY(1,1) NOT NULL CONSTRAINT PK_DimOrganization PRIMARY KEY,
    OrganizationID   int           NOT NULL CONSTRAINT UQ_DimOrganization_OrganizationID UNIQUE,   -- Agency.AgencyID
    OrganizationName nvarchar(200) NOT NULL
);
GO
