import { useMemo, useState } from "react";
import {Grid, GridCellProps, GridColumn, GridPageChangeEvent, GridToolbar} from "@progress/kendo-react-grid";
import {Button, Typography} from "@mui/material";
import { MultiPaymentItem } from "../../../app/models/project/MultiPaymentItem";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import MultiPaymentItemBulkAdd from "../form/MultiPaymentItemBulkAdd";

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
    const [showBulk, setShowBulk] = useState<boolean>(false);
    const [page, setPage] = useState({ skip: 0, take: 10 });

    const handlePageChange = (e: GridPageChangeEvent) => {
        setPage({ skip: e.page.skip, take: e.page.take });
    };

    const handleBulkClose = () => {
        setShowBulk(false);
    };
    
    const GlAccountNameCell = (props: GridCellProps) => (
        <td>
            <Typography
                variant="body2"
                component="span"
                sx={{ color: 'primary.main', cursor: 'pointer', textDecoration: 'underline' }}
                onClick={() => setShowBulk(true)}
            >
                {props.dataItem.glAccountName}
            </Typography>
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
                field: "itemTypeDescription",
                title: getTranslatedLabel(`${localizationKey}.itemType`, "Total"),
                width: "100px",
                format: "{0:n2}",
            },
            {
                field: "estimatedStartDate",
                title: getTranslatedLabel(`${localizationKey}.estimatedStartDate`, "Date"),
                width: "150px",
                format: "{0:d}",
            },
            {
                field: "serviceName",
                title: getTranslatedLabel(`${localizationKey}.serviceName`, "Service"),
                width: "200px",
            },
            {
                field: "productName",
                title: getTranslatedLabel(`${localizationKey}.product`, "Product"),
                width: "200px",
            },
            {
                field: "partyIdSupplierName",
                title: getTranslatedLabel("projects.certificate.form.supplier", "Supplier"),
                width: "200px",
            },
            {
                field: "subProjectName",
                title: getTranslatedLabel("projects.certificate.form.subProject", "Sub Project"),
                width: "150px",
            },
            {
                field: "costCenterName",
                title: getTranslatedLabel("accounting.payments.form.costCenter", "Cost Center"),
                width: "150px",
            },
        ],
        [getTranslatedLabel]
    );

    const totalAmount = useMemo(
        () => items.reduce((sum, item) => sum + (item.total || 0), 0),
        [items]
    );
    
    const gridData = useMemo(() => {
        return items.map(item => ({
            ...item,
            estimatedStartDate: item.estimatedStartDate ? new Date(item.estimatedStartDate) : null
        }));
    }, [items]);

    return (
        <>
            <Grid
                data={gridData.slice(page.skip, page.skip + page.take)}
                total={gridData.length}
                skip={page.skip}
                take={page.take}
                onPageChange={handlePageChange}
                sortable={true}
                resizable={true}
                pageable={true}
                style={{ height: "400px", marginTop: "20px" }}
                key={items.map(item => item.workEffortId).join('-')}
            >
                <GridToolbar>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', width: '100%' }}>
                        <div style={{ display: 'flex', gap: '10px' }}>
                            <Button
                                variant="contained"
                                color="primary"
                                onClick={() => setShowBulk(true)}
                            >
                                {getTranslatedLabel(`${localizationKey}.bulkAdd`, "Bulk Add Items")}
                            </Button>
                        </div>
                        <Typography variant="h6" sx={{ fontWeight: 'bold' }}>
                            {getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount")}: {totalAmount.toFixed(2)}
                        </Typography>
                    </div>
                </GridToolbar>
                {columns.map((column, index) => (
                    <GridColumn key={index} {...column} />
                ))}
            </Grid>
            {showBulk && (
                <ModalContainer show={showBulk} onClose={handleBulkClose} width="95vw" maxWidth={1680}>
                    <MultiPaymentItemBulkAdd
                        workEffortId={workEffortId}
                        onClose={handleBulkClose}
                        addItem={addItem}
                        updateItem={updateItem}
                        deleteItem={deleteItem}
                        initialItems={items}
                    />
                </ModalContainer>
            )}
        </>
    );
}