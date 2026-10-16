# Traceability Matrix

*Requirements starting in 0 are OLAP user stories. OLTP User stories will start with a non-zero integer referring to the user story it originates from.*

| Requirement ID | Requirement Description | User Story | Non-Functional Requirements | ERD tables | Business Need/Justification | Deliverable | Acceptance Criteria | Test Cases | Owner | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| 0.1.0 | Create a way to see visit/schedule trends for the patients and caretakers, logging successes and failures. | OLAP-1 | Usability, Security, Extensibility, Scalability, Testability | FactVisit | Allows Agency to find shortcomings or things that work, allows visibility into trends. | Line chart | Line chart shows monthly success and failure (Missed) visit counts across all agencies (Query 1). | | | |
| 1.0.0 | Make a login page that has a secure login system that creates a private session for users to access their data in the application. | OLTP-1 | Security, Reliability, Usability | UserAccount | A login page is the first step into the application and proves that the site is usable and secure. | Functional Login page | Login page allows users to login securely and links to account registration; 5 failed attempts lock the account for 5 minutes | | | |
| 2.0.0 | Include an easy to follow form to fill out personal information while keeping private details secure. | OLTP-2 | Security, Usability | Person, UserAccount, Senior | Creates an easy way to gather information on the patient. | Profile form/page | Form contains all necessary fields specified in the user story acceptance criteria and are stored securely and safely. | | | |
| 5.0.0 | Create a role hierarchy, Admin > Agency Manager > Caregiver > Senior/Family Member, and provide permissions to view or edit certain pages and items. | OLTP-5 | Security, Scalable, Maintainable | Role, UserAccountRole, UserAccount | Keeping proper track of permissions and roles allows for ease of account creation and role assignments. | Documentation on roles and permissions permitted, and how access is defined. Enforced in the app with role-based [Authorize] on pages. | Roles follows required permission only, least amount of permissions possible. Documented in docs/Roles-and-Permissions.md. | | | |

**Format:** 
Requirement ID:
Requirement Description:
User Story:
Non-Functional Requirements:
ERD tables:
Business Need/Justification:
Deliverable:
Acceptance Criteria:
Test Cases:
Owner:
Status:

**AI Disclosure:** AI was used to format the table for markdown.

