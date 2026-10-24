
USE OlapDW;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LoadDimensions
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. DimDate: generate calendar from
        -- earliest to latest scheduled visit.
        DECLARE @StartDate DATE;
        DECLARE @EndDate DATE;

        SELECT
            @StartDate = MIN(CAST(ScheduledStart AS DATE)),
            @EndDate = MAX(CAST(ScheduledStart AS DATE))
        FROM OLTPAppDb.dbo.Visit;

        IF @StartDate IS NOT NULL
        BEGIN
            ;WITH Numbers AS (
                SELECT TOP (DATEDIFF(DAY,
                    @StartDate, @EndDate) + 1)
                    ROW_NUMBER() OVER
                        (ORDER BY (SELECT NULL)) - 1 AS n
                FROM sys.all_objects a
                CROSS JOIN sys.all_objects b
            ),
            CalendarDates AS (
                SELECT DATEADD(DAY, n, @StartDate)
                    AS FullDate
                FROM Numbers
            )
            INSERT INTO dbo.DimDate (
                DateKey, FullDate, [Day], [Month],
                [Year], [Quarter], [DayOfWeek]
            )
            SELECT
                YEAR(c.FullDate) * 10000
                    + MONTH(c.FullDate) * 100
                    + DAY(c.FullDate),
                c.FullDate,
                DAY(c.FullDate),
                MONTH(c.FullDate),
                YEAR(c.FullDate),
                DATEPART(QUARTER, c.FullDate),
                DATENAME(WEEKDAY, c.FullDate)
            FROM CalendarDates c
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.DimDate d
                WHERE d.FullDate = c.FullDate
            );
        END;

        -- 2. DimSenior
        UPDATE d SET
            d.FirstName = p.FirstName,
            d.LastName = p.LastName,
            d.DateOfBirth = s.DateOfBirth,
            d.IsLivingAlone = s.IsLivingAlone,
            d.City = p.City
        FROM dbo.DimSenior d
        JOIN OLTPAppDb.dbo.Senior s
            ON d.SeniorID = s.SeniorID
        JOIN OLTPAppDb.dbo.Person p
            ON s.SeniorID = p.PersonID;

        INSERT INTO dbo.DimSenior
            (SeniorID, FirstName, LastName,
             DateOfBirth, IsLivingAlone, City)
        SELECT
            s.SeniorID, p.FirstName, p.LastName,
            s.DateOfBirth, s.IsLivingAlone, p.City
        FROM OLTPAppDb.dbo.Senior s
        JOIN OLTPAppDb.dbo.Person p
            ON s.SeniorID = p.PersonID
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.DimSenior d
            WHERE d.SeniorID = s.SeniorID
        );

        -- 3. DimProfessional
        UPDATE d SET
            d.FirstName = p.FirstName,
            d.LastName = p.LastName
        FROM dbo.DimProfessional d
        JOIN OLTPAppDb.dbo.Caregiver c
            ON d.ProfessionalID = c.CaregiverID
        JOIN OLTPAppDb.dbo.Person p
            ON c.CaregiverID = p.PersonID;

        INSERT INTO dbo.DimProfessional
            (ProfessionalID, FirstName, LastName)
        SELECT
            c.CaregiverID, p.FirstName, p.LastName
        FROM OLTPAppDb.dbo.Caregiver c
        JOIN OLTPAppDb.dbo.Person p
            ON c.CaregiverID = p.PersonID
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.DimProfessional d
            WHERE d.ProfessionalID = c.CaregiverID
        );

        -- 4. DimService
        UPDATE d SET
            d.ServiceName = s.ServiceName
        FROM dbo.DimService d
        JOIN OLTPAppDb.dbo.Service s
            ON d.ServiceID = s.ServiceID;

        INSERT INTO dbo.DimService
            (ServiceID, ServiceName)
        SELECT s.ServiceID, s.ServiceName
        FROM OLTPAppDb.dbo.Service s
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.DimService d
            WHERE d.ServiceID = s.ServiceID
        );

        -- 5. DimOrganization
        UPDATE d SET
            d.OrganizationName = a.Name
        FROM dbo.DimOrganization d
        JOIN OLTPAppDb.dbo.Agency a
            ON d.OrganizationID = a.AgencyID;

        INSERT INTO dbo.DimOrganization
            (OrganizationID, OrganizationName)
        SELECT a.AgencyID, a.Name
        FROM OLTPAppDb.dbo.Agency a
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.DimOrganization d
            WHERE d.OrganizationID = a.AgencyID
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO
