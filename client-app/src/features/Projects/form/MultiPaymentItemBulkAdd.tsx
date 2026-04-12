import React, { useState, useCallback, useMemo, memo , useEffect } from "react";
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
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";
import { toast } from "react-toastify";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { MultiPaymentItem } from "../../../app/models/project/MultiPaymentItem";
import { useAppSelector } from "../../../app/store/configureStore";
import {
    useFetchGlAccountOrganizationHierarchyLovQuery,
} from "../../../app/store/apis";
import { useGetCostCentersQuery } from "../../../app/store/apis/accounting/paymentTypesApi";
import { FormDropDownTreeGlAccount2 } from "../../../app/common/form/FormDropDownTreeGlAccount2";
import { FormSimpleComboBoxServiceVirtual } from "../../../app/common/form/FormSimpleComboBoxServiceVirtual";
import { FormSimpleComboBoxRawMaterialVirtual } from "../../../app/common/form/FormSimpleComboBoxRawMaterialVirtual";
import { FormComboBoxVirtualAllParties } from "../../../app/common/form/FormComboBoxVirtualAllParties";
import { MemoizedFormComboBox2 } from "../../../app/common/form/FormComboBox2";
import {useFetchWorkEffortsByGlAccountIdQuery} from "../../../app/store/apis/projectsApi";
import { FormDropDownTreeGlAccount3 } from "../../../app/common/form/FormDropDownTreeGlAccount3";

interface Props {
    onClose: () => void;
    workEffortId: string;
    addItem: (item: MultiPaymentItem) => void;
    updateItem: (item: MultiPaymentItem) => void;
    deleteItem: (itemId: string) => void;
    initialItems?: MultiPaymentItem[];
}

interface BulkAddRow {
    tempId: string;
    workEffortId?: string; // To track existing items for editing
    glAccountId: string;
    glAccountName?: string;
    projectId?: string;
    subProjectId?: string;
    subProjectName?: string;
    costCenterId?: string;
    costCenterName?: string;
    itemType: string;
    serviceId: any; // { ProductId, ProductName }
    productId: any; // { ProductId, ProductName }
    party: any; // { fromPartyId, fromPartyName }
    description: string;
    amount: number;
    estimatedStartDate?: string;
}

interface BulkAddRowItemProps {
    row: BulkAddRow;
    index: number;
    glAccounts: any[];
    handleRowChange: (
        index: number,
        field: keyof BulkAddRow,
        value: any,
        extraFields?: Partial<BulkAddRow>
    ) => void;
    handleRemoveRow: (index: number) => void;
    rowsCount: number;
    costCenters: any[];
    itemTypes: any[];
    localAmount: string;
}

