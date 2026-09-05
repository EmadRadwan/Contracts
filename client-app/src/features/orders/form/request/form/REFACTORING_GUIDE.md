# SalesRequestForm Refactoring Guide

## Overview
This guide explains how to integrate the new collapsible section components into the existing SalesRequestForm to match the design specification with full RTL support and localization.

## New Components Created

### 1. **FormSection.tsx**
- Reusable wrapper component for collapsible sections
- Uses MUI Accordion for expand/collapse functionality
- Features:
  - Icon support (pass MUI icon as prop)
  - Section title and description
  - Tooltip support for info icons
  - Full RTL support via MUI's built-in RTL handling
  - Default expanded state control

### 2. **ProductDetailsSection.tsx**
- Product/Apartment selection
- Sale date picker
- Assigned employee selection
- Project code field
- RTL-aware grid layout (3-4 columns)
- All fields with proper labels and localization

### 3. **AssetSpecificationsSection.tsx**
- Apartment area (m²)
- Garden/Terrace area (m²)
- Building number
- Unit floor
- 4-column grid layout
- All numeric fields with format support

### 4. **FinancialTermsSection.tsx**
- Price per m² (with calculator integration)
- Total contract value
- Advance payment
- Number of installments
- Discount percentage
- Maintenance deposit
- First installment date
- Cheques delivered checkbox
- 4-column grid layout
- Read-only total field

### 5. **CustomerInformationSection.tsx**
- Customer/Party selection with combo
- Customer ID/Passport field
- Contact phone
- Additional comments (textarea)
- Collapsed by default for cleaner initial view
- 3-4 column layout

## How to Update SalesRequestForm.tsx

### Step 1: Import the New Sections
```tsx
import { ProductDetailsSection } from "./ProductDetailsSection";
import { AssetSpecificationsSection } from "./AssetSpecificationsSection";
import { FinancialTermsSection } from "./FinancialTermsSection";
import { CustomerInformationSection } from "./CustomerInformationSection";
import { FormSection } from "./FormSection";
```

### Step 2: Remove Old Section Components
Remove imports for: `ApartmentHeaderSection`, `PricingSection`, `PaymentFieldsSection` (these have been consolidated into the new sections)

### Step 3: Update the Form Render
Replace the old section components with new ones in the form's render method:

```tsx
<FormElement>
  <fieldset className="k-form-fieldset">
    
    <ProductDetailsSection
      formRenderProps={formRenderProps}
      selectedApartment={selectedApartment}
      onProductChange={onProductChange}
      showNewCustomer={showNewCustomer}
      setShowNewCustomer={setShowNewCustomer}
      getTranslatedLabel={getTranslatedLabel}
      partyInputRef={partyInputRef}
      editMode={editMode}
    />
    
    <AssetSpecificationsSection
      formRenderProps={formRenderProps}
      getTranslatedLabel={getTranslatedLabel}
    />
    
    <FinancialTermsSection
      formRenderProps={formRenderProps}
      onPricePerM2Change={handlePricePerM2Change}
      onDiscountChange={handleDiscountChange}
      autoSetDerivedFields={autoSetDerivedFields}
      getTranslatedLabel={getTranslatedLabel}
    />
    
    <CustomerInformationSection
      formRenderProps={formRenderProps}
      getTranslatedLabel={getTranslatedLabel}
    />
    
    {/* Payment Plan Section - keep using FormSection wrapper */}
    <PaymentPlanSection
      formRenderProps={formRenderProps}
      customInstallments={customInstallments}
      onOpenPaymentPlan={onOpenPaymentPlan}
      getTranslatedLabel={getTranslatedLabel}
    />
    
  </fieldset>
</FormElement>
```

## Translation Keys Added to ar.json

All new sections have translation keys added:
- `salesRequest.form.productDetails` - Section title
- `salesRequest.form.productDetailsDesc` - Section description
- `salesRequest.form.assetSpecifications` - Section title
- `salesRequest.form.assetSpecificationsDesc` - Section description
- `salesRequest.form.financialTerms` - Section title
- `salesRequest.form.financialTermsDesc` - Section description
- `salesRequest.form.customerInfo` - Section title
- `salesRequest.form.customerInfoDesc` - Section description

