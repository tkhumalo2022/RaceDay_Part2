# RaceDay API Endpoint Plan

**Module:** PROG6212
**Part:** POE Part 1 - Section B
**Student:** Thabiso Khumalo (ST10477675)

RaceDay uses two application roles: **Organiser** and **Participant**. Authentication credentials are handled by the API authentication layer. Distance is stored on `Race_Category`, because one event may offer several race distances.

## Authentication and Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Register a new Organiser or Participant. | Public | `{ role, organiserName?, firstName?, lastName?, dateOfBirth?, gender?, email, password, phoneNumber }` | `201 Created`; `400 Bad Request`; `409 Conflict` |
| POST | `/api/auth/login` | Authenticate and return a JWT containing user id and role. | Public | `{ email, password }` | `200 OK`; `401 Unauthorized` |
| GET | `/api/users/me` | Return the logged-in user's profile. | Logged-in user | None | `200 OK`; `401 Unauthorized` |
| PUT | `/api/users/me` | Update the logged-in user's own profile. | Logged-in user | Editable profile fields | `200 OK`; `400 Bad Request` |

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events` | List events with optional filters. | Logged-in user | None | `200 OK` |
| GET | `/api/events/{id}` | Return one event. | Logged-in user | None | `200 OK`; `404 Not Found` |
| POST | `/api/events` | Create an event owned by the logged-in organiser. | Organiser | `{ eventName, description, eventDate, location, eventType, status?, closingDate }` | `201 Created`; `400 Bad Request` |
| PUT | `/api/events/{id}` | Update an event owned by the logged-in organiser. | Organiser | Editable event fields | `200 OK`; `403 Forbidden`; `404 Not Found` |
| DELETE | `/api/events/{id}` | Delete an event owned by the logged-in organiser. | Organiser | None | `204 No Content`; `403 Forbidden`; `404 Not Found` |

## Race Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events/{eventId}/categories` | List categories for an event. | Logged-in user | None | `200 OK`; `404 Not Found` |
| POST | `/api/events/{eventId}/categories` | Add a race category to an organiser's event. | Organiser | `{ categoryName, distanceKm, entryFee, maxParticipants }` | `201 Created`; `400 Bad Request`; `403 Forbidden` |
| PUT | `/api/categories/{id}` | Update a category belonging to the organiser's event. | Organiser | Editable category fields | `200 OK`; `403 Forbidden`; `404 Not Found` |
| DELETE | `/api/categories/{id}` | Delete a category belonging to the organiser's event. | Organiser | None | `204 No Content`; `403 Forbidden`; `404 Not Found` |

## Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/events/{eventId}/enrolments` | Enrol the logged-in participant into a category in the event. | Participant | `{ categoryId }` | `201 Created`; `400 Bad Request`; `409 Conflict` |
| GET | `/api/enrolments/me` | List the logged-in participant's enrolments. | Participant | None | `200 OK` |
| GET | `/api/events/{eventId}/enrolments` | List enrolments for an organiser-owned event. | Organiser | None | `200 OK`; `403 Forbidden`; `404 Not Found` |

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/enrolments/{enrolmentId}/results` | Capture the race result for an enrolment in the organiser's event. | Organiser | `{ startTime, finishTime, overallPosition, resultStatus }` | `201 Created`; `400 Bad Request`; `403 Forbidden`; `409 Conflict` |
| GET | `/api/results/me` | Return the logged-in participant's results. | Participant | None | `200 OK` |
| GET | `/api/events/{eventId}/results` | List results for an organiser-owned event. | Organiser | None | `200 OK`; `403 Forbidden`; `404 Not Found` |

## Payments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/enrolments/{enrolmentId}/payments` | Record a payment attempt for the participant's enrolment. | Participant | `{ amount, paymentMethod, transactionReference? }` | `201 Created`; `400 Bad Request`; `403 Forbidden`; `409 Conflict` |
| GET | `/api/enrolments/{enrolmentId}/payments` | View payment attempts for an enrolment the caller is authorised to access. | Participant / Organiser | None | `200 OK`; `403 Forbidden`; `404 Not Found` |

## Business Rules

- Organisers may only change Events, Race Categories, Enrolments and Results connected to events they own.
- Participants may only access their own enrolments, payments and results.
- An enrolment's `Category_ID` must belong to the same `Event_ID`.
- A participant cannot enrol twice in the same race category.
- Category capacity must respect `Max_Participants`.
- An enrolment may have multiple payment attempts, but only one successful `Paid` payment.
- Payment amount must match the selected category's `Entry_Fee`.
- Each enrolment may have at most one `Race_Result`.
- `Race_Number` must be unique.
