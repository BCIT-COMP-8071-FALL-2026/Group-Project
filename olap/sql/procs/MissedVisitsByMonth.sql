
-- =============================================
-- Query 1: Missed Visits by Month
-- Contract: Query1-Contract.md v1.2
-- Database: OlapDW
-- =============================================

USE OlapDW;
GO

CREATE OR ALTER PROCEDURE dbo.usp_MissedVisitsByMonth
AS
BEGIN
    SET NOCOUNT ON;

    -- Use the current UTC time for cutoff evaluation.
    DECLARE @EvaluationTime DATETIME2(7) =
        SYSUTCDATETIME();

    -- Include only non-cancelled visits
    -- whose 60-minute cutoff has passed.
    WITH EligibleVisits AS
    (
        SELECT
            CONVERT(CHAR(7), ScheduledStart, 126) AS [Month],
            ScheduledStart,
            ActualCheckIn
        FROM dbo.FactVisit
        WHERE CancelledCount = 0
          AND DATEADD(MINUTE, 60, ScheduledStart)
              <= @EvaluationTime
    ),
    MonthlyCounts AS
    (
        SELECT
            [Month],
            COUNT(*) AS VisitCount,
            SUM(
                CASE
                    WHEN ActualCheckIn IS NULL
                         OR ActualCheckIn >
                            DATEADD(MINUTE, 60, ScheduledStart)
                    THEN 1
                    ELSE 0
                END
            ) AS MissedCount
        FROM EligibleVisits
        GROUP BY [Month]
    )

    SELECT
        [Month],
        VisitCount,
        MissedCount,
        CAST(
            ROUND(
                MissedCount * 100.0 /
                NULLIF(VisitCount, 0),
                1
            ) AS DECIMAL(5,1)
        ) AS MissedPercent
    FROM MonthlyCounts
    ORDER BY [Month] ASC;
END;
GO
