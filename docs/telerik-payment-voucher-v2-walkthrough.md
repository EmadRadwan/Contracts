# Payment Voucher "v2" (Telerik) — Code Walkthrough

**What this covers:** every file that participates when you click **"معاينة بيان الدفعة (Telerik)"**
on a payment, traced the way a debugger would step through it — frame by frame, which line
hands off to which file, and what's in the key variables at each hop. Plus a section on the
config/infra files that were changed to make Telerik Reporting work at all.

**The request being traced** (from the server log):

```
HTTP GET /api/paymentReports/payment-report-v2/O18037  →  200  (PDF bytes)
```

`O18037` is a disbursement payment (`PaymentParentTypeDescription != "RECEIPT"` → **إيصال صرف**).

---

## 1. The cast — files involved

| # | File | Layer | Role in this request |
|---|---|---|---|
| F1 | `client-app/.../payment/form/EditPaymentForm.tsx` | React UI | The button, the click handler, the PDF preview dialog |
| F2 | `client-app/src/app/store/apis/payment/paymentsApi.ts` | RTK Query | Defines the `getPaymentReportPdfV2` endpoint + generated hook; attaches auth + `Accept-Language` |
| F3 | `API/Controllers/BaseApiController.cs` | API | Base class: route prefix `api/[controller]`, lazy `Mediator` accessor |
| F4 | `API/Controllers/Report/PaymentReports/PaymentReportsController.cs` | API | `GetPaymentReportV2` action — orchestrates data → render → file |
| F5 | `API/Extensions/ApplicationServiceExtensions.cs` | API composition root | DI: binds `IPaymentVoucherReportService` → `TelerikPaymentVoucherReportService`; registers MediatR + pipeline behaviors |
| F6 | `Application/Accounting/Payments/GeneratePaymentReportPdf.cs` | Application (CQRS) | `GetPaymentForReport.Query` + `.Handler` + `PaymentReportDto` — the one DB read |
| F7 | `Application/Interfaces/IPaymentVoucherReportService.cs` | Application (port) | The contract the controller depends on (keeps API → Telerik decoupled at the seam) |
| F8 | `API/Reporting/TelerikPaymentVoucherReportService.cs` | API (adapter) | Wraps `ReportProcessor`; validates format; turns a `PaymentReportDto` into `byte[]` |
| F9 | `API/Reporting/PaymentVoucherReport.cs` | API (report definition) | Code-defined `Telerik.Reporting.Report` — builds the voucher layout item by item |
| F10 | `Application/Accounting/Payments/ArabicPaymentFormatter.cs` | Application (helper) | Amount-in-words, Arabic-Indic digits, currency suffix, date stencil |
| F11 | `Infrastructure/Pdf/PdfGenerationService.cs` | Infrastructure | **Not called here** — the v1 (QuestPDF) engine, shown for contrast |

Everything F6–F10 is also reachable from the **v1** endpoint except F8/F9; both endpoints
share F6 (the data) and F10 (the Arabic helpers).

---

## 2. Debugger walkthrough — frontend

### STEP 1 — `EditPaymentForm.tsx:888` — the click

```tsx
<Button variant="outlined" color="secondary"
        onClick={handlePreviewPdfV2}
        disabled={isPdfV2Fetching || !payment?.paymentId}>
  {isPdfV2Fetching ? "جاري التحضير..." : "معاينة بيان الدفعة (Telerik)"}
</Button>
```

`payment` is the currently-loaded payment object (form state). Click → **jump to** `handlePreviewPdfV2`.

### STEP 2 — `EditPaymentForm.tsx:120` — the handler

```tsx
const handlePreviewPdfV2 = async () => {
    if (!payment?.paymentId) return;              // guard
    setPreviewLoading(true);                       // dialog spinner on
    try {
        const arrayBuffer = await triggerPdfV2(payment.paymentId).unwrap();   // ← line 125
        const blob = new Blob([arrayBuffer], { type: "application/pdf" });
        const url  = URL.createObjectURL(blob);
        setPdfBlobUrl(url);
        setShowPdfViewer(true);
    } catch (error) { /* alert */ } finally { setPreviewLoading(false); }
};
```

