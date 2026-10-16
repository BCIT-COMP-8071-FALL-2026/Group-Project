# Non-Functional Requirements


### Security

- **Authentication:** All routes except `/login` and `/register` must be protected by ASP.NET Core authentication middleware. Unauthenticated requests must receive an HTTP `401` response, 100% of protected routes enforced, with zero unguarded endpoints.
- **Authorization:** Role-based access control (e.g. Admin, Caregiver) must be enforced on all controller actions that are role-sensitive. Unauthorized requests must receive an HTTP `403`, verified by at least one authorization test per protected role boundary.
- **SQL Injection Prevention:** All database interactions must use EF Core's parameterized queries or `SqlCommand` with parameters. Zero raw string-interpolated SQL queries permitted in the codebase, should be enforced by something in code review and confirmed by a basic SQL injection test on all input-accepting endpoints.
- **Secrets Management:** Connection strings, API keys, and credentials must never be committed to source control. Store them in environment variables, `.env` files (gitignored), or `dotnet user-secrets` locally, verified by a `git grep` scan showing zero hardcoded secrets in the repo.
- **Transport Security:** HTTPS must be enforced in all non-development environments. HTTP requests must redirect to HTTPS via `UseHttpsRedirection()` middleware, confirmed by a browser check returning HTTP `301` on any plain HTTP request.


### Performance

Most API calls end-to-end should be less than 1s, unless there's a good reason for it. In the actual code, avoid using `<Task>.WaitAll`, `<Task>.Wait` or other *thread-blocking* actions, leverage C#'s `await <Task>`. For large queries, use pagination. Use **Dependency Injection** for heavy operations like a database connection.

- **Target:** Like 95% of OLTP API responses complete in < 1s and 95% of OLAP report queries complete in < 5s under normal load, measured via browser DevTools or Postman response times.


### Usability

Approach your design choices as the user, would someone like a doctor understand how and where to input a patient's information? For example, use terminology they would understand; don't just show a JSON object on a read query.

- **Target:** A first-time user (unfamiliar with the codebase) can complete a core workflow (e.g. register a senior, log a caregiver visit) in under a few minutes, validated during QA user walkthrough testing.


### Reliability

Simple is reliable, avoid tight coupling between components, validate inputs where necessary, null-check things you aren't sure about, don't make assumptions, and test thoroughly. Handle errors gracefully, but be mindful to not litter `try` and `except` blocks everywhere.

- **Target:** All known invalid inputs (empty required fields, out-of-range values, malformed data) return a meaningful error message rather than an unhandled exception, confirmed by QA covering at least 5 negative test cases per major form/endpoint.


### Privacy

Ensure cross-user data access is protected. For example, if I'm `userid=1` I shouldn't be able to query `userid=2`'s information.

- **Measurable target:** Zero cross-user data leaks, verified by at least one QA test per user-scoped endpoint that confirms a request authenticated as User A cannot retrieve or modify User B's records (expects HTTP `403` or filtered empty result).

