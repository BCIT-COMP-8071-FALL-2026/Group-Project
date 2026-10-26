# Senior Care Management - Data Dictionary

Draft, 2026-10-16; updated 2026-10-25 to match develop (exact types, UTC timestamps, and the OLAP tables). Aligned with the merged ERD, [OLAP_Queries.md](../OLAP_Queries.md) and [Query1-Contract.md](../Query1-Contract.md). Where it conflicts, the user stories, ERD, OLAP_Queries.md and Query1-Contract.md win.

Source: `Senior Care Management - Data Dictionary.xlsx`

## Visit

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| VisitID | INT | Yes | Primary Key, unique | No |
| SeniorID | INT | Yes | Foreign Key -> Senior.SeniorID | Yes |
| ServiceID | INT | Yes | Foreign Key -> Service.ServiceID | No |
| CaregiverID | INT | Yes | Foreign Key -> Caregiver.CaregiverID | Yes |
| AgencyID | INT | Yes | Foreign Key -> Agency.AgencyID; snapshot of the caregiver's agency at scheduling | No |
| CreatedByUserID | VARCHAR(450) | Yes | Foreign Key -> UserAccount; Agency Manager who scheduled | No |
| ScheduledStart | DATETIME2 | Yes | UTC timestamp; valid date and time | No |
| ActualCheckIn | DATETIME2 | No | UTC timestamp; NULL if no check-in | Yes |
| ActualCheckOut | DATETIME2 | No | UTC timestamp; must be >= ActualCheckIn when present | Yes |
| Status | VARCHAR(20) | Yes | Scheduled, InProgress, Completed, Cancelled; Status is never Missed | No |
| DestinationAddress | VARCHAR(300) | No | Optional, e.g. for doctor visits | Yes |
| Rule | Definition |  |  |  |
| Missed Visit | No check-in within 60 minutes of scheduled start (60 minutes counts as within), once scheduled start + 60 minutes has passed, and not Cancelled. Computed, never a Status value; a check-in after the cutoff does not clear it |  |  |  |
| Late Visit | Check-in is more than 10 minutes after scheduled start, and not Missed |  |  |  |
| Completed Visit | Check-in and check-out both recorded, and not Missed |  |  |  |
| Cancelled Visit | Status = Cancelled; excluded from VisitCount and MissedCount |  |  |  |
| Visit Duration | Difference between ActualCheckOut and ActualCheckIn in minutes |  |  |  |

## Person

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| PersonID | INT | Yes | Primary Key, unique | No |
| FirstName | VARCHAR(100) | Yes | Cannot be empty | Yes |
| MiddleName | VARCHAR(100) | No | Optional | Yes |
| LastName | VARCHAR(100) | Yes | Cannot be empty | Yes |
| Street | VARCHAR(200) | Yes | Cannot be empty | Yes |
| City | VARCHAR(100) | Yes | Cannot be empty | Yes |
| Province | VARCHAR(100) | Yes | Cannot be empty | Yes |
| Country | VARCHAR(100) | Yes | Cannot be empty | Yes |
| PostalCode | VARCHAR(20) | Yes | Cannot be empty | Yes |
| Phone | VARCHAR(20) | Yes | Cannot be empty | Yes |
| Rule | Definition |  |  |  |
| Shared details | Names, address and phone for Seniors, Caregivers, Family Members and Agency Managers live here; SeniorID, CaregiverID, FamilyMemberID and AgencyManagerID equal PersonID. Login email is on the user account (AspNetUsers) |  |  |  |

## Senior

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| SeniorID | INT | Yes | Primary Key; Foreign Key -> Person (PersonID) | No |
| DateOfBirth | DATE | Yes | Valid date, cannot be in the future | Yes |
| IsLivingAlone | BIT | Yes | TRUE or FALSE | Yes |
| CurrentDoctorName | VARCHAR(200) | No | Optional | Yes |
| CurrentDoctorPhone | VARCHAR(20) | No | Optional | Yes |
| MedicalConditions | TEXT | No | Optional | Yes |
| CurrentMedication | TEXT | No | Optional, one text box | Yes |
| EmergencyContactName | VARCHAR(200) | No | Optional | Yes |
| EmergencyContactPhone | VARCHAR(20) | No | Optional | Yes |
| EmergencyContactRelationship | VARCHAR(50) | No | Optional | Yes |
| Status | VARCHAR(30) | Yes | Active, Deactivated | No |
| DeactivatedAt | DATETIME2 | No | Required when Status is Deactivated; otherwise NULL | No |
| Rule | Definition |  |  |  |
| Age Calculation | Calculate age using DateOfBirth at the anchor date (OLAP_Queries.md) |  |  |  |
| IsLivingAlone | TRUE = lives alone; FALSE = does not live alone |  |  |  |
| Age Group | Under 65, 65-74, 75-84, 85+, calculated at the anchor date |  |  |  |
| Personal Information | Senior personal and medical information must be protected |  |  |  |

## Caregiver

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| CaregiverID | INT | Yes | Primary Key; Foreign Key -> Person (PersonID) | No |
| AgencyID | INT | Yes | Foreign Key -> Agency.AgencyID; exactly one agency | No |
| Rule | Definition |  |  |  |
| Person details | FirstName, LastName, address and phone are stored in Person; the Caregiver table has no name or email columns |  |  |  |
| Agency Assignment | Each caregiver belongs to one agency; an agency can have multiple caregivers |  |  |  |
| Visit Assignment | One caregiver can be assigned to multiple visits |  |  |  |
| Personal Information | Caregiver contact information (on Person) must be protected |  |  |  |