- `triggerPdfV2` came from `EditPaymentForm.tsx:148`:
  `const [triggerPdfV2, { isFetching: isPdfV2Fetching }] = useLazyGetPaymentReportPdfV2Query();`
- Calling `triggerPdfV2("O18037")` **jumps into RTK Query's generated thunk** → the endpoint
  defined in F2.
- Execution now *suspends* at `await` until the HTTP round-trip resolves.

### STEP 3 — `paymentsApi.ts:290` — the endpoint definition

```ts
getPaymentReportPdfV2: builder.query<ArrayBuffer, string>({
    query: (paymentId) => ({
        url: `/paymentReports/payment-report-v2/${paymentId}`,
        responseHandler: (response) => response.arrayBuffer(),   // raw bytes, not JSON
        cache: "no-cache",
    }),
}),
```

- Base URL: `paymentsApi.ts:14` → `import.meta.env.VITE_API_URL` → `http://localhost:5001/api` (dev).
- Final URL: `http://localhost:5001/api/paymentReports/payment-report-v2/O18037`.
- `paymentsApi.ts:15–24` `prepareHeaders` runs: adds `Authorization: Bearer <token>` and
  `Accept-Language: <ui language>` from the Redux store.
- `responseHandler` overrides the default JSON parse so the 200 body is read as an `ArrayBuffer`.

**→ HTTP request leaves the browser.** Next frame is on the server.

---

## 3. Debugger walkthrough — ASP.NET request pipeline

### STEP 4 — middleware, before the controller

Order (from `API/Program.cs`): Serilog request logging → Response compression →
**Authentication** → Routing → **Authorization**.

- `Program.cs` registers a **global** `AuthorizeFilter` (`RequireAuthenticatedUser`), so the
  bearer token from STEP 3 is mandatory — a missing/expired token would 401 here and never
  reach F4.
- Routing matches `api/PaymentReports/payment-report-v2/{paymentId}` to F4's action
  (case-insensitive; the URL said `paymentReports`).

### STEP 5 — `PaymentReportsController` construction (DI)

Before the action runs, the container builds the controller. Its constructor
(`PaymentReportsController.cs:13`):

```csharp
public PaymentReportsController(
    IPdfGenerationService pdfService,                    // v1 (QuestPDF)  — not used this request
    IPaymentVoucherReportService voucherReportService)   // v2 (Telerik)
```

- `IPaymentVoucherReportService` is resolved because of
  `ApplicationServiceExtensions.cs:91`:
  `services.AddScoped<IPaymentVoucherReportService, TelerikPaymentVoucherReportService>();`
- **If that line were missing:** the container throws *"Unable to resolve service for type
  IPaymentVoucherReportService"* and the request 500s here, before your code.

### STEP 6 — `PaymentReportsController.cs:41` — the action

```csharp
[HttpGet("payment-report-v2/{paymentId}")]
public async Task<IActionResult> GetPaymentReportV2(string paymentId, [FromQuery] string format = "PDF")
{
    var reportData = await Mediator.Send(new GetPaymentForReport.Query(paymentId, "ar"));   // line 43
    var bytes      = _voucherReportService.Render(reportData, format);                       // line 45
    var (contentType, ext) = /* switch on format */ ("application/pdf", "pdf");              // line 47
    var fileName = $"{reportData.PaymentId}_بيان_دفعة.{ext}";                                 // line 56
    return File(bytes, contentType, fileName);                                               // line 58
}
```

Locals on entry: `paymentId = "O18037"`, `format = "PDF"`.

`Mediator` is the property on `BaseApiController.cs:13`:

```csharp
protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
```

Line 43 constructs `new GetPaymentForReport.Query("O18037", "ar")` and **jumps into MediatR**.

---

## 4. Debugger walkthrough — the data read (CQRS)

