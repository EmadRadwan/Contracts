import React from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
} from "@progress/kendo-react-grid";
import {Box, Typography} from "@mui/material";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {useFetchRecordHistoryQuery} from "../../../app/store/configureStore";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {handleDatesArray} from "../../../app/util/utils";
import {EntityAuditLog} from "../../../app/models/auditing/entityAuditLog";

interface Props {
    // Domain class name as the audit log records it, e.g. "Payment", "Invoice", "SalesCommission".
    entityName: string;
    // Composite key text exactly as written by the interceptor, e.g. "PaymentId=O17827".
    pkText: string;
    height?: string;
}

// Renders "field: old -> new, by whom, when" for one business record.
// Drop this into any form as a History tab; it needs nothing but the entity name and key.
export default function RecordHistory({entityName, pkText, height = "45vh"}: Props) {
    const {getTranslatedLabel} = useTranslationHelper();
    const {data, isFetching} = useFetchRecordHistoryQuery(
        {entityName, pkText},
        {skip: !entityName || !pkText},
    );

    const rows: EntityAuditLog[] = React.useMemo(
        () => (data ? handleDatesArray(data as any) : []),
        [data],
    );

    // *CREATE* / *DELETE* markers carry no before/after values, so show them as a single
    // readable line rather than an empty old -> new pair.
    const ValueCell = (props: any) => {
        const marker = props.dataItem.changedFieldName;
        if (marker === "*CREATE*" || marker === "*DELETE*") {
            return (
                <td>
                    <Typography variant="body2" color={marker === "*DELETE*" ? "error.main" : "success.main"}>
                        {marker === "*CREATE*"
                            ? getTranslatedLabel("audit.history.created", "تم الإنشاء")
                            : getTranslatedLabel("audit.history.deleted", "تم الحذف")}
                    </Typography>
                </td>
            );
        }
        return (
            <td>
                <Typography variant="body2" component="span" color="text.secondary">
                    {props.dataItem.oldValueText || "—"}
                </Typography>
                <Typography variant="body2" component="span" sx={{mx: 1}}>←</Typography>
                <Typography variant="body2" component="span" color="primary.main" fontWeight={600}>
                    {props.dataItem.newValueText || "—"}
                </Typography>
            </td>
        );
    };

    // CHANGED_BY_INFO is stored as "name (AspNetUsers.Id)". The GUID is what makes the value
    // survive a later rename, but it is noise on screen — show the name, keep the id in a tooltip.
    const ByCell = (props: any) => {
        const raw: string = props.dataItem.changedByInfo || "";
        const name = raw.includes(" (") ? raw.slice(0, raw.indexOf(" (")) : raw;
        return (
            <td title={raw}>
                <Typography variant="body2">{name || "—"}</Typography>
            </td>
        );
    };

    if (isFetching) {
        return <LoadingComponent message={getTranslatedLabel("audit.history.loading", "جاري تحميل سجل التغييرات...")}/>;
    }

    if (!rows.length) {
        return (
            <Box sx={{p: 3, textAlign: "center"}}>
                <Typography color="text.secondary">
                    {getTranslatedLabel("audit.history.empty", "لا توجد تغييرات مسجلة لهذا السجل")}
                </Typography>
            </Box>
        );
    }

    return (
        <KendoGrid style={{height}} data={rows} resizable={true} sortable={true}>
            <Column
                field="changedDate"
                title={getTranslatedLabel("audit.history.date", "التاريخ")}
                width={160}
                format="{0: dd/MM/yyyy HH:mm}"
            />
            <Column
                field="changedFieldName"
                title={getTranslatedLabel("audit.history.field", "الحقل")}
                width={180}
            />
            <Column
                title={getTranslatedLabel("audit.history.change", "التغيير")}
                cell={ValueCell}
                width={300}
            />
            <Column
                field="changedByInfo"
                title={getTranslatedLabel("audit.history.by", "بواسطة")}
                cell={ByCell}
                width={150}
            />
        </KendoGrid>
    );
}
