# Senior Care Management - Seed Specification

Draft, 2026-10-16. Not aligned with the merged ERD yet. Where it conflicts, [Query1-Contract.md](../Query1-Contract.md) and [OLAP_Queries.md](../OLAP_Queries.md) win. Fixes are tracked in a GitHub issue assigned to Shayan.

Source: `Senior Care Management - Seed Specification.xlsx`

## Overview

| Senior Care Management — Task 3 Seed Specification |  |
|---|---|
| Item | Specification |
| Purpose | Define synthetic seed-data volumes, distributions, relationships, and validation rules. This workbook specifies data only; it does not contain actual seed records. |
| Seniors | 150 |
| Caregivers | 30 |
| Agencies | 8 |
| Services | 10 |
| Visits | 3000 |
| Date range | 2025-01-01 through 2026-03-31 inclusive (15 months) |
| Referential integrity | Every Visit references an existing Senior, Caregiver, Agency through caregiver assignment, and Service. |
| Source definitions | Business rules based on OLAP_Queries.md: missed, late, completed, missed percentage, age bands, and rolling 90-day Query 4. |

## Entity Counts

| Entity | Proposed Count | Relationship / integrity requirement | Assumption status |
|---|---|---|---|
| Senior | 150 | Unique SeniorID; every visit SeniorID must match one senior. | Proposed synthetic count |
| Caregiver | 30 | Unique Caregiver/ProfessionalID; each caregiver assigned to one of 8 agencies. | Proposed synthetic count |
| Agency | 8 | Unique Agency/OrganizationID; all caregiver AgencyID values must match. | Proposed synthetic count |
| Service | 10 | Unique ServiceID; every visit ServiceID must match. | Proposed synthetic count |
| Visit | 3000 | Unique VisitID; exactly one fact row per visit. | Proposed synthetic count |
| Senior–Visit | n/a | Visits distributed across seniors; at least five seniors have 3+ misses in a rolling 90 days. | Required test pattern |
| Caregiver–Agency | n/a | Every caregiver has a valid AgencyID and agencies have multiple caregivers where possible. | Required relationship |
| Visit–Service | n/a | Every visit references one of the 10 services. | Required relationship |

## Visit Distribution

| Visit category | Count | Percent | Status / timestamp rule | OLAP definition compliance |
|---|---|---|---|---|
| Completed on time | 2250 | 75% | VisitStatus not Cancelled; check-in and check-out present; check-in <= scheduled start + 10 minutes. | Completed when both check-in and check-out recorded; on-time is proposed test classification. |
| Completed late | 360 | 12% | VisitStatus not Cancelled; both timestamps present; check-in > scheduled start + 10 minutes. | LateCheckInCount = 1 only when delay >10 minutes; LateMinutes is positive. |
| Missed | 240 | 8% | Non-cancelled; explicit VisitStatus = Missed OR no check-in by 60 minutes after scheduled start once cutoff has elapsed. | A late check-in after 60 minutes does not clear missed. Future/pending visits are not marked missed prematurely. |
| Cancelled | 150 | 5% | VisitStatus = Cancelled; no fabricated check-in or check-out timestamps. | Cancelled visits excluded from missed percentage denominator, pending Query 1 contract confirmation. |
| Total | 3000 | 100% | Counts sum to 3,000. | Synthetic distribution; confirm status vocabulary against OLTP. |

## Monthly Volume

| Month | Scheduled visits | Distribution note |
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
| Age bands | Generate DateOfBirth values so age at scheduled visit date spans 65-74, 75-84, and 85+. | All three bands represented | Age must be evaluated at visit date; a static DimSenior AgeGroup is historically limited. |
| Living arrangement | Set IsLivingAlone = true for a meaningful subset and false for the remainder. | Both values represented | Exact percentage is proposed and should be agreed by group. |
| Visit-specific age band | Derive VisitAgeBand during fact generation or use an approved historical/bridge strategy keyed by VisitID. | Supports Query 2 by age band | Do not change approved physical schema without approval. |
| Caregiver assignment | Distribute 30 caregivers across 8 agencies. | All agencies represented | Some caregivers should have higher late rates for Query 3. |
| Service mix | Use all 10 services with varied but valid visit volumes. | All services represented | ServiceCategory requires approved mapping; do not infer from description. |
| Time of day | Schedule visits in morning, afternoon, and evening. | All categories represented | Boundaries are PROPOSED pending group approval. |

