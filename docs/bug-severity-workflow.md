# Bug Severity & Workflow

How we rate, track, and close bugs in GitHub Issues. File every bug with the **Bug report** template.

## Severity levels

| Severity | Definition | Example |
|---|---|---|
| `Critical` | App unusable, data lost, or sensitive data exposed. | Full SIN is shown on the senior profile page instead of the last 3 digits. |
| `High` | An important workflow is blocked with no workaround. | Saving a senior profile update fails with an error. |
| `Medium` | A feature is impacted, but there is a workaround. | Phone number with dashes is rejected; entering digits only works. |
| `Low` | Cosmetic or minor issue. | Misaligned label or typo on the profile form. |

If unsure, pick the higher level. Triage will adjust it. Critical and High bugs must be resolved before a demo build (see [OLTP Test Plan](OLTP_Test_Plan.md), Exit Criteria).

## Bug lifecycle

| Step | Who moves it | What they do |
|---|---|---|
| **New** | Reporter (anyone) | Opens an issue with the Bug report template. The `bug` label is added automatically. |
| **Assigned** | QA bug tracker (Sunwoo) | Checks the report, adds a `Severity:` label and `Dev-OLTP` or `Dev-OLAP`, and assigns a developer. |
| **Fixed** | Assigned developer | Opens a PR into `develop` that references the issue (`Refs #12`) and comments on the issue with the PR link. |
| **Needs-retest** | Assigned developer | After the PR is merged, adds `Needs-Retest` and `QA-OLTP` or `QA-OLAP`, and reassigns the issue to the reporter or QA. |
| **Closed** | QA tester | Retests on `develop`. **Pass:** removes `Needs-Retest` and closes the issue with a comment. **Fail:** removes `Needs-Retest`, comments what still fails, and reassigns to the developer (back to Assigned). |

> GitHub issues are only Open or Closed, so the step is shown by the assignee and labels.
