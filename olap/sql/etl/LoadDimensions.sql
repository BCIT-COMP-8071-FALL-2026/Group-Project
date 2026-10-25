USE OlapDW;


GO
-- =====================================================
-- A8a ETL: Load Dimensions
-- Source Database: OLTPAppDb
-- Destination Database: OlapDW
--
-- This procedure only loads dimension data.
-- The caller (usp_LoadStar) is responsible for:
-- 1. Clearing facts and dimensions
-- 2. Managing the transaction
-- 3. Loading FactVisit after the dimensions
-- =====================================================
CREATE OR ALTER PROCEDURE dbo.usp_LoadDimensions
AS
BEGIN
    SET NOCOUNT ON;
    -- Require an outer transaction from usp_LoadStar.
    IF @@TRANCOUNT = 0
        BEGIN
            RAISERROR ('usp_LoadDimensions must be called inside a transaction.', 16, 1);
            RETURN;
        END
    -- =================================================
    -- 1. LOAD DimDate
    -- =================================================
    DECLARE @StartDate AS DATE;
    DECLARE @EndDate AS DATE;
    SELECT @StartDate = MIN(CAST (ScheduledStart AS DATE)),
           @EndDate = MAX(CAST (ScheduledStart AS DATE))
    FROM   OLTPAppDb.dbo.Visit;
    IF @StartDate IS NOT NULL
        BEGIN
            ;WITH Numbers
            AS   (SELECT TOP (DATEDIFF(DAY, @StartDate, @EndDate) + 1) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
                  FROM   sys.all_objects AS a
                         CROSS JOIN sys.all_objects AS b),
                 CalendarDates
            AS   (SELECT DATEADD(DAY, n, @StartDate) AS FullDate
                  FROM   Numbers)
            INSERT INTO dbo.DimDate (
                DateKey,
                FullDate,
                [Day],
                [Month],
                [Year],
                [Quarter],
                [DayOfWeek]
            )
            SELECT YEAR(c.FullDate) * 10000 + MONTH(c.FullDate) * 100 + DAY(c.FullDate),
                   c.FullDate,
                   DAY(c.FullDate),
                   MONTH(c.FullDate),
                   YEAR(c.FullDate),
                   DATEPART(QUARTER, c.FullDate),
                   DATENAME(WEEKDAY, c.FullDate)
            FROM   CalendarDates AS c;
        END
    -- =================================================
    -- 2. LOAD DimSenior
    -- =================================================
    INSERT INTO dbo.DimSenior (
        SeniorID,
        FirstName,
        LastName,
        DateOfBirth,
        IsLivingAlone,
        City
    )
    SELECT s.SeniorID,
           p.FirstName,
           p.LastName,
           s.DateOfBirth,
           s.IsLivingAlone,
           p.City
    FROM   OLTPAppDb.dbo.Senior AS s
           INNER JOIN
           OLTPAppDb.dbo.Person AS p
           ON s.SeniorID = p.PersonID;
    -- =================================================
    -- 3. LOAD DimProfessional
    -- =================================================
    INSERT INTO dbo.DimProfessional (
        ProfessionalID,
        FirstName,
        LastName
    )
    SELECT c.CaregiverID,
           p.FirstName,
           p.LastName
    FROM   OLTPAppDb.dbo.Caregiver AS c
           INNER JOIN
           OLTPAppDb.dbo.Person AS p
           ON c.CaregiverID = p.PersonID;
    -- =================================================
    -- 4. LOAD DimService
    -- =================================================
    INSERT INTO dbo.DimService (
        ServiceID,
        ServiceName
    )
    SELECT s.ServiceID,
           s.ServiceName
    FROM   OLTPAppDb.dbo.Service AS s;
    -- =================================================
    -- 5. LOAD DimOrganization
    -- =================================================
    INSERT INTO dbo.DimOrganization (
        OrganizationID,
        OrganizationName
    )
    SELECT a.AgencyID,
           a.Name
    FROM   OLTPAppDb.dbo.Agency AS a;
END;
GO