### STEP 7 — MediatR pipeline behaviors

`Mediator.Send` doesn't call the handler directly. Two behaviors wrap it
(`ApplicationServiceExtensions.cs:77` and `:80`):

```
LoggingBehavior<Query,PaymentReportDto>          ← starts a stopwatch
  └── AuditBehavior<Query,PaymentReportDto>      ← writes an AUDIT_ACTIVITY row (commands only; a query is a no-op here)
        └── GetPaymentForReport.Handler.Handle(query, ct)     ← your code
```

### STEP 8 — `GeneratePaymentReportPdf.cs:20` — `GetPaymentForReport.Handler.Handle`

```csharp
public async Task<PaymentReportDto> Handle(Query request, CancellationToken ct)
{
    var isArabic = request.Language.Equals("ar", ...);            // true

    var query = from pyt in _context.Payments
                where pyt.PaymentId == request.PaymentId          // "O18037"
                join ptt in _context.PaymentTypes  on ...         // INNER
                join sts in _context.StatusItems   on ...         // INNER
                join pty in _context.Parties       on pyt.PartyIdFrom ...   // INNER
                join pmt in _context.PaymentMethods ... into pmtJoin  from pmt in pmtJoin.DefaultIfEmpty()  // LEFT
                join ptyto ... DefaultIfEmpty()                    // LEFT  (PartyIdTo)
                join cc   ... DefaultIfEmpty()                     // LEFT  (CostCenter)
                join proj ... DefaultIfEmpty()                     // LEFT  (WorkEffort / project)
                select new PaymentReportDto { /* ~18 fields, Arabic vs English picked by isArabic */ };

    var result = await query.FirstOrDefaultAsync(ct);             // line 71 — the SQL executes here
    if (result == null) throw new Exception($"Payment ... not found.");
    return result;
}
```

- `_context` is the `DataContext` injected straight into the handler — **no repository layer**
  (house rule). Lazy loading is off, so every needed table is an explicit `join`.
- One `SELECT … LEFT JOIN …` hits MySQL. EF materialises exactly one `PaymentReportDto`
  (`GeneratePaymentReportPdf.cs:82`).
