import React, { useMemo, useRef } from 'react';
import { GridFilterCellProps } from '@progress/kendo-react-grid';
import { DatePicker, DatePickerChangeEvent } from '@progress/kendo-react-dateinputs';
import { Button as KendoButton } from '@progress/kendo-react-buttons';
import { filterClearIcon } from '@progress/kendo-svg-icons';
import { CompositeFilterDescriptor, FilterDescriptor } from '@progress/kendo-data-query';

// See TextFilterCell.tsx for why the root element must be a <td> spreading props.tdProps.
type Props = GridFilterCellProps & { tdProps?: React.TdHTMLAttributes<HTMLTableCellElement> };

export interface DateRange {
    from: Date | null;
    to: Date | null;
}

export interface DateRangeFilterCellOptions {
    /** Returns the grid's current composite filter (read on every render / change). */
    getFilter: () => CompositeFilterDescriptor | undefined;
    /** Receives the rebuilt composite filter (undefined when nothing is left to filter on). */
    onFilterChange: (filter: CompositeFilterDescriptor | undefined) => void;
    fromPlaceholder?: string;
    toPlaceholder?: string;
    format?: string;
}

const startOfDay = (d: Date) => new Date(d.getFullYear(), d.getMonth(), d.getDate(), 0, 0, 0, 0);
const endOfDay = (d: Date) => new Date(d.getFullYear(), d.getMonth(), d.getDate(), 23, 59, 59, 999);

const isFieldDescriptor = (f: FilterDescriptor | CompositeFilterDescriptor, field: string): f is FilterDescriptor =>
    (f as FilterDescriptor).field === field;

/** Reads the from/to pair for `field` out of a composite filter (gte = from, lte = to). */
export const readDateRange = (filter: CompositeFilterDescriptor | undefined, field: string): DateRange => {
    const range: DateRange = { from: null, to: null };
    (filter?.filters ?? []).forEach((f) => {
        if (!isFieldDescriptor(f, field) || !(f.value instanceof Date)) return;
        if (f.operator === 'gte') range.from = f.value;
        if (f.operator === 'lte') range.to = f.value;
    });
    return range;
};

/**
 * Rebuilds the composite filter with the `field` range replaced.
 *
 * Kendo's filter row only ever reads ONE descriptor per field, so a range has to
 * bypass `props.onChange` and be written into the grid's filter state directly as
 * two flat descriptors (`gte` start-of-day, `lte` end-of-day). Keeping them flat
 * (not nested in their own composite) matters: `GridFilterCellContainer` keeps every
 * top-level descriptor whose `field` differs from the column being edited, so the
 * range survives changes to other columns and gets replaced only when this cell
 * changes it again.
 */
export const applyDateRange = (
    filter: CompositeFilterDescriptor | undefined,
    field: string,
    range: DateRange
): CompositeFilterDescriptor | undefined => {
    const others = (filter?.filters ?? []).filter((f) => !isFieldDescriptor(f, field));
    const next: Array<FilterDescriptor | CompositeFilterDescriptor> = [...others];
    if (range.from) next.push({ field, operator: 'gte', value: startOfDay(range.from) });
    if (range.to) next.push({ field, operator: 'lte', value: endOfDay(range.to) });
    return next.length ? { logic: filter?.logic ?? 'and', filters: next } : undefined;
};

/**
 * Factory for a from/to date-range filter cell (two Kendo DatePickers with calendar
 * popups + clear). Prefer `useDateRangeFilterCell` from a component — it keeps the
 * cell's identity stable across renders so the pickers don't remount mid-edit.
 */
export const createDateRangeFilterCell = (options: DateRangeFilterCellOptions) => {
    const format = options.format ?? 'dd/MM/yyyy';

    return (props: Props) => {
        const field = props.field ?? '';
        const range = readDateRange(options.getFilter(), field);

        const update = (patch: Partial<DateRange>) => {
            options.onFilterChange(applyDateRange(options.getFilter(), field, { ...range, ...patch }));
        };

        const clear = (event: React.SyntheticEvent) => {
            event.preventDefault();
            options.onFilterChange(applyDateRange(options.getFilter(), field, { from: null, to: null }));
        };

        return (
            <td {...props.tdProps}>
                <div className="k-filtercell" style={{ width: '100%', padding: '8px 4px', boxSizing: 'border-box' }}>
                    <div className="k-filtercell-wrapper" style={{ display: 'flex', alignItems: 'center', gap: '2px', width: '100%' }}>
                        <DatePicker
                            size="small"
                            format={format}
                            value={range.from}
                            max={range.to ?? undefined}
                            placeholder={options.fromPlaceholder ?? 'From'}
                            onChange={(e: DatePickerChangeEvent) => update({ from: e.value })}
                            style={{ flex: 1, minWidth: 0 }}
                        />
                        <DatePicker
                            size="small"
                            format={format}
                            value={range.to}
                            min={range.from ?? undefined}
                            placeholder={options.toPlaceholder ?? 'To'}
                            onChange={(e: DatePickerChangeEvent) => update({ to: e.value })}
                            style={{ flex: 1, minWidth: 0 }}
                        />
                        <KendoButton
                            icon="filter-clear"
                            svgIcon={filterClearIcon}
                            type="button"
                            title="Clear"
                            disabled={!range.from && !range.to}
                            onClick={clear}
                            style={{ flexShrink: 0, width: '32px', minWidth: '32px', padding: '4px' }}
                        />
                    </div>
                </div>
            </td>
        );
    };
};

export interface UseDateRangeFilterCellArgs {
    filter: CompositeFilterDescriptor | undefined;
    onFilterChange: (filter: CompositeFilterDescriptor | undefined) => void;
    fromPlaceholder?: string;
    toPlaceholder?: string;
    format?: string;
}

/**
 * Hook wrapper around `createDateRangeFilterCell` for controlled grids: pass the
 * grid's `dataState.filter` and a setter; the returned component is created once
 * and reads the latest filter through a ref.
 *
 *   const DateRangeCell = useDateRangeFilterCell({
 *       filter: dataState.filter,
 *       onFilterChange: (filter) => setDataState((prev) => ({ ...prev, filter, skip: 0 })),
 *   });
 *   <Column field="date" filter="date" cells={{ filterCell: DateRangeCell }} />
 */
export const useDateRangeFilterCell = ({ filter, onFilterChange, fromPlaceholder, toPlaceholder, format }: UseDateRangeFilterCellArgs) => {
    const filterRef = useRef(filter);
    filterRef.current = filter;
    const onChangeRef = useRef(onFilterChange);
    onChangeRef.current = onFilterChange;

    return useMemo(
        () =>
            createDateRangeFilterCell({
                getFilter: () => filterRef.current,
                onFilterChange: (next) => onChangeRef.current(next),
                fromPlaceholder,
                toPlaceholder,
                format,
            }),
        // Placeholders/format are display-only; re-creating the cell for them would remount the pickers.
        // eslint-disable-next-line react-hooks/exhaustive-deps
        []
    );
};
