import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridExpandChangeEvent,
} from "@progress/kendo-react-grid";
import {DataResult, State} from "@progress/kendo-data-query";
import {Box, Chip, Grid, Paper, Tooltip, Typography} from "@mui/material";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {
    useFetchAuditActivitiesQuery,
    useFetchChangesByCorrelationQuery,
} from "../../../app/store/configureStore";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {handleDatesArray} from "../../../app/util/utils";

// Expanded row: the field-level changes this one action produced, fetched by correlation id.
// This is the pivot the whole feature exists for — from "who pressed what" to "what moved".
function ChangesDetail(props: any) {
    const {getTranslatedLabel} = useTranslationHelper();
    const correlationId: string | undefined = props.dataItem?.correlationId;
    const {data, isFetching} = useFetchChangesByCorrelationQuery(correlationId!, {
        skip: !correlationId,
    });

    if (isFetching) {
        return <Box sx={{p: 2}}><Typography variant="body2">…</Typography></Box>;
    }

    if (!data || data.length === 0) {
        return (
            <Box sx={{p: 2}}>
                <Typography variant="body2" color="text.secondary">
                    {getTranslatedLabel("audit.trail.noChanges",
                        "لم تُسجَّل تغييرات على مستوى الحقول لهذا الإجراء")}
                </Typography>
                {props.dataItem?.requestJson && (
                    <Box component="pre" sx={{
                        mt: 1, p: 1, maxHeight: 160, overflow: "auto",
                        bgcolor: "grey.100", borderRadius: 1,
                        fontSize: 11, direction: "ltr", textAlign: "left",
                    }}>
                        {props.dataItem.requestJson}
                    </Box>
                )}
            </Box>
        );
    }

    return (
        <Box sx={{p: 1}}>
            <table style={{width: "100%", fontSize: 12, borderCollapse: "collapse"}}>
                <thead>
                <tr style={{background: "#eef2f7"}}>
                    <th style={{textAlign: "right", padding: 4}}>
                        {getTranslatedLabel("audit.trail.entity", "الكيان")}</th>
                    <th style={{textAlign: "right", padding: 4}}>
                        {getTranslatedLabel("audit.trail.record", "السجل")}</th>
                    <th style={{textAlign: "right", padding: 4}}>
                        {getTranslatedLabel("audit.trail.field", "الحقل")}</th>
                    <th style={{textAlign: "right", padding: 4}}>
                        {getTranslatedLabel("audit.trail.old", "القيمة السابقة")}</th>
                    <th style={{textAlign: "right", padding: 4}}>
                        {getTranslatedLabel("audit.trail.new", "القيمة الجديدة")}</th>
                </tr>
                </thead>
                <tbody>
                {data.map((c) => (
                    <tr key={c.auditHistorySeqId} style={{borderBottom: "1px solid #e2e8f0"}}>
                        <td style={{padding: 4}}>{c.changedEntityName}</td>
                        <td style={{padding: 4, direction: "ltr", textAlign: "left"}}>{c.pkCombinedValueText}</td>
                        <td style={{padding: 4}}>{c.changedFieldName}</td>
                        <td style={{padding: 4, color: "#64748b"}}>{c.oldValueText || "—"}</td>
                        <td style={{padding: 4, color: "#1d4ed8", fontWeight: 600}}>{c.newValueText || "—"}</td>
                    </tr>
                ))}
                </tbody>
            </table>
        </Box>
    );
}

export default function AuditTrailList() {
    const {getTranslatedLabel} = useTranslationHelper();
    const [dataState, setDataState] = useState<State>({take: 15, skip: 0});
    const [activities, setActivities] = useState<DataResult>({data: [], total: 0});

    const {data, isFetching} = useFetchAuditActivitiesQuery({...dataState});

    useEffect(() => {
        if (data) {
            // expanded drives the Kendo detail row; default every row collapsed.
            const adjusted = handleDatesArray(data.data).map((r: any) => ({...r, expanded: false}));
            setActivities({data: adjusted, total: data.total});
        }
    }, [data]);

    const dataStateChange = (e: GridDataStateChangeEvent) => setDataState(e.dataState);

    const expandChange = (e: GridExpandChangeEvent) => {
        const next = activities.data.map((row: any) =>
            row.activityId === e.dataItem.activityId ? {...row, expanded: !e.dataItem.expanded} : row,
        );
        setActivities({data: next, total: activities.total});
    };

    const OutcomeCell = (props: any) => (
        <td style={{textAlign: "center"}}>
            {props.dataItem.isSuccess ? (
                <Chip size="small" color="success" variant="outlined"
                      label={getTranslatedLabel("audit.trail.ok", "نجح")}/>
            ) : (
                <Tooltip title={props.dataItem.errorMessage || props.dataItem.exceptionType || ""}>
                    <Chip size="small" color="error"
                          label={getTranslatedLabel("audit.trail.failed", "فشل")}/>
                </Tooltip>
            )}
        </td>
    );

    // Slow commands are worth spotting; this surfaced an 11-second GL posting during testing.
    const DurationCell = (props: any) => {
        const ms: number | undefined = props.dataItem.durationMs;
        const slow = (ms ?? 0) >= 3000;
        return (
            <td style={{textAlign: "left", direction: "ltr", color: slow ? "#c2410c" : undefined,
                fontWeight: slow ? 600 : undefined}}>
                {ms == null ? "—" : `${ms} ms`}
            </td>
        );
    };

    return (
        <Paper elevation={5} className="div-container-withBorderCurved">
            <Grid container columnSpacing={1} alignItems="center">
                <Grid item xs={12}>
                    <Typography variant="h5" sx={{mb: 2}}>
                        {getTranslatedLabel("audit.trail.title", "سجل نشاط المستخدمين")}
                    </Typography>

                    <div className="div-container">
                        <KendoGrid
                            style={{height: "70vh"}}
                            data={activities}
                            resizable={true}
                            filterable={true}
                            sortable={true}
                            pageable={true}
                            {...dataState}
                            onDataStateChange={dataStateChange}
                            detail={ChangesDetail}
                            expandField="expanded"
                            onExpandChange={expandChange}
                        >
                            <Column field="startedAt"
                                    title={getTranslatedLabel("audit.trail.when", "التاريخ والوقت")}
                                    width={170} format="{0: dd/MM/yyyy HH:mm:ss}"/>
                            <Column field="userName"
                                    title={getTranslatedLabel("audit.trail.user", "المستخدم")} width={150}/>
                            <Column field="requestName"
                                    title={getTranslatedLabel("audit.trail.action", "الإجراء")} width={260}/>
                            <Column field="isSuccess"
                                    title={getTranslatedLabel("audit.trail.outcome", "النتيجة")}
                                    width={110} cell={OutcomeCell} filter="boolean"/>
                            <Column field="durationMs"
                                    title={getTranslatedLabel("audit.trail.duration", "المدة")}
                                    width={110} filter="numeric" cell={DurationCell}/>
                            <Column field="requestPath"
                                    title={getTranslatedLabel("audit.trail.path", "المسار")} width={280}/>
                            <Column field="errorMessage"
                                    title={getTranslatedLabel("audit.trail.error", "الخطأ")} width={260}/>
                            <Column field="clientIpAddress"
                                    title={getTranslatedLabel("audit.trail.ip", "عنوان IP")} width={140}/>
                        </KendoGrid>

                        {isFetching && (
                            <LoadingComponent
                                message={getTranslatedLabel("audit.trail.loading", "جاري تحميل سجل النشاط...")}/>
                        )}
                    </div>
                </Grid>
            </Grid>
        </Paper>
    );
}
