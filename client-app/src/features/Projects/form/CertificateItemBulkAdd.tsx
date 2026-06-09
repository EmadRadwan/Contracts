import React, { useState, useCallback, useMemo, memo, useEffect } from "react";
import {
    Button,
    Box,
    Typography,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    IconButton,
    RadioGroup,
    FormControlLabel,
    Radio,
    Popover,
    useTheme,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";
import SettingsIcon from "@mui/icons-material/Settings";
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
    tempId: string;
    productId: any; // { ProductId, ProductName }
    uomId: any; // { UomId, Description }
}

interface BulkAddRowItemProps {
    row: BulkAddRow;
    index: number;
    handleRowChange: (
        index: number,
        field: keyof BulkAddRow,
        value: any,
        extraFields?: Partial<BulkAddRow>
    ) => void;
    handleRemoveRow: (index: number) => void;
    rowsCount: number;
    certificateType: string;
    localValues: { [key: string]: string };
}

const BulkAddRowItem: React.FC<BulkAddRowItemProps> = memo(({
    row,
    index,
    handleRowChange,
    handleRemoveRow,
    rowsCount,
    certificateType,
    localValues,
}) => {
    const theme = useTheme();
    const isRtl = theme.direction === 'rtl';
    const { getTranslatedLabel } = useTranslationHelper();
    const isContracting = certificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";
    const isSupplyProcurement = certificateType === "SUPPLY_PROCUREMENT_CERTIFICATE";
    const isCompanySupply = certificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE";

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const [activeModeField, setActiveModeField] = useState<string | null>(null);

    const handleOpenPopover = (event: React.MouseEvent<HTMLElement>, field: string) => {
        setAnchorEl(event.currentTarget);
        setActiveModeField(field);
    };

    const handleClosePopover = () => {
        setAnchorEl(null);
        setActiveModeField(null);
    };

    const getModeValue = (field: string) => {
        return (row as any)[field] || "value";
    };

    const handleModeChange = (value: string) => {
        if (activeModeField) {
            handleRowChange(index, activeModeField as keyof BulkAddRow, value);
        }
        handleClosePopover();
    };

    return (
        <TableRow>
            {/* Product */}
            <TableCell sx={{ 
                minWidth: 300,
                width: 300,
                maxWidth: 300,
                position: 'sticky',
                [isRtl ? 'right' : 'left']: 0,
                backgroundColor: 'background.paper',
                zIndex: 1,
            }}>
                <FormSimpleComboBoxVirtualProduct
                    value={row.productId}
                    onChange={(e: any) => handleRowChange(index, "productId", e.value)}
                    textField="productName"
                    dataItemKey="productId"
                    name={`productId-${index}`}
                />
            </TableCell>

            {/* UOM */}
            <TableCell sx={{ 
                minWidth: 200,
                width: 200,
                maxWidth: 200,
                position: 'sticky',
                [isRtl ? 'right' : 'left']: 300,
                backgroundColor: 'background.paper',
                zIndex: 1,
            }}>
                <FormComboBoxVirtualUOM
                    value={row.uomId}
                    onChange={(e: any) => handleRowChange(index, "uomId", e.value)}
                    textField="description"
                    dataItemKey="uomId"
                    name={`uomId-${index}`}
                />
            </TableCell>

            {/* Description */}
            <TableCell sx={{ 
                minWidth: 250,
                width: 250,
                maxWidth: 250,
                position: 'sticky',
                [isRtl ? 'right' : 'left']: 500,
                backgroundColor: 'background.paper',
                zIndex: 1,
            }}>
                <TextField
                    fullWidth
                    size="small"
                    multiline={isContracting}
                    rows={isContracting ? 2 : 1}
                    value={localValues.description ?? ""}
                    onChange={(e) => handleRowChange(index, "description", e.target.value)}
                />
            </TableCell>

            {/* Quantity */}
            <TableCell sx={{ minWidth: 100 }}>
                <TextField
                    fullWidth
                    size="small"
                    type="number"
                    value={localValues.quantity ?? ""}
                    onChange={(e) => handleRowChange(index, "quantity", e.target.value)}
                    inputProps={{ min: 0, step: "0.001" }}
                />
            </TableCell>

            {/* Unit Price (for Supply) or Material/Labor Price (for Contracting) */}
            {!isContracting && (
                <TableCell sx={{ minWidth: 120 }}>
                    <TextField
                        fullWidth
                        size="small"
                        type="number"
                        value={localValues.unitPrice ?? ""}
                        onChange={(e) => handleRowChange(index, "unitPrice", e.target.value)}
                        inputProps={{ min: 0, step: "0.001" }}
                    />
                </TableCell>
            )}

            {isContracting && (
                <>
                    <TableCell sx={{ minWidth: 120 }}>
                        <TextField
                            fullWidth
                            size="small"
                            type="number"
                            value={localValues.materialPrice ?? ""}
                            onChange={(e) => handleRowChange(index, "materialPrice", e.target.value)}
                            inputProps={{ min: 0, step: "0.001" }}
                        />
                    </TableCell>
                    <TableCell sx={{ minWidth: 120 }}>
                        <TextField
                            fullWidth
                            size="small"
                            type="number"
                            value={localValues.laborPrice ?? ""}
                            onChange={(e) => handleRowChange(index, "laborPrice", e.target.value)}
                            inputProps={{ min: 0, step: "0.001" }}
                        />
                    </TableCell>
                    <TableCell sx={{ minWidth: 120 }}>
                        <TextField
                            fullWidth
                            size="small"
                            type="number"
                            value={localValues.achievementPercentage ?? ""}
                            onChange={(e) => handleRowChange(index, "achievementPercentage", e.target.value)}
                            inputProps={{ min: 0, max: 100, step: "0.001" }}
                        />
                    </TableCell>
                </>
            )}

            {/* Procurement Date */}
            {!isContracting && (
                <TableCell sx={{ minWidth: 150 }}>
                    <TextField
                        fullWidth
                        size="small"
                        type="date"
                        value={row.procurementDate ? row.procurementDate.split('T')[0] : ""}
                        onChange={(e) => handleRowChange(index, "procurementDate", e.target.value)}
                        InputLabelProps={{ shrink: true }}
                    />
                </TableCell>
            )}

            {/* Discount (Supply Procurement only) */}
            {isSupplyProcurement && (
                <TableCell sx={{ minWidth: 150 }}>
                    <Box display="flex" alignItems="center" gap={1}>
                        <TextField
                            fullWidth
                            size="small"
                            type="number"
                            value={localValues.discount ?? ""}
                            onChange={(e) => handleRowChange(index, "discount", e.target.value)}
                            inputProps={{ min: 0 }}
                        />
                        <IconButton size="small" onClick={(e) => handleOpenPopover(e, "discountMode")}>
                            <SettingsIcon fontSize="small" color={row.discountMode === "percentage" ? "primary" : "inherit"} />
                        </IconButton>
                    </Box>
                </TableCell>
            )}

            {/* Insurance (Contracting only) */}
            {isContracting && (
                <>
                    <TableCell sx={{ minWidth: 150 }}>
                        <Box display="flex" alignItems="center" gap={1}>
                            <TextField
                                fullWidth
                                size="small"
                                type="number"
                                value={localValues.insurance ?? ""}
                                onChange={(e) => handleRowChange(index, "insurance", e.target.value)}
                                inputProps={{ min: 0 }}
                            />
                            <IconButton size="small" onClick={(e) => handleOpenPopover(e, "insuranceMode")}>
                                <SettingsIcon fontSize="small" color={row.insuranceMode === "percentage" ? "primary" : "inherit"} />
                            </IconButton>
                        </Box>
                    </TableCell>
                    <TableCell sx={{ minWidth: 150 }}>
                        <Box display="flex" alignItems="center" gap={1}>
                            <TextField
                                fullWidth
                                size="small"
                                type="number"
                                value={localValues.additionalInsurance ?? ""}
                                onChange={(e) => handleRowChange(index, "additionalInsurance", e.target.value)}
                                inputProps={{ min: 0 }}
                            />
                            <IconButton size="small" onClick={(e) => handleOpenPopover(e, "additionalInsuranceMode")}>
                                <SettingsIcon fontSize="small" color={row.additionalInsuranceMode === "percentage" ? "primary" : "inherit"} />
                            </IconButton>
                        </Box>
                    </TableCell>
                    <TableCell sx={{ minWidth: 120 }}>
                        <TextField
                            fullWidth
                            size="small"
                            type="number"
                            value={localValues.deductions ?? ""}
                            onChange={(e) => handleRowChange(index, "deductions", e.target.value)}
                            inputProps={{ min: 0 }}
                        />
                    </TableCell>
                    <TableCell sx={{ minWidth: 200 }}>
                        <TextField
                            fullWidth
                            size="small"
                            value={localValues.deductionDescription ?? ""}
                            onChange={(e) => handleRowChange(index, "deductionDescription", e.target.value)}
                        />
                    </TableCell>
                </>
            )}

            {/* Transportation & Gratuities (All types) */}
            <TableCell sx={{ minWidth: 120 }}>
                <TextField
                    fullWidth
                    size="small"
                    type="number"
                    value={localValues.transportationExpenses ?? ""}
                    onChange={(e) => handleRowChange(index, "transportationExpenses", e.target.value)}
                    inputProps={{ min: 0 }}
                />
            </TableCell>
            <TableCell sx={{ minWidth: 120 }}>
                <TextField
                    fullWidth
                    size="small"
                    type="number"
                    value={localValues.gratuities ?? ""}
                    onChange={(e) => handleRowChange(index, "gratuities", e.target.value)}
                    inputProps={{ min: 0 }}
                />
            </TableCell>

            {/* Totals (Read-only) */}
            <TableCell sx={{ minWidth: 100 }}>
                <Typography variant="body2">{(row.net || 0).toFixed(2)}</Typography>
            </TableCell>

            {/* Delete */}
            <TableCell sx={{ width: 50 }}>
                <IconButton
                    color="error"
                    onClick={() => handleRemoveRow(index)}
                    disabled={rowsCount === 1}
                >
                    <DeleteIcon />
                </IconButton>
            </TableCell>

            <Popover
                open={Boolean(anchorEl)}
                anchorEl={anchorEl}
                onClose={handleClosePopover}
                anchorOrigin={{
                    vertical: 'bottom',
                    horizontal: 'left',
                }}
            >
                <Box p={2}>
                    <Typography variant="subtitle2" gutterBottom>
                        {getTranslatedLabel("projects.certificate.items.bulkAdd.selectMode", "Select Mode")}
                    </Typography>
                    <RadioGroup
                        value={activeModeField ? getModeValue(activeModeField) : "value"}
                        onChange={(e) => handleModeChange(e.target.value)}
                    >
                        <FormControlLabel 
                            value="value" 
                            control={<Radio size="small" />} 
                            label={getTranslatedLabel("projects.certificate.items.bulkAdd.modeValue", "Value")} 
                        />
                        <FormControlLabel 
                            value="percentage" 
                            control={<Radio size="small" />} 
                            label={getTranslatedLabel("projects.certificate.items.bulkAdd.modePercentage", "%")} 
                        />
                    </RadioGroup>
                </Box>
            </Popover>
        </TableRow>
    );
});

