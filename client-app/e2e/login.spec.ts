import { test, expect } from "@playwright/test";

test("login page renders email, password, and sign in fields", async ({ page }) => {
    await page.goto("/login");

    await expect(page.getByLabel("email", { exact: false })).toBeVisible();
    await expect(page.getByLabel("Password", { exact: false })).toBeVisible();
    await expect(page.getByRole("button", { name: "Sign In" })).toBeVisible();
});
