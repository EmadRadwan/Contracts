# Common Grid & Table Components

This directory contains reusable Kendo Grid styling and filter components that can be applied across the entire application for consistent, professional table UI/UX.

## Quick Start

### 1. Import in Your Component
```tsx
import { TextFilterCell, DateFilterCell, NumericFilterCell } from '../app/common/grid';
import '../app/common/grid/grid.styles.css';
```

### 2. Add the Styling Class
```tsx
<KendoGrid className="kendo-grid-styled" ...>
  {/* columns */}
</KendoGrid>
```

### 3. Configure Columns with Filter Cells
```tsx
<Column field="name" filterable={true} filter="text" filterCell={TextFilterCell} />
<Column field="date" filterable={true} filter="date" filterCell={DateFilterCell} />
<Column field="amount" filterable={true} filter="numeric" filterCell={NumericFilterCell} />
```

## Files in This Directory

### Components
- **TextFilterCell.tsx** - Text filter with "contains" operator (no dropdown)
- **DateFilterCell.tsx** - Date filter with operator dropdown (≥, ≤, =)
- **NumericFilterCell.tsx** - Numeric filter with operator dropdown (=, <, ≤, >, ≥)

### Styling
- **grid.styles.css** - Complete Kendo Grid styling (250+ lines)
  - Header, data rows, hover effects, selected rows
  - Status badges (pending, approved, rejected, cancelled)
  - Filter row, pagination, empty state
  - Responsive design for mobile

### Documentation
- **index.ts** - Barrel export for easy imports
- **GRID_STYLING_GUIDE.md** - Detailed usage guide with examples
- **IMPLEMENTATION.md** - Summary of implementation details
- **USAGE_EXAMPLE.tsx** - Working code example showing all features
- **README.md** - This file

## Features

✅ **Consistent Styling** - Professional, modern look aligned with design system  
✅ **Smart Filters** - Text (contains), Date (with operators), Numeric (with operators)  
✅ **Responsive** - Mobile-friendly with automatic adjustments  
✅ **Zero Overhead** - Pure CSS, no JavaScript complexity  
✅ **Easy to Use** - Import, apply class, configure columns  
✅ **Customizable** - Override CSS for component-specific needs  
✅ **Documented** - Complete guides and working examples  

## Color Scheme

| Element | Color | Purpose |
|---------|-------|---------|
| Primary | #0066CC | Links, focus, active states |
| Header BG | #F5F5F5 | Column headers |
| Row BG | #FFFFFF | Data rows |
| Hover BG | #F8F9FA | Row hover effect |
| Selected BG | #E3F2FD | Selected row background |
| Border | #E0E0E0 | Cell and input borders |
| Text | #212121 | Default text color |
| Secondary Text | #666666 | Icon and placeholder text |

### Status Badges
- **Pending**: Orange (#FF9800) background, light orange (#FFF3E0)
- **Approved**: Green (#4CAF50) background, light green (#E8F5E9)
- **Rejected**: Red (#D32F2F) background, light red (#FFEBEE)
- **Cancelled**: Red (#D32F2F) background, light red (#FFEBEE)

## Current Usage

### OrdersList (✅ Implemented)
All columns now have appropriate filters:
- `orderId` - Text filter
- `fromPartyName` - Text filter
- `orderDate` - Date filter
- `grandTotal` - Numeric filter
- `statusDescription` - Text filter

See: `client-app/src/features/orders/dashboard/order/OrdersList.tsx`

## Apply to Other Components

To apply this styling to another Kendo Grid:

1. **Find your list component** (e.g., `VehiclesList.tsx`, `ProjectsList.tsx`)
2. **Add imports** at the top:
   ```tsx
   import { TextFilterCell, DateFilterCell, NumericFilterCell } from '../app/common/grid';
   import '../app/common/grid/grid.styles.css';
   ```
3. **Add className** to KendoGrid:
   ```tsx
   <KendoGrid className="kendo-grid-styled" ...>
   ```
4. **Update columns** with appropriate filter cells:
   ```tsx
   <Column ... filterable={true} filter="text" filterCell={TextFilterCell} />
   <Column ... filterable={true} filter="date" filterCell={DateFilterCell} />
   <Column ... filterable={true} filter="numeric" filterCell={NumericFilterCell} />
   ```

## Customization

### Override Styling for a Specific Component
```tsx
// In your component's CSS file
.my-grid.kendo-grid-styled .k-header {
    background-color: #2E7D32;  /* Custom green */
    color: #FFFFFF;
}
```

Then apply to the grid:
```tsx
<KendoGrid className="kendo-grid-styled my-grid" ...>
```

### Create a Custom Filter
```tsx
const MyCustomFilter = (props: GridFilterCellProps) => {
    return (
        <div className="k-filtercell">
            {/* Your custom filter UI */}
        </div>
    );
};

// Use in column
<Column field="myField" filterable={true} filterCell={MyCustomFilter} />
```

## Browser Support

- ✅ Chrome/Edge (latest)
- ✅ Firefox (latest)
- ✅ Safari (latest)
- ✅ Mobile browsers (iOS Safari, Chrome Mobile)

## Performance

- **No JavaScript overhead** - Pure CSS styling
- **Single stylesheet import** - Minimal network impact
- **Minimal CSS specificity** - Easy to override
- **Hardware-accelerated transitions** - Smooth 250ms animations

## Troubleshooting

### Filters not showing
- Ensure `filterable={true}` on the Column
- Check that `filterCell` prop is correctly set
- Verify CSS is imported

### Styling not applied
- Make sure `className="kendo-grid-styled"` is on the KendoGrid element
- Verify `grid.styles.css` is imported
- Check browser DevTools to see if CSS is loaded

### Operator dropdown not appearing
- For text fields, use `TextFilterCell` (no dropdown by design)
- For dates, use `DateFilterCell` (has operator dropdown)
- For numbers, use `NumericFilterCell` (has operator dropdown)

## Related Files

- Design System: `client-app/src/app/theme/designSystem.ts`
- Example Component: `client-app/src/features/orders/dashboard/order/OrdersList.tsx`
- Kendo Grid Docs: https://www.telerik.com/kendo-react-ui/components/grid/

## Future Enhancements

- [ ] Status badge cell component for easy status display
- [ ] Boolean/checkbox filter cell
- [ ] Search cell for combined text and numeric search
- [ ] Dark mode theme variant
- [ ] Column grouping styles
- [ ] Export to Excel/PDF styling

---

**Last Updated**: 2024  
**Status**: Production Ready ✅  
**Maintenance**: CSS-only, zero dependencies beyond Kendo Grid  
