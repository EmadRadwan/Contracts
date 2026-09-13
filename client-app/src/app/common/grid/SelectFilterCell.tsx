import React from 'react';
import { GridFilterCellProps } from '@progress/kendo-react-grid';
import { Button as KendoButton } from '@progress/kendo-react-buttons';
import { filterClearIcon } from '@progress/kendo-svg-icons';

export interface SelectFilterOption {
    text: string;
    value: string;
}

// See TextFilterCell.tsx for why the root element must be a <td> spreading props.tdProps.
type Props = GridFilterCellProps & { tdProps?: React.TdHTMLAttributes<HTMLTableCellElement> };

/**
 * Factory for a dropdown ("eq") filter cell over a known, fixed set of values
 * (e.g. a StatusItem-backed enumeration). Use for columns whose values come
 * from a static/lookup table rather than free text.
 *
 * Uses a native <select> (like the date/numeric operator pickers in this same
 * folder) rather than Kendo's DropDownList, to stay consistent with the rest
 * of this folder's filter cells.
 */
export const createSelectFilterCell = (options: SelectFilterOption[], allLabel: string = 'All') => {
    return (props: Props) => {
        const hasValue = props.value !== null && props.value !== undefined && props.value !== '';

        const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
            const value = event.target.value;
            props.onChange({
                value: value ? value : null,
                operator: value ? 'eq' : '',
                syntheticEvent: event,
            });
        };

        const clear = (event: React.SyntheticEvent) => {
            event.preventDefault();
            props.onChange({ value: null, operator: '', syntheticEvent: event });
        };

        return (
            <td {...props.tdProps}>
                <div className="k-filtercell" style={{ width: '100%', padding: '8px 4px', boxSizing: 'border-box' }}>
                    <div className="k-filtercell-wrapper" style={{ display: 'flex', alignItems: 'center', gap: '4px', width: '100%' }}>
                        <select
                            className="k-textbox k-filtercell-input"
                            value={props.value ?? ''}
                            onChange={handleChange}
                            title={props.title}
                            aria-label={props.ariaLabel}
                            style={{ flex: 1, minWidth: 0, boxSizing: 'border-box' }}
                        >
                            <option value="">{allLabel}</option>
                            {options.map((o) => (
                                <option key={o.value} value={o.value}>{o.text}</option>
                            ))}
                        </select>
                        <KendoButton
                            icon="filter-clear"
                            svgIcon={filterClearIcon}
                            type="button"
                            onClick={clear}
                            disabled={!hasValue}
                            style={{ flexShrink: 0, width: '32px', minWidth: '32px', padding: '4px' }}
                        />
                    </div>
                </div>
            </td>
        );
    };
};