## Query Test Scenarios

| Query | Scenario / example IDs | Dates or pattern | Expected result | Seed-design purpose |
|---|---|---|---|---|
| Query 1 | All visits | Monthly volumes listed on Monthly Volume sheet; cancelled excluded from denominator. | Monthly VisitCount, MissedCount, Missed % reconcile to 3,000 total and 150 cancelled. | Tests monthly reliability trend. |
| Query 2 | S001–S050 living alone; S051–S150 not living alone; all age bands represented | Visits span 2025-01-01 to 2026-03-31. | Compare missed rate by IsLivingAlone and AgeBand at visit date. | Tests senior-group comparisons. |
| Query 3 | C001–C005 assigned higher late-rate profiles; A001–A008 all represented | Morning/afternoon/evening scheduled times; proposed boundaries. | Caregivers/agencies with higher Late % and AvgMinutesLate are identifiable. | Tests coaching/scheduling analysis. |
| Query 4 positive | S001: 2025-04-05, 2025-05-10, 2025-06-20 = 3 missed; S002: 2025-07-01, 2025-07-29, 2025-08-15 = 3; S003: 2025-09-03, 2025-09-25, 2025-10-15 = 3; S004: 2026-01-05, 2026-02-01, 2026-03-01 = 3; S005: 2025-11-20, 2025-12-15, 2026-01-10 = 3 | Each listed set fits a rolling 90-day window. | Each senior returns in Query 4 with MissedCount >= 3. | Mandatory outreach test cases. |
| Query 4 negative | S006: one missed on 2025-04-10; S007: two missed on 2025-08-02 and 2025-10-20 | No rolling 90-day period reaches 3 missed visits. | S006 and S007 are excluded. | Confirms threshold logic. |
| Timestamp boundary tests | Example visits around +10 and +60 minutes | Check-in at +10 is not late; +10:01 is late; no check-in at cutoff becomes missed after evaluation; check-in after cutoff remains missed. | Boundary classifications follow strict rules. | Tests ETL edge cases. |

## Data Validation

| Validation check | Rule / formula logic | Expected result | Severity |
|---|---|---|---|
| Entity counts | Count seniors=150, caregivers=30, agencies=8, services=10, visits=3,000. | PASS when exact. | Error |
| Monthly coverage | 15 monthly buckets from 2025-01 through 2026-03; sum scheduled visits=3,000. | PASS when every month represented and total reconciles. | Error |
| Distribution reconciliation | 2250+360+240+150 = 3,000; 75%+12%+8%+5%=100%. | PASS when exact. | Error |
| Visit foreign keys | Every Visit.SeniorID, CaregiverID, ServiceID exists; caregiver AgencyID exists in agencies. | Zero orphan references. | Error |
| Completed timestamps | Completed visits have both check-in and check-out; cancelled visits have neither fabricated timestamp. | No invalid timestamp combinations. | Error |
| Late logic | LateMinutes NULL if no check-in; 0 if delay <=0; positive if delay >0. LateCheckInCount=1 iff delay >10. | All rows comply. | Error |
| Missed logic | Non-cancelled explicit Missed OR no check-in by +60 after cutoff; after-cutoff check-in does not clear; future/pending not missed. | All rows comply. | Error |
| Age band | Age computed at scheduled visit date and falls into 65-74, 75-84, or 85+. | All three bands represented. | Error |
| Query 4 positive cases | At least five seniors have >=3 missed visits within a rolling 90-day window. | S001–S005 qualify. | Error |
| Query 4 negative cases | At least some seniors have <3 missed visits in every rolling 90-day window. | S006–S007 excluded. | Error |
| Time of day | Every scheduled time maps to one proposed category; boundaries require approval. | No unmapped times after approval. | Review |
| Service category | ServiceCategory populated only from an approved mapping, never inferred automatically from Description. | No unapproved category inference. | Review |
