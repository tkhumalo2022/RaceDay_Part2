# Part 2 tutorial follow-up — 18 October 2026

This page records the changes made after checking the ARC Part 2 lesson list against my project. I opened the course lesson players and reviewed the source files, but I have not watched every minute of all 31 recordings. It would be wrong to say the project follows each video exactly.

## Changes made

**Lesson 22 — File storage**

The profile page now lets the signed-in user upload or remove a picture. The upload accepts JPG and PNG up to 2 MB, checks the file header rather than trusting the name alone, creates a new random filename and stores the path on the owner's Identity record. Files are kept out of Git and served from `wwwroot/profile-images`. This is intended for ordinary profile pictures, not confidential documents.

**Lessons 9–10 — Admin dashboard**

The Admin page now displays database totals for users, events, categories, enrolments, results and payment attempts. It does not expose participant account details or allow an admin to edit another organiser's race. The page is restricted to Admin. For a local demonstration, `--create-admin` can create a new Admin account when you supply your own credentials through environment variables. It will not promote an existing account or run outside Development.

**Lessons 20–25 — Identity and registration**

The existing `ApplicationUser`, account registration, login and role checks are retained. I did not copy the lecturer's `Pages.zip` over the working pages. Doing that without checking types and namespaces can cause conflicts with the current Identity setup. Account security pages not customised locally are provided by ASP.NET Core Identity UI where supported.

**Lessons 27–30 — Controllers, roles and demonstration**

The controllers, API endpoints and role checks remain. I added automated tests that check successful picture uploads, invalid image rejection, file removal, authentication/antiforgery protection and Admin dashboard access.

**README and video notes**

I shortened the project explanation and rewrote the presentation notes in straightforward language. The notes describe what can actually be shown, without claiming that unfinished work is complete.

## Confirmed by testing

- Release build with warnings treated as errors: **0 errors, 0 warnings**.
- Local test suite: **35 passed, 0 failed**. The tests create separate SQL Server LocalDB databases.
- The original `RaceDayDB` is unchanged.
- CI on the latest source commit should be checked once pushed.

## Still needs attention

**Lesson 5 and the official database rubric:** The original Part 1 design uses separate integer-key Organiser and Participant tables. The current Part 2 schema uses ASP.NET Core Identity and string foreign keys. This is a real structural difference and cannot be corrected by changing the README or column labels. A full, backed-up migration design must be reviewed before altering the working SQL Server schema. The original Part 1 database must stay intact.

**Video and submission:** Record the actual demonstration, add the unlisted YouTube link to README and submit the correct branch or merged repository after checking it. I haven't recorded or submitted either item.
