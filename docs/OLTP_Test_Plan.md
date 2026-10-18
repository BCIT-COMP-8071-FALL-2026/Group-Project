# OLTP Client App — Test Plan Outline

October 6th Draft

## 1. Scope

**Acceptance Testing**

Verify functional requirements of the app as expressed by users. Ensure the application provides all functionality and features that are in the requirements. Selenium will be used frequently here.

**Unit Testing** 

Achieve an agreed upon amount of coverage throughout the app, making sure that all necessary functions and methods pass all required tests. 

**Wishlist for checks across the whole app**

- **Reliability:** 99% error-free operation by users. Requires hundreds of repeating tests, will involve lots of Black Box testing, and will take advantage of Selenium.
- **Security:** Ensure working authorization, authentication (role-based), SSL, MFA. SCA is a powerful security tool.
- **Performance:** The resources are managed efficiently in the app, and the latency meets accepted requirements. Not as important as other types of testing for our webapp but Visual Studio's memory analyzer and performance profiler are valuable tools worth exploring.
- **Scalability:** The app does not fail under large loads or data volumes. Jmeter is a worthwhile method of load testing.
- **Usability:** Invent user requirements for usability and meet them. Ensure the website is usable for the users by performing manual testing alongside Selenium tests. Static Code Analysis can also be used.
- **Validation:** Missing, invalid, and boundary inputs are handled clearly. Check the server too, since browser validation can be bypassed.
- **Data saving and relationships:** Changes survive reload/restart, links stay attached to the correct records, deactivating/deleting a record works as we want it to.
- **Errors and repeated actions:** Try an unavailable database, failed save, missing record, and double submission.

## 2. Tools

- **xUnit / NUnit:** Both are good options for C# tests: xUnit creates a fresh test class for each test, while NUnit has explicit setup/teardown for prepping and cleaning. xUnit seems to me to have everything we need.
- **Selenium / Playwright:**  Arsh's input (although Tejinder did mention using Selenium for various parts of testing).
- **Jmeter:** Useful for performance and load testing.
- **Static Code Analysis:** Can be helpful for usability testing and sometimes security testing. Modern LINT tooling provides a lot of value.
- **Visual Studio's Performance Profiler**: Uses performance and memory analysis to benchmark and check our app's performance.
- **WebApplicationFactory:** Could use this with our C# test framework to check MVC requests, validation, permissions, and database saves together without opening a browser. It needs separate test configuration/data, and we'd still need UI tests for how pages actually behave. Might be useful, seems optional.
- **k6:** Used to check response times when several users send requests at once, could be useful once we've agreed on performance targets. It does add setup.
- **GitHub Issues:** Track bugs, who's fixing them, and whether the fix has been checked, using Sunwoo's labels and report template.
- **Manual testing:** Readability, mobile/tablet layouts, unexpected inputs.


## 3. Test environment
Start locally, then check the shared QA build as the app comes together. Confirm the .NET version, database, and browser with DevOps. Use the same database engine as the app for database tests, with separate test data.

## 4. Test approach

Start by checking that the app opens, its main pages load, and a record can be saved and viewed. For each feature, test a normal use, invalid inputs, and relevant limits. Reload pages to check that changes really saved. Once roles are agreed, check that changing a URL or submitting a request directly cannot cause someone to break permissions. Rerun related tests after fixes and the main workflows before each sprint/class demo.

Automate repeatable checks for certain rules, database saves etc.

Record pass/fail results against the story, including the build tested, and link any bugs. After a fix, repeat the failing test and related checks.

## 5. Bug Workflow

[Use the established bug reporting template.](https://github.com/BCIT-COMP-8071-FALL-2026/Group-Project/blob/develop/.github/ISSUE_TEMPLATE/bug_report.md)

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
