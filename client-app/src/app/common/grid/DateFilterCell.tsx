import React from 'react';
import { GridFilterCellProps } from '@progress/kendo-react-grid';
import { Button as KendoButton } from '@progress/kendo-react-buttons';
import { filterClearIcon } from '@progress/kendo-svg-icons';

export const DateFilterCell = (props: GridFilterCellProps) => {
    const dateValue = props.value ? new Date(props.value).toISOString().split('T')[0] : '';

    return (
        <div className="k-filtercell" style={{ width: '100%', padding: '8px 4px', boxSizing: 'border-box' }}>
            <div className="k-filtercell-wrapper" style={{ display: 'flex', alignItems: 'center', gap: '2px', width: '100%' }}>
                <select
                    className="k-textbox"
                    value={props.operator || "eq"}
                    onChange={(e) => {
                        props.onChange({
                            value: props.value,
                            operator: e.target.value,
                            syntheticEvent: e
                        });
                    }}
                    style={{ flexShrink: 0, width: '40px', padding: '6px 4px', boxSizing: 'border-box', fontSize: '13px' }}
                >
                    <option value="eq">=</option>
                    <option value="lt">&lt;</option>
                    <option value="lte">≤</option>
                    <option value="gt">&gt;</option>
                    <option value="gte">≥</option>
                </select>
                <input
                    type="date"
                    className="k-textbox"
                    value={dateValue}
                    onChange={(e) => {
                        props.onChange({
                            value: e.target.value ? new Date(e.target.value) : null,
                            operator: props.operator || "eq",
                            syntheticEvent: e
                        });
                    }}
                    style={{ flex: 1, minWidth: '0', boxSizing: 'border-box', fontSize: '12px' }}
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
                    style={{ flexShrink: 0, width: '32px', minWidth: '32px', padding: '4px' }}
                />
            </div>
        </div>
    );
};
