-- =============================================
-- Query 3: Average Minutes Late per Caregiver
-- Story: OLAP-2 (docs/User-Stories.md)
-- Definitions: docs/OLAP_Queries.md
-- Database: OlapDW
-- =============================================

USE OlapDW;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CaregiverLateness
AS
BEGIN
    SET NOCOUNT ON;

    -- Use the current UTC time for cutoff evaluation.
    DECLARE @EvaluationTime DATETIME2(7) =
        SYSUTCDATETIME();

    -- Late: non-cancelled, past its 60-minute cutoff,
    -- checked in more than 10 minutes after scheduled start
    -- and within 60 minutes (not Missed).
    -- Minutes are computed from timestamps, not FactVisit.LateMinutes,
    -- which holds whole minutes only.
    WITH LateVisits AS
    (
        SELECT
            ProfessionalKey,
            DATEDIFF_BIG(MILLISECOND, ScheduledStart, ActualCheckIn)
                / 60000.0 AS MinutesLate
        FROM dbo.FactVisit
        WHERE CancelledCount = 0
          AND DATEADD(MINUTE, 60, ScheduledStart)
              <= @EvaluationTime
          AND ActualCheckIn >
              DATEADD(MINUTE, 10, ScheduledStart)
          AND ActualCheckIn <=
              DATEADD(MINUTE, 60, ScheduledStart)
    )

    -- Caregivers with no Late visits have no rows and are left out.
    SELECT
        p.FirstName + N' ' + p.LastName AS CaregiverName,
        CAST(
            ROUND(AVG(l.MinutesLate), 1)
            AS DECIMAL(5,1)
        ) AS AvgMinutesLate
    FROM LateVisits l
    JOIN dbo.DimProfessional p
        ON p.ProfessionalKey = l.ProfessionalKey
    GROUP BY p.ProfessionalKey, p.FirstName, p.LastName
    ORDER BY AvgMinutesLate DESC, CaregiverName ASC;
END;
GO
