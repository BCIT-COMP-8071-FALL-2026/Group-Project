-- OlapDW: star schema for the OLAP reports (#68).
-- Runs on the same SQL Server as OLTPAppDb. Safe to rerun.

IF DB_ID(N'OlapDW') IS NULL
    CREATE DATABASE OlapDW;
GO
