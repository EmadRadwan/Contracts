# Payment Module Migration Plan: Contracts → BusinessOne

**Created**: 2025-12-16
**Status**: Planning Complete - Ready for Implementation

---

## Overview

This document contains the complete analysis of the Payments module in the Contracts project and a migration plan to bring these features into the BusinessOne project.

---

## Contracts Project Structure (Source)

### Frontend Layer

| File | Path |
|------|------|
| PaymentsList.tsx | `client-app/src/features/accounting/payment/dashboard/PaymentsList.tsx` |
| PaymentForm.tsx | `client-app/src/features/accounting/payment/form/PaymentForm.tsx` |
| paymentsApi.ts | `client-app/src/app/store/apis/payment/paymentsApi.ts` |
| paymentsUiSlice.ts | `client-app/src/features/accounting/payment/slice/paymentsUiSlice.ts` |
| PaymentsDailyExcel.tsx | `client-app/src/features/accounting/payment/report/PaymentsDailyExcel.tsx` |
| PaymentsWithDueAmountsList.tsx | `client-app/src/features/accounting/payment/dashboard/PaymentsWithDueAmountsList.tsx` |
| usePayment.ts | `client-app/src/features/accounting/payment/hook/usePayment.ts` |
| useApplyPayment.ts | `client-app/src/features/accounting/payment/hook/useApplyPayment.ts` |

### API Controllers

| Controller | Path |
|------------|------|
| PaymentsController.cs | `API/Controllers/Accounting/PaymentsController.cs` |
| PaymentRecordsController.cs | `API/Controllers/Accounting/PaymentRecordsController.cs` |
| PaymentApplicationsController.cs | `API/Controllers/Accounting/PaymentApplicationsController.cs` |
| PaymentRecordsWithDueStatusController.cs | `API/Controllers/Accounting/PaymentRecordsWithDueStatusController.cs` |

### Application Layer (Queries/Commands)

| File | Purpose |
|------|---------|
| ListPayments.cs | Fetch payments with OData + paymentType filtering |
| ListPaymentsWithDueStatus.cs | Payments with due status OData |
| ListPaymentsDaily.cs | Daily payments query |
| CreatePaymentAndFinAccountTrans.cs | Create payment with financial transaction |
| UpdatePayment.cs | Update payment details |
| SetPaymentStatusToReceived.cs | Status change command |
| CreatePaymentApplication.cs | Create payment application |
| RemovePaymentApplication.cs | Delete payment application |
| PaymentRecord.cs | DTO with 40+ fields |
| PaymentHelperService.cs | Core business logic (1500+ lines) |

### Domain Layer

| File | Path |
|------|------|
| Payment.cs | `Domain/Payment.cs` |

---

## Key Differences: Contracts vs BusinessOne

### 1. Frontend PaymentsList.tsx

| Feature | Contracts | BusinessOne |
|---------|-----------|-------------|
| `paymentType` prop | ✅ Filters incoming/outgoing | ❌ Shows all |
| Redux state management | `setFormEditMode`, `setPaymentType`, `resetForm` | Local state `setEditMode` |
| Daily Excel Export | ✅ `PaymentsDailyExcel` component | ❌ Missing |
| Grid columns | `orderId`, `certificateNumber`, `projectName`, `costCenterDescription` | Credit card columns |
| Status mapping | Uses `statusId` constants | Uses `statusDescriptionEnglish` strings |

### 2. API paymentsApi.ts

| Endpoint | Contracts | BusinessOne |
|----------|-----------|-------------|
| `fetchPayments` | Includes `paymentType` parameter | No filtering |
| `fetchPaymentsWithDueStatus` | ✅ OData endpoint | ❌ Missing |
| `fetchDailyPaymentsLazy` | ✅ Daily reports | ❌ Missing |
| `createPaymentApplication` | ✅ Mutation | ❌ Missing |
| Tag caching | Sophisticated `PaymentApplications` tags by `paymentId` | Basic tags |

