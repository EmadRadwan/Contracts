# Filter Customization Guide

## Overview
The Sales Requests table now has customizable filters with two main filter types:

1. **Text Filters** - "Contains" search with NO operator dropdown
2. **Specialized Filters** - Date, Numeric, Boolean with full operator controls

---

## Text Filter (TextFilterCell)

### How It Works
The custom `TextFilterCell` component:
- ✅ Always uses **"contains"** operator for text matching
- ✅ Removes operator dropdown menu
- ✅ Simple text input with clear button
- ✅ Case-insensitive substring matching

### Applied To
- Request ID
- Apartment Name
- Customer Name
- Employee Name
- Status Description
- Project Name
- Building Number

### User Experience
Users can simply type in the filter box and results filter automatically using "contains" logic:
- `"apt"` matches `"Apartment 401B"`
- `"mansour"` matches `"Ahmed Mansour"`
- No dropdown menu to confuse users

### Code Example
```typescript
<Column
    field="apartmentName"
    title="Apartment"
    width={250}
    filterable={true}
    filter="text"
    filterCell={TextFilterCell}  // ← Custom filter component
/>
```

---

## Specialized Filters

### Date Filter (SaleDateFilterCell)
- **Operator**: Date comparison (is, before, after, etc.)
- **Input**: Date picker with dd/MM/yyyy format
- **Features**: Full operator dropdown + clear button
- **Columns**: Sale Date

```typescript
<Column
    field="saleDate"
    filter="date"
    filterCell={SaleDateFilterCell}  // ← Custom date filter
/>
```

### Numeric Filter
- **Operator**: Numeric comparison (=, <, >, <=, >=)
- **Input**: Number input
- **Columns**: Total Price, Advance Payment

```typescript
<Column
    field="totalPrice"
    filter="numeric"
    filterable={true}
/>
```

### Boolean Filter
- **Operator**: True/False selection
- **Input**: Dropdown or checkbox
- **Columns**: Cheques Delivered

```typescript
<Column
    field="isChequesDelivered"
    filter="boolean"
    filterable={true}
/>
```

### No Filter
- **Columns**: Commission/Actions

```typescript
<Column
    title="عمولة"
    cell={CommissionCell}
    filterable={false}  // ← Disables filtering
    sortable={false}
/>
```

---

## Per-Column Filter Control

Each column can have its own filter settings:

| Column | Filter Type | Operator Menu | Default Operator |
|--------|-------------|----------------|-----------------|
| Request ID | Text | ❌ No | "contains" |
| Apartment | Text | ❌ No | "contains" |
| Customer | Text | ❌ No | "contains" |
| Employee | Text | ❌ No | "contains" |
| Status | Text | ❌ No | "contains" |
| Sale Date | Date | ✅ Yes | "is" |
| Total | Numeric | ✅ Yes | "=" |
| Cheques Delivered | Boolean | ✅ Yes | - |
| Advance | Numeric | ✅ Yes | "=" |
| Project | Text | ❌ No | "contains" |
| Building | Text | ❌ No | "contains" |
| Commission | - | ❌ Disabled | - |

---

## Creating Custom Filters

### To create a custom filter for a specific column:

1. **Define a filter component** (similar to `TextFilterCell` or `SaleDateFilterCell`)
2. **Assign it to the column** via `filterCell` prop
3. **Handle changes** with `props.onChange({ value, operator, ... })`

Example: Custom phone number filter

```typescript
const PhoneFilterCell = (props: GridFilterCellProps) => {
    return (
        <div className="k-filtercell">
            <div className="k-filtercell-wrapper">
                <input
                    type="tel"
                    className="k-textbox"
                    value={props.value ?? ""}
                    onChange={(e) => {
                        props.onChange({
                            value: e.target.value,
                            operator: "startswith",  // Always startswith for phones
                            syntheticEvent: e
                        });
                    }}
                    placeholder="Search phones..."
                    style={{ flex: 1 }}
                />
                <KendoButton
                    icon="filter-clear"
                    svgIcon={filterClearIcon}
                    type="button"
                    onClick={(e) => {
                        e.preventDefault();
                        props.onChange({
                            value: null,
                            operator: "",
                            syntheticEvent: e
                        });
                    }}
                />
            </div>
        </div>
    );
};
```

---

## Styling

Filter cells are styled in `SalesRequestsList.styles.css`:

```css
.sales-requests-table .k-filtercell {
  padding: 8px 4px;
  background-color: #FAFAFA;
  border-bottom: 1px solid #E0E0E0;
}

.sales-requests-table .k-filtercell .k-textbox:focus {
  border-color: #0066CC;
  box-shadow: 0 0 0 3px rgba(0, 102, 204, 0.1);
}
```

---

## Query Parameters

When filtering is applied, Kendo Grid sends filter parameters to the backend:

### Text Filter
```json
{
  "filter": {
    "logic": "and",
    "filters": [
      {
        "field": "apartmentName",
        "operator": "contains",
        "value": "apt"
      }
    ]
  }
}
```

### Date Filter
```json
{
  "filter": {
    "logic": "and",
    "filters": [
      {
        "field": "saleDate",
        "operator": "gte",
        "value": "2024-01-01"
      }
    ]
  }
}
```

---

## Disabling Filters for Specific Columns

To remove filter from any column:

```typescript
<Column
    field="fieldName"
    filterable={false}  // ← Disable filtering
/>
```

---

## Best Practices

1. **Text fields** → Use custom `TextFilterCell` for simple "contains" search
2. **Dates** → Use `filter="date"` with date picker for range queries
3. **Numbers** → Use `filter="numeric"` for comparison operators
4. **Booleans** → Use `filter="boolean"` for true/false
5. **Actions** → Set `filterable={false}` for action columns

---

## Troubleshooting

### Filter not showing
- Check `filterable={true}` on the Column
- Verify `filter` prop is set correctly

### Operator dropdown showing in text filter
- Ensure column uses `filterCell={TextFilterCell}`
- Check column doesn't have `filter="text"` without `filterCell`

### Filter not working
- Verify backend accepts the filter format
- Check console for OData syntax errors
- Ensure field names match exactly

---

## Related Files
- `SalesRequestsList.tsx` - Main component with filter definitions
- `SalesRequestsList.styles.css` - Filter styling
- `designSystem.ts` - Color tokens used in filters
