CREATE DATABASE SeniorCareTest;
GO

USE SeniorCareTest;
GO

CREATE TABLE SeniorVisits (
    Id INT PRIMARY KEY,
    SeniorName VARCHAR(100) NOT NULL,
    VisitCount INT NOT NULL
);
GO

INSERT INTO SeniorVisits (Id, SeniorName, VisitCount)
VALUES
(1, 'Mary', 5),
(2, 'John', 8),
(3, 'Sara', 3);
GO

SELECT * FROM SeniorVisits;