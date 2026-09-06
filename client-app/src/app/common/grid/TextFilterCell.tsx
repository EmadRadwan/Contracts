import React from 'react';
import { GridFilterCellProps } from '@progress/kendo-react-grid';
import { Button as KendoButton } from '@progress/kendo-react-buttons';
import { filterClearIcon } from '@progress/kendo-svg-icons';

export const TextFilterCell = (props: GridFilterCellProps) => {
    return (
        <div className="k-filtercell" style={{ width: '100%', padding: '8px 4px', boxSizing: 'border-box' }}>
            <div className="k-filtercell-wrapper" style={{ display: 'flex', alignItems: 'center', gap: '4px', width: '100%' }}>
                <input
                    type="text"
                    className="k-textbox"
                    value={props.value ?? ""}
                    onChange={(e) => {
                        props.onChange({
                            value: e.target.value,
                            operator: "contains",
                            syntheticEvent: e
                        });
                    }}
                    placeholder={`Search...`}
                    style={{ flex: 1, minWidth: '0', boxSizing: 'border-box' }}
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
