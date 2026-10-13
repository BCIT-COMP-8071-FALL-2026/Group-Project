# OLTP & OLAP — Test Plan Outline

October 6th Draft — updated October 12th

## 1. Scope

## Acceptance Testing

Verify functional requirements of the app as expressed by users. Ensure the application provides all functionality and features that are in the requirements. UI automation will be used frequently here.

OLAP checks will compare query results against known test data, starting with Query 1's monthly missed-visit counts and percentages.

## Unit Testing

Achieve an agreed upon amount of coverage throughout the app, making sure that all necessary functions and methods pass all required tests.

## Wishlist for checks across the whole app

Targets and features below still need to be agreed where they aren't already in the requirements.

- **Reliability:** 99% error-free operation by users. Requires hundreds of repeating tests, will involve lots of Black Box testing, and will take advantage of UI automation.
- **Security:** Ensure working authorization, authentication (role-based), SSL, MFA. SCA is a powerful security tool.
- **Performance:** The resources are managed efficiently in the app, and the latency meets accepted requirements. Not as important as other types of testing for our webapp but Visual Studio's memory analyzer and performance profiler are valuable tools worth exploring.
- **Scalability:** The app does not fail under large loads or data volumes. Jmeter is a worthwhile method of load testing.
- **Usability:** Invent user requirements for usability and meet them. Ensure the website is usable for the users by performing manual testing alongside automated UI tests. Static Code Analysis can also be used.
- **Validation:** Missing, invalid, and boundary inputs are handled clearly. Check the server too, since browser validation can be bypassed.
- **Data saving and relationships:** Changes survive reload/restart, links stay attached to the correct records, deactivating/deleting a record works as we want it to.
- **Errors and repeated actions:** Try an unavailable database, failed save, missing record, and double submission.

## 2. Tools

- **Unit tests — xUnit:** We will use xUnit for our unit tests in the OLTP app because it fits our C#/.NET setup and has everything we need to check individual methods and rules.
- **UI tests — pending discussion:** Tejinder specifically mentioned Selenium a few times, but I agree with Arsh that Playwright should cover our UI checks well, including login, forms and permissions. Worth asking Tejinder whether Selenium is required or was just an example.
- **API tests — Postman:** We will use shared Postman collections to check API responses, validation and access permissions so everyone can rerun the same checks without rebuilding the requests.
- **OLAP checks:** We will compare query results against known test data so we can check the counts and percentages against answers we calculated ourselves. Postman will also be used where OLAP results are exposed through an API.
- **Jmeter:** Useful for performance and load testing.
- **Static Code Analysis:** Can be helpful for usability testing and sometimes security testing. Modern LINT tooling provides a lot of value.
- **Visual Studio's Performance Profiler:** Uses performance and memory analysis to benchmark and check our app's performance.
- **WebApplicationFactory:** Could use this with our C# test framework to check MVC requests, validation, permissions, and database saves together without opening a browser. It needs separate test configuration/data, and we'd still need UI tests for how pages actually behave. Might be useful, seems optional.
- **k6:** Used to check response times when several users send requests at once, could be useful once we've agreed on performance targets. It does add setup.
- **GitHub Issues:** Track bugs, who's fixing them, and whether the fix has been checked, using Sunwoo's labels and report template.
- **Manual testing:** Readability, mobile/tablet layouts, unexpected inputs.

## 3. Test environment

Start locally, then check the shared QA build as the app comes together, including the Docker environment. Confirm the .NET version, database, and browser with DevOps. Use the same database engine as the app for database tests, with separate test data.

Confirm how to start the Docker services, access the app, reset test data and load the OLAP data.

## 4. Test approach

Start by checking that the app opens, its main pages load, and a record can be saved and viewed. For each feature, test a normal use, invalid inputs, and relevant limits. Reload pages to check that changes really saved. Once roles are agreed, check that changing a URL or submitting a request directly cannot cause someone to break permissions. Rerun related tests after fixes and the main workflows before each sprint/class demo.

Check that the expected OLTP records reach the OLAP side after the data load, and that running the load again does not duplicate the records or change the totals unexpectedly.

For OLAP, load known test data and compare query results against the expected counts and percentages. Check that cancelled visits are handled according to the query definitions.

Include load testing once the app is running and we've agreed on the workload and acceptable response times.

Automate repeatable checks for certain rules, database saves etc.

Record pass/fail results against the story or query, including the build tested, and link any bugs. Mark tests as Not run or Blocked where the feature or required information isn't ready. After a fix, repeat the failing test and related checks.

## 5. Bug Workflow

[Use the established bug reporting template.](https://github.com/BCIT-COMP-8071-FALL-2026/Group-Project/blob/develop/.github/ISSUE_TEMPLATE/bug_report.md)

Include build/commit, steps, expected result, actual result, and a screenshot / other evidence.

Follow Sunwoo's severity guide and workflow: New, Assigned, Fixed, Needs-retest, Closed. Retest the fix before closing the bug.

## 6. Entry criteria

The feature runs, we know what it should do, and we have the data/accounts needed to try it. For Docker testing, the required services are running and reachable. For OLAP checks, the test data is loaded and we know the expected results.

## 7. Exit Criteria

The main workflows, agreed OLAP checks and required automated tests pass on the demo build in the Docker environment. No Critical/High bugs are still unresolved. Keep results linked to the stories or queries, and record any tests that remain blocked or not run.

## 8. Things to confirm

Things to confirm with the analysts/PMs:

- Roles vs Permissions
- Does Tejinder require Selenium, or is Playwright acceptable?
- Can the password requirement maybe say "hashed" instead of "encrypted"?
