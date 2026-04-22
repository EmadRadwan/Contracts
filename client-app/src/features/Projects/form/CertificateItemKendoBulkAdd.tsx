import React, {useState, useCallback, useMemo, useEffect, useRef} from "react";
import { Grid as KendoGrid, GridColumn as Column, GridToolbar, GridItemChangeEvent, GridCellProps } from "@progress/kendo-react-grid";
import { Button, Box, Typography } from "@mui/material";
import { toast } from "react-toastify";

import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { useAppSelector } from "../../../app/store/configureStore";

import { FormSimpleComboBoxVirtualProduct } from "../../../app/common/form/FormSimpleComboBoxVirtualProduct";
import { FormComboBoxVirtualUOM } from "../../../app/common/form/FormComboBoxVirtualUOM";

import { v4 as uuidv4 } from "uuid";

interface Props {
    onClose: () => void;
    addItem: (item: CertificateItem) => void;
    updateItem: (item: CertificateItem) => void;
    deleteItem: (itemId: string) => void;
    initialItems?: CertificateItem[];
}
interface BulkAddRow extends Omit<CertificateItem, 'productId' | 'uomId'> {
    workEffortId: string; // 🔥 single identity
    inEdit?: boolean;
    productId: any;
    uomId: any;
}

const CertificateItemKendoBulkAdd: React.FC<Props> = ({
                                                          onClose,
                                                          addItem,
                                                          updateItem,
                                                          deleteItem,
                                                          initialItems,
                                                      }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "projects.certificate.items.bulkAdd";
    const itemFormLocalizationKey = "projects.certificate.items.list";
    const { currentCertificateType } = useAppSelector((state) => state.certificateUi);
    const [data, setData] = useState<BulkAddRow[]>([]);
    const isContracting = currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";
    const isSupplyProcurement = currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE";
    const isCompanySupply = currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE";

    const createEmptyRow = useCallback((): BulkAddRow => ({
        workEffortId: `TEMP-${uuidv4()}`,
        inEdit: true,
        productId: null,
        uomId: null,
        description: "",
        quantity: 0,
        unitPrice: 0,
        materialPrice: 0,
        laborPrice: 0,
        achievementPercentage: 100,
        procurementDate: new Date(),
        discount: 0,
        insurance: 0,
        additionalInsurance: 0,
        deductions: 0,
        deductionDescription: "",
        transportationExpenses: 0,
        gratuities: 0,
        net: 0,
        deserved: 0,
        totalAmount: 0,
    }), []);



    useEffect(() => {
        setData(prev => {
            const mapped = (initialItems || [])
                .filter(i => !i.isDeleted)
                .map(item => {
                    const existing = prev.find(d => d.workEffortId === item.workEffortId);

                    return {
                        ...item,
                        workEffortId: item.workEffortId!,
                        procurementDate: item.procurementDate
                            ? new Date(item.procurementDate)
                            : new Date(),
                        inEdit: existing?.inEdit ?? false,
                        productId: item.productId
                            ? { ProductId: item.productId, ProductName: item.productName || "" }
                            : null,
                        uomId: item.uomId
                            ? { UomId: item.uomId, Description: item.uomName || "" }
                            : null,
                    } as BulkAddRow;
                });

            const tempRows = prev.filter(d =>
                d.workEffortId.startsWith("TEMP-") &&
                !mapped.some(m => m.workEffortId === d.workEffortId)
            );

            const result = [...mapped, ...tempRows];

            return result.length ? result : [createEmptyRow()];
        });
    }, [initialItems, createEmptyRow]);
    
    // ==================== CALCULATIONS (Aligned with original forms) ====================
    const calculateRowTotals = useCallback((row: BulkAddRow) => {
        const quantity = Number(row.quantity || 0);
        const unitPrice = Number(row.unitPrice || 0);
        const materialPrice = Number(row.materialPrice || 0);
        const laborPrice = Number(row.laborPrice || 0);
        const achievementPercentage = Number(row.achievementPercentage || 0);
        const discountInput = Number(row.discount || 0);
        const insuranceInput = Number(row.insurance || 0);
        const addInsInput = Number(row.additionalInsurance || 0);
        const deductions = Number(row.deductions || 0);
        const transportationExpenses = Number(row.transportationExpenses || 0);
        const gratuities = Number(row.gratuities || 0);

        let total = 0;
        let deserved = 0;
        let net = 0;
        let discount = discountInput;
        let insurance = insuranceInput;
        let additionalInsurance = addInsInput;

        if (isContracting) {
            const pricePerUnit = materialPrice + laborPrice;
            total = Math.round(quantity * pricePerUnit * 1000) / 1000;                    // Gross Total
            deserved = Math.round(total * (achievementPercentage / 100) * 1000) / 1000;   // Deserved after achievement

            net = Math.max(0, Math.round((deserved - deductions - insurance - additionalInsurance) * 1000) / 1000);
        }
        else if (isSupplyProcurement) {
            total = Math.round(quantity * unitPrice * 1000) / 1000;
            net = Math.max(0, Math.round((total - discount + transportationExpenses + gratuities) * 1000) / 1000);
            deserved = total; // For consistency with form
        }
        else if (isCompanySupply) {
            total = Math.round(quantity * unitPrice * 1000) / 1000;
            net = Math.max(0, Math.round((total + transportationExpenses + gratuities) * 1000) / 1000);
            deserved = total;
        }

        return { total, deserved, net, discount, insurance, additionalInsurance };
    }, [isContracting, isSupplyProcurement, isCompanySupply]);

    const validateRow = useCallback((row: BulkAddRow): boolean => {
        const commonValid = !!row.productId && !!row.uomId && !!row.description && row.quantity > 0;

        if (isContracting) {
            return commonValid && (row.materialPrice! > 0 || row.laborPrice! > 0) && row.achievementPercentage! > 0;
        }
        return commonValid && row.unitPrice! > 0;
    }, [isContracting]);

    const serializeRow = useCallback((row: BulkAddRow): CertificateItem => {
        const { total, deserved, net, discount, insurance, additionalInsurance } = calculateRowTotals(row);

        return {
            ...row,
            procurementDate: row.procurementDate instanceof Date ? row.procurementDate.toISOString() : row.procurementDate,
            workEffortId:  row.workEffortId,
            productId: row.productId?.ProductId || "",
            productName: row.productId?.ProductName || "",
            uomId: row.uomId?.UomId || "",
            uomName: row.uomId?.Description || "",
            unitPrice: isContracting ? (row.materialPrice || 0) + (row.laborPrice || 0) : row.unitPrice,
            totalAmount: isContracting ? total : net,     // Backend expects gross for contracting, net for others
            deserved,
            net,
            discount,
            insurance,
            additionalInsurance,
            finalTotal: net,
            isDeleted: false,
        };
    }, [calculateRowTotals, isContracting]);

    // ==================== GRID HANDLERS ====================
    const handleRowChange = (event: GridItemChangeEvent) => {
        const field = event.field || "";

        const newData = data.map((item) => {
            if (item.workEffortId === event.dataItem.workEffortId) { // ✅ FIX
                const newItem = { ...item, [field]: event.value };
                const totals = calculateRowTotals(newItem);

                return {
                    ...newItem,
                    totalAmount: totals.total,
                    deserved: totals.deserved,
                    net: totals.net,
                    discount: totals.discount,
                    insurance: totals.insurance,
                    additionalInsurance: totals.additionalInsurance,
                };
            }
            return item;
        });

        setData(newData);
    };
    const enterEdit = (row: BulkAddRow) => {
        setData(prev =>
            prev.map(r =>
                r.workEffortId === row.workEffortId
                    ? { ...r, inEdit: true }
                    : r
            )
        );
    };

    const cancelEdit = (row: BulkAddRow) => {
        if (!row.workEffortId.startsWith("TEMP-")) {
            const original = initialItems?.find(i => i.workEffortId === row.workEffortId);

            if (original) {
                setData(prev =>
                    prev.map(r =>
                        r.workEffortId === row.workEffortId
                            ? {
                                ...original,
                                workEffortId: original.workEffortId!,
                                inEdit: false,
                                productId: original.productId
                                    ? { ProductId: original.productId, ProductName: original.productName || "" }
                                    : null,
                                uomId: original.uomId
                                    ? { UomId: original.uomId, Description: original.uomName || "" }
                                    : null,
                                procurementDate: original.procurementDate
                                    ? new Date(original.procurementDate)
                                    : new Date(),
                            }
                            : r
                    )
                );
                return;
            }
        }

        setData(prev =>
            prev.map(r =>
                r.workEffortId === row.workEffortId
                    ? { ...r, inEdit: false }
                    : r
            )
        );
    };

    const removeRow = (row: BulkAddRow) => {
        setData(prev => prev.filter(r => r.workEffortId !== row.workEffortId));

        if (!row.workEffortId.startsWith("TEMP-")) {
            deleteItem(row.workEffortId);
        }
    };

    const updateRow = (row: BulkAddRow) => {
        if (!validateRow(row)) {
            toast.error("Please fill mandatory fields.");
            return;
        }

        const serialized = serializeRow(row);

        if (row.workEffortId.startsWith("TEMP-")) {
            addItem(serialized);
        } else {
            updateItem(serialized);
        }

        // optimistic UI: stop editing
        setData(prev =>
            prev.map(r =>
                r.workEffortId === row.workEffortId
                    ? { ...r, inEdit: false }
                    : r
            )
        );
    };
    
    const addNewRow = () => {
        const lastRow = data[data.length - 1];
        if (lastRow && lastRow.inEdit && !validateRow(lastRow)) {
            toast.error(getTranslatedLabel("general.mandatoryFieldsMissing", "Please fill mandatory fields."));
            return;
        }
        setData([...data, createEmptyRow()]);
    };

    const handleClose = () => {
        const editingRows = data.filter(r => r.inEdit);
        if (editingRows.length > 0) {
            toast.error(getTranslatedLabel("projects.certificate.items.bulkAdd.finishEditing", "Please finish editing all rows."));
            return;
        }
        onClose();
    };

    // ==================== CUSTOM CELLS ====================
    const ProductCell = (props: GridCellProps) => {
        if (props.dataItem.inEdit) {
            return (
                <td>
                    <FormSimpleComboBoxVirtualProduct
                        value={props.dataItem.productId}
                        onChange={(e: any) =>
                            props.onChange!({
                                dataItem: props.dataItem,
                                field: "productId",
                                value: e.value ? { ...e.value } : null 
                            } as any)
                        }
                        textField="productName"
                        dataItemKey="productId"
                    />
                </td>
            );
        }
        return <td>{props.dataItem.productId?.ProductName || props.dataItem.productName}</td>;
    };

    const UomCell = (props: GridCellProps) => {
        const { dataItem } = props;

        if (dataItem.inEdit) {
            return (
                <td>
                    <FormComboBoxVirtualUOM
                        value={dataItem.uomId}
                        onChange={(e: any) =>
                            props.onChange!({
                                dataItem,
                                field: "uomId",
                                value: e.value ? { ...e.value } : null // ✅ clone to avoid shared reference bug
                            } as any)
                        }
                        textField="description"
                        dataItemKey="uomId"
                    />
                </td>
            );
        }

        return (
            <td>
                {dataItem.uomId?.Description || dataItem.uomName || ""}
            </td>
        );
    };

    const CommandCell = (props: GridCellProps) => {
        const { dataItem } = props;
        return (
            <td className="k-command-cell">
                {dataItem.inEdit ? (
                    <>
                        <button className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-primary" onClick={() => updateRow(dataItem)}>
                            {getTranslatedLabel("general.save", "Save")}
                        </button>
                        <button className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base" onClick={() => cancelEdit(dataItem)}>
                            {getTranslatedLabel("general.cancel", "Cancel")}
                        </button>
                    </>
                ) : (
                    <>
                        <button className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-primary" onClick={() => enterEdit(dataItem)}>
                            {getTranslatedLabel("general.edit", "Edit")}
                        </button>
                        <button className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base" onClick={() => removeRow(dataItem)}>
                            {getTranslatedLabel("general.remove", "Remove")}
                        </button>
                    </>
                )}
            </td>
        );
    };

    // ==================== DYNAMIC COLUMNS ====================
    const gridColumns = useMemo(() => {
        const cols: React.ReactElement[] = [
            <Column key="commands" cell={CommandCell} width={150} locked={true} />,
            <Column
                key="net"
                field="net"
                title={getTranslatedLabel(`${itemFormLocalizationKey}.net`, "Net")}
                editable={false}
                locked={true}
                width={100}
                cell={(props) => <td><strong>{(props.dataItem.net || 0).toFixed(2)}</strong></td>}
            />,
            <Column key="productId" field="productId" title={getTranslatedLabel(`${itemFormLocalizationKey}.product`, "Product")} cell={ProductCell} width={280} />,
            <Column key="uomId" field="uomId" title={getTranslatedLabel(`${itemFormLocalizationKey}.unitOfMeasure`, "UOM")} cell={UomCell} width={180} />,
            <Column key="description" field="description" title={getTranslatedLabel(`${itemFormLocalizationKey}.description`, "Description")} width={250} />,
            <Column key="quantity" field="quantity" title={getTranslatedLabel(`${itemFormLocalizationKey}.quantity`, "Qty")} editor="numeric" width={100} />,
        ];

        if (isContracting) {
            cols.push(
                <Column key="materialPrice" field="materialPrice" title={getTranslatedLabel(`${itemFormLocalizationKey}.materialPrice`, "Mat. Price")} editor="numeric" width={130} />,
                <Column key="laborPrice" field="laborPrice" title={getTranslatedLabel(`${itemFormLocalizationKey}.laborPrice`, "Lab. Price")} editor="numeric" width={130} />,
                <Column key="achievementPercentage" field="achievementPercentage" title={getTranslatedLabel(`${itemFormLocalizationKey}.achievementPercentage`, "Ach. %")} editor="numeric" width={110} />,
                <Column key="total" field="totalAmount" title={getTranslatedLabel(`${itemFormLocalizationKey}.totalAmount`, "Total")} editable={false} width={110} cell={(p) => <td>{(p.dataItem.totalAmount || 0).toFixed(2)}</td>} />,
                <Column key="deserved" field="deserved" title={getTranslatedLabel(`${itemFormLocalizationKey}.deserved`, "Deserved")} editable={false} width={110} cell={(p) => <td>{(p.dataItem.deserved || 0).toFixed(2)}</td>} />
            );
        } else {
            cols.push(
                <Column key="unitPrice" field="unitPrice" title={getTranslatedLabel(`${itemFormLocalizationKey}.unitPrice`, "Unit Price")} editor="numeric" width={130} />,
                <Column key="procurementDate" field="procurementDate" title={getTranslatedLabel(`${itemFormLocalizationKey}.procurementDate`, "Date")} editor="date" format="{0:yyyy-MM-dd}" width={0} />
            );
        }

        // Discount / Insurance columns
        if (isSupplyProcurement) {
            cols.push(
                <Column key="discount" field="discount" title={getTranslatedLabel(`${itemFormLocalizationKey}.discount`, "Discount")} editor="numeric" width={170} />
            );
        }

        if (isContracting) {
            cols.push(
                <Column key="insurance" field="insurance" title={getTranslatedLabel(`${itemFormLocalizationKey}.insurance`, "Insurance")} editor="numeric" width={170} />,
                <Column key="additionalInsurance" field="additionalInsurance" title={getTranslatedLabel(`${itemFormLocalizationKey}.additionalInsurance`, "Add. Ins.")} editor="numeric" width={170} />,
                <Column key="deductions" field="deductions" title={getTranslatedLabel(`${itemFormLocalizationKey}.deductions`, "Deduc.")} editor="numeric" width={110} />,
                <Column key="deductionDescription" field="deductionDescription" title={getTranslatedLabel(`${itemFormLocalizationKey}.deductionDescription`, "Deduc. Desc.")} width={220} />
            );
        }

        if (!isContracting) {
            cols.push(
                <Column key="transportationExpenses" field="transportationExpenses" title={getTranslatedLabel(`${itemFormLocalizationKey}.transportationExpenses`, "Transp.")} editor="numeric" width={110} />,
                <Column key="gratuities" field="gratuities" title={getTranslatedLabel(`${itemFormLocalizationKey}.gratuities`, "Grat.")} editor="numeric" width={110} />
            );
        }

        // Final Net column
        cols.push(
            <Column
                key="net"
                field="net"
                title={getTranslatedLabel(`${itemFormLocalizationKey}.net`, "Net")}
                editable={false}
                width={120}
                cell={(props) => <td><strong>{(props.dataItem.net || 0).toFixed(2)}</strong></td>}
            />
        );

        return cols;
    }, [isContracting, isSupplyProcurement, isCompanySupply, getTranslatedLabel]);

    const totalNet = useMemo(() => {
        return data.reduce((sum, item) => sum + (Number(item.net) || 0), 0);
    }, [data]);
    
    return (
        <Box p={2}>
            <Typography variant="h6" gutterBottom>
                {getTranslatedLabel(`${localizationKey}.title`, "إضافة بنود المستخلص")}
            </Typography>

            <KendoGrid
                data={data}
                onItemChange={handleRowChange}
                editField="inEdit"
                dataItemKey="workEffortId"
                style={{ height: '70vh' }}
            >
                <GridToolbar>
                    <Box display="flex" justifyContent="space-between" alignItems="center" width="100%">
                        <Button onClick={addNewRow} variant="contained" color="primary">
                            {getTranslatedLabel("general.addRow", "إضافة بند")}
                        </Button>

                        <Typography variant="h6" fontWeight="bold">
                            {getTranslatedLabel("projects.certificate.list.totalAmount", "Total Net")}:{" "}
                            {totalNet.toFixed(2)}
                        </Typography>
                    </Box>
                </GridToolbar>

                {gridColumns}
            </KendoGrid>

            <Box display="flex" justifyContent="flex-end" mt={2} gap={2}>
                <Button onClick={onClose} variant="outlined" color="inherit">
                    {getTranslatedLabel("general.cancel", "Cancel")}
                </Button>
                <Button onClick={handleClose} variant="contained" color="primary">
                    {getTranslatedLabel("general.close", "Close")}
                </Button>
            </Box>
        </Box>
    );
};

export default CertificateItemKendoBulkAdd;