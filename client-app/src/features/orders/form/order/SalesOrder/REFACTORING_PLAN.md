# SalesOrderForm Refactoring Plan

## Overview
Refactor SalesOrderForm to use collapsible sections with proper organization, following the SalesRequestForm pattern.

## Current Structure Issues
1. **Monolithic Component** - 826 lines, deeply nested Grids
2. **Mixed Concerns** - Form fields, items list, totals, modals all in one component
3. **Conditional Rendering Scattered** - Hard to track what renders when
4. **No Section Organization** - Fields scattered across the render tree
5. **Repetitive Props Passing** - formRenderProps, formEditMode passed through many levels

## Target Architecture

### Phase 1: Create Reusable Section Components ✓ PLAN
```
FormSection.tsx (wrapper with Accordion)
├── CustomerInformationSection.tsx
├── OrderItemsSection.tsx
├── OrderSummarySection.tsx
└── PaymentMethodSection.tsx
```

### Phase 2: Extract & Organize
- Move modals outside main render
- Consolidate form field groups
- Create helper functions for conditionals
- Extract calculation logic

### Phase 3: Styling & Design System
- Apply designSystem colors
- Implement RTL support
- Use consistent typography
- Add responsive layout

### Phase 4: Documentation
- Create IMPLEMENTATION_COMPLETE.md
- Create REFACTORING_GUIDE.md
- Document all sections

## Components to Create

### 1. FormSection.tsx (Reusable Wrapper)
```tsx
interface FormSectionProps {
    title: string;
    description?: string;
    icon?: React.ReactNode;
    defaultExpanded?: boolean;
    children: React.ReactNode;
}

// Uses MUI Accordion for expand/collapse
// Full RTL support
// Consistent styling
```

### 2. CustomerInformationSection.tsx
**Fields:**
- Customer Selection (FormComboBoxVirtualCustomer)
- New Customer Button
- Currency Selection (MemoizedFormDropDownList2)
- Add Tax Checkbox (conditional, create mode only)
- Customer Remarks (FormTextArea)
- Internal Remarks (FormTextArea)

**Props:**
- formRenderProps: FormRenderProps
- formEditMode: number
- onCustomerChange: Function
- showNewCustomer: boolean
- setShowNewCustomer: Function
- getTranslatedLabel: Function

### 3. OrderItemsSection.tsx
**Content:**
- SalesOrderItemsList component
- Conditional rendering based on formEditMode

**Props:**
- memoizedSalesOrderItemsList: JSX
- formEditMode: number

### 4. OrderSummarySection.tsx
**Content:**
- Order Totals display
- Payment Method Section (radio group or text display)
- Invoice No link (conditional)
- Payment No link (conditional)
- Billing Account Balance Popover (conditional)

**Props:**
- memoizedOrderTotals: JSX
- formRenderProps: FormRenderProps
- formEditMode: number
- invoiceId?: string
- paymentId?: string
- finalPaymentMethodTypes: any[]
- paymentMethodLabel: string
- isOrderApprovedOrBillingAccountPresent: boolean
- billingAccount: any[]
- getTranslatedLabel: Function

### 5. OrderHeaderSection.tsx
**Content:**
- Title with Order ID or "New Sales Order"
- Status Ribbon (conditional, edit mode > 1)
- Actions Menu
- Quick Ship option (conditional)

**Props:**
- order: any
- formEditMode: number
- status: any
- language: string
- handleMenuSelect: Function
- showQuickShip: boolean
- getTranslatedLabel: Function

## Implementation Steps

### Step 1: Create Section Components
- [ ] FormSection.tsx
- [ ] CustomerInformationSection.tsx
- [ ] OrderItemsSection.tsx
- [ ] OrderSummarySection.tsx
- [ ] OrderHeaderSection.tsx

### Step 2: Update SalesOrderForm.tsx
- [ ] Import all section components
- [ ] Extract header into OrderHeaderSection
- [ ] Extract customer info into CustomerInformationSection
- [ ] Extract items into OrderItemsSection
- [ ] Extract summary into OrderSummarySection
- [ ] Consolidate prop passing
- [ ] Simplify main component

### Step 3: Create Documentation
- [ ] IMPLEMENTATION_COMPLETE.md
- [ ] REFACTORING_GUIDE.md
- [ ] SECTION_COMPONENTS.md

### Step 4: Test & Verify
- [ ] No TypeScript errors
- [ ] All functionality works
- [ ] Form submission works
- [ ] All modals work
- [ ] RTL layout correct
- [ ] Mobile responsive

## Design System Integration

### Colors Used
- Primary: #09419A (from designSystem)
- Success: #2E7D32 (for status ribbons)
- Borders: #E0E0E0
- Text: #212121

### Typography
- Section headers: h5 (20px, semibold)
- Labels: subtitle2 (13px, semibold)
- Body: body2 (14px, regular)

### Spacing
- Section padding: 24px
- Field gap: 16px
- Subsection gap: 12px

## Key Benefits

✅ **Modularity** - Each section is independent
✅ **Maintainability** - Easy to find and modify sections
✅ **Reusability** - FormSection can be used in other forms
✅ **Readability** - Cleaner main component
✅ **Scalability** - Easy to add new sections
✅ **RTL Support** - Built-in from the start
✅ **Design System** - Consistent styling
✅ **Documentation** - Complete guides

## Migration Path

1. Create new section components (non-breaking)
2. Add to SalesOrderForm gradually
3. Keep old structure commented for reference
4. Test thoroughly
5. Remove old code once verified
6. Update documentation

## Estimated Lines of Code

- FormSection.tsx: ~80 lines
- CustomerInformationSection.tsx: ~150 lines
- OrderItemsSection.tsx: ~80 lines
- OrderSummarySection.tsx: ~200 lines
- OrderHeaderSection.tsx: ~120 lines
- Updated SalesOrderForm.tsx: ~400 lines (from 826)

**Total Reduction: 426 lines (51% smaller)**

## Files to Create

```
features/orders/form/order/SalesOrder/
├── sections/
│   ├── FormSection.tsx (NEW)
│   ├── OrderHeaderSection.tsx (NEW)
│   ├── CustomerInformationSection.tsx (NEW)
│   ├── OrderItemsSection.tsx (NEW)
│   └── OrderSummarySection.tsx (NEW)
├── SalesOrderForm.tsx (REFACTORED)
├── REFACTORING_PLAN.md (THIS FILE)
├── IMPLEMENTATION_COMPLETE.md (NEW)
└── REFACTORING_GUIDE.md (NEW)
```

## Next: Start with FormSection.tsx
This is the foundation for all other sections.
