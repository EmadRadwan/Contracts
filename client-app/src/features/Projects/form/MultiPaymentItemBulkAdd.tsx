import React, { useState, useCallback, useMemo, useEffect } from "react";
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
}

const BulkAddRowItem: React.FC<{
    row: BulkAddRow;
    index: number;
    glAccounts: any[];
    handleRowChange: (index: number, field: keyof BulkAddRow, value: any) => void;
    handleRemoveRow: (index: number) => void;
    rowsCount: number;
    costCenters: any[];
    itemTypes: any[];
}> = ({ row, index, glAccounts, handleRowChange, handleRemoveRow, rowsCount, costCenters, itemTypes }) => {
    const { data: projects } = useFetchWorkEffortsByGlAccountIdQuery(
        { glAccountId: row.glAccountId, workEffortTypeId: "PROJECT" },
        { skip: !row.glAccountId }
    );

    const projectId = projects && projects.length > 0 ? projects[0].workEffortId : undefined;

    useEffect(() => {
        if (projectId !== row.projectId) {
            handleRowChange(index, "projectId" as any, projectId);
            if (!projectId) {
                handleRowChange(index, "subProjectId" as any, undefined);
                handleRowChange(index, "subProjectName" as any, undefined);
            }
        }
    }, [projectId, index, handleRowChange, row.projectId]);

    const { data: subProjects } = useFetchWorkEffortsByGlAccountIdQuery(
        { glAccountId: row.glAccountId, workEffortTypeId: "SUB_PROJECT", workEffortParentId: projectId },
        { skip: !projectId }
    );

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
            <TableCell>
                <FormDropDownTreeGlAccount2
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
            <TableCell>
                <TextField
                    fullWidth
                    size="small"
                    value={row.description}
                    onChange={(e) => handleRowChange(index, "description", e.target.value)}
                />
            </TableCell>
            <TableCell>
                <TextField
                    fullWidth
                    size="small"
                    type="number"
                    value={row.amount}
                    onChange={(e) => handleRowChange(index, "amount", parseFloat(e.target.value) || 0)}
                    inputProps={{ min: 0, step: "0.01" }}
                />
            </TableCell>
            <TableCell>
                <MemoizedFormComboBox2
                    data={itemTypes}
                    textField="description"
                    dataItemKey="itemType"
                    value={row.itemType || null}
                    onChange={(e: any) => handleRowChange(index, "itemType", e.value)}
                />
            </TableCell>
            <TableCell>
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
            <TableCell>
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
            <TableCell>
                <FormComboBoxVirtualAllParties
                    value={row.party}
                    onChange={(e: any) => handleRowChange(index, "party", e.value)}
                    name="party"
                    touched={false}
                    visited={false}
                    modified={false}
                />
            </TableCell>
            <TableCell>
                {subProjects && subProjects.length > 0 ? (
                    <MemoizedFormComboBox2
                        data={subProjects.filter((sp: any) => !!sp)}
                        textField="subProjectName"
                        dataItemKey="workEffortId"
                        value={row.subProjectId || null}
                        onChange={(e: any) => {
                            handleRowChange(index, "subProjectId" as any, e.value);
                            const sp = subProjects.find((p: any) => p.workEffortId === e.value);
                            handleRowChange(index, "subProjectName" as any, sp?.subProjectName);
                        }}
                    />
                ) : (
                    "-"
                )}
            </TableCell>
            <TableCell>
                <MemoizedFormComboBox2
                    id="costCenterId"
                    name="costCenterId"
                    data={(costCenters || []).filter((cc: any) => !!cc)}
                    textField="description"
                    dataItemKey="costCenterId"
                    value={row.costCenterId || null}
                    onChange={(e: any) => {
                        handleRowChange(index, "costCenterId" as any, e.value);
                        const cc = costCenters.find((c: any) => c.costCenterId === e.value);
                        handleRowChange(index, "costCenterName" as any, cc?.description);
                    }}
                />
            </TableCell>
            <TableCell>
                <IconButton color="error" onClick={() => handleRemoveRow(index)} disabled={rowsCount === 1}>
                    <DeleteIcon />
                </IconButton>
            </TableCell>
        </TableRow>
    );
};

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
    });

    const [rows, setRows] = useState<BulkAddRow[]>([]);
    const [deletedWorkEffortIds, setDeletedWorkEffortIds] = useState<string[]>([]);

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
            }));
            setRows(mappedRows);
        } else {
            setRows([createEmptyRow()]);
        }
    }, [initialItems]);

    const handleAddRow = () => {
        setRows([...rows, createEmptyRow()]);
    };

    const handleRemoveRow = (index: number) => {
        const rowToRemove = rows[index];
        if (rowToRemove.workEffortId) {
            setDeletedWorkEffortIds((prev) => [...prev, rowToRemove.workEffortId!]);
        }
        const newRows = [...rows];
        newRows.splice(index, 1);
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

    const handleRowChange = (index: number, field: keyof BulkAddRow, value: any) => {
        const newRows = [...rows];
        newRows[index] = { ...newRows[index], [field]: value };
        
        if (field === "glAccountId") {
            newRows[index].glAccountName = findGlAccountNameById(value);
        }

        if (field === "glAccountId") {
            newRows[index].projectId = undefined;
            newRows[index].subProjectId = undefined;
        }

        setRows(newRows);
    };

    const handleSave = () => {
        // Handle deleted items
        deletedWorkEffortIds.forEach((id) => deleteItem(id));

        console.log('rows', rows)
        rows.forEach(row => {
            if (!row.glAccountId || !row.description || !row.itemType || row.amount <= 0) return;

            const selectedItemType = itemTypes.find(t => t.itemType === row.itemType);

            const serializedItem: MultiPaymentItem = {
                workEffortId: row.workEffortId || `temp-${Date.now()}-${row.tempId}`,
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
                discount: 0,
                discountMode: "value",
                transportationExpenses: 0,
                gratuities: 0,
                total: row.amount, // Total is just amount since we ignore discounts etc.
                partyIdSupplier: row.party?.fromPartyId || "",
                partyIdSupplierName: row.party?.fromPartyName || "",
                partyIdContractor: "",
                partyIdContractorName: "",
            };

            if (row.workEffortId) {
                updateItem(serializedItem);
            } else {
                addItem(serializedItem);
            }
        });
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
                    <Button onClick={handleSave} variant="contained" color="primary">
                        {getTranslatedLabel("general.save", "Save")}
                    </Button>
                </Box>
            </Box>
        </Box>
    );
};

export default MultiPaymentItemBulkAdd;
