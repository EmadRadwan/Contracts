# SalesOrderForm Integration Guide - Step by Step

## Phase 2: Integration of Section Components

This guide walks you through integrating the 5 new section components into SalesOrderForm.tsx safely and methodically.

## Current Status
- ✅ All 5 section components created
- ⏳ Section components need to be integrated into SalesOrderForm.tsx
- ⏳ Old JSX needs to be replaced with new components

## Integration Steps

### Step 1: Add Imports ✅ DONE
The import statement has already been added:
```tsx
import {
    FormSection,
    OrderHeaderSection,
    CustomerInformationSection,
    OrderItemsSection,
    OrderSummarySection,
} from "./sections";
```

### Step 2: Replace Header Section ✅ DONE
The OrderHeaderSection component has been placed. It moved the header Grid outside the Form.

### Step 3: Replace Customer Information Section (NEXT)

**Location:** Lines ~470-596 (the nested Grid with customer fields)

**What to replace:**
- Customer field (fromPartyId)
- New Customer button
- Currency dropdown
- Add Tax checkbox (create mode only)
- Customer remarks textarea
- Internal remarks textarea

**Replacement code:**
```tsx
<CustomerInformationSection
    formRenderProps={formRenderProps}
    formEditMode={formEditMode}
    onCustomerChange={onCustomerChange}
    showNewCustomer={showNewCustomer}
    setShowNewCustomer={setShowNewCustomer}
    currencies={currencies}
    isTaxLoading={isTaxLoading}
    getTranslatedLabel={getTranslatedLabel}
/>
```

**Code to remove:**
The entire `<Grid item container xs={9} spacing={2}>...</Grid>` block (lines ~470-596)

### Step 4: Replace Order Totals Section

**Location:** Lines ~598-606 (the Grid with totals)

**What to replace:**
- `{memoizedOrderTotals}` display

This will be moved into OrderSummarySection.

### Step 5: Replace Order Items Section

**Location:** Lines ~615-616 (the Grid with items list)

**Replacement code:**
```tsx
<OrderItemsSection
    memoizedSalesOrderItemsList={memoizedSalesOrderItemsList}
    formEditMode={formEditMode}
    getTranslatedLabel={getTranslatedLabel}
/>
```

**Code to remove:**
```tsx
<Grid item xs={10}>
    {memoizedSalesOrderItemsList}
</Grid>
```

### Step 6: Replace Order Summary Section

**Location:** Lines ~620-770 (the Grid with payment method, invoice, payment, billing account info)

**Replacement code:**
```tsx
<OrderSummarySection
    memoizedOrderTotals={memoizedOrderTotals}
    formRenderProps={formRenderProps}
    formEditMode={formEditMode}
    invoiceId={invoiceId}
    paymentId={paymentId}
    finalPaymentMethodTypes={finalPaymentMethodTypes}
    paymentMethodLabel={paymentMethodLabel}
    isOrderApprovedOrBillingAccountPresent={isOrderApprovedOrBillingAccountPresent}
    billingAccount={billingAccount}
    getTranslatedLabel={getTranslatedLabel}
/>
```

**Code to remove:**
The entire `<Grid item container xs={2}...>` block (lines ~620-770)

### Step 7: Keep Form Buttons at Bottom

The Cancel and Create/Update buttons should remain at the bottom of the form (lines ~773-810).

## Detailed Line-by-Line Replacement

### Customer Section Replacement

**FROM (current lines ~459-607):**
```tsx
<Grid
    container
    spacing={1}
    alignItems="center"
    justifyContent={"flex-start"}
    xs={12}
    item
    sx={{paddingLeft: 3}}
>
    <Grid item container xs={9} spacing={2}>
        {/* All the customer fields */}
    </Grid>

    <Grid
        item
        container
        xs={3}
        spacing={2}
        alignItems="flex-end"
    >
        {memoizedOrderTotals}
    </Grid>
</Grid>
```

**TO (replacement):**
```tsx
<Box sx={{ p: 2 }}>
    <CustomerInformationSection
        formRenderProps={formRenderProps}
        formEditMode={formEditMode}
        onCustomerChange={onCustomerChange}
        showNewCustomer={showNewCustomer}
        setShowNewCustomer={setShowNewCustomer}
        currencies={currencies}
        isTaxLoading={isTaxLoading}
        getTranslatedLabel={getTranslatedLabel}
    />
</Box>
```

### Items & Summary Replacement

**FROM (current lines ~609-770):**
```tsx
<Grid
    container
    item
    alignItems={"center"}
    sx={{ml: 2, mt: 3}}
>
    <Grid item xs={10}>
        {memoizedSalesOrderItemsList}
    </Grid>

    <Grid
        item
        container
        xs={2}
        justifyContent={"flex-start"}
        direction={"column"}
    >
        {/* Payment method, invoice, payment, billing account */}
    </Grid>
</Grid>
```