const BulkAddRowItem: React.FC<BulkAddRowItemProps> = memo(({
                                                                row,
                                                                index,
                                                                glAccounts,
                                                                handleRowChange,
                                                                handleRemoveRow,
                                                                rowsCount,
                                                                costCenters,
                                                                itemTypes,
                                                                localAmount,
                                                            }) => {

    // === Queries ===
    const { data: projects } = useFetchWorkEffortsByGlAccountIdQuery(
        { glAccountId: row.glAccountId, workEffortTypeId: "PROJECT" },
        { skip: !row.glAccountId }
    );

    const projectId = projects && projects.length > 0 ? projects[0].workEffortId : undefined;

    const { data: subProjects } = useFetchWorkEffortsByGlAccountIdQuery(
        {
            glAccountId: row.glAccountId,
            workEffortTypeId: "SUB_PROJECT",
            workEffortParentId: projectId
        },
        { skip: !projectId }
    );

    // === Effects ===
    useEffect(() => {
        if (projectId && projectId !== row.projectId) {
            handleRowChange(index, "projectId" as any, projectId);
        } else if (!projectId && row.projectId) {
            handleRowChange(index, "projectId" as any, undefined);
            handleRowChange(index, "subProjectId" as any, undefined);
            handleRowChange(index, "subProjectName" as any, undefined);
        }
    }, [projectId, row.projectId, index, handleRowChange]);

    useEffect(() => {
        if (subProjects && row.subProjectId) {
            const sp = subProjects.find((p: any) => p.workEffortId === row.subProjectId);
            if (sp && sp.subProjectName !== row.subProjectName) {
                handleRowChange(index, "subProjectName" as any, sp.subProjectName);
            }
        }
    }, [subProjects, row.subProjectId, row.subProjectName, index, handleRowChange]);

    return (
        <TableRow key={row.tempId}>
            {/* GL Account */}
            <TableCell sx={{ minWidth: 350 }}>
                <FormDropDownTreeGlAccount3
                    data={glAccounts || []}
                    value={row.glAccountId}
                    onChange={(e: any) => handleRowChange(index, "glAccountId", e.value)}
                    dataItemKey="glAccountId"
                    textField="text"
                    selectField="selected"
                    expandField="expanded"
                    name="glAccountId"
                    touched={false}
                    visited={false}
                    modified={false}
                />
            </TableCell>

            {/* Description */}
            <TableCell sx={{ minWidth: 250 }}>
                <TextField
                    fullWidth
                    size="small"
                    value={row.description || ""}
                    onChange={(e) => handleRowChange(index, "description", e.target.value)}
                />
            </TableCell>

            {/* Amount */}
            <TableCell sx={{ minWidth: 120 }}>
                <TextField
                    fullWidth
                    size="small"
                    type="number"
                    value={localAmount ?? ""}
                    onChange={(e) => handleRowChange(index, "amount", e.target.value)}
                    inputProps={{ min: 0, step: "0.01" }}
                />
            </TableCell>

            {/* Estimated Start Date - Fixed for calendar */}
            <TableCell sx={{ minWidth: 150 }}>
                <TextField
                    fullWidth
                    size="small"
                    type="date"
                    value={row.estimatedStartDate || ""}
                    onChange={(e) => handleRowChange(index, "estimatedStartDate", e.target.value)}
                    InputLabelProps={{ shrink: true }}
                    onClick={(e) => e.stopPropagation()}
                />
            </TableCell>

            {/* Item Type */}
            <TableCell sx={{ minWidth: 200 }}>
                <MemoizedFormComboBox2
                    data={itemTypes}
                    textField="description"
                    dataItemKey="itemType"
                    value={row.itemType || null}
                    onChange={(e: any) => handleRowChange(index, "itemType", e.value)}
                />
            </TableCell>

            {/* Service */}
            <TableCell sx={{ minWidth: 300 }}>
                <FormSimpleComboBoxServiceVirtual
                    value={row.serviceId}
                    onChange={(e: any) => handleRowChange(index, "serviceId", e.value)}
                    textField="productName"
                    dataItemKey="productId"
                    name="serviceId"
                    touched={false}
                    visited={false}
                    modified={false}
                />
            </TableCell>

            {/* Product */}
            <TableCell sx={{ minWidth: 300 }}>
                <FormSimpleComboBoxRawMaterialVirtual
                    value={row.productId}
                    onChange={(e: any) => handleRowChange(index, "productId", e.value)}
                    textField="productName"
                    dataItemKey="productId"
                    name="productId"
                    touched={false}
                    visited={false}
                    modified={false}
                    disabled={row.itemType !== "MATERIALS"}
                />
            </TableCell>

            {/* Supplier */}
            <TableCell sx={{ minWidth: 300 }}>
                <FormComboBoxVirtualAllParties
                    value={row.party}
                    onChange={(e: any) => handleRowChange(index, "party", e.value)}
                    name="party"
                    touched={false}
                    visited={false}
                    modified={false}
                />
            </TableCell>

            {/* Sub Project */}
            <TableCell sx={{ minWidth: 250 }}>
                {subProjects && subProjects.length > 0 ? (
                    <MemoizedFormComboBox2
                        data={subProjects.filter((sp: any) => !!sp)}
                        textField="subProjectName"
                        dataItemKey="workEffortId"
                        value={row.subProjectId || null}
                        onChange={(e: any) => {
                            const selectedId = e?.value;
                            const sp = subProjects.find((p: any) => p.workEffortId === selectedId);
                            handleRowChange(index, "subProjectId", selectedId, {
                                subProjectName: sp?.subProjectName || ""
                            });
                        }}
                    />
                ) : (
                    "-"
                )}
            </TableCell>

            {/* Cost Center */}
            <TableCell sx={{ minWidth: 250 }}>
                <MemoizedFormComboBox2
                    id={`costCenterId-${row.tempId}`}
                    name="costCenterId"
                    data={(costCenters || []).filter((cc: any) => !!cc)}
                    textField="description"
                    dataItemKey="costCenterId"
                    value={row.costCenterId || null}
                    onChange={(e: any) => {
                        const selectedId = e?.value;
                        const selectedCc = costCenters.find((c: any) => c.costCenterId === selectedId);
                        handleRowChange(index, "costCenterId", selectedId, {
                            costCenterName: selectedCc?.description || ""
                        });
                    }}
                />
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
        </TableRow>
    );
});

