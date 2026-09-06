# OrdersList Grid Styling Guide

## Overview
OrdersList now uses common grid styling and filter components from `app/common/grid/`, providing a professional, consistent user interface aligned with the design system.

## Quick Reference

### Filter Types by Column

| Column | Type | Controls | Example |
|--------|------|----------|---------|
| Order Number | Text | Input + Clear | "ORD001" |
| Customer | Text | Input + Clear | "Ahmed" |
| Status | Text | Input + Clear | "approved" |
| Order Date | Date | Operator + Picker + Clear | = 2024-01-15 |
| Amount | Numeric | Operator + Input + Clear | ≥ 5000 |

### Color Palette

From `designSystem.ts`:

| Element | Color | Usage |
|---------|-------|-------|
| Primary | #0066CC | Links, active, focus |
| Primary Dark | #004A99 | Hover, active states |
| Gray 50 | #FAFAFA | Filter row background |
| Gray 100 | #F5F5F5 | Header background |
| Gray 300 | #E0E0E0 | Borders |
| Gray 900 | #212121 | Text primary |
| Background | #FFFFFF | Cell background |
| Hover | #F8F9FA | Row hover effect |
| Selected | #E3F2FD | Selected row background |

## Filter Behavior

### Text Filters
**Used for:** Order Number, Customer Name, Status

**Behavior:**
- Type any text to search
- Uses "contains" (substring matching)
- Case-insensitive
- Click clear button to reset

**Layout:** `[Input field (flexible)] [Clear button (32px)]`

**Example Usage:**
```
Filter: "ahmed"
Matches: "Ahmed Mansour", "Mohammed Ahmed", "AHMED ALI"
```

### Date Filters
**Used for:** Order Date

**Behavior:**
- Pick a date from date picker
- Choose operator: = (on), < (before), ≤, > (after), ≥
- Click clear button to reset

**Layout:** `[Operator (40px)] [Date picker (flexible)] [Clear button (32px)]`

**Operators:**
- `=` - Exact date match
- `<` - Before this date
- `≤` - On or before this date
- `>` - After this date
- `≥` - On or after this date

**Example Usage:**
```
Operator: ≥ (after/on)
Date: 2024-01-15
Results: All orders from Jan 15, 2024 onwards
```

### Numeric Filters
**Used for:** Amount (Grand Total)

**Behavior:**
- Enter a number value
- Choose operator: =, <, ≤, >, ≥
- Click clear button to reset

**Layout:** `[Operator (40px)] [Number input (flexible)] [Clear button (32px)]`

**Operators:**
- `=` - Exact amount
- `<` - Less than
- `≤` - Less than or equal
- `>` - Greater than
- `≥` - Greater than or equal

**Example Usage:**
```
Operator: ≥ (greater than or equal)
Amount: 5000
Results: All orders with amount ≥ 5000
```

## Visual Styling

