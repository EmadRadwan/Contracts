# Common Grid & Table Styling Guide

## Overview
The `client-app/src/app/common/grid/` directory contains reusable Kendo Grid components and styling that can be applied across the entire application for consistent UI/UX.

## Files Included

### 1. **grid.styles.css**
Comprehensive styling for Kendo Grid data tables with:
- Header row styling (uppercase, semibold, gray background)
- Data row styling with hover effects
- Selected row highlighting
- Status badge styling (pending, approved, rejected, cancelled)
- Filter row styling with focus states
- Pagination controls styling
- Sort/filter icons styling
- Links styling
- Responsive design for mobile (< 768px)

### 2. **TextFilterCell.tsx**
Custom filter component for text-based columns.

**Features:**
- Always uses "contains" operator (no dropdown clutter)
- Simple text input with clear button
- Case-insensitive substring matching

**Usage:**
```tsx
import { TextFilterCell } from '../app/common/grid';

<Column
    field="customerName"
    title="Customer"
    filterable={true}
    filter="text"
    filterCell={TextFilterCell}
/>
```

### 3. **DateFilterCell.tsx**
Custom filter component for date columns.

**Features:**
- Date picker input with dd/mm/yyyy format
- Operator dropdown (≥ After, ≤ Before, = On)
- Clear button

**Usage:**
```tsx
import { DateFilterCell } from '../app/common/grid';

<Column
    field="orderDate"
    title="Order Date"
    format="{0: dd/MM/yyyy}"
    filterable={true}
    filter="date"
    filterCell={DateFilterCell}
/>
```

### 4. **NumericFilterCell.tsx**
Custom filter component for numeric columns.

**Features:**
- Numeric input with validation
- Operator dropdown (=, <, ≤, >, ≥)
- Clear button

**Usage:**
```tsx
import { NumericFilterCell } from '../app/common/grid';

<Column
    field="grandTotal"
    title="Amount"
    filter="numeric"
    filterable={true}
    filterCell={NumericFilterCell}
/>
```

## How to Use in a Component

### Step 1: Import the styling
```tsx
import '../../../app/common/grid/grid.styles.css';
```

### Step 2: Import filter components
```tsx
import { TextFilterCell, DateFilterCell, NumericFilterCell } from '../../../app/common/grid';
```

### Step 3: Add CSS class to KendoGrid
```tsx
<KendoGrid
    className="kendo-grid-styled"
    // ... other props
>
```

### Step 4: Configure columns with filter cells
```tsx
<Column
    field="orderId"
    title="Order Number"
    filterable={true}
    filter="text"
    filterCell={TextFilterCell}
/>

<Column
    field="orderDate"
    title="Order Date"
    format="{0: dd/MM/yyyy}"
    filterable={true}
    filter="date"
    filterCell={DateFilterCell}
/>

<Column
    field="grandTotal"
    title="Amount"
    filterable={true}
    filter="numeric"
    filterCell={NumericFilterCell}
/>
```

## Color Scheme (from designSystem)

| Element | Color | Usage |
|---------|-------|-------|
| Primary | #0066CC | Links, active states, focus borders |
| Primary Dark | #004A99 | Hover states |
| Gray 100 | #F5F5F5 | Header background |
| Gray 300 | #E0E0E0 | Borders |
| Gray 900 | #212121 | Text color |
| Background | #FFFFFF | Cell background |
| Background Hover | #F8F9FA | Row hover effect |
| Background Selected | #E3F2FD | Selected row |

## Status Badge Colors

| Status | Background | Text | Border |
|--------|------------|------|--------|
| PENDING | #FFF3E0 | #FF9800 | #FFB74D |
| APPROVED | #E8F5E9 | #4CAF50 | #81C784 |
| REJECTED | #FFEBEE | #F44336 | #EF5350 |
| CANCELLED | #FFEBEE | #D32F2F | #E53935 |

## Responsive Behavior

The grid automatically adjusts for mobile devices (< 768px):
- Header font size: 11px (from 12px)
- Cell padding: 8px 12px (from 12px 16px)
- Font size: 13px (from 14px)

## Browser Compatibility
- ✅ Chrome/Edge: Full support
- ✅ Firefox: Full support
- ✅ Safari: Full support
- ✅ Mobile Browsers: Responsive styling

## CSS Class Reference

| Class | Purpose |
|-------|---------|
| `.kendo-grid-styled` | Main grid container (apply to KendoGrid) |
| `.k-grid-header` | Header row container |
| `.k-header` | Individual header cell |
| `.k-grid-table tbody > tr` | Data row |
| `.k-grid-table tbody > tr > td` | Data cell |
| `.k-selected` | Selected row |
| `.k-badge` | Status badges |
| `.k-filtercell` | Filter input row |
| `.k-pager` | Pagination controls |

## Applying Status Badge Styling

To add status badges to your columns, use the badge classes in a custom cell component:

```tsx
const StatusCell = (props: any) => {
    const statusMap: Record<string, string> = {
        'ORDER_CREATED': 'badge-pending',
        'ORDER_APPROVED': 'badge-approved',
        'ORDER_COMPLETED': 'badge-approved',
        'ORDER_CANCELLED': 'badge-cancelled',
    };

    const badgeClass = statusMap[props.dataItem.statusId] || '';

    return (
        <td>
            <span className={`k-badge ${badgeClass}`}>
                {props.dataItem.statusDescription}
            </span>
        </td>
    );
};
```

Then use in a column:
```tsx
<Column
    field="statusDescription"
    title="Status"
    cell={StatusCell}
/>
```

## Performance Notes
- Pure CSS styling (no JavaScript overhead)
- Single stylesheet import
- Minimal specificity (easy to override if needed)
- Hardware-accelerated transitions where applicable

## Customization

To customize the styling for your specific component, you can:

1. **Override CSS** - Add component-specific CSS after importing `grid.styles.css`
2. **Use inline styles** - For one-off changes (not recommended for large projects)
3. **Create theme variants** - Duplicate `grid.styles.css` and modify colors/spacing

Example custom override:
```css
/* MyCustomGrid.css */
.my-custom-grid.kendo-grid-styled .k-header {
    background-color: #2E7D32;  /* Custom green header */
    color: #FFFFFF;
}
```

## Common Integration Pattern

```tsx
import { TextFilterCell, DateFilterCell, NumericFilterCell } from '../../../app/common/grid';
import '../../../app/common/grid/grid.styles.css';

export function MyListComponent() {
    return (
        <KendoGrid
            className="kendo-grid-styled"
            // ... other props
        >
            <Column field="name" filterable={true} filter="text" filterCell={TextFilterCell} />
            <Column field="date" filterable={true} filter="date" filterCell={DateFilterCell} />
            <Column field="amount" filterable={true} filter="numeric" filterCell={NumericFilterCell} />
        </KendoGrid>
    );
}
```

## Next Steps

Apply this styling to other list/grid components:
- [ ] VehiclesList
- [ ] ProjectsList
- [ ] InventoryList
- [ ] Any other Kendo Grid-based list
