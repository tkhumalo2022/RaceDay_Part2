# RaceDay Part 3 login — Cloudflare Pages peer review

This is a static *review page*, styled from the coursework's real `src/RaCeDay.Portal/Views/Account/Login.cshtml`. No passwords are accepted; all inputs and the submit control are disabled.

For actual login, the ASP.NET Core portal and REST API must be hosted with secure sessions, CSRF, confirmed-account policy, private persistent SQL Server, EF Core migrations and secure Azure Blob storage. **Cloudflare Pages alone does not run the ASP.NET Core application**. Do not represent this as a functioning authentication site or university assessment evidence.

Production marketing SEO is blocked via `robots.txt`, `noindex`, and `_headers`. No scripts or assets are loaded from third parties. Links point only to the real coursework code and verified read-only walkthrough. A separate `review/sam-cloudflare-login-20261109` Git branch preserves this work without merging to coursework.

Deploy prebuilt assets: `npx -y wrangler pages deploy cloudflare-login --project-name=raceday-part3-login-review --branch=main` from the review worktree.

QA: both tab panels work with keyboard arrows/Home/End and clicking; buttons/inputs are disabled, viewport has no overflow, no JS errors, 200 at / and /Account/Login.