### Header Row
- Background: Light gray (#F5F5F5)
- Text: Uppercase, semibold, 12px, dark gray (#212121)
- Borders: Light gray (#E0E0E0)
- Padding: 12px vertical, 16px horizontal

### Data Rows
- Background: White (#FFFFFF)
- Text: Regular, 14px, dark gray (#212121)
- Borders: Light gray (#E0E0E0)
- Padding: 12px vertical, 16px horizontal

### Row Hover Effect
- Background: Light blue (#F8F9FA)
- Transition: Smooth 250ms animation
- Cursor: Pointer (on clickable cells)

### Selected Row
- Background: Light blue (#E3F2FD)
- Bottom border: Primary blue (2px, #0066CC)
- Text: Primary blue (#0066CC)

### Filter Row
- Background: Off-white (#FAFAFA)
- Input boxes: White background, light gray borders (#E0E0E0)
- Focus state: Blue border (#0066CC) + light blue shadow
- Border radius: 4px

### Clear Button
- Default: White background, blue text (#0066CC), light border
- Hover: Blue background (#0066CC), white text, blue shadow
- Active: Dark blue background (#004A99)
- Disabled: Gray background (#F5F5F5), gray text (#BDBDBD)

### Pagination
- Background: Light gray (#F5F5F5)
- Link color: Primary blue (#0066CC)
- Active page: Blue background, white text
- Hover state: Light blue background, blue border

## Responsive Design

### Desktop (≥ 768px)
- Header font: 12px
- Cell padding: 12px 16px
- Filter font: 14px
- Full functionality

### Mobile (< 768px)
- Header font: 11px (reduced)
- Cell padding: 8px 12px (reduced)
- Filter font: 12px (reduced)
- Layout remains functional
- Touch-friendly buttons

## Layout & Spacing

### Column Widths
```
Order Number:  150px (fixed)
Type:          Auto (flexible)
Customer:      Auto (flexible)
Amount:        130px (fixed)
Currency:      Auto (flexible)
Order Date:    Auto (flexible)
Status:        Auto (flexible)
```

### Gaps & Spacing
- Filter component gap: 2px
- Cell padding: 12px 16px (desktop), 8px 12px (mobile)
- Header padding: 12px 16px
- Filter row padding: 8px 4px

## Keyboard Navigation

✅ Tab - Navigate through filter inputs
✅ Enter - Apply filter
✅ Escape - Clear focus
✅ Arrow Keys - Navigate table rows
✅ Space - Select/deselect row

## Accessibility Features

✅ Proper ARIA labels
✅ Keyboard navigation support
✅ Sufficient color contrast (WCAG AA)
✅ Focus indicators on interactive elements
✅ Placeholder text for inputs
✅ Semantic HTML structure

## Browser Support

| Browser | Version | Support |
|---------|---------|---------|
| Chrome | Latest | ✅ Full |
| Firefox | Latest | ✅ Full |
| Safari | Latest | ✅ Full |
| Edge | Latest | ✅ Full |
| Mobile Chrome | Latest | ✅ Full |
| Mobile Safari | Latest | ✅ Full |

## Performance

✅ **CSS-Only Styling** - No JavaScript overhead
✅ **Single Stylesheet** - `grid.styles.css` (250+ lines)
✅ **Hardware Acceleration** - Smooth transitions
✅ **No Grid Impact** - Sorting/filtering/pagination work normally
✅ **Minimal Bundle Size** - Pure CSS, no extra dependencies

## Customization

### Override Colors
```css
.kendo-grid-styled .k-header {
    background-color: #2E7D32;  /* Custom green */
    color: #FFFFFF;
}
```

### Override Spacing
```css
.kendo-grid-styled .k-grid-table tbody > tr > td {
    padding: 16px 20px;  /* Larger padding */
}
```

### Add Custom Styles
```css
.my-orders-grid.kendo-grid-styled .k-header {
    border-left: 4px solid #0066CC;  /* Accent border */
}
```

Then apply to grid:
```tsx
<KendoGrid className="kendo-grid-styled my-orders-grid" ...>
```

## Filter Tips & Tricks

### Text Search
- Search is case-insensitive
- Partial matches work ("ahmed" finds "Mohammed Ahmed")
- Use clear button to reset

### Date Filtering
- Pick date from calendar picker
- Use operators to define range
- Example: 2024-01-01 ≤ orderDate ≤ 2024-12-31

### Numeric Filtering
- Supports decimal numbers
- Use operators for comparisons
- Example: amount ≥ 5000

### Combining Filters
- Multiple filters work together (AND logic)
- Example: Customer="Ahmed" AND Amount≥5000

## Common Issues & Solutions

### Issue: Filters not appearing
**Solution:** Ensure `filterable={true}` and `filterCell={XyzFilterCell}` on column

### Issue: Filter scrolling internally
**Solution:** Already fixed - all elements visible, operator/button don't shrink

### Issue: Header scrolling
**Solution:** Already fixed - header has `overflow: hidden`

### Issue: Styling not applied
**Solution:** Verify `className="kendo-grid-styled"` on KendoGrid and CSS import

## Advanced Features

### Sorting with Filters
- Click column header to sort
- Sorting works independently of filters
- Combined: filtered + sorted data

### Pagination with Filters
- Filters apply across all pages
- Page size (6 items/page by default)
- Navigate with pagination controls

### Selection with Filters
- Row selection works with filters
- Select visible (filtered) rows only
- Selection persists across pages

## Related Resources

- **designSystem.ts** - Color and typography definitions
- **IMPLEMENTATION_COMPLETE.md** - What was changed
- **REFACTORING_GUIDE.md** - How changes were made
- **common/grid/README.md** - Filter components reference
- **common/grid/GRID_STYLING_GUIDE.md** - Generic grid guide

---

**Last Updated**: 2024  
**Version**: 1.0  
**Status**: Complete ✅  
