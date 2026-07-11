import React, { useState, useCallback, useMemo, useEffect, useRef, useContext, createContext } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GridItemChangeEvent,
    GridCellProps,
} from "@progress/kendo-react-grid";
import { Button, Box, Typography } from "@mui/material";
import { NumericTextBox, NumericTextBoxChangeEvent } from "@progress/kendo-react-inputs";
import { toast } from "react-toastify";

import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { useAppSelector } from "../../../app/store/configureStore";
import { FormSimpleComboBoxVirtualProductWithCategory } from "../../../app/common/form/FormSimpleComboBoxVirtualProductWithCategory";
import { FormComboBoxVirtualUOM } from "../../../app/common/form/FormComboBoxVirtualUOM";
import { v4 as uuidv4 } from "uuid";

// Purpose: Plain <td> totals (read-only cells / toolbar text) don't get Kendo's built-in
// numeric-editor thousands separator, unlike the editable numeric columns — mirrors the
// formatNumber convention already used elsewhere (e.g. IncomeStatement.tsx, CertificateItemsList.tsx).
const formatNumber = (value: number | undefined) =>
    (value ?? 0).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

// ── Types ──────────────────────────────────────────────────────────────────────

interface Props {
    onClose: () => void;
    addItem: (item: CertificateItem) => void;
    updateItem: (item: CertificateItem) => void;
    deleteItem: (itemId: string) => void;
    initialItems?: CertificateItem[];
}

interface BulkAddRow extends Omit<CertificateItem, "productId" | "uomId"> {
    workEffortId: string;
    inEdit: true;       // always true — all rows are always editable
    productId: any;
    uomId: any;
    _isValid: boolean;
}

// ── Grid Context ───────────────────────────────────────────────────────────────
// Passes stable callbacks down to cell components defined outside the main component,
// avoiding the "new component type each render → unmount/remount" anti-pattern.

interface GridCtxValue {
    onRemove: (row: BulkAddRow) => void;
    getLabel: (key: string, fallback: string) => string;
}

const GridCtx = createContext<GridCtxValue>({
    onRemove: () => {},
    getLabel: (_k, f) => f,
});

// ── Cell components (defined outside — stable references across renders) ────────

const ProductCell = ({ dataItem, onChange }: GridCellProps) => (
    <td>
        <FormSimpleComboBoxVirtualProductWithCategory
            value={dataItem.productId}
            onChange={(e: any) =>
                onChange!({ dataItem, field: "productId", value: e.value ? { ...e.value } : null } as any)
            }
            textField="productName"
            dataItemKey="productId"
        />
    </td>
);

const UomCell = ({ dataItem, onChange }: GridCellProps) => (
    <td>
        <FormComboBoxVirtualUOM
            value={dataItem.uomId}
            onChange={(e: any) =>
                onChange!({ dataItem, field: "uomId", value: e.value ? { ...e.value } : null } as any)
            }
            textField="description"
            dataItemKey="uomId"
        />
    </td>
);

// Purpose: Every row in this grid is always in edit mode (`inEdit: true` is hardcoded — see
// BulkAddRow), so Kendo never uses the Column's `format` prop for this field; `format` only
// formats the static (non-edit) display cell, and Kendo's built-in `editor="numeric"` always
// falls back to NumericTextBox's own default format (2 decimals) regardless of Column.format.
// A custom cell with an explicit NumericTextBox `format="n9"` is the only way to keep the
// agreed 9-decimal precision visible while the value is being edited.
const AchievementPercentageCell = ({ dataItem, field, onChange }: GridCellProps) => (
    <td>
        <NumericTextBox
            value={dataItem.achievementPercentage ?? 0}
            format="n9"
            onChange={(e: NumericTextBoxChangeEvent) =>
                onChange!({ dataItem, field: field || "achievementPercentage", value: e.value ?? 0 } as any)
            }
        />
    </td>
);