### 3. Backend PaymentsController.cs

| Endpoint | Contracts | BusinessOne |
|----------|-----------|-------------|
| `createPaymentApplication` | ✅ POST | ❌ Missing |
| `daily` | ✅ GET with paymentType | ❌ Missing |

### 4. Application Layer - ListPayments.cs

| Feature | Contracts | BusinessOne |
|---------|-----------|-------------|
| PaymentType filter | ✅ `incoming`/`outgoing` | ❌ No filter |
| LEFT JOINs | OrderPaymentPreference, OrderHeaders, WorkEfforts, CostCenters | CreditCards only |
| PaymentRecord fields | +10 additional fields | Credit card fields |

### 5. PaymentRecord.cs Fields

| Field | Contracts | BusinessOne |
|-------|-----------|-------------|
| `OrderId` | ✅ | ❌ |
| `CertificateNumber` | ✅ | ❌ |
| `ChequeNumber` | ✅ | ❌ |
| `ChequeDate` | ✅ | ❌ |
| `ProjectId` | ✅ | ❌ |
| `ProjectName` | ✅ | ❌ |
| `CostCenterId` | ✅ | ❌ |
| `CostCenterDescription` | ✅ | ❌ |
| `DaysUntilDue` | ✅ | ❌ |
| `CreditCardNumber` | ❌ | ✅ |
| `CreditCardExpiryDate` | ❌ | ✅ |

---

## Migration Plan

### Phase 1: Domain/Models Layer

1. **Update Payment.cs (Domain)** - Add missing fields:
   - `ChequeNumber`, `ChequeDate`, `WorkEffortId`, `CostCenterId`
   - Verify navigation properties exist

2. **Update PaymentRecord.cs (Application)** - Add:
   ```csharp
   public string? CertificateNumber { get; set; }
   public string? OrderId { get; set; }
   public string? ChequeNumber { get; set; }
   public DateTime? ChequeDate { get; set; }
   public string? ProjectId { get; set; }
   public string? ProjectName { get; set; }
   public string? CostCenterId { get; set; }
   public string? CostCenterDescription { get; set; }
   public int DaysUntilDue { get; set; }
   ```

### Phase 2: Application Layer - Queries/Commands

3. **Update ListPayments.cs** - Add:
   - `PaymentType` parameter for incoming/outgoing filtering
   - LEFT JOINs for OrderPaymentPreference, OrderHeaders, WorkEfforts, CostCenters
   - Map new fields in PaymentRecord projection

4. **Create ListPaymentsWithDueStatus.cs** - Copy from Contracts

5. **Create ListPaymentsDaily.cs** - Copy from Contracts

6. **Update CreatePaymentApplication.cs** - Ensure it matches Contracts version

### Phase 3: API Controllers

7. **Update PaymentsController.cs** - Add endpoints:
   ```csharp
   [HttpPost("createPaymentApplication")]
   public async Task<IActionResult> CreatePaymentApplication([FromBody] PaymentApplicationParam param)
   {
       var result = await Mediator.Send(new CreatePaymentApplication.Command { Param = param });
       if (!result.IsSuccess)
       {
           return BadRequest(result.Error);
       }
       return Ok(result.Value);
   }

   [HttpGet("daily")]
   public async Task<ActionResult<PaymentsDailyResponse>> GetDailyPayments(
       [FromQuery] string paymentType,
       CancellationToken ct = default)
   {
       var language = GetLanguage();
       var query = new ListPaymentsDaily.Query
       {
           PaymentType = paymentType,
           Language = language
       };
       var result = await Mediator.Send(query, ct);
       return Ok(result);
   }
   ```

