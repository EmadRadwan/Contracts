import { useMemo, useState } from "react";
import { Grid, GridColumn, GridToolbar } from "@progress/kendo-react-grid";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useGetMultiPaymentItemsQuery } from "../../../app/store/apis/multiPaymentCertificateApi";
import { Button } from "@mui/material";
import MultiPaymentItemForm from "../form/MultiPaymentItemForm";
import ModalContainer from "../../../app/common/modals/ModalContainer";

interface MultiPaymentItemsListProps {
    workEffortId: string;
}

export default function MultiPaymentItemsList({ workEffortId }: MultiPaymentItemsListProps) {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.multiPaymentCertificate.items";
    const { data: items = [], isLoading, isError } = useGetMultiPaymentItemsQuery(workEffortId);
    const [show, setShow] = useState<boolean>(false);
    const [itemEditMode, setItemEditMode] = useState<number>(0);
    

    const handleAddClick = () => {
        setItemEditMode(1);
        setShow(true);
    };

    const handleClose = () => {
        setShow(false);
        setItemEditMode(0);
    };

    const columns = useMemo(
        () => [
            {
                field: "projectName",
                title: getTranslatedLabel(`${localizationKey}.project`, "Project"),
                width: "150px",
            },
            {
                field: "subProjectName",
                title: getTranslatedLabel(`${localizationKey}.subProject`, "Sub-Project"),
                width: "150px",
            },
            {
                field: "itemType",
                title: getTranslatedLabel(`${localizationKey}.itemType`, "Item Type"),
                width: "120px",
            },
            {
                field: "productName",
                title: getTranslatedLabel(`${localizationKey}.product`, "Product"),
                width: "200px",
            },
            {
                field: "uomName",
                title: getTranslatedLabel(`${localizationKey}.uom`, "Unit of Measure"),
                width: "120px",
            },
            {
                field: "description",
                title: getTranslatedLabel(`${localizationKey}.description`, "Description"),
                width: "200px",
            },
            {
                field: "amount",
                title: getTranslatedLabel(`${localizationKey}.amount`, "Amount"),
                width: "100px",
                format: "{0:c}",
            },
            {
                field: "discount",
                title: getTranslatedLabel(`${localizationKey}.discount`, "Discount"),
                width: "100px",
                format: "{0:c}",
            },
            {
                field: "transportationExpenses",
                title: getTranslatedLabel(`${localizationKey}.transportationExpenses`, "Transportation Expenses"),
                width: "150px",
                format: "{0:c}",
            },
            {
                field: "gratuities",
                title: getTranslatedLabel(`${localizationKey}.gratuities`, "Gratuities"),
                width: "100px",
                format: "{0:c}",
            },
            {
                field: "total",
                title: getTranslatedLabel(`${localizationKey}.total`, "Total"),
                width: "100px",
                format: "{0:c}",
            },
        ],
        [getTranslatedLabel]
    );

    // REFACTOR: Retained totalAmount calculation, ensuring it uses the 'total' field for accuracy
    const totalAmount = useMemo(
        () => items.reduce((sum, item) => sum + (item.total || 0), 0),
        [items]
    );

    return (
        <>
            <Grid
                data={items}
                sortable={true}
                pageable={true}
                style={{ height: "400px", marginTop: "20px" }}
            >
                {/* REFACTOR: Added GridToolbar with a Button to trigger the add form */}
                <GridToolbar>
                    <Button
                        variant="contained"
                        color="primary"
                        onClick={handleAddClick}
                        //disabled={isLoading || isError}
                    >
                        {getTranslatedLabel(`${localizationKey}.addItem`, "Add Payment Item")}
                    </Button>
                </GridToolbar>
                {columns.map((column, index) => (
                    <GridColumn key={index} {...column} />
                ))}
                <GridColumn
                    title={getTranslatedLabel(`${localizationKey}.total`, "Total")}
                    cell={() => (
                        <td colSpan={columns.length}>
                            {getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount")}: {totalAmount.toFixed(2)}
                        </td>
                    )}
                />
            </Grid>
            {/* REFACTOR: Conditionally render MultiPaymentItemForm when editMode is 1 */}
            {show && (
                <ModalContainer show={show} onClose={handleClose} width={900}>
                    <MultiPaymentItemForm
                        workEffortId={workEffortId}
                        editMode={itemEditMode}
                        onClose={handleClose}
                        formEditMode={itemEditMode}
                    />
                </ModalContainer>
            )}
        </>
    );
}