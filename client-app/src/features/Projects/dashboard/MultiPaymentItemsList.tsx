import { useMemo, useState } from "react";
import {Grid, GridCellProps, GridColumn, GridToolbar} from "@progress/kendo-react-grid";
import {Button, Typography} from "@mui/material";
import { MultiPaymentItem } from "../../../app/models/project/MultiPaymentItem";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import MultiPaymentItemForm from "../form/MultiPaymentItemForm";

interface MultiPaymentItemsListProps {
    workEffortId: string;
    items: MultiPaymentItem[];
    addItem: (item: MultiPaymentItem) => void;
    updateItem: (item: MultiPaymentItem) => void;
    deleteItem: (itemId: string) => void;
}

export default function MultiPaymentItemsList({ workEffortId, items, addItem, updateItem, deleteItem }: MultiPaymentItemsListProps) {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "projects.multiPaymentCertificate.items";
    const [show, setShow] = useState<boolean>(false);
    const [itemEditMode, setItemEditMode] = useState<number>(0);
    const [selectedItem, setSelectedItem] = useState<MultiPaymentItem | undefined>(undefined);

    const handleAddClick = () => {
        setSelectedItem(undefined);
        setItemEditMode(1);
        setShow(true);
    };

    const handleEditClick = (item: MultiPaymentItem) => {
        setSelectedItem(item);
        setItemEditMode(2);
        setShow(true);
    };

    const handleDeleteClick = (workEffortId: string) => {
        deleteItem(workEffortId);
    };

    const handleClose = () => {
        setShow(false);
        setItemEditMode(0);
        setSelectedItem(undefined);
    };
    
    const GlAccountNameCell = (props: GridCellProps) => (
        <td>
            <Typography
                variant="body2"
                component="span"
                sx={{ color: 'primary.main', cursor: 'pointer', textDecoration: 'underline' }}
                onClick={() => handleEditClick(props.dataItem)}
            >
                {props.dataItem.glAccountName}
            </Typography>
        </td>
    );

    const DeleteCell = (props: GridCellProps) => (
        <td>
            <Button
                variant="outlined"
                color="error"
                size="small"
                onClick={() => handleDeleteClick(props.dataItem.workEffortId)}
            >
                {getTranslatedLabel("general.delete", "Delete")}
            </Button>
        </td>
    );

    
    const columns = useMemo(
        () => [
            {
                field: "glAccountName",
                title: getTranslatedLabel(`${localizationKey}.glAccountName`, "glAccountName"),
                width: "200px",
                cell: GlAccountNameCell,
            },
            {
                field: "itemTypeDescription",
                title: getTranslatedLabel(`${localizationKey}.itemType`, "Item Type"),
                width: "120px",
            },
            {
                field: "serviceId",
                title: getTranslatedLabel(`${localizationKey}.serviceId`, "Service ID"),
                width: "150px",
            },
            {
                field: "serviceName",
                title: getTranslatedLabel(`${localizationKey}.serviceName`, "Service Name"),
                width: "200px",
            },
            {
                field: "productName",
                title: getTranslatedLabel(`${localizationKey}.product`, "Product"),
                width: "200px",
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
                format: "{0:n2}",
            },
            {
                field: "discount",
                title: getTranslatedLabel(`${localizationKey}.discount`, "Discount"),
                width: "100px",
                format: "{0:n2}",
            },
            {
                field: "transportationExpenses",
                title: getTranslatedLabel(`${localizationKey}.transportationExpenses`, "Transportation Expenses"),
                width: "150px",
                format: "{0:n2}",
            },
            {
                field: "gratuities",
                title: getTranslatedLabel(`${localizationKey}.gratuities`, "Gratuities"),
                width: "100px",
                format: "{0:n2}",
            },
            {
                field: "total",
                title: getTranslatedLabel(`${localizationKey}.total`, "Total"),
                width: "100px",
                format: "{0:n2}",
            },
            {
                title: getTranslatedLabel("general.delete", "Delete"),
                width: "100px",
                cell: DeleteCell,
            },
        ],
        [getTranslatedLabel]
    );

    const totalAmount = useMemo(
        () => items.reduce((sum, item) => sum + (item.total || 0), 0),
        [items]
    );
    
    return (
        <>
            <Grid
                data={items} 
                sortable={true}
                resizable={true}
                pageable={true}
                style={{ height: "400px", marginTop: "20px" }}
                key={items.map(item => item.workEffortId).join('-')}
            >
                <GridToolbar>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', width: '100%' }}>
                        <Button
                            variant="contained"
                            color="primary"
                            onClick={handleAddClick}
                        >
                            {getTranslatedLabel(`${localizationKey}.addItem`, "Add Payment Item")}
                        </Button>
                        <Typography variant="h6" sx={{ fontWeight: 'bold' }}>
                            {getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount")}: {totalAmount.toFixed(2)}
                        </Typography>
                    </div>
                </GridToolbar>
                {columns.map((column, index) => (
                    <GridColumn key={index} {...column} />
                ))}
            </Grid>
            {show && (
                <ModalContainer show={show} onClose={handleClose} width={900}>
                    <MultiPaymentItemForm
                        workEffortId={workEffortId}
                        multiPaymentItem={selectedItem}
                        editMode={itemEditMode}
                        onClose={handleClose}
                        formEditMode={itemEditMode}
                        addItem={addItem}
                        updateItem={updateItem}
                    />
                </ModalContainer>
            )}
        </>
    );
}