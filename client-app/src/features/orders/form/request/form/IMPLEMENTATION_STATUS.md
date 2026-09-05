# SalesRequestForm Refactoring - Implementation Status

## ✅ Completed

### New Components Created
- [x] FormSection.tsx - Collapsible section wrapper with icons
- [x] ProductDetailsSection.tsx - Product/apartment, sale date, employee, project
- [x] AssetSpecificationsSection.tsx - Dimensions and building info
- [x] FinancialTermsSection.tsx - Pricing and payment fields
- [x] CustomerInformationSection.tsx - Customer details and comments
- [x] Translation keys added to ar.json

### SalesRequestForm Updated
- [x] Added imports for new section components
- [x] Replaced old sections with new ones in form render
- [x] Commented out old sections for easy rollback
- [x] Kept FormActionsSection unchanged (still functional)

## 🔧 Fixes Needed (Priority Order)

### 1. Fix Form Component Imports in New Sections
The new section components reference form components that may not exist. Need to verify:

**In ProductDetailsSection.tsx:**
- [ ] `FormDatePicker` - verify this component exists or use Kendo DatePicker
- [ ] `FormComboBox` - verify this component exists or use Kendo ComboBox
- [ ] `FormNumericTextBox` - verify this component exists or use Kendo NumericTextBox
- [ ] `FormTextBox` - verify this component exists or use Kendo TextBox

**Files to check:**
```
client-app/src/app/common/form/
```

### 2. Update Section Components with Correct Form Components
Once verified, update each section to use the correct Kendo form components.

**Example mapping (verify these exist):**
```typescript
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import FormTextBox from "../../../../../app/common/form/FormTextBox";
import FormComboBox from "../../../../../app/common/form/FormComboBox";
import FormDatePicker from "../../../../../app/common/form/FormDatePicker";
```

### 3. Add Missing Fields
Some section components are missing field bindings that exist in the form:

**ProductDetailsSection:**
- [ ] Add `partyInputRef` integration for customer selection
- [ ] Ensure customer/party combo works properly

**FinancialTermsSection:**
- [ ] Add `monthsBetweenInstallments` field
- [ ] Add `showCalculatorModal` prop handling (may need payment plan section)

**CustomerInformationSection:**
- [ ] Map `customerIdPassport` to proper field name
- [ ] Map `contactPhone` to proper field name
- [ ] Ensure these fields exist in form initial values

### 4. Payment Plan Section
- [ ] Create PaymentPlanSection.tsx to wrap the payment/installments section
- [ ] This section includes:
  - Number of installments
  - Months between installments
  - First installment date
  - Payment plan modal trigger
  - Custom installments handling

### 5. Testing & Validation
- [ ] Form render without errors
- [ ] All sections collapsible/expandable
- [ ] Form values update correctly
- [ ] Calculations still work (price, advance, discount)
- [ ] Payment plan modal opens
- [ ] Form submission works
- [ ] RTL layout (test in Arabic)
- [ ] Mobile responsiveness

## 🚨 Known Issues to Address

### Issue 1: Missing Form Component Definitions
The new sections import form components that may not be available. These need to be replaced with actual Kendo components or verified to exist.

**Action:** Check `client-app/src/app/common/form/` directory and update imports in:
- ProductDetailsSection.tsx
- AssetSpecificationsSection.tsx
- FinancialTermsSection.tsx
- CustomerInformationSection.tsx

### Issue 2: Missing Field Mappings
Some fields used in the new sections may not be properly mapped in the form's initial values.

**Fields to verify exist in SalesRequest model and form initial values:**
- `customerIdPassport` 
- `contactPhone`
- `monthsBetweenInstallments`
- `buildingNumber`

### Issue 3: Missing Payment Plan Section
The payment plan/installments fields are not yet organized into a new section. This section needs to be created similar to the others.

## 📋 Rollback Instructions (If Needed)

To rollback to the old layout:

1. In `SalesRequestForm.tsx` around line 734:
   - Uncomment the old `ApartmentHeaderSection`, `PricingSection`, and `PaymentFieldsSection`
   - Comment out or remove the new section components

2. The form will immediately revert to the previous layout

3. All old component imports are still available and haven't been deleted

## 🎯 Next Immediate Steps

1. **Verify Form Components** - Check what form component wrappers actually exist in the project
2. **Fix Imports** - Update new section components to use actual available components
3. **Add Missing Fields** - Add any fields missing from form initial values
4. **Create PaymentPlanSection** - Complete the refactoring with payment section
5. **Test** - Verify form works end-to-end
6. **Polish** - Fine-tune styling and spacing for RTL

## 💡 Testing Checklist

- [ ] Form loads without console errors
- [ ] All 4 sections render and are collapsible
- [ ] Can enter data in all fields
- [ ] Calculations update correctly
- [ ] Payment plan modal opens from "Edit Payment Plan" button
- [ ] Form submission works
- [ ] Test with Arabic (RTL layout)
- [ ] Test on mobile (responsive design)
- [ ] Rollback to old layout works if needed

## 📞 Support

If you encounter issues:
1. Check the console for error messages
2. Verify all form component imports exist
3. Check form field names match initial values
4. Use rollback instructions above if critical errors occur
