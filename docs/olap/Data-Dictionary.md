# Senior Care Management - Data Dictionary

Draft, 2026-10-16. Table and field names now match the merged ERD. Rule fixes are still open. Where it conflicts, [Query1-Contract.md](../Query1-Contract.md) and [OLAP_Queries.md](../OLAP_Queries.md) win. Fixes are tracked in a GitHub issue assigned to Shayan.

Source: `Senior Care Management - Data Dictionary.xlsx`

## Visit

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| VisitID | INT | Yes | Primary Key, unique | No |
| SeniorID | INT | Yes | Foreign Key -> Senior | Yes |
| ServiceID | INT | Yes | Foreign Key -> Service | No |
| CaregiverID | INT | Yes | Foreign Key -> Caregiver | Yes |
| AgencyID | INT | Yes | Foreign Key -> Agency; caregiver's agency at scheduling | No |
| CreatedByUserID | VARCHAR(450) | Yes | Foreign Key -> UserAccount; Agency Manager who scheduled | No |
| ScheduledStart | DATETIME | Yes | Valid date and time | No |
| ActualCheckIn | DATETIME | No | Valid date and time; NULL if no check-in | Yes |
| ActualCheckOut | DATETIME | No | Must be after ActualCheckIn | Yes |
| Status | VARCHAR(20) | Yes | Scheduled, InProgress, Completed, Cancelled | No |
| DestinationAddress | VARCHAR(300) | No | Optional, e.g. for doctor visits | Yes |
| Rule | Definition |  |  |  |
| Missed Visit | Status = Missed OR no check-in within 60 minutes after scheduled start |  |  |  |
| Late Visit | Check-in is more than 10 minutes after scheduled start |  |  |  |
| Completed Visit | Status = Completed |  |  |  |
| Cancelled Visit | Status = Cancelled; excluded from missed counts |  |  |  |
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
| Shared details | Names, address and phone for Seniors, Caregivers, Family Members and Agency Managers live here; login email is on UserAccount |  |  |  |

## Senior

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| SeniorID | INT | Yes | Primary Key; Foreign Key -> Person (PersonID) | No |
| DateOfBirth | DATE | Yes | Valid date, cannot be in the future | Yes |
| IsLivingAlone | BOOLEAN | Yes | TRUE or FALSE | Yes |
| CurrentDoctorName | VARCHAR(200) | No | Optional | Yes |
| CurrentDoctorPhone | VARCHAR(20) | No | Optional | Yes |
| MedicalConditions | TEXT | No | Optional | Yes |
| CurrentMedication | TEXT | No | Optional, one text box | Yes |
| EmergencyContactName | VARCHAR(200) | No | Optional | Yes |
| EmergencyContactPhone | VARCHAR(20) | No | Optional | Yes |
| EmergencyContactRelationship | VARCHAR(50) | No | Optional | Yes |
| Status | VARCHAR(30) | Yes | Active, Deactivated | No |
| DeactivatedAt | DATETIME | No | Set when Status becomes Deactivated | No |
| Rule | Definition |  |  |  |
| Age Calculation | Calculate age using DateOfBirth and the visit date |  |  |  |
| IsLivingAlone | TRUE = lives alone; FALSE = does not live alone |  |  |  |
| Age Group | Calculated during ETL using age at the visit date |  |  |  |
| Personal Information | Senior personal and medical information must be protected |  |  |  |

## Caregiver

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| CaregiverID | INT | Yes | Primary Key; Foreign Key -> Person (PersonID) | No |
| AgencyID | INT | Yes | Foreign Key -> Agency | No |
| Rule | Definition |  |  |  |
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
| Email | VARCHAR(256) | Yes | Valid email format | No |
| Rule | Definition |  |  |  |
| Caregiver Assignment | One agency can employ multiple caregivers |  |  |  |
| Agency Relationship | Each caregiver belongs to one agency through AgencyID |  |  |  |
| Contact Information | Agency contact details must follow valid formats |  |  |  |

## Service

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| ServiceID | INT | Yes | Primary Key, unique | No |
| ServiceName | VARCHAR(100) | Yes | Cannot be empty | No |
| Description | TEXT | No | Optional | No |