- Return value bubbles back out through `AuditBehavior` → `LoggingBehavior` (which logs the
  handler's elapsed ms) → the `await` on `PaymentReportsController.cs:43`.

**State now:** `reportData` is a populated `PaymentReportDto`
(`PaymentId`, `Amount`, `EffectiveDate`, `FromPartyName`, `ToPartyName`,
`PaymentMethodId`, `IsBankTransfer`, `Comments`, …).

---

## 5. Debugger walkthrough — rendering (Telerik)

### STEP 9 — `PaymentReportsController.cs:45` → `TelerikPaymentVoucherReportService.cs:20`

```csharp
public byte[] Render(PaymentReportDto data, string format = "PDF", string companyName = "Golden Land")
{
    ArgumentNullException.ThrowIfNull(data);
    var normalizedFormat = "PDF";                                   // trimmed + upper-cased
    if (!SupportedFormats.Contains(normalizedFormat)) throw ...;    // PDF/XLSX/DOCX/CSV/IMAGE/PPTX

    using var report   = new PaymentVoucherReport(data, companyName);   // ← line 28, jump to F9
    var reportSource   = new InstanceReportSource { ReportDocument = report };
    var processor      = new ReportProcessor();
    var result         = processor.RenderReport(normalizedFormat, reportSource, new Hashtable());   // line 32

    if (result.HasErrors) throw new System.InvalidOperationException(...);
    return result.DocumentBytes;                                    // line 39 — the PDF
}
```

### STEP 10 — `PaymentVoucherReport.cs:29` — the report constructor

This is where the layout is assembled. Walking the constructor:

| Line | What happens | Notes |
|---|---|---|
| `31` | null-check `data` | |
| `33–40` | derive booleans: `isReceipt`, `isCash`, `isBankTransfer`, `isCheque` | same rules as the QuestPDF voucher |
| `42` | `ArabicPaymentFormatter.CurrencySuffix(data.CurrencyUomId)` | **jump to F10:16** → `"جنيه مصرى"` etc. |
| `43` | `ArabicPaymentFormatter.AmountToWords(data.Amount, suffix)` | **jump to F10:45** → recurses through `NumberToWords` (F10:58) to build `"مائتان … جنيه مصرى لا غير"` |
| `44–45` | split `Amount` into whole pounds + piastres | |
| `48` | `Width = Unit.Cm(18.0)` | **first touch of `Telerik.Reporting.Drawing.Unit`** → its static ctor runs → it loads the drawing backend `Telerik.Drawing.Skia`. *This is the line that threw `DrawingFactoryUnavailableException` before the `Telerik.Drawing.Skia` package was added.* |
| `49–50` | `PageSettings.PaperKind = A4`, 1.5 cm margins | |
| `52–55` | create `PageHeaderSection` + `DetailSection`, add to `report.Items` | a `DetailSection` with **no data source renders exactly once** — the trick that makes a "static" report |
| `57` | `BuildHeader(...)` | → STEP 11 |
| `58` | `BuildBody(...)` | → STEP 12 |

### STEP 11 — `PaymentVoucherReport.cs:61` — `BuildHeader`

- `63–77` load `wwwroot/goldenlandlogo.jpg` and set it via `SKBitmap.Decode(logoPath)`.
  `PictureBox.Value` accepts `string | IImage | System.Drawing.Image | SKBitmap` — **not**
  `byte[]` (that was `ArgumentException: You can assign …` before the `SKBitmap.Decode` fix).
- `79–89` title (`إيصال صرف` / `إيصال قبض`), payment id, 3 company lines — each built by the
  `Text(...)` factory (`PaymentVoucherReport.cs:210`).
- `91–105` a header rule = an empty `TextBox` with only a bottom border (replaced the
  `Shape`/`LineShape` item, whose namespace moved between Reporting versions → `CS0246`).

### STEP 12 — `PaymentVoucherReport.cs:108` — `BuildBody`

A single running `y` cursor (cm from the top of the detail section). Each block:
`y` is read, one-to-three items are added at that `y`, then `y` is advanced.

```
y=0.2   cbBank / cbCheque / cbCash        (Checkbox factory :250 — a Panel = [X] box + caption)
y=1.4   date line                          "تحريراً فى : ٢٠٢٦/..."
y=2.5   [piastres][جنيه][pounds][قرش]      (Boxed factory :240 — full border)
y=3.8   recipient caption + name           (Underlined factory :230 — bottom border only)
y=4.9   amount-in-words                    ← the AmountToWords string from STEP 10
y=6.1   نقداً / بموجب
y=7.1   مسحوب على بنك
y=8.1   cheque no. + date
y=9.1   تحويل بنكى/اون لاين
y=10.3  وذلك عن :  + Comments
y=12.7  signatures block (يعتمد / المحاسب / المستلم + lines)
y≈16    مرجع: O18037
```

Factories `Text` / `Underlined` / `Boxed` / `Checkbox` (`PaymentVoucherReport.cs:210–288`)
each `new` a `TextBox` (or `Panel`), set `Location`/`Size` in cm, font = `"Arial"`
(`FontFamily` const — swap for an Arabic naskh face once the Docker image ships one), and
border style. Nothing is data-bound; every `.Value` is a literal string already computed
from `data`.

Constructor returns → back to `TelerikPaymentVoucherReportService.cs:29`.

### STEP 13 — `TelerikPaymentVoucherReportService.cs:32` — `ReportProcessor.RenderReport`

- Telerik walks the report's item tree, uses **Skia** (via `Telerik.Drawing.Skia`) to measure
  and shape text (Arabic shaping/bidi happens here) and to rasterize, then emits a PDF stream.
- `Telerik.Licensing` is consulted during processing — because the DevCraft key is present
  (`~/.telerik/telerik-license.txt` locally, `TELERIK_LICENSE` in Docker) there is **no trial
  watermark**.
- `result.DocumentBytes` → `byte[]` of the finished PDF. Returned to the controller.

---

## 6. Debugger walkthrough — response & display

### STEP 14 — `PaymentReportsController.cs:47–58`

```csharp
var (contentType, ext) = "PDF" switch { … _ => ("application/pdf", "pdf") };
var fileName = "O18037_بيان_دفعة.pdf";
return File(bytes, "application/pdf", fileName);          // FileContentResult
```

MVC serializes a `200 OK`, `Content-Type: application/pdf`, body = the bytes. Response
compression may gzip it on the way out. Serilog logs `responded 200 in <ms>`.

### STEP 15 — back in the browser, `EditPaymentForm.tsx:125`

The suspended `await triggerPdfV2(...).unwrap()` now resolves:

```
arrayBuffer  ← response.arrayBuffer()      (from paymentsApi.ts responseHandler)
blob         ← new Blob([arrayBuffer], {type:"application/pdf"})
url          ← URL.createObjectURL(blob)    "blob:http://localhost:3000/…"
setPdfBlobUrl(url); setShowPdfViewer(true);
```

The dialog (further down `EditPaymentForm.tsx`) renders `<Viewer fileUrl={pdfBlobUrl} …/>`
(`@react-pdf-viewer`) inside a MUI `Dialog`. `handleClosePdfViewer` later calls
`URL.revokeObjectURL` to free the blob.

---

## 7. One picture — the call sequence

```
F1  EditPaymentForm.tsx      click "معاينة بيان الدفعة (Telerik)"
      │  handlePreviewPdfV2()  →  triggerPdfV2("O18037")
      ▼
F2  paymentsApi.ts           GET {VITE_API_URL}/paymentReports/payment-report-v2/O18037
      │  + Authorization: Bearer …   + Accept-Language: ar
      ▼  ─────────────────────────── HTTP ───────────────────────────
    Program.cs pipeline      Serilog → compression → auth filter → routing
      ▼
F4  PaymentReportsController  GetPaymentReportV2("O18037", "PDF")
      │
      ├─(a)─► F3 BaseApiController.Mediator.Send( GetPaymentForReport.Query )
      │         └─► F5 LoggingBehavior → AuditBehavior
      │               └─► F6 GetPaymentForReport.Handler.Handle
      │                     └─ one LEFT-JOIN SELECT on MySQL  ─►  PaymentReportDto
      │        ◄──────────────────────────────────────────────────  reportData
      │
      ├─(b)─► F8 TelerikPaymentVoucherReportService.Render(reportData, "PDF")
      │         ├─► F9 new PaymentVoucherReport(dto)
      │         │      ├─ F10 ArabicPaymentFormatter (words / digits / suffix)
      │         │      ├─ BuildHeader()   (logo via SKBitmap, title, company)
      │         │      └─ BuildBody()     (~30 TextBox/Panel items down a y cursor)
      │         └─► ReportProcessor.RenderReport("PDF", …)  ─►  byte[]  (Skia + licence)
      │        ◄──────────────────────────────────────────────────  bytes
      │
      └─(c)─► return File(bytes, "application/pdf", "O18037_بيان_دفعة.pdf")  →  200
      ▼  ─────────────────────────── HTTP ───────────────────────────
F1  EditPaymentForm.tsx      ArrayBuffer → Blob → URL.createObjectURL → <Viewer fileUrl=…>
```

---

## 8. v1 vs v2 — same data, different engine

| | **v1** (existing) | **v2** (this walkthrough) |
|---|---|---|
| Endpoint | `GET /paymentReports/payment-report/{id}` | `GET /paymentReports/payment-report-v2/{id}?format=PDF` |
| Controller action | `GetPaymentReportPdf` (F4:22) | `GetPaymentReportV2` (F4:41) |
| Data source | `GetPaymentForReport.Query` (F6) | **same** `GetPaymentForReport.Query` (F6) |
| Arabic helpers | private methods inside F11 | `ArabicPaymentFormatter` (F10) |
| Rendering | `IPdfGenerationService` → QuestPDF (F11) | `IPaymentVoucherReportService` → Telerik (F8 → F9) |
| Output | PDF only | PDF / XLSX / DOCX / CSV / IMAGE (via `?format=`) |
| Frontend | `getPaymentReportPdf` + "معاينة بيان الدفعة (PDF)" | `getPaymentReportPdfV2` + "معاينة بيان الدفعة (Telerik)" |

The split point is a single DI line (F5:91). The controller depends only on the *interface*
(F7), so swapping engines never touches F4's logic.

---

## 9. Config / infra files changed for Telerik

### 9.1 `nuget.config` — **new, repo root**

Telerik Reporting packages are **not on nuget.org** — they come from the Telerik private feed.

```xml
<packageSources>
  <clear />
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  <add key="telerik"   value="https://nuget.telerik.com/v3/index.json" />
</packageSources>
<packageSourceCredentials>
  <telerik>
    <add key="Username" value="api-key" />
    <add key="ClearTextPassword" value="%TELERIK_NUGET_KEY%" />   <!-- from env, never committed -->
  </telerik>
</packageSourceCredentials>
<packageSourceMapping>
  <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  <packageSource key="telerik"><package pattern="Telerik.*" /></packageSource>
</packageSourceMapping>
```

- `packageSourceMapping` keeps every non-Telerik package on nuget.org — a missing key only
  breaks the `Telerik.*` restore, nothing else.
- Dev must `export TELERIK_NUGET_KEY=<key>` (from telerik.com → account → NuGet keys) before
  `dotnet restore`.
- **Without this file:** `error NU1101: Unable to find package Telerik.Reporting`.

### 9.2 `API/API.csproj` — four package references

```xml
<PackageReference Include="Telerik.Reporting"                Version="20.2.26.812" />
<PackageReference Include="Telerik.Reporting.OpenXmlRendering" Version="20.2.26.812" />
<PackageReference Include="Telerik.Drawing.Skia"             Version="20.2.26.812" />
<PackageReference Include="Telerik.Licensing"                Version="1.9.2" />
```

| Package | Why it's needed |
|---|---|
| `Telerik.Reporting` | the report object model + `ReportProcessor` (F8, F9) |
| `Telerik.Reporting.OpenXmlRendering` | the `XLSX` / `DOCX` / `PPTX` render formats (the `?format=` options) |
| `Telerik.Drawing.Skia` | `Telerik.Reporting` ships only the drawing *abstraction*; this is the concrete cross-platform (Skia) implementation. Pulls `SkiaSharp 3.119.1` — the **same** version QuestPDF already uses, so no conflict. **Without it:** `DrawingFactoryUnavailableException: Cannot load assembly Telerik.Drawing.Skia` on the first `Unit.Cm(...)` call. |
| `Telerik.Licensing` | activates the DevCraft key at build time so output has no trial watermark |

### 9.3 `API/Extensions/ApplicationServiceExtensions.cs` — the DI binding

```csharp
using API.Reporting;                                                            // line 24
...
services.AddScoped<IPaymentVoucherReportService, TelerikPaymentVoucherReportService>();   // line 91
```

The one line that connects the port (F7) to the adapter (F8). **Without it:** the container
can't build `PaymentReportsController` and every call to *either* payment-report endpoint
500s (the constructor needs both services).

### 9.4 `Dockerfile.vm` — build key, license, fonts

```dockerfile
# build stage
ARG TELERIK_NUGET_KEY
ARG TELERIK_LICENSE
ENV TELERIK_NUGET_KEY=$TELERIK_NUGET_KEY
ENV TELERIK_LICENSE=$TELERIK_LICENSE
COPY ./nuget.config ./                # so restore can see the Telerik feed

# runtime stage
RUN apt-get install -y --no-install-recommends \
      openssl libfontconfig1 libfreetype6 fontconfig fonts-noto-core \
 && fc-cache -f
ARG TELERIK_LICENSE
ENV TELERIK_LICENSE=$TELERIK_LICENSE
```

- `nuget.config` is copied *before* `dotnet restore`.
- Skia measures/rasterizes against **real font files** — QuestPDF ships its own, Telerik does
  not, and the base `aspnet` image has almost none. `fonts-noto-core` covers Latin; an Arabic
  naskh face (Amiri / Cairo / Noto Naskh Arabic) still needs adding before the container render
  of the Arabic voucher looks right.
- Build command gains:
  `--build-arg TELERIK_NUGET_KEY="$TELERIK_NUGET_KEY" --build-arg TELERIK_LICENSE="$(cat ~/.telerik/telerik-license.txt)"`

### 9.5 `docker-compose.vm.yml`

```yaml
build:
  args:
    TELERIK_NUGET_KEY: ${TELERIK_NUGET_KEY:-}
    TELERIK_LICENSE:   ${TELERIK_LICENSE:-}
environment:
  TELERIK_LICENSE:   ${TELERIK_LICENSE:-}     # runtime licence check parity
```

### 9.6 `~/.telerik/telerik-license.txt` — **not in the repo**

Machine-level file (same one the KendoReact DevCraft upgrade created). `Telerik.Licensing`
reads it automatically at build and runtime on a dev box. In Docker there is no such file, so
the `TELERIK_LICENSE` env var (its *contents*) stands in.

---

## 10. The setup, as a bug journey

Each of these was hit, in order, bringing v2 up:

| # | Symptom | Root cause | Fix |
|---|---|---|---|
| 1 | `NU1101: Unable to find package Telerik.Reporting` (restore) | Telerik packages aren't on nuget.org | added `nuget.config` with the private feed + `TELERIK_NUGET_KEY` |
| 2 | `CS0104: 'InvalidOperationException' is an ambiguous reference` (compile) | `API.Middleware` declares its own `InvalidOperationException` | qualified as `System.InvalidOperationException` (F8:35) |
| 3 | `CS0246: The type or namespace name 'LineShape' could not be found` (compile) | shape item types moved namespace between Reporting versions | replaced the header rule with a bottom-bordered `TextBox` (F9:93) |
| 4 | `DrawingFactoryUnavailableException: Cannot load assembly Telerik.Drawing.Skia` (runtime, first `Unit.Cm`) | `Telerik.Reporting` ships only the drawing abstraction | added `Telerik.Drawing.Skia` package |
| 5 | `ArgumentException: You can assign System.String, IImage, System.Drawing.Image, or SKBitmap instances only` (runtime, `PictureBox.Value`) | `PictureBox.Value` doesn't accept `byte[]` | `SKBitmap.Decode(logoPath)` (F9:71) |
| — | works: `GET …/payment-report-v2/O18037 → 200` | | |

---

## 11. Trace it yourself — where to put breakpoints

| Break at | To watch |
|---|---|
| `PaymentReportsController.cs:43` | the incoming `paymentId`, `format` |
| `GeneratePaymentReportPdf.cs:71` | the EF query about to hit MySQL; inspect `result` after |
| `TelerikPaymentVoucherReportService.cs:28` | `data` fully populated, just before the report is built |
| `PaymentVoucherReport.cs:48` | the Telerik drawing backend loads on this line — step over to confirm no exception |
| `PaymentVoucherReport.cs:58` | after `BuildHeader`, before `BuildBody` — `report.Items[0]` has its children |
| `TelerikPaymentVoucherReportService.cs:34` | `result.HasErrors` / `result.DocumentBytes.Length` |
| `EditPaymentForm.tsx:126` | `arrayBuffer.byteLength` — the PDF size that came back |

Frontend: DevTools → Network → the `payment-report-v2/…` request shows the exact URL, the
`Authorization` / `Accept-Language` headers, status, and timing.
