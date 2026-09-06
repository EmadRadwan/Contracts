# Design System Documentation

## Overview
This design system provides a centralized source of truth for all Sales Request components' styling, colors, typography, spacing, and component-specific styles. All 4 components now use this unified system to ensure consistency across the UI.

## File Structure
- `designSystem.ts` - Main design system file with all tokens and component styles

## Affected Components
1. **SalesRequestsList.tsx** - Sales Requests table/list view
2. **SalesRequestForm.tsx** - New/Edit Sales Request form
3. **SalesRequestsAndApartmentsDateRangeExcel.tsx** - Export Data modal
4. **InstallmentPriceCalculatorModal.tsx** - PV Calculator modal

## Design System Sections

### Colors
- **Primary**: #0066CC (blue) - used for main actions and primary buttons
- **Secondary**: #28A745 (green) - used for create/success actions
- **Status Colors**: 
  - Pending: #FF9800 (orange)
  - Approved: #4CAF50 (green)
  - Rejected: #F44336 (red)
  - Cancelled: #D32F2F (dark red)
- **Neutral Grays**: 50-900 scale for backgrounds, borders, and text
- **Semantic Colors**: Specific use cases for backgrounds, text, borders, and feedback

### Typography
- **Font Family**: Roboto with system font fallbacks
- **Font Sizes**: xs (12px) to 4xl (28px)
- **Font Weights**: Light (300) to Bold (700)
- **Predefined Styles**: h1-h5, label, body, caption for consistent text rendering

### Spacing
- Standard scale: xs (4px) → 3xl (48px)
- Used for padding, gaps, and margins throughout components

### Border Radius
- xs (2px) → full (9999px)
- Consistent rounded corners across buttons, inputs, and containers

### Shadows
- Five levels: xs → xl
- Used for elevation and depth in cards and modals

### Components
Pre-configured styles for:
- **Buttons**: Primary, Secondary, Success variants
- **Inputs**: Default styling with focus states
- **Badges**: Status-specific badges (pending, approved, rejected, cancelled)
- **Cards**: Default and elevated variants
- **Modals**: Backdrop, container, header, and footer styles
- **Tables**: Header and cell styling

## Usage Example

```typescript
import { designSystem } from '@app/theme/designSystem';

// In your component
<Button
    sx={{
        backgroundColor: designSystem.colors.primary.main,
        color: designSystem.colors.text.inverse,
        padding: '8px 16px',
        fontSize: designSystem.typography.fontSize.base,
        fontWeight: designSystem.typography.fontWeight.semibold,
        '&:hover': {
            backgroundColor: designSystem.colors.primary.dark,
        },
    }}
>
    Action
</Button>
```

## Light Mode Only
Currently implemented for light mode only. Dark mode tokens can be added in future iterations by extending the design system structure.

## Consistency Rules
1. **Colors**: Always use named colors from `designSystem.colors`, not hardcoded hex values
2. **Typography**: Use predefined styles from `designSystem.typography.styles` for headings/text
3. **Spacing**: Use the spacing scale for all padding/margin/gap values
4. **Borders**: Use `designSystem.colors.border` for all border colors
5. **Shadows**: Use predefined shadow levels instead of custom box-shadows

## Future Enhancements
- Dark mode implementation
- Additional component style presets
- Responsive spacing adjustments
- Animation/transition configurations
