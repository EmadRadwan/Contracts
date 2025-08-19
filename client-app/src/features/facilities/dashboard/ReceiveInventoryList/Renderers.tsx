import {GridCellProps, GridRowProps} from '@progress/kendo-react-grid';
import * as React from 'react';
import {OrderItem} from "../../../../app/models/order/orderItem";

interface CellRenderProps {
    originalProps: GridCellProps;
    td: React.ReactElement<HTMLTableCellElement>;
    enterEdit: (dataItem: OrderItem, field: string | undefined) => void;
    editField: string | undefined;
}

interface RowRenderProps {
    originalProps: GridRowProps;
    tr: React.ReactElement<HTMLTableRowElement>;
    exitEdit: () => void;
    editField: string | undefined;
    isValid: boolean;
}

export const CellRender = (props: CellRenderProps) => {
    const dataItem = props.originalProps.dataItem;
    const cellField = props.originalProps.field;
    //console.log('cellField', cellField)
    const inEditField = dataItem[props.editField || ''];
    //console.log('inEditField', dataItem[props.editField || '']);
    //console.log('props.editField', props.editField);
    //console.log('additionalProps', cellField && cellField === inEditField);

    if (cellField === 'returnQuantity' && dataItem.returnReasonId === null) {
        return null; // Do not render the cell if returnReasonId is null
    }


    const additionalProps =
        cellField && cellField === inEditField
            ? {
                ref: (td: any) => {
                    const input = td && td.querySelector('input');
                    //console.log('input', input)
                    const activeElement = document.activeElement;
                    ////console.log('activeElement', activeElement)
                    if (
                        !input ||
                        !activeElement ||
                        input === activeElement ||
                        !activeElement.contains(input)
                    ) {
                        return;
                    }

                    if (input.type === 'checkbox') {
                        input.focus();
                    } else {
                        input.select();
                    }
                },
            }
            : {
                onClick: () => {
                    setTimeout(() => {
                        props.enterEdit(dataItem, cellField);
                    });
                },
            };
    ////console.log('cellField && cellField === inEditField', cellField && cellField === inEditField)
    const clonedProps: any = {...props.td.props, ...additionalProps};
    return React.cloneElement(props.td, clonedProps, props.td.props.children);
};

export const RowRender = (props: RowRenderProps) => {
    const trProps = {
        ...props.tr.props,
        className: props.isValid ? null : 'invalid-row',
        onBlur: () => {
            setTimeout(() => {
                const activeElement = document.activeElement;
                if (
                    activeElement &&
                    activeElement.className &&
                    activeElement.className.indexOf("k-dropdown") < 0
                ) {
                    props.exitEdit();
                }
            });
        }
    };

    return React.cloneElement(props.tr, trProps, props.tr.props.children);
};
