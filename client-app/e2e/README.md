# End-to-end tests (Playwright)

## Running

Both servers must be up first — Playwright starts the frontend if needed, but
**never** starts the API:

```bash
# terminal 1 - API on :5001   (from the solution root)
dotnet run --project API

# terminal 2 - tests           (from client-app)
yarn test:e2e                     # all tests, headless
yarn test:e2e --headed            # watch it happen
yarn test:e2e:ui                  # interactive runner - best for debugging
yarn test:e2e e2e/crm             # just the CRM specs
npx playwright show-report        # open the last HTML report
```

## How authentication works

`e2e/auth.setup.ts` runs first, logs in once through the login form, and saves
the session to `e2e/.auth/admin.json` (gitignored). Every other spec starts
already authenticated.

If the whole suite fails, **look at the `setup` project first** — everything
downstream depends on it.

Overrides:

```bash
E2E_EMAIL=someone@example.com E2E_PASSWORD=... yarn test:e2e
E2E_LANG=ar yarn test:e2e          # exercise the Arabic UI
```

## Known constraints

**Test data is shared.** These run against your dev database. The assignment
spec really does assign a lead, and the validation spec really does create one
(named `E2E Test Lead <timestamp>`). There is no seeding or teardown yet, so
runs accumulate data. Clean up with:

```sql
DELETE FROM PARTY WHERE DESCRIPTION LIKE 'E2E Test Lead %';
```

**Selectors match visible text.** The CRM components have no `data-testid`
attributes, so these tests match on roles and wording, and are therefore
sensitive to copy changes and to the UI language. Adding `data-testid` to the
CRM components is the single highest-value improvement to this suite — do it
before the suite grows.

**Language is pinned to English** by the setup, because the assertions are
written in English. `E2E_LANG=ar` switches the app but the assertions will then
fail; Arabic coverage needs its own expectations.

## What is covered

| Spec | Mirrors | Notes |
|---|---|---|
| `login.spec.ts` | — | Unauthenticated smoke test of the login page |
| `crm/leads-assignment.spec.ts` | Suite B | Assign / reassign / history / bulk toolbar. Includes a regression guard for the rep-dropdown bug of 25 Aug |
| `crm/lead-validation.spec.ts` | Suite I (11.1) | Required vs optional fields after the upload-file alignment |

Suites C–H from `CRM-Lead-Assignment-Test-Plan.pdf` are not yet automated.
Suite F (visibility scoping) is the most valuable next one, but it needs a
second, non-admin login — see the plan's §8 setup notes.
