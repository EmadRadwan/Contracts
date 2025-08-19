import {GridCellProps} from "@progress/kendo-react-grid";
import {DropDownList, DropDownListChangeEvent} from "@progress/kendo-react-dropdowns";
import React from "react";
import {useFetchAllPaymentMethodTypesQuery} from "../../../../../app/store/apis";

export const DropDownCell = (props: GridCellProps) => {
    const {data: paymentMethodTypesData} = useFetchAllPaymentMethodTypesQuery(undefined);

    // loop through paymentMethodTypesData and create an array of objects with text and value properties
    const localizedData = paymentMethodTypesData ? paymentMethodTypesData.map((pmt: any) => {
        return {value: pmt.paymentMethodTypeId, text: pmt.description}
    }) : [];
    // console.log('pymnt DropDownCell localizedData:', localizedData);

    const handleChange = (e: DropDownListChangeEvent) => {
        if (props.onChange) {
            props.onChange({
                dataIndex: 0,
                dataItem: props.dataItem,
                field: props.field,
                syntheticEvent: e.syntheticEvent,
                value: e.target.value.value,
            });
        }
    };

    const {dataItem} = props;
    const field = props.field || "";
    const dataValue = dataItem[field] === null ? "" : dataItem[field];
    // console.log('pymnt DropDownCell dataValue:', dataValue);
    return (
        <td>
            {dataItem.inEdit ? (
                <DropDownList
                    style={{width: "100px"}}
                    onChange={handleChange}
                    value={localizedData.find((c) => c.value === dataValue)}
                    data={localizedData}
                    textField="text"
                />
            ) : (
                dataValue.toString()
            )}
        </td>
    );
};