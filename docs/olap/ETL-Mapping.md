# Senior Care Management - ETL Mapping

Draft, 2026-10-16. Aligned with the merged ERD, [OLAP_Queries.md](../OLAP_Queries.md) and [Query1-Contract.md](../Query1-Contract.md). Dimension names (DimProfessional, DimOrganization) are still open. Where it conflicts, the user stories, ERD, OLAP_Queries.md and Query1-Contract.md win.

Source: `Senior Care Management - ETL Mapping.xlsx`

## DimDate

| OLAP Column | OLAP Data Type | OLTP Source Table | OLTP Source Field | Transformation Rule | Load Order | Notes / Dependencies |
|---|---|---|---|---|---|---|
| DateKey | int | Derived calendar | N/A | Surrogate key, typically YYYYMMDD, generated from FullDate. | 1 | Primary key; load before FactVisit. |
| FullDate | date | Visit | ScheduledStart | Date portion of ScheduledStart (UTC, no timezone conversion); populate a complete calendar for the required reporting horizon. | 1 | Source date is scheduled visit date in UTC. |
| Day | int | Derived calendar | N/A | Day of month from FullDate. | 1 | Derived. |
| Month | int | Derived calendar | N/A | Month number from FullDate. | 1 | Used for monthly reporting. |
| Year | int | Derived calendar | N/A | Calendar year from FullDate. | 1 | Used for monthly reporting. |
| Quarter | int | Derived calendar | N/A | Quarter from FullDate (1-4). | 1 | Derived. |
| DayOfWeek | varchar | Derived calendar | N/A | Day name or standardized weekday label from FullDate. | 1 | Derived; exact label convention not shown. |

## DimSenior

| OLAP Column | OLAP Data Type | OLTP Source Table | OLTP Source Field | Transformation Rule | Load Order | Notes / Dependencies |
|---|---|---|---|---|---|---|
| SeniorKey | int | Senior | SeniorID | Generate warehouse surrogate key; retain stable key across refreshes. | 2 | Primary key; referenced by FactVisit. |
| SeniorID | int | Senior | SeniorID | Copy natural OLTP identifier. | 2 | Natural/business key. |
| FirstName | varchar | Person | FirstName | Join Senior.SeniorID to Person.PersonID; copy FirstName. | 2 | Person details are normalized in OLTP. |
| LastName | varchar | Person | LastName | Join Senior.SeniorID to Person.PersonID; copy LastName. | 2 | Person details are normalized in OLTP. |
| DateOfBirth | date | Senior | DateOfBirth | Copy DateOfBirth. | 2 | No stored AgeGroup. The query computes the age band (Under 65, 65-74, 75-84, 85+) at the anchor date (OLAP_Queries.md). |
| IsLivingAlone | bit | Senior | IsLivingAlone | Copy IsLivingAlone. | 2 | Same name and type as OLTP. |
| City | varchar | Person | City | Join Senior.SeniorID to Person.PersonID; copy City. | 2 | Person stores City as its own field. |

## DimProfessional

| OLAP Column | OLAP Data Type | OLTP Source Table | OLTP Source Field | Transformation Rule | Load Order | Notes / Dependencies |
|---|---|---|---|---|---|---|
| ProfessionalKey | int | Caregiver | CaregiverID | Generate warehouse surrogate key. | 2 | Primary key; referenced by FactVisit. |
| ProfessionalID | int | Caregiver | CaregiverID | Copy natural OLTP identifier. | 2 | Natural/business key. |
| FirstName | varchar | Person | FirstName | Join Caregiver.CaregiverID to Person.PersonID; copy FirstName. | 2 | Person details are normalized in OLTP. |
| LastName | varchar | Person | LastName | Join Caregiver.CaregiverID to Person.PersonID; copy LastName. | 2 | Person details are normalized in OLTP. |

## DimService

| OLAP Column | OLAP Data Type | OLTP Source Table | OLTP Source Field | Transformation Rule | Load Order | Notes / Dependencies |
|---|---|---|---|---|---|---|
| ServiceKey | int | Service | ServiceID | Generate warehouse surrogate key. | 2 | Primary key; referenced by FactVisit. |
| ServiceID | int | Service | ServiceID | Copy natural OLTP identifier. | 2 | Natural/business key. |
| ServiceName | varchar | Service | ServiceName | Copy service name. | 2 | ERD source field appears as ServiceName. |

## DimOrganization

