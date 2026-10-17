# Senior Care Management - Seed Specification

Draft, 2026-10-16. Aligned with the merged ERD, [OLAP_Queries.md](../OLAP_Queries.md) and [Query1-Contract.md](../Query1-Contract.md). Where it conflicts, the user stories, ERD, OLAP_Queries.md and Query1-Contract.md win.

Source: `Senior Care Management - Seed Specification.xlsx`

## Overview

| Senior Care Management — Task 3 Seed Specification |  |
|---|---|
| Item | Specification |
| Purpose | Define synthetic seed-data volumes, distributions, relationships, and validation rules. This workbook specifies data only; it does not contain actual seed records. |
| Seniors | 150 |
| Caregivers | 30 |
| Agencies | 8 |
| Services | 3 |
| Visits | 3000 |
| Date range | 2025-01-01 through 2026-03-31 inclusive (15 months) |
| Referential integrity | Every Visit references an existing Senior, Caregiver, Service and Agency. Visit.AgencyID equals the caregiver's AgencyID at scheduling. |
| Source definitions | Business rules from OLAP_Queries.md and Query1-Contract.md: missed, late, completed, missed percentage, age bands at the anchor date, and the Query 4 window of 90 days before the anchor date. |

## Entity Counts

| Entity | Proposed Count | Relationship / integrity requirement | Assumption status |
|---|---|---|---|
| Senior | 150 | Unique SeniorID; every visit SeniorID must match one senior. | Proposed synthetic count |
| Caregiver | 30 | Unique Caregiver/ProfessionalID; each caregiver assigned to one of 8 agencies. | Proposed synthetic count |
| Agency | 8 | Unique Agency/OrganizationID; all caregiver AgencyID values must match. | Proposed synthetic count |
| Agency Manager | 8 | One per agency; a Person row, an AgencyManager row (AgencyManagerID = PersonID, AgencyID), and a user account in the Agency Manager role linked by PersonID. | Required by OLTP-18 |
| Person | 188 | One per senior, caregiver and Agency Manager (150 + 30 + 8). Senior.SeniorID and Caregiver.CaregiverID equal Person.PersonID. Each has FirstName, LastName, Street, City, Province, Country, PostalCode and Phone. | Required relationship |
| Service | 3 | Doctor, Home visit, Bathing (OLTP-16); every visit ServiceID must match. | Required by OLTP-16 |
| Visit | 3000 | Unique VisitID; exactly one fact row per visit. | Proposed synthetic count |
| Senior–Visit | n/a | Visits distributed across seniors; exactly five seniors (S001-S005) have 3+ misses in the 90 days before the anchor date. | Required test pattern |
| Caregiver–Agency | n/a | Every caregiver has a valid AgencyID and agencies have multiple caregivers where possible. | Required relationship |
| Visit-Agency | n/a | Every visit's AgencyID equals its caregiver's AgencyID at scheduling. | Required relationship |
| Visit–Service | n/a | Every visit references one of the 3 services. | Required relationship |

## Visit Distribution

| Visit category | Count | Percent | Status / timestamp rule | OLAP definition compliance |
|---|---|---|---|---|
| Completed on time | 2250 | 75% | Status = Completed; check-in and check-out present; check-in <= scheduled start + 10 minutes. | Completed (both timestamps recorded, not Missed); not Late. |
| Completed late | 360 | 12% | Status = Completed; both timestamps present; check-in > scheduled start + 10 minutes and <= scheduled start + 60 minutes. | Late (more than 10 minutes, not Missed); LateMinutes is positive. |
| Missed | 240 | 8% | Scheduled start + 60 minutes is in the past, and either Status = Scheduled with no check-in, or Status = Completed with check-in after scheduled start + 60 minutes. | Missed is computed, never a Status value. A check-in after 60 minutes does not clear it, and Completed does not prevent it. |
| Cancelled | 150 | 5% | Status = Cancelled; no check-in or check-out timestamps. | Excluded from VisitCount and MissedCount (Query1-Contract.md). |
| Total | 3000 | 100% | Counts sum to 3,000. Query 1 VisitCount sums to 2,850 (cancelled excluded). InProgress is not seeded, since every seed visit is in the past. | Synthetic distribution. |

## Monthly Volume

Volumes include cancelled visits. Query 1 VisitCount for a month is that month's volume minus its cancelled visits.

| Month | Scheduled visits (incl. cancelled) | Distribution note |
|---|---|---|
| 2025-01 | 170 | Proposed variation across the full date range |
| 2025-02 | 185 | Proposed variation across the full date range |
| 2025-03 | 195 | Proposed variation across the full date range |
| 2025-04 | 205 | Proposed variation across the full date range |
| 2025-05 | 220 | Proposed variation across the full date range |
| 2025-06 | 230 | Proposed variation across the full date range |
| 2025-07 | 240 | Proposed variation across the full date range |
| 2025-08 | 210 | Proposed variation across the full date range |
| 2025-09 | 190 | Proposed variation across the full date range |
| 2025-10 | 180 | Proposed variation across the full date range |
| 2025-11 | 185 | Proposed variation across the full date range |
| 2025-12 | 200 | Proposed variation across the full date range |
| 2026-01 | 220 | Proposed variation across the full date range |
| 2026-02 | 225 | Proposed variation across the full date range |
| 2026-03 | 145 | Proposed variation across the full date range |

## Senior Demographics

