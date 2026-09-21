import { CompositeFilterDescriptor, FilterDescriptor, State } from '@progress/kendo-data-query';

// Operators that are meaningful without a value — everything else needs one.
const VALUELESS_OPERATORS = new Set(['isnull', 'isnotnull', 'isempty', 'isnotempty']);

const isComposite = (f: FilterDescriptor | CompositeFilterDescriptor): f is CompositeFilterDescriptor =>
    Array.isArray((f as CompositeFilterDescriptor).filters);

const hasValue = (f: FilterDescriptor) =>
    f.value !== null && f.value !== undefined && f.value !== '';

/**
 * Drops filter descriptors that carry an operator but no value.
 *
 * Kendo's filter row pushes a descriptor into the grid state as soon as the operator
 * dropdown changes — before the user has typed anything — and `toODataString` then
 * serializes it literally as `totalPrice gt null` / `saleDate gt undefined`. OData/EF
 * rejects that with a 500, RTK Query keeps the previous page's `data`, and the grid
 * silently shows stale rows as if the filter had been ignored. Stripping the incomplete
 * descriptor here keeps the chosen operator visible in the cell (it stays in grid state)
 * while sending only well-formed clauses to the server.
 */
export const sanitizeGridFilter = (
    filter: CompositeFilterDescriptor | undefined | null
): CompositeFilterDescriptor | undefined => {
    if (!filter?.filters?.length) return undefined;

    const kept = filter.filters
        .map((f) => (isComposite(f) ? sanitizeGridFilter(f) : f))
        .filter((f): f is FilterDescriptor | CompositeFilterDescriptor => {
            if (!f) return false;
            if (isComposite(f)) return true;
            return VALUELESS_OPERATORS.has(String(f.operator)) || hasValue(f);
        });

    return kept.length ? { logic: filter.logic, filters: kept } : undefined;
};

/** Returns a copy of the Kendo grid `State` with an OData-safe `filter` (see sanitizeGridFilter). */
export const sanitizeGridState = (state: State): State => {
    const filter = sanitizeGridFilter(state.filter);
    const { filter: _ignored, ...rest } = state;
    return filter ? { ...rest, filter } : rest;
};