const CertificateItemBulkAdd: React.FC<Props> = ({ onClose, addItem, updateItem, deleteItem, initialItems }) => {
    const theme = useTheme();
    const isRtl = theme.direction === 'rtl';
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "projects.certificate.items.bulkAdd";
    const itemFormLocalizationKey = "projects.certificate.items.list";
    const { currentCertificateType } = useAppSelector((state) => state.certificateUi);

    const createEmptyRow = useCallback((): BulkAddRow => ({
        tempId: uuidv4(),
        productId: null,
        uomId: null,
        description: "",
        quantity: 0,
        unitPrice: 0,
        materialPrice: 0,
        laborPrice: 0,
        achievementPercentage: 100,
        procurementDate: new Date().toISOString(),
        discount: 0,
        discountMode: "value",
        insurance: 0,
        insuranceMode: "value",
        additionalInsurance: 0,
        additionalInsuranceMode: "value",
        deductions: 0,
        deductionDescription: "",
        transportationExpenses: 0,
        gratuities: 0,
        net: 0,
        deserved: 0,
        totalAmount: 0,
    }), []);

    const [rows, setRows] = useState<BulkAddRow[]>([]);
    const [localValues, setLocalValues] = useState<{ [tempId: string]: { [field: string]: string } }>({});
    const isInitialized = React.useRef(false);

    useEffect(() => {
        if (isInitialized.current) return;

        if (initialItems && initialItems.length > 0) {
            const mappedRows = initialItems.map((item) => {
                const tempId = item.workEffortId || uuidv4();
                return {
                    ...item,
                    tempId: tempId,
                    productId: item.productId ? { ProductId: item.productId, ProductName: item.productName || "" } : null,
                    uomId: item.uomId ? { UomId: item.uomId, Description: item.uomName || "" } : null,
                };
            });
            setRows(mappedRows);
            const lv: { [tempId: string]: { [field: string]: string } } = {};
            mappedRows.forEach(r => {
                lv[r.tempId] = {
                    description: r.description || "",
                    quantity: (r.quantity || 0).toString(),
                    unitPrice: (r.unitPrice || 0).toString(),
                    materialPrice: (r.materialPrice || 0).toString(),
                    laborPrice: (r.laborPrice || 0).toString(),
                    achievementPercentage: (r.achievementPercentage || 0).toString(),
                    discount: (r.discount || 0).toString(),
                    insurance: (r.insurance || 0).toString(),
                    additionalInsurance: (r.additionalInsurance || 0).toString(),
                    deductions: (r.deductions || 0).toString(),
                    deductionDescription: r.deductionDescription || "",
                    transportationExpenses: (r.transportationExpenses || 0).toString(),
                    gratuities: (r.gratuities || 0).toString(),
                };
            });
            setLocalValues(lv);
            isInitialized.current = true;
        } else if (initialItems !== undefined) {
            const emptyRow = createEmptyRow();
            setRows([emptyRow]);
            setLocalValues({ [emptyRow.tempId]: {
                quantity: "0",
                unitPrice: "0",
                materialPrice: "0",
                laborPrice: "0",
                achievementPercentage: "100",
                discount: "0",
                insurance: "0",
                additionalInsurance: "0",
                deductions: "0",
                transportationExpenses: "0",
                gratuities: "0",
            } });
            isInitialized.current = true;
        }
    }, [initialItems, createEmptyRow]);

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
        let discount = 0;
        let insurance = 0;
        let additionalInsurance = 0;

        if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
            const pricePerUnit = materialPrice + laborPrice;
            total = Math.round(quantity * pricePerUnit * 1000) / 1000;
            const grossAchieved = Math.round(total * (achievementPercentage / 100) * 1000) / 1000;
            deserved = grossAchieved;
            insurance = row.insuranceMode === "percentage" ? Math.round((insuranceInput / 100) * grossAchieved * 1000) / 1000 : insuranceInput;
            additionalInsurance = row.additionalInsuranceMode === "percentage" ? Math.round((addInsInput / 100) * grossAchieved * 1000) / 1000 : addInsInput;
            net = Math.max(0, Math.round((deserved - deductions - insurance - additionalInsurance) * 1000) / 1000);
        } else if (currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE") {
            total = Math.round(quantity * unitPrice * 1000) / 1000;
            deserved = total;
            discount = row.discountMode === "percentage" ? Math.round((discountInput / 100) * total * 1000) / 1000 : discountInput;
            net = Math.max(0, Math.round((total - discount + transportationExpenses + gratuities) * 1000) / 1000);
        } else if (currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE") {
            total = Math.round(quantity * unitPrice * 1000) / 1000;
            deserved = total;
            net = Math.max(0, Math.round((total + transportationExpenses + gratuities) * 1000) / 1000);
        }

        return { total, deserved, net, discount, insurance, additionalInsurance };
    }, [currentCertificateType]);

    const validateRow = useCallback((row: BulkAddRow): boolean => {
        const commonValid = !!row.productId && !!row.uomId && !!row.description && row.quantity > 0;
        if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
            return commonValid && (row.materialPrice! > 0 || row.laborPrice! > 0) && row.achievementPercentage! > 0;
        }
        return commonValid && row.unitPrice! > 0;
    }, [currentCertificateType]);

    const serializeRow = useCallback((row: BulkAddRow): CertificateItem => {
        const { total, deserved, net, discount, insurance, additionalInsurance } = calculateRowTotals(row);
        return {
            ...row,
            workEffortId: row.workEffortId || `TEMP-${row.tempId}`,
            productId: row.productId?.ProductId || "",
            productName: row.productId?.ProductName || "",
            uomId: row.uomId?.UomId || "",
            uomName: row.uomId?.Description || "",
            unitPrice: currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE" 
                ? (row.materialPrice || 0) + (row.laborPrice || 0)
                : row.unitPrice,
            totalAmount: currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? total : net,
            deserved,
            net,
            discount,
            insurance,
            additionalInsurance,
            finalTotal: net,
            isDeleted: false,
        };
    }, [calculateRowTotals, currentCertificateType]);

    const handleRowChange = useCallback((
        index: number,
        field: keyof BulkAddRow,
        value: any,
        extraFields: Partial<BulkAddRow> = {}
    ) => {
        setRows((prevRows) => {
            const newRows = [...prevRows];
            const currentRow = { ...newRows[index] };
            const tempId = currentRow.tempId;

            // Update local value for performance and controlled input behavior
            if (["description", "quantity", "unitPrice", "materialPrice", "laborPrice", "achievementPercentage", "discount", "insurance", "additionalInsurance", "deductions", "deductionDescription", "transportationExpenses", "gratuities"].includes(field as string)) {
                setLocalValues(prev => ({
                    ...prev,
                    [tempId]: {
                        ...prev[tempId],
                        [field]: value
                    }
                }));
            }

            let processedValue = value;
            if (["quantity", "unitPrice", "materialPrice", "laborPrice", "achievementPercentage", "discount", "insurance", "additionalInsurance", "deductions", "transportationExpenses", "gratuities"].includes(field as string)) {
                processedValue = parseFloat(value) || 0;
            }

            (currentRow as any)[field] = processedValue;
            Object.assign(currentRow, extraFields);

            // Re-calculate totals for visual feedback
            const { total, deserved, net, discount, insurance, additionalInsurance } = calculateRowTotals(currentRow);
            currentRow.totalAmount = total;
            currentRow.deserved = deserved;
            currentRow.net = net;
            currentRow.discount = discount;
            currentRow.insurance = insurance;
            currentRow.additionalInsurance = additionalInsurance;

            newRows[index] = currentRow;

            // Sync with parent if valid
            if (validateRow(currentRow)) {
                const serialized = serializeRow(currentRow);
                if (currentRow.workEffortId) {
                    updateItem(serialized);
                } else {
                    currentRow.workEffortId = `TEMP-${currentRow.tempId}`;
                    addItem(serialized);
                }
            }

            return newRows;
        });
    }, [calculateRowTotals, validateRow, serializeRow, addItem, updateItem]);

    const handleAddRow = () => {
        const lastRow = rows[rows.length - 1];
        if (lastRow && !validateRow(lastRow)) {
            toast.error(getTranslatedLabel("general.mandatoryFieldsMissing", "Please fill mandatory fields."));
            return;
        }
        const emptyRow = createEmptyRow();
        setRows([...rows, emptyRow]);
        setLocalValues(prev => ({ ...prev, [emptyRow.tempId]: {
            quantity: "0",
            unitPrice: "0",
            materialPrice: "0",
            laborPrice: "0",
            achievementPercentage: "100",
            discount: "0",
            insurance: "0",
            additionalInsurance: "0",
            deductions: "0",
            transportationExpenses: "0",
            gratuities: "0",
        } }));
    };

    const handleRemoveRow = (index: number) => {
        const rowToRemove = rows[index];
        if (rowToRemove.workEffortId) {
            deleteItem(rowToRemove.workEffortId);
        }
        const newRows = [...rows];
        newRows.splice(index, 1);
        if (newRows.length === 0) {
            newRows.push(createEmptyRow());
        }
        setRows(newRows);
    };

    const handleClose = () => {
        const incompleteRows = rows.filter(row => (row.productId || row.description || row.quantity > 0) && !validateRow(row));
        if (incompleteRows.length > 0) {
            toast.error(getTranslatedLabel("general.mandatoryFieldsMissing", "Some rows are incomplete."));
            return;
        }
        onClose();
    };

    const isContracting = currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";
    const isSupplyProcurement = currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE";

    return (
        <Box p={2}>
            <Typography variant="h6" gutterBottom>
                {getTranslatedLabel(`${localizationKey}.title`, "إضافة بنود المستخلص")}
            </Typography>

            <TableContainer component={Paper} sx={{ maxHeight: '70vh' }}>
                <Table size="small" stickyHeader sx={{ borderCollapse: 'separate' }}>
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ 
                                minWidth: 300,
                                width: 300,
                                maxWidth: 300,
                                position: 'sticky',
                                [isRtl ? 'right' : 'left']: 0,
                                zIndex: 3,
                                backgroundColor: 'background.paper',
                                top: 0,
                            }}>{getTranslatedLabel(`${itemFormLocalizationKey}.product`, "Product")}</TableCell>
                            <TableCell sx={{ 
                                minWidth: 200,
                                width: 200,
                                maxWidth: 200,
                                position: 'sticky',
                                [isRtl ? 'right' : 'left']: 300,
                                zIndex: 3,
                                backgroundColor: 'background.paper',
                                top: 0,
                            }}>{getTranslatedLabel(`${itemFormLocalizationKey}.unitOfMeasure`, "UOM")}</TableCell>
                            <TableCell sx={{ 
                                minWidth: 250,
                                width: 250,
                                maxWidth: 250,
                                position: 'sticky',
                                [isRtl ? 'right' : 'left']: 500,
                                zIndex: 3,
                                backgroundColor: 'background.paper',
                                top: 0,
                            }}>{getTranslatedLabel(`${itemFormLocalizationKey}.description`, "Description")}</TableCell>
                            <TableCell sx={{ minWidth: 100 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.quantity`, "Qty")}</TableCell>
                            {!isContracting && <TableCell sx={{ minWidth: 120 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.unitPrice`, "Price")}</TableCell>}
                            {isContracting && (
                                <>
                                    <TableCell sx={{ minWidth: 120 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.materialPrice`, "Mat. Price")}</TableCell>
                                    <TableCell sx={{ minWidth: 120 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.laborPrice`, "Lab. Price")}</TableCell>
                                    <TableCell sx={{ minWidth: 120 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.achievementPercentage`, "Ach. %")}</TableCell>
                                </>
                            )}
                            {!isContracting && <TableCell sx={{ minWidth: 150 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.procurementDate`, "Date")}</TableCell>}
                            {isSupplyProcurement && <TableCell sx={{ minWidth: 150 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.discount`, "Discount")}</TableCell>}
                            {isContracting && (
                                <>
                                    <TableCell sx={{ minWidth: 150 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.insurance`, "Insurance")}</TableCell>
                                    <TableCell sx={{ minWidth: 150 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.additionalInsurance`, "Add. Ins.")}</TableCell>
                                    <TableCell sx={{ minWidth: 120 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.deductions`, "Deduc.")}</TableCell>
                                    <TableCell sx={{ minWidth: 200 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.deductionDescription`, "Deduc. Desc.")}</TableCell>
                                </>
                            )}
                            <TableCell sx={{ minWidth: 120 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.transportationExpenses`, "Transp.")}</TableCell>
                            <TableCell sx={{ minWidth: 120 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.gratuities`, "Grat.")}</TableCell>
                            <TableCell sx={{ minWidth: 100 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.totalAmount`, "Total")}</TableCell>
                            <TableCell sx={{ width: 50 }}></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {rows.map((row, index) => (
                            <BulkAddRowItem
                                key={row.tempId}
                                row={row}
                                index={index}
                                handleRowChange={handleRowChange}
                                handleRemoveRow={handleRemoveRow}
                                rowsCount={rows.length}
                                certificateType={currentCertificateType}
                                localValues={localValues[row.tempId] || {}}
                            />
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>

            <Box display="flex" justifyContent="space-between" mt={2}>
                <Button startIcon={<AddIcon />} onClick={handleAddRow} variant="outlined">
                    {getTranslatedLabel("general.addRow", "إضافة بند")}
                </Button>
                <Box display="flex" gap={2}>
                    <Button onClick={onClose} variant="outlined" color="inherit">
                        {getTranslatedLabel("general.cancel", "Cancel")}
                    </Button>
                    <Button onClick={handleClose} variant="contained" color="primary">
                        {getTranslatedLabel("general.close", "Close")}
                    </Button>
                </Box>
            </Box>
        </Box>
    );
};

export default CertificateItemBulkAdd;
