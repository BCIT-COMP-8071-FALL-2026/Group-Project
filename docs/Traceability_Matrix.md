# Traceability Matrix

*Requirements starting in 0 are OLAP user stories. OLTP User stories will start with a non-zero integer referring to the user story it originates from.*

| Requirement ID | Requirement Description | User Story | Non-Functional Requirements | ERD tables | Business Need/Justification | Deliverable | Acceptance Criteria | Test Cases | Owner | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| 0.1.0 | Create a way to see visit/schedule trends for the patients and caretakers, logging successes and failures. | OLAP-1 | Usability, Security, Extensibility, Scalability, Testability | FactVisit | Allows Agency to find shortcomings or things that work, allows visibility into trends. | Line chart | Line chart data can be edited to find certain trends, such as missed visits for specific caretakers or patients, and for which types of visits. | | | |
| 1.0.0 | Make a login page that has a secure login system that creates a private session for users to access their data in the application. | OLTP-1 | Security, Reliability, Usability | Person | A login page is the first step into the application and proves that the site is usable and secure. | Functional Login page | Login page allows users to login securely, acts as a landing page to navigate to password recovery and account registration | | | |
| 2.0.0 | Include an easy to follow form to fill out personal information while keeping private details secure. | OLTP-2 | Security, Usability | Person, Senior, Scalable | Creates an easy way to gather information on the patient. | Profile form/page | Form contains all necessary fields specified in the user story acceptance criteria and are stored securely and safely. | | | |
| 5.0.0 | Create a role hierarchy, Admin > Agency > Agency Manager > Caretakers > Seniors/Family Member, and provide permissions to view or edit certain pages and items. | OLTP-5 | Security, Scalable, Maintainable | ProfessionalRoles, Person | Keeping proper track of permissions and roles allows for ease of account creation and role assignments. | Documentation on roles and permissions permitted, and how access is defined. Implemented at a database level. | Roles follows required permission only, least amount of permissions possible. Documented and added somewhere in the GitHub. | | | |

Format: 
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

*AI Disclosure:* AI was used to format the table for markdown.
Status:
