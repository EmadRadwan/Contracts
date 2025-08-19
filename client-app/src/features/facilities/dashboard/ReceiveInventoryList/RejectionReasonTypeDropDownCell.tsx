import {GridCellProps} from "@progress/kendo-react-grid";
import {DropDownList, DropDownListChangeEvent} from "@progress/kendo-react-dropdowns";

interface Props extends GridCellProps {
    rejectionReasonTypesData: any[] | undefined; // Add the returnTypesData prop
    validateGrid: () => boolean;
}

export const RejectionReasonTypeDropDownCell = (props: Props): JSX.Element | null => {
    console.log('RejectionReasonTypeDropDownCell rendered');
    const {ariaColumnIndex, columnIndex, render} = props;
    const {dataItem} = props;
    const field = props.field || "";
    const dataValue = dataItem[field] === null ? "" : dataItem[field];
    //console.log('dataItem: ', dataItem);
    const isInEdit = dataItem.inEdit;
    const localizedData = props.rejectionReasonTypesData ? props.rejectionReasonTypesData.map((rtr: any) => {
        return {text: rtr.description, value: rtr.rejectionId}
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
        //props.validateGrid();
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