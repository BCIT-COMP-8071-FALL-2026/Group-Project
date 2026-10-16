# Senior Care Management - Data Dictionary

Draft, 2026-10-16. Not aligned with the merged ERD yet. Where it conflicts, [Query1-Contract.md](../Query1-Contract.md) and [OLAP_Queries.md](../OLAP_Queries.md) win. Fixes are tracked in a GitHub issue assigned to Shayan.

Source: `Senior Care Management - Data Dictionary.xlsx`

## Visit

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| VisitID | INT | Yes | Primary Key, unique | No |
| SeniorID | INT | Yes | Foreign Key -> senior | Yes |
| ServiceID | INT | Yes | Foreign Key -> Service | No |
| CaregiverID | INT | Yes | Foreign Key -> Caregiver | Yes |
| SchedueledStartTime | DATETIME | Yes | Valid date and time | No |
| CehckInDateTime | DATETIME | No | Valid date and time; NULL if no check-in | Yes |
| CheckOutDateTime | DATETIME | No | Must be after CheckInDateTime | Yes |
| VisitStatus | VARCHAR(20) | Yes | Scheduled, Completed, Missed, Cancelled | No |
| Rule | Definition |  |  |  |
| Missed Visit | VisitStatus = Missed OR no check-in within 60 minutes after scheduled start |  |  |  |
| Late Visit | Check-in is more than 10 minutes after scheduled start |  |  |  |
| Completed Visit | VisitStatus = Completed |  |  |  |
| Cancelled Visit | VisitStatus = Cancelled; excluded from missed counts |  |  |  |
| Visit Duration | Difference between CheckOutDateTime and CheckInDateTime in minutes |  |  |  |

## Senior

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| SeniorID | INT | Yes | Primary Key, unique | No |
| Person ID | INT | Yes | Foreign Key -> Person, unique | Yes |
| DateOfBirth | DATE | Yes | Valid date, cannot be in the future | Yes |
| IsLivingAlone | BOOLEAN | Yes | TRUE or FALSE | Yes |
| AdditionalNotes | TEXT | No | Optional notes about the senior | Yes |
| ProfessionalID | INT | No | Foreign Key → Caregiver | Yes |
| InsuranceID | INT | No | Foreign Key → InsuranceCoverage | Yes |
| Rule | Definition |  |  |  |
| Age Calculation | Calculate age using DateOfBirth and the visit date |  |  |  |
| IsLivingAlone | TRUE = lives alone; FALSE = does not live alone |  |  |  |
| Age Group | Calculated during ETL using age at the visit date |  |  |  |
| Personal Information | Senior personal and medical information must be protected |  |  |  |

## CareGiver

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| CaregiverID | INT | Yes | Primary Key, unique | No |
| AgencyID | INT | Yes | Foreign Key -> Agency | No |
| FirstName | VARCHAR(100) | Yes | Cannot be empty | Yes |
| LastName | VARCHAR(100) | Yes | Cannot be empty | Yes |
| Email | VARCHAR(255) | Yes | Valid email format, unique | Yes |
| Phone | VARCHAR(20) | No | Valid phone number | Yes |
| RoleID | INT | Yes | Foreign Key → ProfessionalRoles | No |
| IsActive | BOOLEAN | Yes | TRUE or FALSE | No |
| Rule | Definition |  |  |  |
| Agency Assignment | Each caregiver belongs to one agency; an agency can have multiple caregivers |  |  |  |
| Visit Assignment | One caregiver can be assigned to multiple visits |  |  |  |
| Active Status | TRUE = currently active; FALSE = inactive |  |  |  |
| Role Assignment | Each caregiver has a role defined in ProfessionalRoles |  |  |  |
| Personal Information | Caregiver contact information must be protected |  |  |  |

## Agency

| Field Name | Data Type | Required | Allowed Values/Rules | Sensitive |
|---|---|---|---|---|
| AgencyID | INT | Yes | Primary Key, unique | No |
| AgencyName | VARCHAR(150) | Yes | Cannot be empty | No |
| Address | VARCHAR(255) | Yes | Valid business address | No |
| Phone | VARCHAR(20) | Yes | Valid phone number | No |
| Email | VARCHAR(255) | No | Valid email format | No |
| AgencyType | VARCHAR(50) | Yes | Home Care, Nursing, Medical, Other | No |
| IsActive | BOOLEAN | Yes | TRUE or FALSE | No |
| Rule | Definition |  |  |  |
| Caregiver Assignment | One agency can employ multiple caregivers |  |  |  |
| Agency Relationship | Each caregiver belongs to one agency through AgencyID |  |  |  |
| Agency Type | Identifies the category of care services provided |  |  |  |
| Active Status | TRUE = active agency; FALSE = inactive agency |  |  |  |
| Contact Information | Agency contact details must follow valid formats |  |  |  |
