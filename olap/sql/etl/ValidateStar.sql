USE OlapDW;
GO

SELECT 'DimDate' AS TableName, COUNT(*) AS Rows FROM dbo.DimDate
UNION ALL SELECT 'DimSenior',       COUNT(*) FROM dbo.DimSenior
UNION ALL SELECT 'DimProfessional', COUNT(*) FROM dbo.DimProfessional
UNION ALL SELECT 'DimService',      COUNT(*) FROM dbo.DimService
UNION ALL SELECT 'DimOrganization', COUNT(*) FROM dbo.DimOrganization
UNION ALL SELECT 'FactVisit',       COUNT(*) FROM dbo.FactVisit;
GO

-- Duplicate natural keys: every result set must be empty after a full reload.
SELECT 'DimSenior' AS TableName, SeniorID AS NaturalKey, COUNT(*) AS Duplicates
FROM dbo.DimSenior GROUP BY SeniorID HAVING COUNT(*) > 1
UNION ALL
SELECT 'DimProfessional', ProfessionalID, COUNT(*) FROM dbo.DimProfessional GROUP BY ProfessionalID HAVING COUNT(*) > 1
UNION ALL
SELECT 'DimService', ServiceID, COUNT(*) FROM dbo.DimService GROUP BY ServiceID HAVING COUNT(*) > 1
UNION ALL
SELECT 'DimOrganization', OrganizationID, COUNT(*) FROM dbo.DimOrganization GROUP BY OrganizationID HAVING COUNT(*) > 1;
GO