const CommandCell = ({ dataItem }: GridCellProps) => {
    const { onRemove, getLabel } = useContext(GridCtx);
    return (
        <td className="k-command-cell">
            {!dataItem._isValid && dataItem.productId && (
                <span
                    title="Incomplete row"
                    style={{ color: "#ed6c02", marginRight: 6, fontSize: 16, verticalAlign: "middle" }}
                >
                    ⚠
                </span>
            )}
            <button
                className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base"
                onClick={() => onRemove(dataItem)}
            >
                {getLabel("general.remove", "Remove")}
            </button>
        </td>
    );
};

// ── Main component ─────────────────────────────────────────────────────────────

const CertificateItemKendoBulkAddV2: React.FC<Props> = ({
    onClose,
    addItem,
    updateItem,
    deleteItem,
    initialItems,
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "projects.certificate.items.bulkAdd";
    const itemKey = "projects.certificate.items.list";
    const { currentCertificateType } = useAppSelector(s => s.certificateUi);

    const [data, setData] = useState<BulkAddRow[]>([]);

    const isContracting       = currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";
    const isSupplyProcurement = currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE";
    const isCompanySupply     = currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE";

    // Refs for debounced auto-save
    const dataRef        = useRef<BulkAddRow[]>([]);
    const pendingTimers  = useRef<Map<string, ReturnType<typeof setTimeout>>>(new Map());
    const submittedTemps = useRef<Set<string>>(new Set());

    useEffect(() => { dataRef.current = data; }, [data]);

    // ── Row factory ─────────────────────────────────────────────────────────────

    const createEmptyRow = useCallback((productId: any = null): BulkAddRow => ({
        workEffortId: `TEMP-${uuidv4()}`,
        inEdit: true,
        productId,
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
        _isValid: false,
    }), []);

    // ── Sync from initialItems ──────────────────────────────────────────────────

    useEffect(() => {
        setData(prev => {
            const mapped = (initialItems || [])
                .filter(i => !i.isDeleted)
                .map(item => ({
                    ...item,
                    workEffortId: item.workEffortId!,
                    inEdit: true as const,
                    procurementDate: item.procurementDate ? new Date(item.procurementDate) : new Date(),
                    productId: item.productId
                        ? { ProductId: item.productId, ProductName: item.productName || "" }
                        : null,
                    uomId: item.uomId
                        ? { UomId: item.uomId, Description: item.uomName || "" }
                        : null,
                    _isValid: true,     // server-persisted rows are assumed valid
                } as BulkAddRow));

            const tempRows = prev.filter(
                d => d.workEffortId.startsWith("TEMP-") &&
                    !mapped.some(m => m.workEffortId === d.workEffortId)
            );

            const result = [...mapped, ...tempRows];
            return result.length ? result : [createEmptyRow()];
        });
    }, [initialItems, createEmptyRow]);

    // ── Calculations ────────────────────────────────────────────────────────────

    const calculateRowTotals = useCallback((row: BulkAddRow) => {
        const qty    = Number(row.quantity || 0);
        const unit   = Number(row.unitPrice || 0);
        const mat    = Number(row.materialPrice || 0);
        const lab    = Number(row.laborPrice || 0);
        const ach    = Number(row.achievementPercentage || 0);
        const disc   = Number(row.discount || 0);
        const ins    = Number(row.insurance || 0);
        const addIns = Number(row.additionalInsurance || 0);
        const deduct = Number(row.deductions || 0);
        const transp = Number(row.transportationExpenses || 0);
        const grat   = Number(row.gratuities || 0);

        let total = 0, deserved = 0, net = 0;

        if (isContracting) {
            total    = Math.round(qty * (mat + lab) * 1000) / 1000;
            deserved = Math.round(total * (ach / 100) * 1000) / 1000;
            net      = Math.max(0, Math.round((deserved - deduct - ins - addIns) * 1000) / 1000);
        } else if (isSupplyProcurement) {
            total    = Math.round(qty * unit * 1000) / 1000;
            deserved = total;
            net      = Math.max(0, Math.round((total - disc + transp + grat) * 1000) / 1000);
        } else if (isCompanySupply) {
            total    = Math.round(qty * unit * 1000) / 1000;
            deserved = total;
            net      = Math.max(0, Math.round((total + transp + grat) * 1000) / 1000);
        }

        return { total, deserved, net, discount: disc, insurance: ins, additionalInsurance: addIns };
    }, [isContracting, isSupplyProcurement, isCompanySupply]);

    // Purpose: description is only mandatory for Workmanship Contracting Certificates;
    // shared here so both the validity check and the toast message stay in sync on which
    // fields are actually required for the current certificate type.
    const getMissingFieldLabels = useCallback((row: BulkAddRow): string[] => {
        const missing: string[] = [];

        if (!row.productId) missing.push(getTranslatedLabel(`${itemKey}.product`, "المنتج"));
        if (!row.uomId) missing.push(getTranslatedLabel(`${itemKey}.unitOfMeasure`, "وحدة القياس"));
        if (!(row.quantity > 0)) missing.push(getTranslatedLabel(`${itemKey}.quantity`, "الكمية"));

        if (isContracting) {
            if (!row.description) missing.push(getTranslatedLabel(`${itemKey}.description`, "الوصف"));
            if (!((row.materialPrice || 0) > 0 || (row.laborPrice || 0) > 0)) {
                missing.push(
                    `${getTranslatedLabel(`${itemKey}.materialPrice`, "سعر المواد")}/${getTranslatedLabel(`${itemKey}.laborPrice`, "سعر العمالة")}`
                );
            }
            if (!((row.achievementPercentage || 0) > 0)) {
                missing.push(getTranslatedLabel(`${itemKey}.achievementPercentage`, "نسبة الإنجاز"));
            }
        } else if (!((row.unitPrice || 0) > 0)) {
            missing.push(getTranslatedLabel(`${itemKey}.unitPrice`, "سعر الوحدة"));
        }

        return missing;
    }, [isContracting, getTranslatedLabel]);

    const validateRow = useCallback(
        (row: BulkAddRow): boolean => getMissingFieldLabels(row).length === 0,
        [getMissingFieldLabels]
    );

    const serializeRow = useCallback((row: BulkAddRow): CertificateItem => {
        const { total, deserved, net, discount, insurance, additionalInsurance } = calculateRowTotals(row);
        return {
            ...row,
            procurementDate: row.procurementDate instanceof Date ? row.procurementDate.toISOString() : row.procurementDate,
            productId: row.productId?.ProductId || "",
            productName: row.productId?.ProductName || "",
            uomId: row.uomId?.UomId || "",
            uomName: row.uomId?.Description || "",
            unitPrice: isContracting ? (row.materialPrice || 0) + (row.laborPrice || 0) : row.unitPrice,
            totalAmount: isContracting ? total : net,
            deserved,
            net,
            discount,
            insurance,
            additionalInsurance,
            finalTotal: net,
            isDeleted: false,
        };
    }, [calculateRowTotals, isContracting]);

    // ── Auto-save (debounced, 600 ms per row) ───────────────────────────────────

    const scheduleAutoSave = useCallback((workEffortId: string) => {
        const existing = pendingTimers.current.get(workEffortId);
        if (existing) clearTimeout(existing);

        const timer = setTimeout(() => {
            pendingTimers.current.delete(workEffortId);
            const row = dataRef.current.find(r => r.workEffortId === workEffortId);
            if (!row || !validateRow(row)) return;

            const serialized = serializeRow(row);

            if (workEffortId.startsWith("TEMP-")) {
                if (!submittedTemps.current.has(workEffortId)) {
                    submittedTemps.current.add(workEffortId);
                    addItem(serialized);
                } else {
                    // Row already added; propagate further edits as updates
                    updateItem(serialized);
                }
            } else {
                updateItem(serialized);
            }
        }, 600);

        pendingTimers.current.set(workEffortId, timer);
    }, [validateRow, serializeRow, addItem, updateItem]);

    // ── Grid handlers ───────────────────────────────────────────────────────────

    const handleRowChange = useCallback((event: GridItemChangeEvent) => {
        const field = event.field || "";

        setData(prev => prev.map(item => {
            if (item.workEffortId !== event.dataItem.workEffortId) return item;

            const next = { ...item, [field]: event.value };
            const totals = calculateRowTotals(next);

            return {
                ...next,
                totalAmount: totals.total,
                deserved: totals.deserved,
                net: totals.net,
                discount: totals.discount,
                insurance: totals.insurance,
                additionalInsurance: totals.additionalInsurance,
                _isValid: validateRow(next),
            };
        }));

        scheduleAutoSave(event.dataItem.workEffortId);
    }, [calculateRowTotals, validateRow, scheduleAutoSave]);

    const removeRow = useCallback((row: BulkAddRow) => {
        const timer = pendingTimers.current.get(row.workEffortId);
        if (timer) {
            clearTimeout(timer);
            pendingTimers.current.delete(row.workEffortId);
        }

        setData(prev => prev.filter(r => r.workEffortId !== row.workEffortId));

        if (!row.workEffortId.startsWith("TEMP-") || submittedTemps.current.has(row.workEffortId)) {
            deleteItem(row.workEffortId);
        }
    }, [deleteItem]);

    const addNewRow = useCallback(() => {
        const lastRow = data[data.length - 1];
        if (lastRow && !lastRow._isValid) {
            const missing = getMissingFieldLabels(lastRow);
            toast.warning(
                `${getTranslatedLabel(`${localizationKey}.missingFields`, "يرجى استكمال الحقول التالية")}: ${missing.join("، ")}`
            );
            return;
        }
        // Purpose: Carrying the previous row's product forward saves re-picking it when a user
        // is entering several consecutive rows for the same product (e.g. different batches/dates).
        const carriedProductId = lastRow?.productId ? { ...lastRow.productId } : null;
        setData(prev => [...prev, createEmptyRow(carriedProductId)]);
    }, [data, createEmptyRow, getTranslatedLabel, getMissingFieldLabels]);

    // Flush any debounced saves that haven't fired yet before closing
    const handleClose = useCallback(() => {
        pendingTimers.current.forEach((timer, id) => {
            clearTimeout(timer);
            const row = dataRef.current.find(r => r.workEffortId === id);
            if (!row || !validateRow(row)) return;
            const serialized = serializeRow(row);
            if (id.startsWith("TEMP-") && !submittedTemps.current.has(id)) {
                submittedTemps.current.add(id);
                addItem(serialized);
            } else {
                updateItem(serialized);
            }
        });
        pendingTimers.current.clear();
        onClose();
    }, [validateRow, serializeRow, addItem, updateItem, onClose]);

    // ── Context value (stable — CommandCell reads this) ─────────────────────────

    const gridCtxValue = useMemo<GridCtxValue>(() => ({
        onRemove: removeRow,
        getLabel: getTranslatedLabel,
    }), [removeRow, getTranslatedLabel]);

    // ── Row highlight for started-but-incomplete rows ───────────────────────────

    const rowRender = useCallback(
        (trElement: React.ReactElement, { dataItem }: { dataItem: BulkAddRow }) => {
            if (!dataItem._isValid && dataItem.productId) {
                return React.cloneElement(trElement, {
                    style: { backgroundColor: "#fff8e1" },
                });
            }
            return trElement;
        },
        []
    );

    // ── Columns ─────────────────────────────────────────────────────────────────

    const gridColumns = useMemo(() => {
        const cols: React.ReactElement[] = [
            <Column key="productId"   field="productId"   title={isContracting ? getTranslatedLabel(`${itemKey}.product`, "Work Item") : getTranslatedLabel(`${itemKey}.productItem`, "Item")}     cell={ProductCell} width={280} />,
            <Column key="description" field="description" title={getTranslatedLabel(`${itemKey}.description`,    "Description")}                    width={250} />,
            <Column key="uomId"       field="uomId"       title={getTranslatedLabel(`${itemKey}.unitOfMeasure`,  "UOM")}         cell={UomCell}     width={290} />,
            <Column key="quantity"    field="quantity"    title={getTranslatedLabel(`${itemKey}.quantity`,       "Qty")}  editor="numeric"           width={100} />,
        ];

        if (isContracting) {
            cols.push(
                <Column key="materialPrice"        field="materialPrice"        title={getTranslatedLabel(`${itemKey}.materialPrice`,        "Mat. Price")}   editor="numeric"  width={130} />,
                <Column key="laborPrice"           field="laborPrice"           title={getTranslatedLabel(`${itemKey}.laborPrice`,           "Lab. Price")}   editor="numeric"  width={130} />,
                <Column key="achievementPercentage" field="achievementPercentage" title={getTranslatedLabel(`${itemKey}.achievementPercentage`, "Ach. %")}   cell={AchievementPercentageCell}  width={200} />,
                <Column key="totalAmount"          field="totalAmount"          title={getTranslatedLabel(`${itemKey}.totalAmount`,          "Total")}        editable={false}  width={110} cell={p => <td>{formatNumber(p.dataItem.totalAmount)}</td>} />,
                <Column key="deserved"             field="deserved"             title={getTranslatedLabel(`${itemKey}.deserved`,             "Deserved")}     editable={false}  width={110} cell={p => <td>{formatNumber(p.dataItem.deserved)}</td>} />,
                <Column key="insurance"            field="insurance"            title={getTranslatedLabel(`${itemKey}.insurance`,            "Insurance")}    editor="numeric"  width={130} />,
                <Column key="additionalInsurance"  field="additionalInsurance"  title={getTranslatedLabel(`${itemKey}.additionalInsurance`,  "Add. Ins.")}    editor="numeric"  width={130} />,
                <Column key="deductions"           field="deductions"           title={getTranslatedLabel(`${itemKey}.deductions`,           "Deduc.")}       editor="numeric"  width={110} />,
                <Column key="deductionDescription" field="deductionDescription" title={getTranslatedLabel(`${itemKey}.deductionDescription`, "Deduc. Desc.")}                  width={220} />,
            );
        } else {
            cols.push(
                <Column key="unitPrice"       field="unitPrice"       title={getTranslatedLabel(`${itemKey}.unitPrice`,       "Unit Price")} editor="numeric" width={130} />,
                <Column key="procurementDate" field="procurementDate" title={getTranslatedLabel(`${itemKey}.procurementDate`, "Date")}       editor="date"   format="{0:yyyy-MM-dd}" width={150} />,
            );
        }

        if (isSupplyProcurement) {
            cols.push(
                <Column key="discount" field="discount" title={getTranslatedLabel(`${itemKey}.discount`, "Discount")} editor="numeric" width={130} />,
            );
        }

        if (!isContracting) {
            cols.push(
                <Column key="transportationExpenses" field="transportationExpenses" title={getTranslatedLabel(`${itemKey}.transportationExpenses`, "Transp.")} editor="numeric" width={110} />,
                <Column key="gratuities"             field="gratuities"             title={getTranslatedLabel(`${itemKey}.gratuities`,             "Grat.")}   editor="numeric" width={110} />,
            );
        }

        cols.push(
            <Column
                key="net"
                field="net"
                title={getTranslatedLabel(`${itemKey}.net`, "Net")}
                editable={false}
                width={120}
                cell={p => <td><strong>{formatNumber(p.dataItem.net)}</strong></td>}
            />,
            <Column key="commands" cell={CommandCell} width={110} locked />,
        );

        return cols;
    }, [isContracting, isSupplyProcurement, isCompanySupply, getTranslatedLabel]);

    const totalNet = useMemo(
        () => data.reduce((sum, row) => sum + (Number(row.net) || 0), 0),
        [data]
    );

    return (
        <GridCtx.Provider value={gridCtxValue}>
            <Box p={2}>
                <Typography variant="h6" gutterBottom>
                    {getTranslatedLabel(`${localizationKey}.title`, "إضافة بنود المستخلص")}
                </Typography>

                <KendoGrid
                    data={data}
                    onItemChange={handleRowChange}
                    editField="inEdit"
                    dataItemKey="workEffortId"
                    rowRender={rowRender as any}
                    style={{ height: "70vh" }}
                >
                    <GridToolbar>
                        <Box display="flex" justifyContent="space-between" alignItems="center" width="100%">
                            <Button onClick={addNewRow} variant="contained" color="primary">
                                {getTranslatedLabel("general.addRow", "إضافة بند")}
                            </Button>
                            <Typography variant="h6" fontWeight="bold">
                                {getTranslatedLabel("projects.certificate.list.totalAmount", "Total Net")}:{" "}
                                {formatNumber(totalNet)}
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
        </GridCtx.Provider>
    );
};

export default CertificateItemKendoBulkAddV2;
