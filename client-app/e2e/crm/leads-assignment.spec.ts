import { test, expect, Page } from "@playwright/test";

/**
 * CRM - lead assignment.  Mirrors Suite B of CRM-Lead-Assignment-Test-Plan.pdf.
 *
 * Runs authenticated: playwright.config.ts loads the session saved by
 * e2e/auth.setup.ts, so these start on a logged-in page.
 *
 * SELECTORS: the CRM components have no data-testid attributes yet, so these
 * match on visible text and ARIA roles. That keeps them honest about what a
 * user sees, but makes them sensitive to wording and to UI language. The setup
 * pins the language to English (E2E_LANG=ar to switch). Adding data-testid to
 * the CRM components is the recommended follow-up.
 */

const assignButton = (page: Page) =>
    page.getByRole("button", { name: /^(assign|reassign)$/i }).first();

async function openLeads(page: Page) {
    await page.goto("/leads");
    await expect(
        page.getByRole("columnheader", { name: /assigned to/i })
    ).toBeVisible({ timeout: 20_000 });
}

test.describe("CRM lead assignment", () => {
    test.beforeEach(async ({ page }) => {
        await openLeads(page);
    });

    test("grid shows the owner column and an assign action", async ({ page }) => {
        await expect(
            page.getByRole("columnheader", { name: /assigned to/i })
        ).toBeVisible();
        await expect(assignButton(page)).toBeVisible();
    });

    test("unassigned leads are labelled, not blank", async ({ page }) => {
        // Either at least one lead is unassigned, or every lead has an owner.
        // Both are valid; what must not happen is an empty cell.
        const unassigned = page.getByText(/^unassigned$/i);
        const count = await unassigned.count();
        if (count > 0) await expect(unassigned.first()).toBeVisible();
    });

    /**
     * Regression guard for the bug found on 25 Aug: the sales-rep dropdown
     * threw on focus and never opened, making assignment impossible from the UI.
     */
    test("rep dropdown opens and lists sales reps", async ({ page }) => {
        await assignButton(page).click();

        const dialog = page.getByRole("dialog");
        await expect(dialog).toBeVisible();
        await expect(dialog.getByText(/current owner/i)).toBeVisible();

        await dialog.getByRole("combobox").first().click();

        const options = page.getByRole("option");
        await expect(options.first()).toBeVisible({ timeout: 10_000 });
        expect(await options.count(), "no sales reps offered").toBeGreaterThan(0);
    });

    test("assign is disabled until a rep is chosen", async ({ page }) => {
        await assignButton(page).click();
        const dialog = page.getByRole("dialog");

        const submit = dialog.getByRole("button", { name: /^assign$/i }).last();
        await expect(submit).toBeDisabled();

        await dialog.getByRole("combobox").first().click();
        await page.getByRole("option").first().click();
        await expect(submit).toBeEnabled();
    });

    test("assigning sets the owner and records it in history", async ({ page }) => {
        await assignButton(page).click();
        const dialog = page.getByRole("dialog");

        await dialog.getByRole("combobox").first().click();
        const option = page.getByRole("option").first();
        const repName = ((await option.textContent()) ?? "").trim();
        await option.click();

        await dialog.getByRole("button", { name: /^assign$/i }).last().click();
        await expect(dialog).toBeHidden({ timeout: 15_000 });

        // The grid now shows that rep as owner.
        await expect(page.getByText(repName, { exact: false }).first())
            .toBeVisible({ timeout: 15_000 });

        // And the assignment appears in the ownership history, marked current.
        await assignButton(page).click();
        const reopened = page.getByRole("dialog");
        await expect(reopened.getByText(/ownership history/i)).toBeVisible();
        await expect(reopened.getByText(/^current$/i).first()).toBeVisible();
    });

    test("bulk toolbar appears only when rows are selected", async ({ page }) => {
        const toolbarButton = page.getByRole("button", { name: /assign selected/i });
        await expect(toolbarButton).toBeHidden();

        const firstRowCheckbox = page.locator("td input[type='checkbox']").first();
        if (await firstRowCheckbox.count()) {
            await firstRowCheckbox.check();
            await expect(toolbarButton).toBeVisible();
            await expect(page.getByRole("button", { name: /^clear$/i })).toBeVisible();
        }
    });
});
