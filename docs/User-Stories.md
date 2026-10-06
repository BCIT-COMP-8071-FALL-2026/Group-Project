# User Stories

|Epic | User Story | Acceptance Criteria| 
|-------|---------|---------|
|User Accounts and Login| As a user I want to be able to login so that I can information that pertains to me. | <ul><li> Warning System <ul> <li>Attempts 1-2: error message only</li> <li>Attempt 3: warning</li> <li>Attempt 5: account locked until email reset </li> </ul> <li>Successful login will show the home page</li> <li>Unsuccessful login will show a warning</li></ul>|
||As a user I want to be able to easily set up a secure account so that I can keep of my personal information.|<ul><li>Password is encrypted and stored safely</li><li>Form contains boxes for:<ul><li>First Name (R) </li><li>Middle Name (O)</li><li>Last Name (R)</li><li>Email Address (R)</li><li>Phone number (R)</li><li>Address (R) <ul><li>Ensure Street, City, Country, Province, Postal Code are separate boxes</li><li>Add auto-fill eventually?</li></ul></ul><li>Insurance (O)<ul><li>Company</li><li>Account #</li></ul></li> <li>Password and email are stored and encrypted in the database</li><li>Selection option for Patient or Family </li> </ul>|
||As a user I want my sessions to be secure so that I can have safe of mind that my data is protected.|<ul><li>Cookie sessions implemented</li><li>Session expires after 30 minutes of inactivity</li><li>Logout and Login are secured</li><li>Patient pages should not be accessible without the correct accounts</li>|
|User Roles and Permissions|
