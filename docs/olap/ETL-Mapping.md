# Senior Care Management - ETL Mapping

Draft, 2026-10-16. OLTP source tables and fields now match the merged ERD. Rule fixes and dimension names are still open. Where it conflicts, [Query1-Contract.md](../Query1-Contract.md) and [OLAP_Queries.md](../OLAP_Queries.md) win. Fixes are tracked in a GitHub issue assigned to Shayan.

Source: `Senior Care Management - ETL Mapping.xlsx`

## DimDate

| OLAP Column | OLAP Data Type | OLTP Source Table | OLTP Source Field | Transformation Rule | Load Order | Notes / Dependencies |
|---|---|---|---|---|---|---|
| DateKey | int | Derived calendar | N/A | Surrogate key, typically YYYYMMDD, generated from FullDate. | 1 | Primary key; load before FactVisit. |
| FullDate | date | Visit | ScheduledStart | Date portion of ScheduledStart; populate a complete calendar for the required reporting horizon. | 1 | Source date is scheduled visit date. |
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
| AgeGroup | varchar | Senior | DateOfBirth | Do not treat AgeGroup as a timeless senior attribute. Calculate age at each visit's scheduled date from DateOfBirth, then band as 65-74, 75-84, or 85+. Preferred approved-schema approach: derive the band during FactVisit ETL (or use a visit-specific bridge/history keyed by VisitID) while retaining this dimension column for compatibility. | 2 | Limitation: one static AgeGroup per senior becomes historically incorrect as the senior crosses a band. No physical OLAP schema change is approved; confirm whether the group prefers a FactVisit-derived VisitAgeBand or a historical/Type 2 dimension strategy. |
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
| DateKey | int | Visit | ScheduledStart | Lookup DimDate.DateKey using date portion of ScheduledStart. | 3 | Depends on DimDate. |
| SeniorKey | int | Visit | SeniorID | Lookup DimSenior.SeniorKey by SeniorID. | 3 | Depends on DimSenior. |
| ProfessionalKey | int | Visit | CaregiverID | Lookup DimProfessional.ProfessionalKey by CaregiverID. | 3 | Depends on DimProfessional. |
| ServiceKey | int | Visit | ServiceID | Lookup DimService.ServiceKey by ServiceID. | 3 | Depends on DimService. |
| OrganizationKey | int | Caregiver | AgencyID | Lookup DimOrganization.OrganizationKey through Caregiver.AgencyID to Agency.AgencyID. | 3 | Depends on DimProfessional and DimOrganization. |
| ScheduledStart | datetime | Visit | ScheduledStart | Copy scheduled timestamp. | 3 | Same name as OLTP and the Query 1 contract. |
| ActualCheckIn | datetime | Visit | ActualCheckIn | Copy actual check-in timestamp; preserve null when absent. | 3 | Same name as OLTP and the Query 1 contract. |
| ActualCheckOut | datetime | Visit | ActualCheckOut | Copy actual check-out timestamp; preserve null when absent. | 3 | Same name as OLTP. |
| DurationMinutes | int | Visit | ActualCheckIn, ActualCheckOut | DATEDIFF in minutes when both timestamps exist; otherwise null. | 3 | Completed requires both check-in and check-out. |
| LateMinutes | int | Visit | ScheduledStart, ActualCheckIn | NULL when ActualCheckIn is missing; 0 when check-in is early or on time; otherwise positive minutes from ScheduledStart to ActualCheckIn. LateCheckInCount = 1 only when this delay is greater than 10 minutes; otherwise 0. | 3 | Missing check-in is NULL, not zero. Late threshold is strictly >10 minutes. |
| VisitCount | int | Visit | VisitID, Status | Set 1 for a non-cancelled scheduled visit and 0 for cancelled visits; confirm this cancelled-visit exclusion against the OLAP Query 1 contract before finalizing the denominator. | 3 | Query 1 contract confirmation required for cancelled-visit treatment in scheduled VisitCount. |
| CompletedCount | int | Visit | ActualCheckIn, ActualCheckOut, Status | Set CompletedCount = 1 when Status is not Cancelled and both ActualCheckIn and ActualCheckOut are present; otherwise 0. | 3 | Completed means both check-in and check-out are recorded for a non-cancelled visit. |
| MissedCount | int | Visit | Status, ScheduledStart, ActualCheckIn | Set MissedCount = 1 for non-cancelled visits explicitly marked Missed, or when no check-in occurred within 60 minutes of ScheduledStart and the evaluation time has reached that cutoff. A check-in after the cutoff does not clear the missed flag. Future and pending visits must not be marked missed prematurely. | 3 | Core missed rule from OLAP_Queries. |
| CancelledCount | int | Visit | Status | 1 when Status = Cancelled; else 0. | 3 | Supports cancelled exclusion and auditability. |
| LateCheckInCount | int | Visit | ScheduledStart, ActualCheckIn, Status | 1 when non-cancelled check-in is more than 10 minutes after scheduled start; else 0. | 3 | Core late rule from OLAP_Queries. |
| TimeOfDay | varchar | Visit | ScheduledStart | PROPOSED only: derive Morning/Afternoon/Evening from ScheduledStart after the group approves category boundaries. Do not treat the categories or cutoffs as finalized schema. | 3 | Time-of-day support is required by Query 3, but boundaries require group approval; no schema change is made here. |

## Summary

| ETL Mapping Summary |  |  |
|---|---|---|
| Item | Details |  |
| Source basis | docs/ERD.drawio (OLTP), docs/OLAP_Queries.md and docs/Query1-Contract.md. |  |
| Recommended load order | 1. DimDate; 2. DimSenior, DimProfessional, DimService, DimOrganization; 3. FactVisit; 4. Aggregate/report queries. |  |
| Fact grain | One FactVisit row per VisitID. |  |
| Missed rule | Status = Missed OR no check-in within 60 minutes after scheduled start, excluding cancelled visits. |  |
| Late rule | Check-in more than 10 minutes after scheduled start. |  |
| Age band rule | Calculate senior age at the scheduled visit date: 65-74, 75-84, 85+. |  |
| Time-of-day gap | OLAP Query 3 requires Morning/Afternoon/Evening, but no TimeOfDay field or DimTime is shown in the OLAP ERD. FactVisit.TimeOfDay is marked PROPOSED. |  |
| Unresolved schema issues |  |  |
| Issue Type | Field / Naming | Resolution or confirmation needed |
| Schema | AgeGroup storage | Static DimSenior AgeGroup cannot preserve visit-date history; approve a FactVisit-derived band or historical dimension strategy. |
| Schema | TimeOfDay | Required by Query 3 but proposed in FactVisit; Morning/Afternoon/Evening boundaries require group approval. |
