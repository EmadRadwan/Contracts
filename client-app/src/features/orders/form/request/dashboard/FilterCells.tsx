/**
 * Reusable Filter Cell Components for Kendo Grid
 *
 * Provides standardized filter cells for different data types:
 * - TextFilterCell: Text input with "contains" operator
 * - NumericFilterCell: Number input with "equals" operator
 * - BooleanFilterCell: Select dropdown with "equals" operator
 * - DateFilterCell: Date picker with "equals" operator
 *
 * All components feature:
 * - Consistent layout with flexbox
 * - Fixed-size clear button that never shrinks
 * - Clean UI without operator dropdowns
 * - Responsive to column width
 *
 * Usage:
 * <Column field="fieldName" filterCell={TextFilterCell} filterable={true} filter="text" />
 */

import React from 'react';
import { GridFilterCellProps } from '@progress/kendo-react-grid';
import { DatePicker, DatePickerChangeEvent } from '@progress/kendo-react-dateinputs';
import { Button as KendoButton } from '@progress/kendo-react-buttons';
import { filterClearIcon } from '@progress/kendo-svg-icons';

interface FilterCellProps {
  getTranslatedLabel: (key: string, defaultValue: string) => string;
}

// ============================================================
// TEXT FILTER CELL
// ============================================================
export const createTextFilterCell = (getTranslatedLabel: (key: string, defaultValue: string) => string) => {
  return (props: GridFilterCellProps) => {
    const hasValue = props.value !== null && props.value !== undefined && props.value !== "";

    const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value;
      const operator = value ? "contains" : "";
      props.onChange({ value, operator, syntheticEvent: event });
    };

    const clear = (event: React.SyntheticEvent) => {
      event.preventDefault();
      props.onChange({ value: null, operator: "", syntheticEvent: event });
    };

    return (
      <div className="k-filtercell k-filtercell-text">
        <input
          type="text"
          className="k-textbox k-filtercell-input"
          value={props.value ?? ""}
          onChange={handleChange}
          placeholder={props.title}
          title={props.title}
          aria-label={props.ariaLabel}
        />
        <KendoButton
          icon="filter-clear"
          svgIcon={filterClearIcon}
          type="button"
          title={getTranslatedLabel("general.clear", "Clear")}
          onClick={clear}
          disabled={!hasValue}
          className="k-filtercell-button"
        />
      </div>
    );
  };
};

// ============================================================
// NUMERIC FILTER CELL
// ============================================================
export const createNumericFilterCell = (getTranslatedLabel: (key: string, defaultValue: string) => string) => {
  return (props: GridFilterCellProps) => {
    const hasValue = props.value !== null && props.value !== undefined && props.value !== "";

    const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value ? Number(event.target.value) : null;
      const operator = value ? "eq" : "";
      props.onChange({ value, operator, syntheticEvent: event });
    };

    const clear = (event: React.SyntheticEvent) => {
      event.preventDefault();
      props.onChange({ value: null, operator: "", syntheticEvent: event });
    };

    return (
      <div className="k-filtercell k-filtercell-numeric">
        <input
          type="number"
          className="k-textbox k-filtercell-input"
          value={props.value ?? ""}
          onChange={handleChange}
          placeholder={props.title}
          title={props.title}
          aria-label={props.ariaLabel}
        />
        <KendoButton
          icon="filter-clear"
          svgIcon={filterClearIcon}
          type="button"
          title={getTranslatedLabel("general.clear", "Clear")}
          onClick={clear}
          disabled={!hasValue}
          className="k-filtercell-button"
        />
      </div>
    );
  };
};

// ============================================================
// BOOLEAN FILTER CELL
// ============================================================
export const createBooleanFilterCell = (getTranslatedLabel: (key: string, defaultValue: string) => string) => {
  return (props: GridFilterCellProps) => {
    const hasValue = props.value !== null && props.value !== undefined && props.value !== "";

    const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
      const value = event.target.value === "" ? null : event.target.value === "true";
      const operator = value !== null ? "eq" : "";
      props.onChange({ value, operator, syntheticEvent: event });
    };

    const clear = (event: React.SyntheticEvent) => {
      event.preventDefault();
      props.onChange({ value: null, operator: "", syntheticEvent: event });
    };

    return (
      <div className="k-filtercell k-filtercell-boolean">
        <select
          className="k-textbox k-filtercell-input"
          value={props.value === null || props.value === undefined ? "" : String(props.value)}
          onChange={handleChange}
          title={props.title}
          aria-label={props.ariaLabel}
        >
          <option value="">-- Select --</option>
          <option value="true">{getTranslatedLabel("common.yes", "Yes")}</option>
          <option value="false">{getTranslatedLabel("common.no", "No")}</option>
        </select>
        <KendoButton
          icon="filter-clear"
          svgIcon={filterClearIcon}
          type="button"
          title={getTranslatedLabel("general.clear", "Clear")}
          onClick={clear}
          disabled={!hasValue}
          className="k-filtercell-button"
        />
      </div>
    );
  };
};

// ============================================================
// DATE FILTER CELL
// ============================================================
export const createDateFilterCell = (getTranslatedLabel: (key: string, defaultValue: string) => string) => {
  return (props: GridFilterCellProps) => {
    const hasValue = props.value !== null && props.value !== undefined && props.value !== "";

    const handleDateChange = (event: DatePickerChangeEvent) => {
      const value = event.value;
      const operator = value ? "eq" : "";
      props.onChange({ value, operator, syntheticEvent: event.syntheticEvent as React.SyntheticEvent });
    };

    const clear = (event: React.SyntheticEvent) => {
      event.preventDefault();
      props.onChange({ value: null, operator: "", syntheticEvent: event });
    };

    return (
      <div className="k-filtercell k-filtercell-date">
        <DatePicker
          value={props.value ?? null}
          format="dd/MM/yyyy"
          formatPlaceholder={{ year: "yyyy", month: "mm", day: "dd" }}
          onChange={handleDateChange}
          title={props.title}
          ariaLabel={props.ariaLabel}
          className="k-filtercell-input"
        />
        <KendoButton
          icon="filter-clear"
          svgIcon={filterClearIcon}
          type="button"
          title={getTranslatedLabel("general.clear", "Clear")}
          onClick={clear}
          disabled={!hasValue}
          className="k-filtercell-button"
        />
      </div>
    );
  };
};

// ============================================================
// EXPORT ALL CREATORS
// ============================================================
export const FilterCellFactory = {
  text: createTextFilterCell,
  numeric: createNumericFilterCell,
  boolean: createBooleanFilterCell,
  date: createDateFilterCell,
};
