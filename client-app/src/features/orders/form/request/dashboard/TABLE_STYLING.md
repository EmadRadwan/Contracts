# Sales Requests Table Styling Guide

## Overview
The Kendo Grid data table now includes comprehensive styling aligned with the design system. All styles are applied via CSS while maintaining 100% of the original functionality.

## Styling Implementation

### File Structure
- **SalesRequestsList.styles.css** - Complete table styling rules
- **SalesRequestsList.tsx** - Component with CSS import and className

### Applied Styling

#### Header Row
- **Background**: Light gray (#F5F5F5)
- **Text**: Uppercase labels, semibold weight
- **Spacing**: 12px vertical, 16px horizontal padding
- **Borders**: Light gray (#E0E0E0) bottom and right borders
- **Font Size**: 12px with 0.5px letter spacing

#### Data Rows
- **Background**: White (#FFFFFF)
- **Text**: 14px, regular weight, dark gray (#212121)
- **Spacing**: 12px vertical, 16px horizontal padding
- **Borders**: Light gray (#E0E0E0) between columns
- **Hover Effect**: Light blue background (#F8F9FA) with smooth transition

#### Selected Rows
- **Background**: Light blue (#E3F2FD)
- **Text Color**: Primary blue (#0066CC)
- **Bottom Border**: Primary blue highlight

#### Status Badges (Inline)
Built-in support for colorized status badges:

| Status | Background | Text Color | Border |
|--------|------------|-----------|--------|
| **PENDING** | #FFF3E0 | #FF9800 | #FFB74D |
| **APPROVED** | #E8F5E9 | #4CAF50 | #81C784 |
| **REJECTED** | #FFEBEE | #D32F2F | #E53935 |
| **CANCELLED** | #FFEBEE | #D32F2F | #E53935 |

#### Filter Row
- **Background**: Off-white (#FAFAFA)
- **Input Boxes**: White background with light borders
- **Focus State**: Primary blue border (#0066CC) with light blue shadow
- **Border Radius**: 4px

#### Pagination Controls
- **Background**: Light gray (#F5F5F5)
- **Link Color**: Primary blue (#0066CC)
- **Active Page**: Blue background with white text
- **Hover State**: Light blue background with blue border

#### Sort/Filter Icons
- **Default Color**: Gray (#666666)
- **Hover Color**: Primary blue (#0066CC)

#### Links (Request IDs)
- **Color**: Primary blue (#0066CC)
- **Hover**: Darker blue (#004A99) with underline
- **Transition**: Smooth 250ms transition

#### Empty State
- **Message Color**: Light gray (#999999)
- **Font Size**: 14px
- **Padding**: 32px vertical, 16px horizontal

### Responsive Design
Mobile adjustments (screens < 768px):
- **Padding Reduced**: 8px vertical, 12px horizontal
- **Font Size**: Slightly reduced for compact view
- **Header Font Size**: 11px

## Design System Alignment

All colors used are from `designSystem.ts`:
- Primary: #0066CC
- Secondary: #28A745
- Gray Scale: #F5F5F5 → #212121
- Borders: #E0E0E0
- Backgrounds: #FFFFFF, #FAFAFA

## Functionality Preserved
✅ Sorting - Header icons remain functional
✅ Filtering - Filter row fully operational
✅ Pagination - Page navigation styled but functional
✅ Selection - Row selection with visual feedback
✅ Column Resizing - Resize handles visible
✅ All Data Binding - Display remains unchanged

## Browser Compatibility
- Chrome/Edge: Full support
- Firefox: Full support
- Safari: Full support
- Mobile Browsers: Responsive styling applied

## Future Enhancements
- Dark mode theme variant
- Striped row alternating colors
- Column drag-and-drop styling
- Multi-select checkbox styling
- Expandable rows styling

## CSS Class Reference

| Class | Purpose |
|-------|---------|
| `.sales-requests-table` | Main table container |
| `.k-grid-header` | Header row container |
| `.k-header` | Individual header cell |
| `.k-grid-table tbody > tr` | Data row |
| `.k-grid-table tbody > tr > td` | Data cell |
| `.k-selected` | Selected row |
| `.k-badge` | Status badges |
| `.k-filtercell` | Filter input row |
| `.k-pager` | Pagination controls |

## Performance Notes
- Pure CSS styling (no JavaScript overhead)
- Single stylesheet import
- Minimal specificity (easy to override if needed)
- Hardware-accelerated transitions where applicable
