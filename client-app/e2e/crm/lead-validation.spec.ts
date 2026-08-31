import { test, expect, Page } from "@playwright/test";

/**
 * CRM - lead form validation.  Mirrors Suite I (11.1) of
 * CRM-Lead-Assignment-Test-Plan.pdf.
 *
 * The rule, taken from the columns of the Excel upload file:
 *   required : name, mobile, lead source
 *   optional : email (but must be valid if supplied), country, address, title
 */

async function openNewLeadForm(page: Page) {
    await page.goto("/leads");
    const newLead = page.getByRole("button", { name: /new lead/i });
    await expect(newLead).toBeVisible({ timeout: 20_000 });
    await newLead.click();
}

const uniqueName = () => `E2E Test Lead ${Date.now()}`;

test.describe("CRM lead form validation", () => {
    test("required fields are enforced, optional ones are not", async ({ page }) => {
        await openNewLeadForm(page);

        await page.getByRole("button", { name: /^(create|save)$/i }).first().click();

        // Name, mobile and lead source must complain.
        await expect(page.getByText(/required/i).first()).toBeVisible();

        // Country must NOT be among the complaints - it was made optional so the
        // form matches the upload file, which has no Country column.
        const countryError = page.getByText(/country.*required|required.*country/i);
        await expect(countryError).toHaveCount(0);
    });

    test("email is optional but must be valid when supplied", async ({ page }) => {
        await openNewLeadForm(page);

        const email = page.getByLabel(/email/i).first();
        await email.fill("abc@");
        await email.blur();
        await expect(page.getByText(/not valid|invalid/i).first()).toBeVisible();

        // Clearing it removes the complaint - blank is acceptable.
        await email.fill("");
        await email.blur();
        await expect(page.getByText(/not valid|invalid/i)).toHaveCount(0);
    });

    test("a lead saves with name, mobile and lead source only", async ({ page }) => {
        await openNewLeadForm(page);

        const name = uniqueName();
        await page.getByLabel(/first name|name/i).first().fill(name);
        await page.getByLabel(/mobile/i).first().fill("01000000001");

        // Lead source is a dropdown; pick the first available option.
        const source = page.getByRole("combobox").filter({ hasNot: page.locator("[disabled]") }).last();
        await source.click();
        await page.getByRole("option").first().click();

        await page.getByRole("button", { name: /^(create|save)$/i }).first().click();

        // Back on the grid with the new lead visible - no email, no country needed.
        await expect(page.getByText(name, { exact: false }).first())
            .toBeVisible({ timeout: 20_000 });
    });
});
