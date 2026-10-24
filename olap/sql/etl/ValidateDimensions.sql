
USE OlapDW;
GO

-- Check dimension row counts
SELECT 'DimDate' AS DimensionName, COUNT(*) AS [RowCount]
FROM dbo.DimDate

UNION ALL
SELECT 'DimSenior', COUNT(*)
FROM dbo.DimSenior

UNION ALL
SELECT 'DimProfessional', COUNT(*)
FROM dbo.DimProfessional

UNION ALL
SELECT 'DimService', COUNT(*)
FROM dbo.DimService

UNION ALL
SELECT 'DimOrganization', COUNT(*)
FROM dbo.DimOrganization;
GO

-- Check duplicate natural keys
SELECT SeniorID, COUNT(*) AS DuplicateCount
FROM dbo.DimSenior
GROUP BY SeniorID
HAVING COUNT(*) > 1;

SELECT ProfessionalID, COUNT(*) AS DuplicateCount
FROM dbo.DimProfessional
GROUP BY ProfessionalID
HAVING COUNT(*) > 1;

SELECT ServiceID, COUNT(*) AS DuplicateCount
FROM dbo.DimService
GROUP BY ServiceID
HAVING COUNT(*) > 1;

SELECT OrganizationID, COUNT(*) AS DuplicateCount
FROM dbo.DimOrganization
GROUP BY OrganizationID
HAVING COUNT(*) > 1;
GO
