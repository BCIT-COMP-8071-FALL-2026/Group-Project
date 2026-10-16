## OLAP Queries

| # | Who asks | Plain-English query | Measures (calculated from OLTP tables) | Dimensions (field in OLTP tables) | Decision it drives |
|---|---|---|---|---|---|
| 1 (Query for demoing) | Agency manager | How many visits were missed each month, and what percentage of that month's visits is that (cancelled and not yet due visits excluded)? | VisitCount, MissedCount, Missed % | Date (month, year) | Is service reliability improving or getting worse? |
| 2 | Agency manager | Of the seniors in Query 4, which live alone and in which age band? (shown as columns in the Query 4 table) | MissedCount | Senior (IsLivingAlone, AgeBand), Date (90 days before the anchor date) | Which senior groups need extra attention or more frequent visits? |
| 3 | Agency manager | Which caregivers are late, and by how many minutes on average? | AvgMinutesLate (over Late visits only) | Caregiver | Who needs coaching? |
| 4 | Agency manager | Which seniors had 3 or more missed visits in the 90 days before the anchor date? | MissedCount | Senior, Service, Date (90 days before the anchor date) | Outreach before an emergency happens (Selling point in Demo)|

## Definitions

| Term | Definition |
|---|---|
| Missed | No check-in within 60 minutes of scheduled start (60 minutes counts as within), once scheduled start + 60 minutes has passed, and not Cancelled |
| Not yet due | Visit whose scheduled start + 60 minutes has not passed; excluded from all counts |
| Anchor date | Latest scheduled start that has passed, excluding cancelled visits; 90-day windows include both ends |
| Late | Check-in more than 10 minutes after scheduled start, and not Missed |
| Completed | Check-in and check-out both recorded, and not Missed |
| Missed % | MissedCount / VisitCount (all visits except cancelled and not yet due) |
| Age band | Under 65, 65-74, 75-84, 85+, calculated at the anchor date |
| Time | All visit times are stored in UTC; cutoffs compare exact timestamps, including fractions of a second |

## Fields the OLTP team must capture

| Table | Fields |
|---|---|
| Visit | ScheduledStart, ActualCheckIn, ActualCheckOut, Status |
| Senior | DateOfBirth, IsLivingAlone |
| Caregiver | Link to Agency |
