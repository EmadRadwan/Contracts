import {GridCellProps} from "@progress/kendo-react-grid";
import {DropDownList, DropDownListChangeEvent} from "@progress/kendo-react-dropdowns";

interface Props extends GridCellProps {
    facilityList: any[] | undefined; // Add the facilityList prop
}

export const ReceiveInventoryFacilityDropDown = (props: Props): JSX.Element | null => {
    console.log('ReceiveInventoryFacilityDropDown rendered');
    const {ariaColumnIndex, columnIndex, render} = props;
    const {dataItem} = props;
    const field = props.field || "";
    const dataValue = dataItem[field] === null ? "" : dataItem[field];
    //console.log('dataItem: ', dataItem);
    const isInEdit = dataItem.inEdit;
    const localizedData = props.facilityList ? props.facilityList.map((fId: any) => {
        return {text: fId.facilityName, value: fId.facilityId}
    }) : [];

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

    let editor = null;

    editor = (
        <DropDownList
            //style={{width: "100px"}}
            onChange={handleChange}
            value={localizedData.find((c: any) => c.value === dataValue)}
            data={localizedData || []}
            textField="text"
        />
    );
    const defaultRendering = (
        <td
            style={{textAlign: 'center'}}
            aria-colindex={ariaColumnIndex}
            data-grid-col-index={columnIndex}
        >
            {isInEdit ? <div>{editor}</div> : dataValue ? <div>{editor}</div> : null}
        </td>
    );

    if (render) {
        return render(defaultRendering, props);
    }


    return defaultRendering;
};