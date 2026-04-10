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
import { useFetchGlAccountOrganizationHierarchyLovQuery } from "../../../app/store/apis";
import { FormDropDownTreeGlAccount2 } from "../../../app/common/form/FormDropDownTreeGlAccount2";
import { FormSimpleComboBoxServiceVirtual } from "../../../app/common/form/FormSimpleComboBoxServiceVirtual";
import { FormSimpleComboBoxRawMaterialVirtual } from "../../../app/common/form/FormSimpleComboBoxRawMaterialVirtual";
import { FormComboBoxVirtualAllParties } from "../../../app/common/form/FormComboBoxVirtualAllParties";

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
    serviceId: any; // { ProductId, ProductName }
    productId: any; // { ProductId, ProductName }
    party: any; // { fromPartyId, fromPartyName }
    description: string;
    amount: number;
}

const MultiPaymentItemBulkAdd: React.FC<Props> = ({ onClose, workEffortId, addItem, updateItem, deleteItem, initialItems }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "projects.multiPaymentCertificate.bulkAdd";
    const itemFormLocalizationKey = "projects.multiPaymentCertificate.itemForm";
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";

    const { data: glAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, {
        skip: !companyId,
    });

    const createEmptyRow = (): BulkAddRow => ({
        tempId: Math.random().toString(36).substring(7),
        glAccountId: "",
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

        if (field === "serviceId" && value) {
            newRows[index].productId = null;
        }

        if (field === "productId" && value) {
            newRows[index].serviceId = null;
        }

        setRows(newRows);
    };

    const handleSave = () => {
        // Handle deleted items
        deletedWorkEffortIds.forEach((id) => deleteItem(id));

        rows.forEach(row => {
            if (!row.glAccountId || row.amount <= 0) return;

            const serializedItem: MultiPaymentItem = {
                workEffortId: row.workEffortId || `temp-${Date.now()}-${row.tempId}`,
                workEffortIdParent: workEffortId,
                glAccountId: row.glAccountId,
                glAccountName: row.glAccountName,
                itemType: row.productId ? "MATERIALS" : "SERVICES",
                itemTypeDescription: row.productId ? "Materials" : "Services",
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
                            <TableCell sx={{ minWidth: 250 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.glAccountId`, "GL Account")}</TableCell>
                            <TableCell sx={{ minWidth: 250 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.description`, "Description")}</TableCell>
                            <TableCell sx={{ minWidth: 120 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.amount`, "Amount")}</TableCell>
                            <TableCell sx={{ minWidth: 200 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.service`, "Service")}</TableCell>
                            <TableCell sx={{ minWidth: 200 }}>{getTranslatedLabel(`${itemFormLocalizationKey}.product`, "Product")}</TableCell>
                            <TableCell sx={{ minWidth: 200 }}>{getTranslatedLabel("projects.certificate.form.supplier", "Supplier")}</TableCell>
                            <TableCell sx={{ width: 50 }}></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {rows.map((row, index) => (
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
                                        // Mock FieldRenderProps
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
                                    <FormSimpleComboBoxServiceVirtual
                                        value={row.serviceId}
                                        onChange={(e: any) => handleRowChange(index, "serviceId", e.value)}
                                        textField="productName"
                                        dataItemKey="productId"
                                        // Mock FieldRenderProps
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
                                        // Mock FieldRenderProps
                                        name="productId"
                                        touched={false}
                                        visited={false}
                                        modified={false}
                                    />
                                </TableCell>
                                <TableCell>
                                    <FormComboBoxVirtualAllParties
                                        value={row.party}
                                        onChange={(e: any) => handleRowChange(index, "party", e.value)}
                                        // Mock FieldRenderProps
                                        name="party"
                                        touched={false}
                                        visited={false}
                                        modified={false}
                                    />
                                </TableCell>
                                <TableCell>
                                    <IconButton color="error" onClick={() => handleRemoveRow(index)} disabled={rows.length === 1}>
                                        <DeleteIcon />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
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