And field labels:
- `salesRequest.form.productSelection`
- `salesRequest.form.projectCode`
- `salesRequest.form.assignedEmployee`
- `salesRequest.form.apartmentM2`
- `salesRequest.form.gardenM2`
- `salesRequest.form.buildingNumber`
- `salesRequest.form.unitFloor`
- `salesRequest.form.pricePerM2`
- `salesRequest.form.totalPrice`
- `salesRequest.form.advancePayment`
- `salesRequest.form.numberOfInstallments`
- `salesRequest.form.discount`
- `salesRequest.form.maintenanceDeposit`
- `salesRequest.form.firstInstallmentDate`
- `salesRequest.form.chequesDelivered`
- `salesRequest.form.fullLegalName`
- `salesRequest.form.customerId`
- `salesRequest.form.contactPhone`
- `salesRequest.form.additionalComments`

## RTL Support Features

All components include:
1. **MUI Accordion RTL Support** - Automatically handles direction in FormSection
2. **Direction-Aware Spacing** - Grid layouts respect text direction
3. **FlexBox Alignment** - All flex layouts work in both LTR and RTL
4. **Icon Alignment** - Section icons positioned correctly for RTL
5. **Text Alignment** - All labels inherit document direction

## Remaining Tasks

1. **Create PaymentPlanSection** - Wrap the payment plan/installments section
2. **Update FormActionsSection** - Button alignment for RTL
3. **Test RTL Layout** - Verify all sections display correctly in Arabic
4. **Move Existing Logic** - Move calculation functions from old sections to new ones
5. **Remove Old Component Files** - Clean up ApartmentHeaderSection, PricingSection, PaymentFieldsSection
6. **Add Form Header** - Keep SalesRequestHeader as is
7. **Update Modal Imports** - Ensure all modals are still accessible

## Field Mapping Reference

### Old → New Location
| Old Component | Old Field | New Component | Notes |
|---|---|---|---|
| ApartmentHeaderSection | productId | ProductDetailsSection | Party selection still in form |
| ApartmentHeaderSection | saleDate | ProductDetailsSection | ✓ |
| ApartmentHeaderSection | employeePartyId | ProductDetailsSection | ✓ |
| PricingSection | apartmentPricePerM2 | FinancialTermsSection | ✓ |
| PricingSection | totalPrice | FinancialTermsSection | Read-only, derived ✓ |
| PricingSection | discount | FinancialTermsSection | ✓ |
| PricingSection | advancePayment | FinancialTermsSection | ✓ |
| PaymentFieldsSection | numberOfInstallments | FinancialTermsSection | ✓ |
| PaymentFieldsSection | monthsBetweenInstallments | PaymentPlanSection | To be created |
| Form Fields | apartmentSpaceM2, gardenSpaceM2 | AssetSpecificationsSection | ✓ |
| Form Fields | buildingNumber, floorNumber | AssetSpecificationsSection | ✓ |
| Form Fields | comments | CustomerInformationSection | ✓ |

## Styling Consistency

All sections use:
- `designSystem.colors.*` for colors
- `designSystem.typography.*` for fonts
- `designSystem.borderRadius.*` for borders
- `designSystem.shadow.*` for shadows
- Consistent spacing (margins and paddings)
- MUI Grid system for responsive layouts

## Benefits of This Refactoring

✅ **Better UX** - Collapsible sections reduce cognitive load
✅ **Cleaner Layout** - Matches professional design
✅ **RTL Ready** - Full bidirectional text support
✅ **Fully Localized** - All text via translation system
✅ **Maintainable** - Separated concerns in components
✅ **Responsive** - Works on all screen sizes
✅ **Accessible** - Proper ARIA labels and keyboard navigation

## Testing Checklist

- [ ] All sections expand/collapse correctly
- [ ] Form values persist when switching sections
- [ ] Calculations work (price, advance, discount)
- [ ] RTL layout renders correctly (test with Arabic)
- [ ] All translation keys display
- [ ] Payment plan modal still opens correctly
- [ ] Form submission works with new layout
- [ ] Mobile responsiveness (xs, sm, md breakpoints)
- [ ] Keyboard navigation works
- [ ] Field validators still function

## Notes

- Keep existing calculation logic unchanged
- Payment plan modal integration stays the same
- SalesRequestHeader component remains unchanged
- FormActionsSection may need minor updates for button alignment
- All Kendo components work as before, just reorganized
