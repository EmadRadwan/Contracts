import { defineConfig, devices } from "@playwright/test";

const STORAGE_STATE = "e2e/.auth/admin.json";

export default defineConfig({
    testDir: "./e2e",
    fullyParallel: true,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 2 : 0,
    reporter: "html",
    use: {
        baseURL: "http://localhost:3000",
        trace: "on-first-retry",
        // Screenshot and video only on failure - useful when a run is handed
        // to someone else to look at.
        screenshot: "only-on-failure",
        video: "retain-on-failure",
    },
    projects: [
        // Runs first; logs in once and writes e2e/.auth/admin.json
        { name: "setup", testMatch: /auth\.setup\.ts/ },

        {
            name: "chromium",
            use: { ...devices["Desktop Chrome"], storageState: STORAGE_STATE },
            dependencies: ["setup"],
            // The login spec must run without a stored session.
            testIgnore: /login\.spec\.ts/,
        },

        // Unauthenticated checks (the existing login page test).
        {
            name: "chromium-anon",
            use: { ...devices["Desktop Chrome"] },
            testMatch: /login\.spec\.ts/,
        },
    ],
    webServer: {
        command: "yarn dev",
        url: "http://localhost:3000",
        reuseExistingServer: !process.env.CI,
        timeout: 120_000,
    },
});
