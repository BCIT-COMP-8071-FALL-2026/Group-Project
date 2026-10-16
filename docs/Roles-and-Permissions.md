# Roles and Permissions

Defines what each role can see and do in the OLTP app (OLTP-5). Built from the user stories in [User-Stories.md](User-Stories.md).

## Rules
- Anything not listed below is denied by default (least privilege)
- Pages require login, except Login and Register; logged-out users are redirected to Login
- A Senior sees only their own data
- A Family Member sees only seniors linked to them by Admin (OLTP-11)
- A Caregiver sees only visits assigned to them
- An Agency Manager sees only visits of their own agency (OLTP-18)
- A Staff account has no role and only sees the home page until Admin assigns a role (OLTP-2, OLTP-4)
- The Admin account is created by the seed script

## Permissions

R = read, W = write, blank = denied

| Page or action | Story | Senior | Family Member | Caregiver | Agency Manager | Admin |
|---|---|---|---|---|---|---|
| Register an account | OLTP-2 | Open to anyone (not logged in) | | | | |
| Login, logout | OLTP-1, OLTP-3 | W | W | W | W | W |
| Create (OLTP-6) and view (OLTP-7) own profile | OLTP-6, OLTP-7 | R/W | | | | |
| Edit own profile, except names, birthday and email | OLTP-8 | W | | | | |
| Any senior's profile, including names, birthday and login email | OLTP-8 | | | | | R/W |
| Deactivate a senior | OLTP-9 | | | | | W |
| Linked senior's read-only tab (profile, medical conditions, medication, caregiver notes) | OLTP-11 | | R | | | |
| Admin page: user list, role + agency, family link | OLTP-4, OLTP-11, OLTP-18 | | | | | W |
| Schedule a visit | OLTP-16 | | | | | W |
| Own visit list (senior name, address, visit details) | OLTP-12 | | | R | | |
| Check in, check out | OLTP-14 | | | W | | |
| Notes for the senior on the visit page | OLTP-12, OLTP-15 | | | R/W | | |
| Own agency's visits page | OLTP-18 | | | | R | |

## OLAP app
The OLAP app (OLAP-1 to OLAP-4) is used by the Agency Manager. It has no login, so access is not enforced by role, and it shows all agencies.
