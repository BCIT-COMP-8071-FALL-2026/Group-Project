## OLAP Queries

| # | Who asks | Plain-English query | Measures (calculated from OLTP tables) | Dimensions (field in OLTP tables) | Decision it drives |
|---|---|---|---|---|---|
| 1 (Query for demoing) | Agency manager | How many scheduled visits were missed each month, and what percentage of all scheduled visits is that? | VisitCount, MissedCount, Missed % | Date (month, year) | Is service reliability improving or getting worse? |
| 2 | Agency manager, healthcare professional | Do seniors who live alone have a higher missed-visit rate than those who don't, and how does the rate differ by age band? | VisitCount, MissedCount, Missed % | Senior (IsLivingAlone, AgeBand), Date | Which senior groups need extra attention or more frequent visits? |
| 3 | Agency manager | Which caregivers or agencies, and which times of day (morning, afternoon, evening), have the highest late-visit rate and average minutes late? | VisitCount, LateCount, Late %, AvgMinutesLate | Caregiver, Agency, Time of day, Date | Who needs coaching, and should schedules or agency contracts change? |
| 4 | Agency manager, family member | Which seniors had 3 or more missed visits in the last 90 days? | MissedCount | Senior, Date (last 90 days) | Outreach before an emergency happens (Selling point in Demo)|

## Definitions

| Term | Definition |
|---|---|
| Missed | Status = Missed, or no check-in within 60 minutes of scheduled start |
| Late | Check-in more than 10 minutes after scheduled start |
| Completed | Check-in and check-out both recorded |
| Missed % | MissedCount / scheduled VisitCount (cancelled visits excluded) |
| Age band | 65-74, 75-84, 85+, calculated at the visit date |

## Fields the OLTP team must capture

| Table | Fields |
|---|---|
| Visit | ScheduledStart, ActualCheckIn, ActualCheckOut, Status |
| Senior | DateOfBirth, IsLivingAlone |
| Caregiver | Link to Agency |
