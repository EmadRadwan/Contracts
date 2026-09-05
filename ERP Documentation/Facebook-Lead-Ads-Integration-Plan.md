# Facebook Lead Ads → CRM Leads Integration

## Context

The client runs Facebook Lead Ads campaigns that collect prospect information directly on Facebook (name, email, phone, custom questions) via Meta's native lead form. Right now those leads sit inside Facebook/Meta Business Suite and have to be manually exported and re-entered into this ERP's CRM Leads module. The goal is to make that automatic: Facebook notifies our backend the moment someone submits a lead form, our backend fetches the full lead details from Facebook's Graph API, and a new `Lead` (Party with `RoleTypeId="LEAD"`) is created in the existing CRM automatically, tagged with `DataSourceId="FACEBOOK"` (a DataSource row that already exists in seed data).

Facebook's webhook only sends a lightweight notification (`leadgen_id`, `page_id`, `form_id`, `ad_id`) — it does **not** include the actual field answers. The real lead data must be pulled separately via a Graph API call using a Page Access Token. This shapes the whole design: a fast, signature-verified webhook receiver that just records the event, plus a background poller that does the actual Graph API fetch + Lead creation.

Architecture decisions already confirmed with the user:
1. **Token setup**: manual paste — client/admin generates a long-lived Facebook Page Access Token themselves (via a Business Manager System User, recommended since it doesn't expire) and pastes it into a new ERP settings screen. No "Connect with Facebook" OAuth flow is being built now.
2. **Processing model**: store-then-poll — the webhook POST handler only verifies the signature, persists the raw event, and returns 200 immediately; a background `IHostedService` polls every ~20s, calls the Graph API, and creates the Lead via the existing `CreateLead` command.
3. **Scope**: single Facebook Page for now, but the config entity's natural key is the Page ID (not a singleton), so adding more pages later doesn't require a schema change.

This is greenfield for this codebase: there is no existing webhook receiver, no `IHttpClientFactory` usage, and no background-job infrastructure anywhere in the repo — this feature introduces the first instance of each, using only built-in ASP.NET Core mechanisms (no Hangfire/Quartz).

## Backend

### Domain (new entities, flat POCOs per repo convention)
- `Domain/FacebookPageConfig.cs` — one row per connected Page. `PageId` (PK, natural key), `PageName`, `EncryptedPageAccessToken`, `VerifyToken`, `IsActive`, `SubscriptionStatus`, `LastEventReceivedStamp`, `LastTestedStamp`, `LastTestResult`, `CreatedStamp`, `LastUpdatedStamp`.
- `Domain/FacebookLeadEvent.cs` — event log for the store-then-poll pipeline. `FacebookLeadEventId` (PK, via `IUtilityService.GetNextSequence("FacebookLeadEvent")`), `LeadgenId` (**unique index** — Facebook redelivers events, this is the dedupe key), `PageId` (FK), `FormId`, `AdId`, `FacebookCreatedTime`, `RawPayload`, `StatusId` (`PENDING`/`PROCESSING`/`PROCESSED`/`FAILED`/`DEAD_LETTER`, plain string), `PartyId` (set once the Lead is created), `ErrorMessage`, `RetryCount`, `ReceivedStamp`, `ProcessedStamp`.

### Persistence
- `Persistence/DataContext.cs`: add `DbSet<FacebookPageConfig>` / `DbSet<FacebookLeadEvent>` and `OnModelCreating` blocks (`ToTable`, `HasColumnName` per field — mirror the `WebAnalyticsConfig` block), plus `HasIndex(e => e.LeadgenId).IsUnique()` and `HasIndex(e => e.StatusId)` on the event table.
- Generate the migration for review only: `dotnet ef migrations add AddFacebookLeadIntegration --project Persistence --startup-project API`. **Do not apply it** — `Program.cs` already runs `context.Database.MigrateAsync()` on startup, so once the migration is reviewed and committed it applies itself on next deploy.
- `Persistence/SeedContracts.cs`: add one idempotent `SequenceValueItem` seed block for `"FacebookLeadEvent"`, following the existing `"PaymentReceipt"` pattern (~line 540). **No new `DataSource` seed is needed** — `"FACEBOOK"` already exists in `API/Json/data_sources.json`.

### Infrastructure (`Infrastructure/Facebook/` — new)
- `Infrastructure/Infrastructure.csproj`: add `Microsoft.Extensions.Http` package reference (Infrastructure is a plain SDK project, not `.Web`, so `IHttpClientFactory` isn't available for free there — this is the first HTTP-client usage in the repo).
- `FacebookGraphApiSettings.cs` — typed options (`AppId`, `AppSecret`, `GraphApiVersion`, `GraphApiBaseUrl`), bound from a new `Facebook` config section, mirroring `DigitalOceanSettings.cs`.
- `IFacebookGraphApiClient` (in `Application/Interfaces/`, implementation in `Infrastructure/Facebook/FacebookGraphApiClient.cs`) — `GetLeadFieldDataAsync(leadgenId, pageAccessToken)` calls `GET {graph}/{ver}/{leadgen_id}?fields=field_data,ad_id,form_id,created_time&access_token=...`; `TestPageTokenAsync(pageId, pageAccessToken)` calls `GET {graph}/{ver}/{page_id}?fields=name&access_token=...` for the "Test Connection" button. Registered via `AddHttpClient<IFacebookGraphApiClient, FacebookGraphApiClient>()`.
- `ITokenProtector` / `FacebookTokenProtector.cs` — thin wrapper around ASP.NET Core's built-in `IDataProtectionProvider` (already active in this app via Identity's `AddDefaultTokenProviders()` — confirmed the VM already persists the key ring via a Docker volume mount in `docker-compose.vm.yml`, so **no new Data Protection setup is required**). Encrypts the Page Access Token before it's stored; decrypts only inside the poller/test-connection handler, never returned to the frontend.
- `FacebookSignatureValidator.cs` — static helper verifying the `X-Hub-Signature-256` header via HMAC-SHA256 over the raw request body using the App Secret, constant-time compare.

### Application (`Application/CRM/FacebookIntegration/` — new, follows the CQRS-nested-class-per-file convention)
- `FacebookPageConfigDto.cs`
- `GetFacebookPageConfig.cs` — `Query`/`Handler`, returns config with the token omitted/masked, never the plaintext.
- `SaveFacebookPageConfig.cs` — `Command`/`Handler`, upserts by `PageId`; if the token field is left blank on an edit, keeps the existing encrypted value instead of overwriting it.
- `TestFacebookConnection.cs` — `Command`/`Handler`, decrypts the token, calls `IFacebookGraphApiClient.TestPageTokenAsync`, updates `LastTestedStamp`/`LastTestResult`/`PageName`.
- `RecordFacebookLeadEvent.cs` — `Command`/`Handler` called from the webhook controller; idempotency check on `LeadgenId` before inserting a `PENDING` row.
- `FacebookFieldDataMapper.cs` — plain static mapper from Graph API `field_data` (`[{name, values}]`) to the existing `LeadDto` (`FullName`, `Email`, `Phone`, `City`, `PostalCode`, `DataSourceId="FACEBOOK"`). Known field names (`full_name`/`first_name`/`last_name`/`email`/`phone_number`/`city`/`post_code`) map directly; any unmapped custom-question fields (lead forms commonly have client-specific questions) are appended as `"question: answer"` pairs into `LeadDto.Address2` rather than dropped, since they carry real sales value and there's no dedicated notes field on the DTO yet — each unmapped field name is also logged as a warning so recurring custom questions become visible for a future explicit mapping.
- **Reuses `Application/CRM/Leads/CreateLead.cs` as-is** for actually creating the Lead — do not fork or modify it. (Note: there's a second, older `Application/Parties/Parties/CreateLead.cs` used by the manual "New Lead" button in the UI — unrelated, do not touch.)

### API (`API/Controllers/CRM/`)
- `FacebookWebhookController.cs` — `[AllowAnonymous]` (global auth policy requires this opt-out, same precedent as `AccountController`).
  - `GET api/facebook/webhook` — Meta's verify handshake: compares `hub.verify_token` against the stored `VerifyToken`, echoes back `hub.challenge` on match, else 403.
  - `POST api/facebook/webhook` — reads the **raw request body bytes** (via `Request.EnableBuffering()` before touching the stream, so JSON parsing can happen after signature verification), validates `X-Hub-Signature-256` via `FacebookSignatureValidator`, extracts `leadgen`-field changes, sends `RecordFacebookLeadEvent.Command` per entry, and **always returns 200 immediately** — no Graph API calls happen in this request.
- `FacebookIntegrationController.cs` — inherits `BaseApiController` (normal authenticated route, no anonymous access): `GET config`, `POST config` (save), `POST config/{pageId}/test` (test connection) — backs the new admin settings screen.

### Background processing
- `API/Services/FacebookLeadPollingService.cs` — a `BackgroundService` (built into ASP.NET Core, `AddHostedService<FacebookLeadPollingService>()`), polling every ~20s via `IServiceScopeFactory` (since `DataContext`/`IMediator` are scoped, the hosted service itself is a singleton). For each `PENDING`/retryable `FAILED` event (capped batch of ~25, oldest first): decrypt the Page's token, call the Graph API, map to `LeadDto`, send `CreateLead.Command` via `IMediator`, mark `PROCESSED` (storing the resulting `PartyId`) or increment `RetryCount`/`ErrorMessage` and mark `FAILED`, promoting to `DEAD_LETTER` after 5 retries so it stops being auto-retried and is visible for manual follow-up later.

### Wiring
- `API/Extensions/ApplicationServiceExtensions.cs`: `services.Configure<FacebookGraphApiSettings>(config.GetSection("Facebook"))`, `AddHttpClient<IFacebookGraphApiClient, FacebookGraphApiClient>()`, `AddScoped<ITokenProtector, FacebookTokenProtector>()`, `AddHostedService<FacebookLeadPollingService>()`.
- `API/appsettings.json` / `appsettings.Production.json`: new `Facebook` section (`AppId`, `AppSecret` blank/placeholder, `GraphApiVersion`, `GraphApiBaseUrl`) — `AppSecret` supplied via environment variable at deploy time, same pattern as the existing SendGrid key.
- `docker-compose.vm.yml`: add `Facebook__AppSecret` / `Facebook__AppId` env passthroughs next to the existing SendGrid entry.

## Frontend (`client-app/src/`)

- `src/app/store/apis/crm/facebookIntegrationApi.ts` — new RTK Query slice (modeled on `leadsApi.ts`/`dataSourcesApi.ts`): `fetchFacebookConfig`, `saveFacebookConfig`, `testFacebookConnection`.
- `src/features/CRM/facebook-integration/FacebookLeadSettings.tsx` — new settings page (Kendo Form, styled like `LeadsForm.tsx`): Page ID, Page Name (read-only, populated by Test Connection), Access Token (masked input, "leave blank to keep current" on edit), Verify Token (with a client-side "Generate" button), Save, Test Connection, and a read-only status panel (subscription status, last tested, last lead received).
- `src/app/router/Routes.tsx`: add a `{path: "facebook-integration", element: <FacebookLeadSettings/>}` route inside the existing CRM block (~line 231-239), reusing the `CRM_View` role for now — a stricter role can be introduced later as a one-line change if the client wants credentials restricted to fewer admins.
- `src/features/CRM/menu/CRMMenu.tsx`: add a nav entry for the new page.
- `src/app/store/configureStore.ts`: register the new api slice (import, reducer map, middleware chain, re-export) — same three-plus-one edit pattern as every other CRM api slice.

All new labels go through `getTranslatedLabel('crm.facebookIntegration.<key>', 'Fallback')` for Arabic support, per `client-app/CLAUDE.md`.

## Execution order
1. Domain entities → `DataContext` wiring → generate migration (review only, do not apply) → sequence seed.
2. Infrastructure: csproj package, settings, token protector, signature validator, Graph API client.
3. Application: DTOs + the 5 CQRS files + field mapper.
4. API: both controllers, DI registrations, appsettings sections, hosted service.
5. Frontend: api slice, settings page, routing, nav entry, store registration.
6. **Stop after code changes for manual review.** Do not run `dotnet build`/`dotnet run`, `npm run build`/`npm start`, or `dotnet ef database update` — per repo convention, report what changed and let the developer build/apply/test manually.

## Verification (manual, after developer review)
1. Review the generated EF migration by hand for correct column types/lengths before it's committed.
2. Apply the migration manually in dev, then use Graph API Explorer (or `curl`) to simulate a `POST` to `/api/facebook/webhook` with a valid `X-Hub-Signature-256` computed from a test App Secret, and confirm a `FacebookLeadEvent` row is inserted with `StatusId=PENDING`.
3. Watch `FacebookLeadPollingService` logs on the next poll cycle pick up the event, call the Graph API (against a real test lead in Facebook's Lead Ads Testing Tool), and confirm a new `Lead` appears in the existing Leads list UI with `DataSourceId=FACEBOOK`.
4. In the browser, open the new "Facebook Integration" settings page, paste a real System User Page Access Token, hit "Test Connection", and confirm `PageName`/`SubscriptionStatus` populate correctly.
5. Confirm the `GET /api/facebook/webhook` verify handshake returns the `hub.challenge` value when Meta's Webhooks product config is pointed at the deployed production HTTPS endpoint.

---

## What the client must provide / do

1. **Meta Business/Developer App**: a Meta Business Manager account and a Meta App (type "Business") created at developers.facebook.com, with the **Webhooks** product added.
2. **Page admin access**: admin access to the specific Facebook Page running the Lead Ads campaigns.
3. **A long-lived Page Access Token via a Business Manager System User** (recommended — doesn't expire, unlike a personal-account token): Business Settings → Users → System Users → create one, assign the Page as an asset, generate a token scoped to that Page requesting the `leads_retrieval` permission (plus `pages_show_list`, and `pages_manage_ads` if ad-level data beyond leads is ever wanted). This token gets pasted into the new ERP settings screen — nothing else in the flow needs it.
4. **Subscribing the Page to send events** — two one-time steps, done via the Meta App Dashboard and a single Graph API call (not automated by the ERP in this version):
   - Configure the App's Webhooks product with the callback URL and a chosen Verify Token, subscribed to the `page` object's `leadgen` field.
   - Call `POST https://graph.facebook.com/v20.0/{page-id}/subscribed_apps?subscribed_fields=leadgen&access_token={page-access-token}` — the App-level webhook config alone isn't enough, each Page must individually opt in.
5. **A public HTTPS callback URL**: Meta will not call `localhost` or accept a self-signed cert — the webhook must point at the production HTTPS endpoint (this repo serves HTTPS on port 8544 per `CLAUDE.md`), e.g. `https://<production-host>:8544/api/facebook/webhook`.
6. **Privacy Policy URL + Data Use Checkup**: the `leads_retrieval` permission requires the Meta App to have a valid, publicly reachable Privacy Policy URL in App Settings, and to pass Meta's periodic Data Use Checkup self-certification about how lead data is stored/used/deleted.
7. **Fast path — no formal Meta App Review needed for this single-client setup**: as long as the App stays in Development Mode and the client's own Facebook account (the one administering the Page) is added as an Admin/Developer/Tester on the Meta App, that person's Page can use `leads_retrieval` and receive webhooks without submitting the App for Meta's formal review process. This is the reason no OAuth/App-Review flow is being built now. If the ERP is ever extended to pull leads from Pages belonging to people who are *not* Admins/Testers on the App (e.g. onboarding other businesses later), full Meta App Review becomes mandatory at that point — worth flagging to the client as a future consideration, not a blocker today.
8. **Verify Token**: an arbitrary secret string (e.g. a generated UUID) the client/implementer invents and enters in both Meta's Webhooks config and the ERP settings screen — proves the GET handshake request genuinely came from the configured webhook setup.
9. **App Secret**: found in Meta App Dashboard → Settings → Basic — goes only into the server's environment variable (`Facebook__AppSecret`), never into the database or frontend; used solely to verify the `X-Hub-Signature-256` header on incoming POSTs.
