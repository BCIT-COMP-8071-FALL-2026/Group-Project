# Query 1 contract: missed visits by month

Version 1.2, 2026-10-16. Owners: Jason and Shayan. Story: OLAP-1. Query: Query 1 in [OLAP_Queries.md](OLAP_Queries.md).

Amal's procedure, Cadan's endpoint and Leon's Postman tests all build against this file. Any change to it needs a PR and a post in #dev.

## Procedure (Amal)

| Item | Value |
|---|---|
| Name | `dbo.usp_MissedVisitsByMonth` |
| Database | The OLAP database named in the API's connection string |
| Parameters | None. Covers all agencies and all months. |
| Source | OLAP star schema. Internals are up to Amal, except that Missed and not yet due are computed in the procedure (see Evaluation time), not read from flags set by the ETL. |
| Sort | `Month` ascending |

| Column | SQL type | Rule |
|---|---|---|
| `Month` | `char(7)` | Month of ScheduledStart (UTC), formatted `YYYY-MM` |
| `VisitCount` | `int` | Visits in the month, excluding cancelled and not yet due |
| `MissedCount` | `int` | Visits counted in `VisitCount` that are Missed |
| `MissedPercent` | `decimal(5,1)` | `CAST(ROUND(MissedCount * 100.0 / VisitCount, 1) AS decimal(5,1))` |

## Endpoint (Cadan)

| Item | Value |
|---|---|
| Method and path | `GET /api/reports/missed-visits-by-month` |
| Base URL | Set in config. Postman uses a `{{baseUrl}}` variable. |
| Request | No parameters, no body. Query string parameters are ignored and still return `200`. |
| Other methods | `405 Method Not Allowed` |
| Success | `200 OK`, `Content-Type: application/json`, a JSON array of rows sorted by `month` ascending |
| No data | `200 OK` with `[]` |
| Database unavailable or query fails | `500`. The response body is not part of the contract. |

| JSON field | Type | From column |
|---|---|---|
| `month` | string, `YYYY-MM` | `Month` |
| `visitCount` | integer | `VisitCount` |
| `missedCount` | integer | `MissedCount` |
| `missedPercent` | number, rounded to 1 decimal place. A whole value may appear as `25` or `25.0`, so compare as numbers. | `MissedPercent` |

Field names are camelCase (the ASP.NET Core default). The API returns no other fields. The chart works out Success as `visitCount - missedCount` (OLAP-1). The table shows `missedPercent` with 1 decimal place and a % sign, for example `33.3%`.

## Rules

These follow the Definitions table in [OLAP_Queries.md](OLAP_Queries.md).

| Term | Rule |
|---|---|
| Missed | No ActualCheckIn at or before ScheduledStart + 60 minutes, once that cutoff has passed, and not Cancelled. A check-in before ScheduledStart counts as within. A check-in after the cutoff does not clear it. |
| Missed source | Computed from ScheduledStart and ActualCheckIn. Status is used only to exclude Cancelled. Status is never Missed, and Completed does not prevent Missed. |
| Cancelled | Status = Cancelled. Excluded from both counts. |
| Not yet due | ScheduledStart + 60 minutes is later than the evaluation time. Excluded from both counts. |
| Evaluation time | Cutoffs are judged when the procedure runs, using `SYSUTCDATETIME()`. The star schema must keep ScheduledStart and ActualCheckIn per visit so the procedure can do this. |
| Months with no counted visits | Left out of the result |
| Time | All times are UTC. Cutoffs compare exact timestamps, including fractions of a second: a check-in at 10:00:00.4 is after a 10:00:00 cutoff. |

## Worked example

Sample visits, with the query run on 2026-10-16:

| VisitID | ScheduledStart (UTC) | ActualCheckIn (UTC) | Status | Counted? | Missed? | Why |
|---|---|---|---|---|---|---|
| 1 | 2025-01-06 09:00:00 | 2025-01-06 09:05:00 | Completed | Yes | No | Checked in 5 minutes after start |
| 2 | 2025-01-10 14:00:00 | 2025-01-10 15:00:00 | Completed | Yes | No | Exactly 60 minutes counts as within |
| 3 | 2025-01-15 10:00:00 | none | Scheduled | Yes | Yes | No check-in |
| 4 | 2025-01-20 11:00:00 | none | Cancelled | No | No | Cancelled |
| 5 | 2025-01-25 10:00:00 | 2025-01-25 09:50:00 | Completed | Yes | No | Checked in 10 minutes before start |
| 6 | 2025-02-03 09:00:00 | 2025-02-03 10:00:01 | Completed | Yes | Yes | Checked in 1 second after the cutoff. Status Completed does not matter. |
| 7 | 2025-02-12 13:00:00 | 2025-02-12 13:02:00 | Completed | Yes | No | Checked in 2 minutes after start |
| 8 | 2025-02-20 08:00:00 | none | Scheduled | Yes | Yes | No check-in |
| 9 | 2099-01-05 09:00:00 | none | Scheduled | No | No | Not yet due, so 2099-01 is left out |

Procedure output:

| Month | VisitCount | MissedCount | MissedPercent |
|---|---|---|---|
| 2025-01 | 4 | 1 | 25.0 |
| 2025-02 | 3 | 2 | 66.7 |

Endpoint response:

```json
[
  { "month": "2025-01", "visitCount": 4, "missedCount": 1, "missedPercent": 25.0 },
  { "month": "2025-02", "visitCount": 3, "missedCount": 2, "missedPercent": 66.7 }
]
```

## Testing (Leon)

The worked example is not seeded, so Postman checks the rules below against whatever the database holds. Amrit's reconciliation scripts check the values against the raw visits.

| Check | Expected |
|---|---|
| Status | `200` |
| Keys | Each row has exactly `month`, `visitCount`, `missedCount`, `missedPercent` |
| `month` | Matches `^\d{4}-\d{2}$`, unique, ascending |
| `visitCount` | Integer, greater than 0 |
| `missedCount` | Integer, from 0 to `visitCount` |
| `missedPercent` | Equals `missedCount * 100 / visitCount` rounded to 1 decimal place, within 0.05 |
| Query string added | Still `200`, same body |
| `POST` | `405` |
| Database stopped | `500` |
| Empty database | `200` with `[]` |