const MultiPaymentItemBulkAdd: React.FC<Props> = ({ onClose, workEffortId, addItem, updateItem, deleteItem, initialItems }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "projects.multiPaymentCertificate.bulkAdd";
    const itemFormLocalizationKey = "projects.multiPaymentCertificate.itemForm";
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";

    const { data: glAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, {
        skip: !companyId,
    });

    const { data: costCenters = [] } = useGetCostCentersQuery({ type: 'out' });

    const itemTypes = useMemo(
        () => [
            { itemType: "MATERIALS", description: "المواد" },
            { itemType: "LABOR", description: "العمالة" },
            { itemType: "EQUIPMENT", description: "المعدات" },
            { itemType: "EXPENSES", description: "المصروفات" },
        ],
        []
    );

    const createEmptyRow = (): BulkAddRow => ({
        tempId: Math.random().toString(36).substring(7),
        glAccountId: "",
        costCenterId: "",
        itemType: "",
        serviceId: null,
        productId: null,
        party: null,
        description: "",
        amount: 0,
        estimatedStartDate: "",
    });

    const [rows, setRows] = useState<BulkAddRow[]>([]);
    const [localAmounts, setLocalAmounts] = useState<{ [tempId: string]: string }>({});

    useEffect(() => {
        if (initialItems && initialItems.length > 0) {
            const mappedRows = initialItems.map((item) => ({
                tempId: Math.random().toString(36).substring(7),
                workEffortId: item.workEffortId,
                glAccountId: item.glAccountId || "",
                glAccountName: item.glAccountName || "",
                projectId: item.projectId,
                subProjectId: item.subProjectId,
                subProjectName: item.subProjectName,
                costCenterId: item.costCenterId,
                costCenterName: item.costCenterName,
                itemType: item.itemType || "",
                serviceId: item.serviceId ? { ProductId: item.serviceId, ProductName: item.serviceName || "" } : null,
                productId: item.productId ? { ProductId: item.productId, ProductName: item.productName || "" } : null,
                party: item.partyIdSupplier ? { fromPartyId: item.partyIdSupplier, fromPartyName: item.partyIdSupplierName || "" } : null,
                description: item.description || "",
                amount: item.amount || 0,
                estimatedStartDate: item.estimatedStartDate ? item.estimatedStartDate.split('T')[0] : "",
            }));
            setRows(mappedRows);
            const amounts: { [tempId: string]: string } = {};
            mappedRows.forEach(r => {
                amounts[r.tempId] = r.amount.toString();
            });
            setLocalAmounts(amounts);
        } else {
            const emptyRow = createEmptyRow();
            setRows([emptyRow]);
            setLocalAmounts({ [emptyRow.tempId]: "0" });
        }
    }, [initialItems]);

    const validateRow = (row: BulkAddRow): boolean => {
        return !!row.glAccountId && !!row.description && row.amount > 0;
    };

    const serializeRow = (row: BulkAddRow): MultiPaymentItem => {
        const selectedItemType = itemTypes.find(t => t.itemType === row.itemType);
        const partyId = (row.party && row.party.fromPartyId && row.party.fromPartyId !== "0") ? row.party.fromPartyId : "";
        const partyName = partyId ? row.party.fromPartyName : "";
        return {
            workEffortId: row.workEffortId || `temp-${row.tempId}`,
            workEffortIdParent: workEffortId,
            glAccountId: row.glAccountId,
            glAccountName: row.glAccountName,
            projectId: row.projectId,
            subProjectId: row.subProjectId,
            subProjectName: row.subProjectName,
            costCenterId: row.costCenterId,
            costCenterName: row.costCenterName,
            itemType: row.itemType,
            itemTypeDescription: selectedItemType?.description || "",
            serviceId: row.serviceId?.ProductId || "",
            serviceName: row.serviceId?.ProductName || "",
            productId: row.productId?.ProductId || "",
            productName: row.productId?.ProductName || "",
            description: row.description,
            amount: row.amount,
            estimatedStartDate: row.estimatedStartDate || undefined,
            discount: 0,
            discountMode: "value",
            transportationExpenses: 0,
            gratuities: 0,
            total: row.amount,
            partyIdSupplier: partyId,
            partyIdSupplierName: partyName,
            partyIdContractor: "",
            partyIdContractorName: "",
        };
    };

    const handleAddRow = () => {
        const lastRow = rows[rows.length - 1];
        if (lastRow && !validateRow(lastRow)) {
            toast.error(getTranslatedLabel("general.mandatoryFieldsMissing", "Please fill mandatory fields: GL Account, Description, and Amount."));
            return;
        }
        const emptyRow = createEmptyRow();
        setRows([...rows, emptyRow]);
        setLocalAmounts(prev => ({ ...prev, [emptyRow.tempId]: "0" }));
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

    const findGlAccountNameById = useCallback((glAccountId: string | undefined | null): string => {
        if (!glAccountId || !Array.isArray(glAccounts) || glAccounts.length === 0) {
            return "";
        }

        const search = (nodes: any[]): string => {
            for (const node of nodes) {
                if (String(node.glAccountId) === String(glAccountId)) {
                    return node.text ?? node.accountName ?? "";
                }
                if (Array.isArray(node.items) && node.items.length > 0) {
                    const found = search(node.items);
                    if (found) return found;
                }
            }
            return "";
        };

        return search(glAccounts) || "";
    }, [glAccounts]);

    const handleRowChange = useCallback((
        index: number,
        field: keyof BulkAddRow,
        value: any,
        extraFields: Partial<BulkAddRow> = {}
    ) => {
        // Special handling for amount and date - update local state first to prevent clearing
        if (field === "amount") {
            const tempId = rows[index]?.tempId;
            if (tempId) {
                setLocalAmounts(prev => ({ ...prev, [tempId]: value || "" }));
            }
        }

        setRows((prevRows) => {
            const newRows = [...prevRows];
            const currentRow = { ...newRows[index] };

            let processedValue = value;

            if (field === "amount") {
                processedValue = parseFloat(value) || 0;
            } else if (field === "estimatedStartDate") {
                processedValue = value; // keep as string (YYYY-MM-DD)
            }

            (currentRow as any)[field] = processedValue;

            // Apply extra fields (costCenterName, subProjectName, etc.)
            Object.assign(currentRow, extraFields);

            // GL Account special logic
            if (field === "glAccountId") {
                currentRow.glAccountName = findGlAccountNameById(value);
                currentRow.projectId = undefined;
                currentRow.subProjectId = undefined;
                currentRow.subProjectName = undefined;
            }

            newRows[index] = currentRow;

            // Only sync to parent if the row is valid
            if (validateRow(currentRow)) {
                const serialized = serializeRow(currentRow);
                if (currentRow.workEffortId) {
                    updateItem(serialized);
                } else {
                    const newWorkEffortId = `temp-${currentRow.tempId}`;
                    currentRow.workEffortId = newWorkEffortId;
                    addItem(serialized);
                }
            }

            return newRows;
        });
    }, [rows, findGlAccountNameById, validateRow, serializeRow, addItem, updateItem]);
    
    const handleClose = () => {
        const invalidRows = rows.filter(row => (row.glAccountId || row.description || row.amount > 0) && !validateRow(row));
        if (invalidRows.length > 0) {
            toast.error(getTranslatedLabel("general.mandatoryFieldsMissing", "Some rows are incomplete. Please fill GL Account, Description, and Amount."));
            return;
        }
        onClose();
    };

    return (
        <Box p={2}>
            <Typography variant="h6" gutterBottom>
                {getTranslatedLabel(`${localizationKey}.title`, "إضافة بنود المدفوعات")}
            </Typography>

            <TableContainer component={Paper} sx={{ maxHeight: '60vh' }}>
                <Table size="small" stickyHeader>
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ minWidth: 350 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.glAccountId`, "GL Account")}</TableCell>
                            <TableCell sx={{ minWidth: 250 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.description`, "Description")}</TableCell>
                            <TableCell sx={{ minWidth: 120 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.amount`, "Amount")}</TableCell>
                            <TableCell sx={{ minWidth: 150 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.estimatedStartDate`, "Date")}</TableCell>
                            <TableCell sx={{ minWidth: 200 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.itemType`, "Item Type")}</TableCell>
                            <TableCell sx={{ minWidth: 300 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.service`, "Service")}</TableCell>
                            <TableCell sx={{ minWidth: 300 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.product`, "Product")}</TableCell>
                            <TableCell sx={{ minWidth: 300 }}>{getTranslatedLabel("projects.certificate.form.supplier", "Supplier")}</TableCell>
                            <TableCell sx={{ minWidth: 250 }}>{getTranslatedLabel("projects.multiPaymentCertificate.items.subProject", "Sub Project")}</TableCell>
                            <TableCell sx={{ minWidth: 250 }}>{getTranslatedLabel("accounting.payments.form.costCenter", "Cost Center")}</TableCell>
                            <TableCell sx={{ width: 50 }}></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {rows.map((row, index) => (
                            <BulkAddRowItem
                                key={row.tempId}
                                row={row}
                                index={index}
                                glAccounts={glAccounts || []}
                                handleRowChange={handleRowChange}
                                handleRemoveRow={handleRemoveRow}
                                rowsCount={rows.length}
                                costCenters={costCenters}
                                itemTypes={itemTypes}
                                localAmount={localAmounts[row.tempId] || "0"}
                            />
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>

            <Box display="flex" justifyContent="space-between" mt={2}>
                <Button startIcon={<AddIcon />} onClick={handleAddRow} variant="outlined">
                    {getTranslatedLabel("general.addRow", "إضافة بند دفع")}
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

export default MultiPaymentItemBulkAdd;