**TO (replacement):**
```tsx
<Box sx={{ p: 2 }}>
    <OrderItemsSection
        memoizedSalesOrderItemsList={memoizedSalesOrderItemsList}
        formEditMode={formEditMode}
        getTranslatedLabel={getTranslatedLabel}
    />

    <OrderSummarySection
        memoizedOrderTotals={memoizedOrderTotals}
        formRenderProps={formRenderProps}
        formEditMode={formEditMode}
        invoiceId={invoiceId}
        paymentId={paymentId}
        finalPaymentMethodTypes={finalPaymentMethodTypes}
        paymentMethodLabel={paymentMethodLabel}
        isOrderApprovedOrBillingAccountPresent={isOrderApprovedOrBillingAccountPresent}
        billingAccount={billingAccount}
        getTranslatedLabel={getTranslatedLabel}
    />
</Box>
```

## Safe Integration Process

### 1. Backup First
```bash
# Create a backup of the original file
cp SalesOrderForm.tsx SalesOrderForm.tsx.backup
```

### 2. One Section at a Time
- Replace ONE section at a time
- Test after each replacement
- Verify form still submits
- Verify all features still work

### 3. Order of Integration
1. ✅ Header (already done)
2. → Customer Info (next)
3. → Items Section
4. → Summary Section
5. → Test everything
6. → Remove old imports
7. → Clean up

### 4. Testing After Each Step
- **Does form load?** Yes/No
- **Can you interact with fields?** Yes/No
- **Does form submit?** Yes/No
- **No TypeScript errors?** Yes/No

## Expected File Size Reduction

| Component | Before | After | Reduction |
|-----------|--------|-------|-----------|
| Total lines | 826 | ~420 | 406 (49%) |
| JSX lines | 326 | ~120 | 206 (63%) |
| Readability | Low | High | ✅ |

## Common Issues & Solutions

### Issue: TypeScript errors after replacement
**Solution:** Ensure all props are passed correctly. Check prop names match interface definitions.

### Issue: Form submission fails
**Solution:** Verify `formRenderProps` is passed to all field-containing sections.

### Issue: Style/layout looks wrong
**Solution:** Check Box/Grid padding and spacing. Sections use proper MUI Grid spacing.

### Issue: Fields not visible
**Solution:** Make sure sections are rendering. Check console for errors. Verify component imports.

## Rollback Strategy

If integration breaks:
1. Restore backup: `cp SalesOrderForm.tsx.backup SalesOrderForm.tsx`
2. Start again with one section at a time
3. Commit to git after each successful replacement

## Files to Modify

**Main file:**
- `SalesOrderForm.tsx` - Add new components, remove old JSX

**No other files need modification** - All components are self-contained.

## Translation Keys

These translation keys should already exist (added to ar.json earlier):
```
order.so.form.customerInfo
order.so.form.customerInfoDesc
order.so.form.items
order.so.form.itemsDesc
order.so.form.summary
order.so.form.summaryDesc
order.so.form.showBillingBalance
general.accountLimit
general.accountBalance
```

If missing, add them to `client-app/src/app/common/messages/ar.json`

## Next: After Integration

Once all sections are integrated:
1. ✅ Test create mode (new order)
2. ✅ Test edit mode (update)
3. ✅ Test view mode (read-only)
4. ✅ Test all modals
5. ✅ Test RTL layout
6. ✅ Test mobile
7. ✅ Remove `.backup` file
8. ✅ Commit to git

## Commands to Run After Integration

```bash
# Type checking
npm run build

# Run tests (if exist)
npm test

# Format code
npm run format
```

## Architecture After Integration

```
SalesOrderForm.tsx (420 lines)
├── Imports (60 lines)
├── State & hooks (150 lines)
├── Event handlers (100 lines)
└── JSX render (110 lines)
    ├── Modal (CreateCustomerModalForm)
    ├── Menu (OrderMenu)
    ├── Paper container
    │   ├── <OrderHeaderSection /> ✅ DONE
    │   ├── <Form>
    │   │   ├── <CustomerInformationSection /> → NEXT
    │   │   ├── <OrderItemsSection />
    │   │   ├── <OrderSummarySection />
    │   │   └── Buttons (Cancel, Create/Update)
    │   └── </Form>
```

## Checklist

### Pre-Integration
- [ ] Backup original file
- [ ] Read this guide completely
- [ ] Have section components ready

### During Integration
- [ ] Step 1: Header - ✅ DONE
- [ ] Step 2: Customer Info - Replace Grid section
- [ ] Step 3: Test and verify
- [ ] Step 4: Items Section - Replace Grid
- [ ] Step 5: Test and verify
- [ ] Step 6: Summary Section - Replace Grid
- [ ] Step 7: Test and verify
- [ ] Step 8: Clean up old imports

### Post-Integration
- [ ] All features work (create/edit/view)
- [ ] Form submits
- [ ] Modals work
- [ ] No TypeScript errors
- [ ] Mobile responsive
- [ ] RTL layout correct
- [ ] Code formatted
- [ ] Git committed

---

**Ready to start?** Follow Step 3 next!  
**Questions?** Check the issues section above.  
**Need help?** Review the section component interfaces.
