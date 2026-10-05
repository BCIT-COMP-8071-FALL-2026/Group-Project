# OLTP Client App — Test Plan Outline

Draft for October 6.

This is for our ASP.NET Core MVC + Entity Framework client app, starting from Assignment 1b. I've used Jason's messages and the FigJam material we've got so far. I will update as features and acceptance criteria become more solid.

## 1. Scope

**Feature checks from FigJam and Assignment 1b**

Start with senior intake, viewing/updating profiles, emergency contacts/guardians, medication instructions. Check the intended result and that the information belongs to the right senior. Might make sense to keep coverage for the Assignment 1b features in case we decide to carry some forward: CRUD, service links, photos, date selection, and validation.

Visits/check-ins, vitals, appointments, messages, emergency flags, and billing are possible additions from the personas. We can discuss them more if/when they are selected for a sprint.

**Checks across the whole app**

- **Validation:** Missing, invalid, and boundary inputs are handled clearly. Check the server too, since browser validation can be bypassed.
- **Permissions:** Once roles are agreed, check access to other seniors' records, direct URLs, and logged-out access.
- **Data saving and relationships:** Changes survive reload/restart, links stay attached to the correct records, deactivating/deleting a record works as we want it to.
- **Errors and repeated actions:** Try an unavailable database, failed save, missing record, and double submission.
- **Usability and compatibility:** Check readable text, helpful messages, keyboard navigation, and the agreed browsers/mobile/tablet layouts. 

## 3. Tools

- **xUnit / NUnit:** Both are good options for C# tests: xUnit creates a fresh test class for each test, while NUnit has explicit setup/teardown for prepping and cleaning. xUnit seems to me to have everything we need.

- **Selenium / Playwright:**  Arsh's input

- **WebApplicationFactory:** Could use this with our C# test framework to check MVC requests, validation, permissions, and database saves together without opening a browser. It needs separate test configuration/data, and we'd still need UI tests for how pages actually behave. Might be useful, seems optional.
- **k6:** Used to check response times when several users send requests at once, could be useful once we've agreed on performance targets. It does add setup. Seems very optional to me.
- **GitHub Issues:** Track bugs, who's fixing them, and whether the fix has been checked, using Sunwoo's labels and report template.
- **Manual testing:** Readability, mobile/tablet layouts, unexpected inputs.

## 3. Test environments

Start locally, then check the shared QA build as the app comes together. Confirm the .NET version, database, and browser with DevOps. Use the same database engine as the app for database tests, with separate test data.

## 4. Test approach

Start by checking that the app opens, its main pages load, and a record can be saved and viewed. For each feature, test a normal use, invalid inputs, and relevant limits. Reload pages to check that changes really saved. Once roles are agreed, check that changing a URL or submitting a request directly cannot cause someone to break permissions. Rerun related tests after fixes and the main workflows before each sprint/class demo.

Automate repeatable checks for certain rules, database saves etc.

Record pass/fail results against the story, including the build tested, and link any bugs. After a fix, repeat the failing test and related checks.

## 5. Bug Workflow

Sunwoo is setting up the labels and template, but probably something like:

Reported → Triaged → In progress → Needs retest → Closed

Include build/commit, steps, expected result, actual result, and a screenshot / other evidence. 

## 6. Entry criteria

The feature runs, we know what it should do, and we have the data/accounts needed to try it.

## 7. Exit Criteria

The main workflows and required automated tests pass on the demo build. No Critical/High bugs are still unresolved. Keep results linked to the stories.

## 8. Who is doing what / things to confirm

Scott: test plan and checking coverage. 
Sunwoo: bug tracking. 
Arsh: UI tool comparison. 

We'll divide test cases later as they become more apparent.

Things to confirm with the analysts/PMs:

- Which persona features are actually in scope for each sprint?
- Who can view which senior's information, and which fields can each role change?