| Topic | Specification | Coverage target | Notes / approval |
|---|---|---|---|
| Age bands | Generate DateOfBirth values so age at the anchor date (2026-03-31) spans Under 65, 65-74, 75-84, and 85+. | All four bands represented | The query computes the band at the anchor date (OLAP_Queries.md). |
| Living arrangement | Set IsLivingAlone = true for a meaningful subset and false for the remainder. | Both values represented | Exact percentage is proposed and should be agreed by group. |
| Caregiver assignment | Distribute 30 caregivers across 8 agencies. | All agencies represented | Some caregivers should have higher late rates, and at least one has no Late visits, for Query 3. |
| Service mix | Use all 3 services (Doctor, Home visit, Bathing) with varied visit volumes. | All services represented | Seeded names per OLTP-16. |

## Query Test Scenarios

| Query | Scenario / example IDs | Dates or pattern | Expected result | Seed-design purpose |
|---|---|---|---|---|
| Query 1 | All visits | Monthly volumes listed on Monthly Volume, including cancelled. | VisitCount totals 2,850 and MissedCount 240 (8.4% overall); cancelled excluded from both. | Tests monthly reliability trend. |
| Query 2 | S001-S005 (the Query 4 positive set): S001, S003, S005 living alone; S002, S004 not. Age at the anchor date: S001 Under 65, S002 65-74, S003 75-84, S004 85+, S005 75-84. | Lives alone and Age band are columns on the Query 4 list. | Both IsLivingAlone values and all four age bands appear in the Query 4 result. | Tests Query 2 columns. |
| Query 3 | C001-C005 assigned higher late-rate profiles; C030 has only on-time visits plus one check-in after +60 (Missed, not Late); A001-A008 all represented | Late = check-in more than 10 minutes after scheduled start, and not Missed. | One bar per caregiver with Late visits; AvgMinutesLate over Late visits only, rounded to 1 decimal; C030 not shown. | Tests coaching analysis (OLAP-2). |
| Query 4 positive | Anchor date: the latest non-cancelled visit, on 2026-03-31. S001: 2026-01-05 (Doctor), 2026-02-10 (Home visit), 2026-03-15 (Bathing); S002: 2026-01-08 (Home visit), 2026-01-29 (Home visit), 2026-02-20 (Bathing); S003: 2026-01-12 (Doctor), 2026-02-25 (Doctor), 2026-03-20 (Home visit); S004: 2026-01-05 (Bathing), 2026-02-01 (Doctor), 2026-03-01 (Bathing); S005: 2026-01-20 (Home visit), 2026-02-15 (Bathing), 2026-03-10 (Doctor) | Each listed set falls inside the window 2025-12-31 to 2026-03-31, both ends included. | Each senior returns in Query 4 with MissedCount >= 3 and their missed appointment types listed. | Mandatory outreach test cases. |
| Query 4 negative | S006: one missed on 2026-02-10; S007: missed on 2025-12-01, 2026-01-15 and 2026-03-05 | Fewer than 3 misses inside the window; S007's 2025-12-01 miss is before it. | S006 and S007 are excluded. No senior outside S001-S005 has 3+ misses in the window. | Confirms threshold and window logic. |
| Timestamp boundary tests | Example visits around +10 and +60 minutes | Check-in at +10:00 is not Late; +10:01 is Late; +60:00 exact is within (Late, not Missed); +60:00.4 is Missed and not Late; no check-in becomes Missed once +60 has passed; check-in after +60 with Status Completed stays Missed. | Classifications follow OLAP_Queries.md and Query1-Contract.md. | Tests procedure and ETL edge cases. |

## Data Validation

| Validation check | Rule / formula logic | Expected result | Severity |
|---|---|---|---|
| Entity counts | Count seniors=150, caregivers=30, agencies=8, agency managers=8, persons=188, services=3, visits=3,000. | PASS when exact. | Error |
| Monthly coverage | 15 monthly buckets from 2025-01 through 2026-03; sum scheduled visits=3,000. | PASS when every month represented and total reconciles. | Error |
| Distribution reconciliation | 2250+360+240+150 = 3,000; 75%+12%+8%+5%=100%; Query 1 VisitCount = 3,000 - 150 = 2,850. | PASS when exact. | Error |
| Visit foreign keys | Every Visit.SeniorID, CaregiverID, ServiceID and AgencyID exists; Visit.AgencyID equals the caregiver's AgencyID; every SeniorID and CaregiverID has a Person row. | Zero orphan references. | Error |
| Completed timestamps | Completed visits have both check-in and check-out; cancelled visits have neither fabricated timestamp. | No invalid timestamp combinations. | Error |
| Late logic | LateMinutes NULL if no check-in; 0 if delay <=0; positive if delay >0. Late iff delay >10 minutes and check-in at or before +60 (not Missed). | All rows comply. | Error |
| Missed logic | Not Cancelled, and no check-in at or before +60 once +60 has passed. Status is never Missed; a check-in after +60 does not clear it; not yet due visits are not Missed. | All rows comply. | Error |
| Age band | Age computed at the anchor date and falls into Under 65, 65-74, 75-84, or 85+. | All four bands represented. | Error |
| Query 4 positive cases | Seniors with >=3 missed visits in the 90 days before the anchor date, both ends included. | Exactly S001-S005 qualify. | Error |
| Query 4 negative cases | S006 and S007 have <3 missed visits inside the window. | S006-S007 excluded. | Error |
| Query 3 zero-late caregiver | C030 has only on-time visits plus one check-in after +60, so no Late visits. | C030 absent from OLAP-2. | Error |
| Anchor date | A non-cancelled visit exists on 2026-03-31 and no visit is scheduled after it. | Anchor date is 2026-03-31. | Error |