8. **Update PaymentRecordsController.cs** - Add paymentType parameter:
   ```csharp
   [HttpGet]
   [EnableQuery]
   public async Task<IActionResult> Get(ODataQueryOptions<PaymentRecord> options, [FromQuery] string? paymentType = null)
   {
       var language = GetLanguage();
       var query = await Mediator.Send(new ListPayments.Query { Options = options, Language = language, PaymentType = paymentType });
       return await HandleODataQueryAsync(query, options);
   }
   ```

9. **Create PaymentRecordsWithDueStatusController.cs** - Copy from Contracts

### Phase 4: Frontend - API Layer

10. **Update paymentsApi.ts** - Add:
    ```typescript
    interface PaymentQueryArgs extends State {
        paymentType?: 'incoming' | 'outgoing';
    }

    // Update fetchPayments to include paymentType
    fetchPayments: builder.query<ListResponse<Payment>, PaymentQueryArgs>({
        providesTags: ['Payments'],
        query: (queryArgs) => {
            const baseUrl = `/odata/paymentRecords?count=true&${toODataString(queryArgs)}`;
            const paymentTypeParam = queryArgs.paymentType ? `&paymentType=${queryArgs.paymentType}` : '';
            return {
                url: `${baseUrl}${paymentTypeParam}`,
                method: 'GET',
            };
        },
        // ... rest
    }),

    // Add new endpoints
    fetchPaymentsWithDueStatus: builder.query<ListResponse<PaymentWithDueStatus>, PaymentQueryArgs>({
        providesTags: ['Payments'],
        query: (queryArgs) => {
            const baseUrl = `/odata/paymentRecordsWithDueStatus?count=true&${toODataString(queryArgs)}`;
            return { url: baseUrl, method: 'GET' };
        },
        transformResponse: (response: any, meta, arg) => {
            const { totalCount } = JSON.parse(meta!.response!.headers.get('count')!);
            return { data: response, total: totalCount };
        },
    }),

    createPaymentApplication: builder.mutation<PaymentApplicationParam, PaymentApplicationParam>({
        query: (body) => ({
            url: "/payments/createPaymentApplication",
            method: "POST",
            body,
        }),
        invalidatesTags: (result, error, arg) => [
            { type: 'PaymentApplications', id: arg.paymentId },
            { type: 'NotAppliedInvoices', id: arg.paymentId },
            "Payments"
        ],
    }),

    fetchDailyPaymentsLazy: builder.query<
        { data: PaymentRecordDto[]; total: number },
        { paymentType: 'incoming' | 'outgoing' }
    >({
        query: ({ paymentType }) => ({
            url: `/payments/daily`,
            method: 'GET',
            params: { paymentType },
        }),
        providesTags: ['DailyPayments'],
    }),
    ```

11. **Update tag caching** - Add to tagTypes:
    ```typescript
    tagTypes: ["Payments", "PaymentApplications", "NotAppliedInvoices", "DailyPayments"],
    ```

### Phase 5: Frontend - UI Components

12. **Update PaymentsList.tsx** - Major changes:
    - Add `paymentType` prop interface
    - Replace local `editMode` state with Redux `formEditMode`
    - Update `useFetchPaymentsQuery` to pass `paymentType`
    - Add missing grid columns (orderId, certificateNumber, projectName, costCenterDescription)
    - Update status mapping to use `statusId` instead of `statusDescriptionEnglish`
    - Add `PaymentsDailyExcel` component integration

13. **Update paymentsUiSlice.ts** - Add `resetForm` action if missing

14. **Create PaymentsDailyExcel.tsx** - Copy from Contracts

15. **Update PaymentForm.tsx** - Align with Contracts version:
    - Add ChequeNumber/ChequeDate fields
    - Add Project/CostCenter fields
    - Update PAYMENT_TYPE_FILTERS constants

### Phase 6: Supporting Files

16. **Copy/merge additional files**:
    - `PaymentsWithDueAmountsList.tsx`
    - `CreateCostCenterModal.tsx`
    - Related report files

17. **Update models/types** - Ensure TypeScript interfaces match

---

