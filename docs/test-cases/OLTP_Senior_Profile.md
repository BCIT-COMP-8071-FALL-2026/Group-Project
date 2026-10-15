# Test Cases: Senior Profile (OLTP-6, 7, 8)

Manual test cases for creating, viewing, and updating a senior profile. Log failures with the **Bug report** template (see [Bug Severity & Workflow](bug-severity-workflow.md)).

**Build tested:** ____  **Tester:** ____  **Date:** ____

## Setup

- Logged in as a **Senior** account. For OLTP-6, the account has no profile yet.
- Valid test data: SIN `123456789`, DOB `1950-01-01`, Living alone `Yes`.

**Assumptions** (to confirm with analysts): SIN must be exactly 9 digits. DOB cannot be in the future. Test IDs follow the traceability matrix (`TC-6.01` → requirement `6.0.0`).

## OLTP-6: Create profile

| ID | Test | Steps | Expected | Result |
|---|---|---|---|---|
| TC-6.01 | Create with valid data | Fill all required fields with valid data, submit. | Profile saved, success message shown. | |
| TC-6.02 | Required fields missing | Leave each required field blank, one at a time: SIN, DOB, Living alone, First name, Last name, Street, City, Province, Country, Postal code, Phone. Submit. | Error shown on that field. Profile not saved. | |
| TC-6.03 | Optional fields blank | Leave Middle name, Doctor, Insurance, Medical conditions blank. Submit. | Profile saves. | |
| TC-6.04 | Invalid SIN | Submit SIN as `12345678` (8 digits), `1234567890` (10), `12345678a`. | Each rejected with an error. | |
| TC-6.05 | Future DOB | Submit DOB as tomorrow's date. | Rejected with an error. | |
| TC-6.06 | Max length | First name with 100 characters, then 101. | 100 saves, 101 is rejected. | |
| TC-6.07 | Server-side validation | Remove a required field's value or `required` attribute in dev tools, submit. | Server rejects it. Profile not saved. | |
| TC-6.08 | SIN stored encrypted | After TC-6.01, check the `Senior` table. | `SINEncrypted` is not readable plain text. `SINLast3` = `789`. | |
| TC-6.09 | Double submit | Click Submit twice quickly. | Only one profile is created. | |

## OLTP-7: View profile

| ID | Test | Steps | Expected | Result |
|---|---|---|---|---|
| TC-7.01 | All data shown | Open the profile page after TC-6.01. | All entered fields shown and match what was entered. | |
| TC-7.02 | SIN masked | Look at the SIN on the profile page. | Only last 3 digits shown (e.g. `***-***-789`). | |
| TC-7.03 | SIN not in page source | View page source / dev tools on the profile page. | Full SIN appears nowhere. | |
| TC-7.04 | DOB and Living alone persist | Log out, log back in, open the profile. | DOB is `1950-01-01` (not shifted by a day). Living alone is `Yes`. | |
| TC-7.05 | Not logged in | Log out, open the profile URL directly. | Redirected to login. | |
| TC-7.06 | Other senior's profile | Logged in as Senior A, change the ID in the URL to Senior B's. | Access denied. Senior B's data not shown. | |

## OLTP-8: Update profile

| ID | Test | Steps | Expected | Result |
|---|---|---|---|---|
| TC-8.01 | Greyed-out fields | Open the edit page. | First, Middle, Last name, SIN, and DOB are greyed out and cannot be edited. SIN shows last 3 digits only. | |
| TC-8.02 | Greyed-out fields can't be forced | In dev tools, enable the DOB and Last name fields, change them, submit. Reload. | Changes are rejected or ignored. Original values remain. | |
| TC-8.03 | Edit allowed fields | Change Email, Phone, Address, Doctor. Save, reload. | New values shown. | |
| TC-8.04 | Living alone update persists | Change Living alone `Yes` → `No`. Save, log out and back in. Repeat `No` → `Yes`. | New value shown each time. | |
| TC-8.05 | DOB unchanged by other edits | Change Phone only. Save, reload. | DOB is still `1950-01-01`. | |
| TC-8.06 | Required editable field cleared | Clear Phone, save. | Error shown. Old value kept. | |
| TC-8.07 | Customer support note | Open the edit page. | Page tells the user to contact customer support to fix greyed-out fields. | |
