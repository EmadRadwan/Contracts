import React from 'react';
import { GridFilterCellProps } from '@progress/kendo-react-grid';
import { Button as KendoButton } from '@progress/kendo-react-buttons';
import { filterClearIcon } from '@progress/kendo-svg-icons';

// KendoReact v16 requires custom filterCell components to render the <td> themselves
// (Kendo passes it as props.tdProps, the same way custom `cells.data` renderers already
// do in this app) — without it the grid ends up with a stray <div> as a direct child of
// <tr>, which breaks the table layout and blows up the filter row's height.
type Props = GridFilterCellProps & { tdProps?: React.TdHTMLAttributes<HTMLTableCellElement> };

export const TextFilterCell = (props: Props) => {
    return (
        <td {...props.tdProps}>
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
        </td>
    );
};
