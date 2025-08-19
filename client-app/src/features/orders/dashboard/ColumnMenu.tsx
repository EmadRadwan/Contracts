import * as React from 'react';
import {GridColumnMenuCheckboxFilter, GridColumnMenuFilter, GridColumnMenuProps} from '@progress/kendo-react-grid';

export const ColumnMenu = (props: GridColumnMenuProps) => {
    return (
        <div>
            <GridColumnMenuFilter {...props} expanded={true}/>
        </div>
    );
};


const orderTypes = [
    {
        "orderTypeId": "PURCHASE_ORDER",
        "orderTypeDescription": "Purchase",
    },
    {
        "orderTypeId": "SALES_ORDER",
        "orderTypeDescription": "Sales",
    }
];

export const ColumnMenuOrderTypeFilter = (props: GridColumnMenuProps) => {
    return (
        <div>
            <GridColumnMenuCheckboxFilter {...props} data={orderTypes} expanded={true}/>
        </div>
    );
};


