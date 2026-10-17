# RaCeDay payment-attempt workflow (development only)

This implementation records **Pending** attempts against existing race enrolments, in line with the PROG6212 Part 1 database design. It does **not** charge a customer, verify an EFT/card transaction, connect to any payment processor, or set Paid.

- Only the enrolment's Participant may record attempts. The Participant and owning Organiser may view the attempts. All others are forbidden.
- The server checks that Amount exactly equals the enrolled category's EntryFee. It also checks the enrolment is active and prevents recording after a successful Paid record exists.
- Only the server can select Status (Pending). Any client-supplied status is ignored.
- TransactionReference is optional but, when supplied, unique in the database.
- A filtered unique SQL Server index ensures a maximum of one Paid row per enrolment, even if a future trusted reconciliation service is introduced.
- The shared RaceWorkflow applies the same validation to MVC and REST endpoints; cookie-authenticated state changes require CSRF protection.

## Routes

- GET /api/enrolments/{enrolmentId}/payments (authenticated Participant or owning Organiser)
- POST /api/enrolments/{enrolmentId}/payments (owning Participant, creates Pending only)
- GET /Payments/Index?enrolmentId={id}
- GET/POST /Payments/Create?enrolmentId={id}

## Explicitly unimplemented

Payment gateway integration, confirmation webhook, handling of declined/refunded settlements, and reconciliation. No tests or UI must imply a Pending attempt represents real money received.

Official POE Part 2 rubric remains inaccessible, so this is aligned with the student's Part 1 plan, not a verified rubric requirement.