| OLAP Column | OLAP Data Type | OLTP Source Table | OLTP Source Field | Transformation Rule | Load Order | Notes / Dependencies |
|---|---|---|---|---|---|---|
| OrganizationKey | int | Agency | AgencyID | Generate warehouse surrogate key. | 2 | Primary key; referenced by FactVisit. |
| OrganizationID | int | Agency | AgencyID | Copy natural OLTP identifier. | 2 | Natural/business key. |
| OrganizationName | varchar | Agency | Name | Copy agency name. | 2 | |

## FactVisit

| OLAP Column | OLAP Data Type | OLTP Source Table | OLTP Source Field | Transformation Rule | Load Order | Notes / Dependencies |
|---|---|---|---|---|---|---|
| VisitFactID | int | Visit | VisitID | Use VisitID as fact row identifier or generate a fact surrogate if warehouse policy requires; preserve one row per visit. | 3 | Primary key; one fact row per OLTP visit. |
| DateKey | int | Visit | ScheduledStart | Lookup DimDate.DateKey using date portion of ScheduledStart (UTC, no timezone conversion). | 3 | Depends on DimDate. |
| SeniorKey | int | Visit | SeniorID | Lookup DimSenior.SeniorKey by SeniorID. | 3 | Depends on DimSenior. |
| ProfessionalKey | int | Visit | CaregiverID | Lookup DimProfessional.ProfessionalKey by CaregiverID. | 3 | Depends on DimProfessional. |
| ServiceKey | int | Visit | ServiceID | Lookup DimService.ServiceKey by ServiceID. | 3 | Depends on DimService. |
| OrganizationKey | int | Visit | AgencyID | Lookup DimOrganization.OrganizationKey by Visit.AgencyID. | 3 | Depends on DimOrganization. Visit.AgencyID is the caregiver's agency at scheduling. |
| ScheduledStart | datetime | Visit | ScheduledStart | Copy scheduled timestamp. | 3 | Same name as OLTP and the Query 1 contract. |
| ActualCheckIn | datetime | Visit | ActualCheckIn | Copy actual check-in timestamp; preserve null when absent. | 3 | Same name as OLTP and the Query 1 contract. |
| ActualCheckOut | datetime | Visit | ActualCheckOut | Copy actual check-out timestamp; preserve null when absent. | 3 | Same name as OLTP. |
| DurationMinutes | int | Visit | ActualCheckIn, ActualCheckOut | DATEDIFF in minutes when both timestamps exist; otherwise null. | 3 | Completed requires both check-in and check-out. |
| LateMinutes | int | Visit | ScheduledStart, ActualCheckIn | NULL when ActualCheckIn is missing; 0 when check-in is early or on time; otherwise positive minutes from ScheduledStart to ActualCheckIn. LateCheckInCount = 1 only when this delay is greater than 10 minutes and the visit is not Missed; otherwise 0. | 3 | Missing check-in is NULL, not zero. Late threshold is strictly >10 minutes. |
| CompletedCount | int | Visit | ActualCheckIn, ActualCheckOut, Status | Set CompletedCount = 1 when Status is not Cancelled, both ActualCheckIn and ActualCheckOut are present, and ActualCheckIn is at or before ScheduledStart + 60 minutes (not Missed); otherwise 0. | 3 | Completed means both check-in and check-out are recorded, and not Missed. |
| CancelledCount | int | Visit | Status | 1 when Status = Cancelled; else 0. | 3 | Supports cancelled exclusion and auditability. |
| LateCheckInCount | int | Visit | ScheduledStart, ActualCheckIn, Status | 1 when non-cancelled check-in is more than 10 minutes after scheduled start and at or before scheduled start + 60 minutes (not Missed); else 0. | 3 | Core late rule from OLAP_Queries. |

## Summary

| ETL Mapping Summary |  |  |
|---|---|---|
| Item | Details |  |
| Source basis | docs/ERD.drawio (OLTP), docs/OLAP_Queries.md and docs/Query1-Contract.md. |  |
| Recommended load order | 1. DimDate; 2. DimSenior, DimProfessional, DimService, DimOrganization; 3. FactVisit; 4. Aggregate/report queries. |  |
| Fact grain | One FactVisit row per VisitID. |  |
| Missed rule | Computed by each query procedure (Queries 1, 2 and 4) at run time, not by the ETL: no check-in within 60 minutes of scheduled start (60 minutes counts as within), once that cutoff has passed, and not Cancelled. VisitCount (excluding cancelled and not yet due) is also computed there. |  |
| Late rule | Check-in more than 10 minutes after scheduled start, and not Missed. |  |
| Time rule | All times are UTC with no timezone conversion; DateKey and month come from UTC ScheduledStart. |  |
| Age band rule | Calculate senior age at the anchor date: Under 65, 65-74, 75-84, 85+. |  |
