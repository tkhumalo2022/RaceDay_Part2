# RaCeDay Part 2 — What to say in my video

**Student:** Thabiso Khumalo
**Module:** PROG6212 Programming 2B
**Target length:** 8–10 minutes

Use the project as you speak. Don't read the file names out one by one. Show the work on screen and explain why each part matters.

## 1. Start with the homepage

"Hi, I'm Thabiso Khumalo, and this is my PROG6212 Part 2 project, RaCeDay.

It's a race management system. Organisers can add races and categories, while participants can register, enter events and view their results.

In this part, I'm showing the API, the SQL Server database, how login and roles work, and how I tested the system."

## 2. Open the code in Visual Studio / VS Code

"I built this using C# and ASP.NET Core. Program.cs sets up SQL Server, Identity, sessions and Swagger. My controllers handle the requests, and RaceWorkflow.cs handles things like checking who owns an event, whether a category is full, and whether registration is still open."

Show `Program.cs`, `RaceApiController.cs`, `RaceWorkflow.cs` and `ApplicationDbContext.cs`.

## 3. Show SQL Server Management Studio

"I'm using SQL Server 2022 with Entity Framework Core. I created a separate Part 2 database because I didn't want to overwrite the SQL work I did for Part 1.

The migrations create the Identity tables and the race tables. There are also two database triggers: one keeps the category's participant count up to date, and the other checks that a payment amount matches the entry fee."

Open the saved read-only SQL query and press F5. Show six migrations, the three roles and both triggers.

**Be clear about the database change:** "For Part 2, I used ASP.NET Core Identity to manage user accounts. This means the new database does not have exactly the same organiser and participant relationships as my Part 1 ERD. I've recorded those differences, and I'm checking whether the lecturer accepts that change."

## 4. Register and sign in

"Now I'll register an account. When someone signs up, they choose Organiser or Participant. The system checks the information and stores a hashed password. You can't choose Admin when registering.

After login, the application remembers the user's ID and role in a session. The role checks stop a participant from using organiser-only functions."

Show the registration page and the role-specific dashboard. In Swagger, show the auth endpoints, `GET /api/auth/session` and an unauthorised request. If using Swagger writes, obtain the antiforgery token first.

## 5. Organiser creates a race

"An organiser can create a race, set the location and dates, and add categories with different distances, fees and limits.

I check these details before saving them. An organiser also can't edit an event belonging to someone else."

Create a **future** event and a 5 km category.

## 6. Participant joins the race

"I'll switch to a Participant account. Here I can browse events and enter one of the categories.

The system checks the closing date, available spaces and duplicate registrations. The organiser can then see who's entered."

Show an entry and try a duplicate request so the lecturer can see how errors are handled.

## 7. Results and profile

"When the organiser closes the race, they can record results. Participants can only view their own results.

I've also added profile editing. The user can change their details and upload a profile picture. Pictures are limited to JPG or PNG files under 2 MB."

Show the result screens, profile editor and upload if you have a suitable photo.

## 8. Tests, Swagger and GitHub

"Swagger gives me a place to see and test my API endpoints. I also wrote automated tests for registration, permissions, SQL Server rules, payments and race entries.

I'll run the tests now. The Release build is checked for warnings and errors, and I can see the test results.

GitHub Actions runs the same build and tests after I push changes. Here's the successful run in my repository."

Show the real output of `dotnet test RaCeDay.slnx -c Release` and the green workflow. Only state the number of passing tests shown by the most recent run; older screenshots may have fewer tests.

## 9. Finish

"That's my Part 2 demonstration. I've covered the API, the database, registration and roles, race entries, results and testing. Thanks for watching."

## Before I submit

- Check the database mapping requirement with the lecturer; the original Part 1 `RaceDayDB` is not modified.
- Make sure the latest GitHub Actions run is green and the correct Part 2 branch is accessible.
- Record the real demonstration, upload it to YouTube as **Unlisted**, add the URL to README and submit through ARC.
- Don't claim payments are processed. The application saves pending payment attempts only.
- Don't claim the original Visual Studio project was recovered or the schema is identical to Part 1.
