import React, {useState, useEffect} from "react";
import { 
    Grid as KendoGrid, 
    GridColumn as Column, 
    GridItemChangeEvent, 
    GridCellProps,
    GridDataStateChangeEvent,
    GridRowProps,
    GridToolbar 
} from "@progress/kendo-react-grid";
import { Button, Box, Typography, CircularProgress } from "@mui/material";
import { ComboBox } from "@progress/kendo-react-dropdowns";
import { toast } from "react-toastify";
import { process, State } from "@progress/kendo-data-query";

import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { GlAccount } from "../../../../app/models/accounting/globalGlSettings";
import { 
    useFetchGlReportsQuery, 
    useFetchGlClassCoursesQuery, 
    useFetchGlSubClassesQuery, 
    useFetchGlSubClasses2Query, 
    useFetchGlAccountCourseLabelsQuery 
} from "../../../../app/store/apis/accounting/globalGlSettingsApi";
import { 
    useFetchOrganizationGlAccountsForBulkEditQuery, 
    useBulkUpdateOrganizationGlAccountPropsMutation,
    useFetchFullChartOfAccountsQuery
} from "../../../../app/store/apis/accounting/organizationGlChartOfAccountsApi";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { AdjustPowerBIPropsExcel } from "../report/AdjustPowerBIPropsExcel";

interface Props {
    companyId: string;
    onClose: () => void;
}

interface BulkEditRow extends GlAccount {
    inEdit?: boolean;
}

