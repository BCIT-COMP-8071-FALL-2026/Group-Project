## QA tools

- **Unit tests — xUnit:** We will use xUnit for our unit tests in the OLTP app. It fits our .NET setup and has everything we need to check both functions and specific rules.

- **UI tests — pending discussion:** I acknowledge Tejinder specifically mentioned Selenium a few times, but looking into it a bit I agree with Arsh and think Playwright would be as good or better. Maybe worth asking Tejinder?

- **API tests — Postman:** We will use shared Postman collections to check API responses, validation and access permissions. This will help everyone rerun the same checks without having to rebuild requests individually.

- **OLAP checks:** We will compare query results against known test data. Postman will also be used where OLAP results are exposed through an API.
