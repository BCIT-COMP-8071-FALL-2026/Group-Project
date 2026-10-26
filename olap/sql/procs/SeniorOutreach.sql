-- =============================================
-- Query 2/4: Senior Outreach List
-- Seniors with 3+ missed visits in the 90 days before the anchor date,
-- with lives alone and age band (Query 2 columns).
-- Story: OLAP-3 (docs/User-Stories.md)
-- Definitions: docs/OLAP_Queries.md
-- Database: OlapDW
-- =============================================

USE OlapDW;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SeniorOutreach
AS
BEGIN
    SET NOCOUNT ON;

    -- Use the current UTC time for cutoff evaluation.
    DECLARE @EvaluationTime DATETIME2(7) =
        SYSUTCDATETIME();

    -- Anchor: latest scheduled start that has passed,
    -- excluding cancelled visits. NULL when there are none,
    -- which returns no rows.
    DECLARE @Anchor DATETIME2(7) =
    (
        SELECT MAX(ScheduledStart)
        FROM dbo.FactVisit
        WHERE CancelledCount = 0
          AND ScheduledStart <= @EvaluationTime
    );

    DECLARE @AnchorDate DATE = CAST(@Anchor AS DATE);

    -- Missed visits in the 90-day window (both ends included).
    -- Not yet due visits are excluded, as in Query 1.
    WITH MissedVisits AS
    (
        SELECT
            SeniorKey,
            ServiceKey
        FROM dbo.FactVisit
        WHERE CancelledCount = 0
          AND ScheduledStart >= DATEADD(DAY, -90, @Anchor)
          AND ScheduledStart <= @Anchor
          AND DATEADD(MINUTE, 60, ScheduledStart)
              <= @EvaluationTime
          AND (
                ActualCheckIn IS NULL
                OR ActualCheckIn >
                   DATEADD(MINUTE, 60, ScheduledStart)
              )
    ),
    SeniorCounts AS
    (
        SELECT
            SeniorKey,
            COUNT(*) AS MissedCount
        FROM MissedVisits
        GROUP BY SeniorKey
        HAVING COUNT(*) >= 3
    ),
    -- Service types of the missed visits, one row per senior and service.
    SeniorServices AS
    (
        SELECT DISTINCT
            m.SeniorKey,
            s.ServiceName
        FROM MissedVisits m
        JOIN dbo.DimService s
            ON s.ServiceKey = m.ServiceKey
    ),
    ServiceLists AS
    (
        SELECT
            SeniorKey,
            STRING_AGG(ServiceName, N', ')
                WITHIN GROUP (ORDER BY ServiceName) AS ServiceTypes
        FROM SeniorServices
        GROUP BY SeniorKey
    ),
    -- Age in whole years at the anchor date.
    SeniorAges AS
    (
        SELECT
            SeniorKey,
            DATEDIFF(YEAR, DateOfBirth, @AnchorDate)
            - CASE
                WHEN DATEADD(
                         YEAR,
                         DATEDIFF(YEAR, DateOfBirth, @AnchorDate),
                         DateOfBirth
                     ) > @AnchorDate
                THEN 1
                ELSE 0
              END AS Age
        FROM dbo.DimSenior
    )

    SELECT
        d.FirstName + N' ' + d.LastName AS SeniorName,
        c.MissedCount,
        l.ServiceTypes,
        d.IsLivingAlone AS LivesAlone,
        CASE
            WHEN a.Age < 65 THEN N'Under 65'
            WHEN a.Age < 75 THEN N'65-74'
            WHEN a.Age < 85 THEN N'75-84'
            ELSE N'85+'
        END AS AgeBand
    FROM SeniorCounts c
    JOIN dbo.DimSenior d
        ON d.SeniorKey = c.SeniorKey
    JOIN ServiceLists l
        ON l.SeniorKey = c.SeniorKey
    JOIN SeniorAges a
        ON a.SeniorKey = c.SeniorKey
    ORDER BY c.MissedCount DESC, SeniorName ASC;
END;
GO