## File-by-File Migration Checklist

| Source (Contracts) | Target (BusinessOne) | Action | Status |
|--------------------|---------------------|--------|--------|
| `PaymentsList.tsx` | `PaymentsList.tsx` | Update/Merge | ⬜ |
| `PaymentForm.tsx` | `PaymentForm.tsx` | Update/Merge | ⬜ |
| `paymentsApi.ts` | `paymentsApi.ts` | Add endpoints | ⬜ |
| `paymentsUiSlice.ts` | `paymentsUiSlice.ts` | Add resetForm | ⬜ |
| `PaymentsDailyExcel.tsx` | Create new | Copy | ⬜ |
| `PaymentsWithDueAmountsList.tsx` | Create new | Copy | ⬜ |
| `PaymentsController.cs` | `PaymentsController.cs` | Add endpoints | ⬜ |
| `PaymentRecordsController.cs` | `PaymentRecordsController.cs` | Add paymentType param | ⬜ |
| `PaymentRecordsWithDueStatusController.cs` | Create new | Copy | ⬜ |
| `ListPayments.cs` | `ListPayments.cs` | Add filters + joins | ⬜ |
| `ListPaymentsWithDueStatus.cs` | Create new | Copy | ⬜ |
| `ListPaymentsDaily.cs` | Create new | Copy | ⬜ |
| `PaymentRecord.cs` | `PaymentRecord.cs` | Add fields | ⬜ |
| `PaymentHelperService.cs` | `PaymentHelperService.cs` | Verify parity | ⬜ |

---

## API Endpoints Summary

### Contracts Payments API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/odata/paymentRecords` | List payments with OData + paymentType filter |
| GET | `/odata/paymentRecordsWithDueStatus` | Payments with due status |
| GET | `/payments/{orderId}/getPaymentsForOrder` | Payments for order |
| GET | `/payments/getPaymentsMethods` | Payment methods |
| GET | `/payments/{partyId}/getPaymentsMethodsByPartyId` | Payment methods by party |
| GET | `/payments/getPaymentsTypesIncoming` | Incoming payment types |
| GET | `/payments/getPaymentsTypesOutgoing` | Outgoing payment types |
| GET | `/payments/getPaymentsIncoming` | Incoming payments list |
| GET | `/payments/getPaymentsOutgoing` | Outgoing payments list |
| GET | `/payments/getPaymentsTypes` | All payment types |
| GET | `/payments/{invoiceId}/getPaymentsApplicationsForInvoice` | Payment applications for invoice |
| GET | `/payments/{paymentId}/getPaymentsApplicationsForPayment` | Payment applications for payment |
| GET | `/payments/daily` | Daily payments by type |
| POST | `/payments/createPaymentAndFinAccountTrans` | Create payment + fin account trans |
| POST | `/payments/createPaymentApplication` | Create payment application |
| POST | `/payments/CalculatePaymentTotals` | Calculate totals |
| PUT | `/payments/updatePayment` | Update payment |
| PUT | `/payments/setPaymentStatusToReceived` | Change status |
| PUT | `/payments/updateOrApproveSalesOrderPayments` | Update/approve SO payments |
| PUT | `/payments/completeSalesOrderPayments` | Complete SO payments |
| DELETE | `/paymentApplications/{paymentApplicationId}` | Delete payment application |

---

## Notes

- Both projects use the same tech stack: .NET 8, MediatR, OData, React, RTK Query, Kendo UI
- The PaymentHelperService is similar in both projects but Contracts has additional fields mapped
- Database schema should be verified - ensure CostCenter, WorkEffort tables exist in BusinessOne
- Consider running both projects side-by-side during migration for comparison testing

---

## Next Steps

1. Start with Phase 1 (Domain layer) to ensure database compatibility
2. Work bottom-up: Domain → Application → API → Frontend
3. Test each phase before moving to the next
4. Use feature flags if needed for gradual rollout