## Agency

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| AgencyID | INT | Yes | Primary Key, unique | No |
| Name | VARCHAR(200) | Yes | Cannot be empty | No |
| Street | VARCHAR(200) | Yes | Cannot be empty | No |
| City | VARCHAR(100) | Yes | Cannot be empty | No |
| Province | VARCHAR(100) | Yes | Cannot be empty | No |
| Country | VARCHAR(100) | Yes | Cannot be empty | No |
| PostalCode | VARCHAR(20) | Yes | Cannot be empty | No |
| Phone | VARCHAR(20) | Yes | Valid phone number | No |
| Email | VARCHAR(256) | Yes | Valid email format | Yes |
| Rule | Definition |  |  |  |
| Caregiver Assignment | One agency can employ multiple caregivers |  |  |  |
| Agency Relationship | Each caregiver belongs to one agency through AgencyID |  |  |  |
| Contact Information | Agency contact details must follow valid formats |  |  |  |

## Service

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| ServiceID | INT | Yes | Primary Key, unique | No |
| ServiceName | VARCHAR(100) | Yes | Cannot be empty; seeded as Doctor, Home visit, Bathing (OLTP-16) | No |
| Description | TEXT | No | Optional | No |

## OLAP DimDate

| Column | Data Type | Nullability | Key / Relationship | Description |
|---|---|---|---|---|
| DateKey | int | NOT NULL | PK; YYYYMMDD | Calendar surrogate key |
| FullDate | date | NOT NULL | UNIQUE | Calendar date |
| Day | int | NOT NULL |  | Day of month |
| Month | int | NOT NULL |  | Month number |
| Year | int | NOT NULL |  | Calendar year |
| Quarter | int | NOT NULL |  | Quarter number |
| DayOfWeek | nvarchar(10) | NOT NULL |  | Weekday label |

## OLAP DimSenior

| Column | Data Type | Nullability | Key / Relationship | Description |
|---|---|---|---|---|
| SeniorKey | int IDENTITY | NOT NULL | PK | Warehouse surrogate key |
| SeniorID | int | NOT NULL | UNIQUE; OLTP Senior.SeniorID | Natural key |
| FirstName | nvarchar(100) | NOT NULL |  | From Person |
| LastName | nvarchar(100) | NOT NULL |  | From Person |
| DateOfBirth | date | NOT NULL |  | Used for anchor-date age band |
| IsLivingAlone | bit | NOT NULL |  | From OLTP Senior |
| City | nvarchar(100) | NOT NULL |  | From Person.City |

## OLAP DimProfessional

| Column | Data Type | Nullability | Key / Relationship | Description |
|---|---|---|---|---|
| ProfessionalKey | int IDENTITY | NOT NULL | PK | Warehouse surrogate key |
| ProfessionalID | int | NOT NULL | UNIQUE; Caregiver.CaregiverID | Natural key |
| FirstName | nvarchar(100) | NOT NULL |  | From Person |
| LastName | nvarchar(100) | NOT NULL |  | From Person |

## OLAP DimService

| Column | Data Type | Nullability | Key / Relationship | Description |
|---|---|---|---|---|
| ServiceKey | int IDENTITY | NOT NULL | PK | Warehouse surrogate key |
| ServiceID | int | NOT NULL | UNIQUE; Service.ServiceID | Natural key |
| ServiceName | nvarchar(100) | NOT NULL |  | From OLTP Service |

## OLAP DimOrganization

| Column | Data Type | Nullability | Key / Relationship | Description |
|---|---|---|---|---|
| OrganizationKey | int IDENTITY | NOT NULL | PK | Warehouse surrogate key |
| OrganizationID | int | NOT NULL | UNIQUE; Agency.AgencyID | Natural key |
| OrganizationName | nvarchar(200) | NOT NULL |  | From Agency.Name |

## OLAP FactVisit

| Column | Data Type | Nullability | Key / Relationship | Description |
|---|---|---|---|---|
| VisitFactID | int | NOT NULL | PK; OLTP Visit.VisitID | One row per visit |
| DateKey | int | NOT NULL | FK -> DimDate.DateKey | Date from ScheduledStart UTC |
| SeniorKey | int | NOT NULL | FK -> DimSenior.SeniorKey |  |
| ProfessionalKey | int | NOT NULL | FK -> DimProfessional.ProfessionalKey |  |
| ServiceKey | int | NOT NULL | FK -> DimService.ServiceKey |  |
| OrganizationKey | int | NOT NULL | FK -> DimOrganization.OrganizationKey | From Visit.AgencyID |
| ScheduledStart | datetime2 | NOT NULL |  | UTC scheduled timestamp |
| ActualCheckIn | datetime2 | NULL |  | UTC check-in |
| ActualCheckOut | datetime2 | NULL |  | UTC check-out |
| DurationMinutes | int | NULL |  | DATEDIFF when both timestamps exist |
| LateMinutes | int | NULL |  | NULL missing check-in; otherwise max(0, delay) |
| CompletedCount | int | NOT NULL | CHECK 0/1 | Non-cancelled with both check-in and check-out and not Missed |
| CancelledCount | int | NOT NULL | CHECK 0/1 | Status = Cancelled |
| LateCheckInCount | int | NOT NULL | CHECK 0/1 | Non-cancelled, not Missed, delay >10 minutes |