const AdjustPowerBIPropsBulkEdit: React.FC<Props> = ({ companyId, onClose }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    
    const [data, setData] = useState<BulkEditRow[]>([]);
    const [total, setTotal] = useState(0);
    const [dataState, setDataState] = useState<State>({
        take: 10,
        skip: 0,
        sort: [],
        filter: { logic: "and", filters: [] }
    });
    const [pendingChanges, setPendingChanges] = useState<Record<string, Partial<GlAccount>>>({});
    const [activeEditIds, setActiveEditIds] = useState<string[]>([]);

    const { data: pagedData, isFetching } = useFetchOrganizationGlAccountsForBulkEditQuery(
        { companyId, dataState }, 
        { skip: !companyId }
    );

    const { data: allAccountsData } = useFetchFullChartOfAccountsQuery(
        { companyId }, 
        { skip: !companyId }
    );
    
    // LOVs
    const { data: glReportsData } = useFetchGlReportsQuery({});
    const { data: glClassCoursesData } = useFetchGlClassCoursesQuery({});
    const { data: glSubClassesData } = useFetchGlSubClassesQuery({});
    const { data: glSubClasses2Data } = useFetchGlSubClasses2Query({});
    const { data: glAccountCourseLabelsData } = useFetchGlAccountCourseLabelsQuery({});

    const [bulkUpdate, { isLoading: isUpdating }] = useBulkUpdateOrganizationGlAccountPropsMutation();

    useEffect(() => {
        console.log("🔄 AdjustPowerBIPropsBulkEdit useEffect triggered", {
            pagedDataLength: pagedData?.data?.length,
            pendingChangesCount: Object.keys(pendingChanges).length,
            activeEditIds
        });
        if (pagedData) {
            const merged = pagedData.data.map(item => {
                const id = item.glAccountId || (item as any).GlAccountId;
                const pending = pendingChanges[id];
                return { 
                    ...item, 
                    glAccountId: id,
                    ...pending, 
                    inEdit: activeEditIds.includes(id) 
                };
            });
            setData(merged);
            setTotal(pagedData.total);
        }
    }, [pagedData, pendingChanges, activeEditIds]);

    const handleRowChange = (event: GridItemChangeEvent) => {
        const field = event.field || "";
        const glAccountId = event.dataItem.glAccountId || (event.dataItem as any).GlAccountId;

        console.log("✅ handleRowChange triggered!", {
            field,
            value: event.value,
            glAccountId
        });

        if (!glAccountId) {
            console.error("❌ glAccountId is missing in event.dataItem", event.dataItem);
            return;
        }

        setPendingChanges(prev => {
            const newPending = {
                ...prev,
                [glAccountId]: {
                    ...(prev[glAccountId] || {}),
                    glAccountId,
                    [field]: event.value
                }
            };
            console.log("📝 Updated pendingChanges:", newPending);
            return newPending;
        });
    };

    const onDataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const enterEdit = (row: BulkEditRow) => {
        const id = row.glAccountId || (row as any).GlAccountId;
        setActiveEditIds(prev => [...prev, id]);
    };

    const cancelEdit = (row: BulkEditRow) => {
        const id = row.glAccountId || (row as any).GlAccountId;
        setActiveEditIds(prev => prev.filter(item => item !== id));
        // Revert to original data from pagedData if it was in pendingChanges but not yet saved to server
        const original = pagedData?.data.find(o => (o.glAccountId || (o as any).GlAccountId) === id);
        if (original) {
            setData(prev => prev.map(r => (r.glAccountId || (r as any).GlAccountId) === id ? { ...original, inEdit: false } : r));
            setPendingChanges(prev => {
                const updated = { ...prev };
                delete updated[id];
                return updated;
            });
        }
    };

    const doneEdit = (row: BulkEditRow) => {
        const id = row.glAccountId || (row as any).GlAccountId;
        setActiveEditIds(prev => prev.filter(item => item !== id));
    };

    const saveAllChanges = async () => {
        const updates = Object.values(pendingChanges);
        if (updates.length === 0) return;

        try {
            await bulkUpdate({ updates }).unwrap();
            toast.success(getTranslatedLabel("general.success", "Updated successfully"));
            setPendingChanges({});
            setActiveEditIds([]);
        } catch (error) {
            toast.error(getTranslatedLabel("general.error", "Update failed"));
        }
    };

    const hasPendingChanges = Object.keys(pendingChanges).length > 0;

    // Custom Cells for Dropdowns
    const DropDownCell = (props: GridCellProps, dataItems: any[], dataItemKey: string, textField: string) => {
        const { dataItem } = props;
        const field = props.field || "";

        if (dataItem.inEdit) {
            const selectedValue = dataItems.find(i => i[dataItemKey] === dataItem[field]);
            return (
                <td>
                    <ComboBox
                        data={dataItems}
                        value={selectedValue || null}
                        onChange={(e) => {
                            props.onChange!({
                                dataItem,
                                field,
                                value: e.value ? e.value[dataItemKey] : null
                            } as any);
                        }}
                        textField={textField}
                        dataItemKey={dataItemKey}
                        filterable={true}
                    />
                </td>
            );
        }

        // Display description from dataItems if available (to reflect local ID changes), otherwise from record
        const descField = field.replace("Id", "Description");
        const selectedItem = dataItems.find(i => i[dataItemKey] === dataItem[field]);
        const displayValue = selectedItem?.[textField] || dataItem[descField] || dataItem[field] || "";
        return <td>{displayValue}</td>;
    };

    const CommandCell = (props: GridCellProps) => {
        const { dataItem } = props;
        const id = dataItem.glAccountId || (dataItem as any).GlAccountId;
        const isInEdit = activeEditIds.includes(id);

        return (
            <td className="k-command-cell">
                {isInEdit ? (
                    <Box display="flex" gap={1} alignItems="center">
                        <Button 
                            onClick={() => doneEdit(dataItem)} 
                            variant="contained" 
                            size="small" 
                            color="success"
                        >
                            {getTranslatedLabel("general.done", "Done")}
                        </Button>
                        <Button onClick={() => cancelEdit(dataItem)} variant="outlined" size="small" color="error">
                            {getTranslatedLabel("general.cancel", "Cancel")}
                        </Button>
                    </Box>
                ) : (
                    <Button onClick={() => enterEdit(dataItem)} variant="contained" size="small">
                        {getTranslatedLabel("general.edit", "Edit")}
                    </Button>
                )}
            </td>
        );
    };

    const rowRender = (trElement: React.ReactElement<HTMLTableRowElement>, props: GridRowProps) => {
        const id = props.dataItem.glAccountId || (props.dataItem as any).GlAccountId;
        const isPending = !!pendingChanges[id];
        const style = isPending 
            ? { ...trElement.props.style, backgroundColor: 'rgba(255, 255, 0, 0.15)' } 
            : trElement.props.style;
        return React.cloneElement(trElement, { ...trElement.props, style });
    };

    return (
        <>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                <Typography variant="h6">
                    {getTranslatedLabel("accounting.glAccount.bulkEdit.powerBiProps", "Adjust Power BI Props")}
                </Typography>
            </Box>

            <KendoGrid
                data={data}
                total={total}
                {...dataState}
                onDataStateChange={onDataStateChange}
                sortable={true}
                filterable={true}
                pageable={true}
                onItemChange={handleRowChange}
                editField="inEdit"
                dataItemKey="glAccountId"
                style={{ height: '75vh', width: '85%' }}
                resizable={true}
                rowRender={rowRender}
            >
                <GridToolbar>
                    <Box display="flex" alignItems="center">
                        <Button 
                            onClick={saveAllChanges} 
                            variant="contained" 
                            color="primary" 
                            disabled={!hasPendingChanges || isUpdating} 
                            sx={{ mr: 1 }}
                        >
                            {isUpdating ? <CircularProgress size={20} color="inherit" /> : getTranslatedLabel("general.save", "Save Changes")}
                        </Button>
                        <Button onClick={onClose} variant="outlined">
                            {getTranslatedLabel("general.back", "Back")}
                        </Button>
                        {allAccountsData && (
                            <AdjustPowerBIPropsExcel 
                                accounts={allAccountsData} 
                                companyId={companyId} 
                                getTranslatedLabel={getTranslatedLabel} 
                            />
                        )}
                    </Box>
                </GridToolbar>
                <Column cell={CommandCell} width={180} locked />
                <Column field="glAccountId" title={getTranslatedLabel("accounting.glAccount.list.accountId", "Account ID")} width={120} editable={false} />
                <Column field="accountName" title={getTranslatedLabel("accounting.glAccount.list.accountName", "Account Name")} width={300} editable={false} />
                
                <Column 
                    field="glReportId" 
                    title={getTranslatedLabel("accounting.glAccount.list.report", "Report")} 
                    width={220}
                    cell={(p) => DropDownCell(p, glReportsData?.glReports || [], "glReportId", "description")}
                />
                <Column 
                    field="glClassCourseId" 
                    title={getTranslatedLabel("accounting.glAccount.list.classCourse", "Class Course")} 
                    width={220}
                    cell={(p) => DropDownCell(p, glClassCoursesData?.glClassCourses || [], "glClassCourseId", "description")}
                />
                <Column 
                    field="glSubClassId" 
                    title={getTranslatedLabel("accounting.glAccount.list.subClass", "Sub Class")} 
                    width={220}
                    cell={(p) => DropDownCell(p, glSubClassesData?.glSubClasses || [], "glSubClassId", "description")}
                />
                <Column 
                    field="glSubClass2Id" 
                    title={getTranslatedLabel("accounting.glAccount.list.subClass2", "Sub Class 2")} 
                    width={220}
                    cell={(p) => DropDownCell(p, glSubClasses2Data?.glSubClasses2 || [], "glSubClass2Id", "description")}
                />
                <Column 
                    field="glAccountCourseLabelId" 
                    title={getTranslatedLabel("accounting.glAccount.list.courseLabel", "Course Label")} 
                    width={220}
                    cell={(p) => DropDownCell(p, glAccountCourseLabelsData?.glAccountCourseLabels || [], "glAccountCourseLabelId", "description")}
                />
            </KendoGrid>

            {(isFetching || isUpdating) && <LoadingComponent />}
        </>
    );
};

export default AdjustPowerBIPropsBulkEdit;
