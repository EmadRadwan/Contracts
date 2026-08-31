import { test as setup, expect } from "@playwright/test";

/**
 * Establishes an authenticated session once; every other spec reuses it.
 *
 * Authentication goes through the API rather than the login form. That is the
 * approach Playwright's own docs recommend for token-based apps: it is faster,
 * it does not re-test the login form in every run, and when it fails the error
 * points at authentication rather than at whatever feature the spec was about.
 *
 * The login FORM is still covered - by e2e/login.spec.ts, which runs
 * unauthenticated in the chromium-anon project.
 */

export const STORAGE_STATE = "e2e/.auth/admin.json";

const API = process.env.E2E_API_URL ?? "http://localhost:5001/api";
const EMAIL = process.env.E2E_EMAIL ?? "eradwan1967@gmail.com";
const PASSWORD = process.env.E2E_PASSWORD ?? "Pa$$w0rd";
const LANG = process.env.E2E_LANG ?? "en"; // "ar" to exercise the Arabic UI

setup("authenticate", async ({ page, request }) => {
    // 1. Get a real session from the API.
    const res = await request.post(`${API}/account/login`, {
        data: { email: EMAIL, password: PASSWORD },
    });
    expect(
        res.ok(),
        `API login failed with ${res.status()}. Is the API running on ${API}?`
    ).toBeTruthy();

    const user = await res.json();
    expect(user.token, "login response contained no token").toBeTruthy();

    // 2. Seed it before any app code runs. addInitScript applies to every
    //    navigation in this context, so it survives reloads.
    await page.addInitScript(
        ([u, lang]) => {
            window.localStorage.setItem("user", JSON.stringify(u));
            window.localStorage.setItem("selectedLang", lang as string);
        },
        [user, LANG] as const
    );

    // 3. Prove the app actually accepts it, rather than assuming.
    await page.goto("/leads");
    await expect(
        page,
        "the app bounced to /login - it rejected a token the API had just issued"
    ).not.toHaveURL(/\/login/, { timeout: 20_000 });

    const stored = await page.evaluate(() => window.localStorage.getItem("user"));
    expect(stored, "the app cleared the session on load").toBeTruthy();

    await page.context().storageState({ path: STORAGE_STATE });
});
