# OLTP QA Test Cases

Covers OLTP-1 (login), OLTP-2 (account setup), and OLTP-5 (permissions).

These are initial test cases that are based on the acceptance criteria and user stories. The features are currently unavailable for testing so this will update as they become available.

| Test ID | Story | Test steps | Expected result | Status |
| --- | --- | --- | --- | --- |
| QA-01 | OLTP-1 | Log in with valid credentials and an unlocked account. | Login succeeds and the home page appears. | Not run |
| QA-02 | OLTP-1 | Enter an incorrect password for the first time. | Error message appears. Login fails and the user stays logged out. | Not run |
| QA-03 | OLTP-1 | Enter an incorrect password a second time for the same account. | Error message appears again. Login fails and the user stays logged out. | Not run |
| QA-04 | OLTP-1 | Enter an incorrect password a third time for the same account. | A warning appears on attempt 3. The user remains logged out. | Not run |
| QA-05 | OLTP-1 | Continue entering an incorrect password until attempt 5, then try the correct password. | The account locks on attempt 5. The correct password cannot log in until the email-reset process is completed. | Not run |
| QA-06 | OLTP-2 | Fill in valid account details, then leave each required field blank separately and submit. Required fields: First Name, Last Name, Email, Phone and Address. | Each missing required field prevents submission, shows a validation message and does not create an account. | Not run |
| QA-07 | OLTP-2 | Create a test account and check password storage with a developer. | The password is stored as a secure hash, not plaintext. | Not run |
| QA-08 | OLTP-5 | Log in as each role and try the actions that role is allowed to use. | Each permitted page/action is accessible and works. | Not run |
| QA-09 | OLTP-5 | Log in as each role and try restricted actions. | Access is denied and actions cannot be performed. | Not run |
