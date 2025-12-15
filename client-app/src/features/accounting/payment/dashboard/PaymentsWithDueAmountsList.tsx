import { RootState, useAppDispatch, useAppSelector } from "../../../../app/store/configureStore"; // REFACTOR: Updated import to use the new RTK Query hook for the combined payments endpoint
import React, { useEffect, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridColumnMenuFilter,
    GridDataStateChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import Button from "@mui/material/Button";
import { Grid, Paper } from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { handleDatesArray } from "../../../../app/util/utils";
import PaymentForm from "../form/PaymentForm";
import { resetForm, setFormEditMode, setPaymentType } from "../slice/paymentsUiSlice";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useLocation, useNavigate } from "react-router";
import { setSelectedPayment } from "../../slice/accountingSharedUiSlice";
import { useSelector } from "react-redux";
import {useFetchPaymentsWithDueStatusQuery} from "../../../../app/store/apis"; // Note: You may need to update or create a version of this report that handles both directions

// REFACTOR: Removed paymentType prop – this component now shows both incoming and outgoing payments with due status
// This simplifies the UI and eliminates the need for separate incoming/outgoing lists
export default function PaymentsWithDueAmountsList() {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.payments.list";
    const location = useLocation();
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const companyName = useSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);

    const [payments, setPayments] = React.useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = React.useState<State>({
        sort: [{ field: "effectiveDate", dir: "desc" }],
        skip: 0,
        take: 20, // REFACTOR: Increased default page size slightly for combined view
    });

    const formEditMode = useAppSelector((s) => s.paymentsUi.formEditMode);

    useEffect(() => {
        if (location.state?.resetPaymentForm) {
            dispatch(resetForm());
            navigate(location.pathname, { replace: true, state: {} });
        }
    }, [location.state, dispatch, navigate]);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    // REFACTOR: Switched to new query hook and removed paymentType argument
    // The new endpoint returns both payment directions and includes DueStatusArabic
    const { data, error, isFetching, refetch } = useFetchPaymentsWithDueStatusQuery(dataState);

    React.useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setPayments({ data: adjustedData, total: data.total });
        }
    }, [data]);

    const handleSelectPayment = (paymentId: string) => {
        const pay = payments.data.find((p: any) => p.paymentId === paymentId);
        if (!pay) return;

        dispatch(setSelectedPayment(pay));

        // REFACTOR: Dynamically set paymentType based on IsDisbursement flag from the new data
        // This correctly handles both incoming and outgoing payments in the same list
        dispatch(setPaymentType(pay.isDisbursement ? 2 : 1)); // 2 = outgoing/disbursement, 1 = incoming

        const statusMap: Record<string, number> = {
            "PMNT_NOT_PAID": 2,
            "PMNT_RECEIVED": 3,
            "PMNT_SENT": 4,
            "PMNT_CONFIRMED": 5,
            "PMNT_CANCELLED": 6,
        };
        const mode = statusMap[pay.statusId] ?? 2;
        dispatch(setFormEditMode(mode));
    };

    const PaymentDescriptionCell = (props: any) => {
        const field = props.field || "";
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);

        return (
            <td
                className={props.className}
                style={{ ...props.style, color: "blue" }}
                colSpan={props.colSpan}
                role={"gridcell"}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button onClick={() => handleSelectPayment(props.dataItem.paymentId)}>
                    {props.dataItem.paymentId}
                </Button>
            </td>
        );
    };

    
    if (formEditMode > 0) {
        return <PaymentForm editMode={formEditMode} cancelEdit={() => dispatch(resetForm())} />;
    }

    const getDueStatusArabic = (daysUntilDue: number, isDisbursement: boolean): string => {
        const type = isDisbursement ? "دفعة" : "مستحق";
        const typePaid = isDisbursement ? "دفعة مستحقة" : "مستحق";

        if (daysUntilDue < 0) {
            const daysOverdue = Math.abs(daysUntilDue);
            if (daysOverdue <= 30) {
                return `${type} متأخرة منذ ${daysOverdue} يوم`;
            }
            return `${type} متأخرة جداً`;
        }

        if (daysUntilDue === 0) return `${typePaid} اليوم`;
        if (daysUntilDue === 1) return `${typePaid} غداً`;
        if (daysUntilDue <= 3) return `${typePaid} بعد ${daysUntilDue} أيام`;
        if (daysUntilDue <= 7) return `${typePaid} هذا الأسبوع`;
        if (daysUntilDue <= 30) return `${typePaid} خلال الشهر`;
        if (daysUntilDue <= 90) return `${typePaid} خلال 3 أشهر`;

        return `${typePaid} لاحقاً`;
    };

// Custom cell for Due Status
    const DueStatusCell = (props: any) => {
        const { daysUntilDue, isDisbursement } = props.dataItem;
        const status = getDueStatusArabic(daysUntilDue ?? 0, isDisbursement ?? false);

        return (
            <td style={{ ...props.style, textAlign: "center" }}>
      <span style={{
          padding: "4px 8px",
          borderRadius: "4px",
          backgroundColor: daysUntilDue < 0 ? "#ffebee" : daysUntilDue <= 7 ? "#fff3e0" : "#e8f5e8",
          color: daysUntilDue < 0 ? "#c62828" : daysUntilDue <= 7 ? "#ef6c00" : "#2e7d32",
          fontWeight: "bold"
      }}>
        {status}
      </span>
            </td>
        );
    };
    

    return (
        <>
            <AccountingMenu selectedMenuItem={"/payments"} />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{ height: "65vh", flex: 1 }}
                                data={payments || { data: [], total: 0 }}
                                resizable={true}
                                filterable={true}
                                sortable={true}
                                pageable={true}
                                {...dataState}
                                onDataStateChange={dataStateChange}
                            >

                                <Column
                                    field="paymentId"
                                    title={getTranslatedLabel(`${localizationKey}.paymentId`, "Payment Number")}
                                    cell={PaymentDescriptionCell}
                                    width={150}
                                />
                                <Column
                                    field="paymentTypeDescription"
                                    title={getTranslatedLabel(`${localizationKey}.paymentType`, "Payment Type")}
                                    width={150}
                                />
                                <Column field="orderId" title={getTranslatedLabel(`${localizationKey}.orderId`, "Order ID")} width={150} />
                                <Column
                                    field="certificateNumber"
                                    title={getTranslatedLabel(`${localizationKey}.certificateNumber`, "Certificate Number")}
                                    width={150}
                                />
                                <Column
                                    field="partyIdFromName"
                                    title={getTranslatedLabel(`${localizationKey}.from`, "From Party")}
                                    width={180}
                                />
                                <Column
                                    field="partyIdToName"
                                    title={getTranslatedLabel(`${localizationKey}.to`, "To Party")}
                                    width={180}
                                />
                                <Column
                                    field="effectiveDate"
                                    title={getTranslatedLabel(`${localizationKey}.date`, "Payment Date")}
                                    width={150}
                                    format="{0:dd/MM/yyyy}"
                                />
                                <Column
                                    field="daysUntilDue"
                                    title={getTranslatedLabel(`${localizationKey}.dueStatus`, "Due Status")}
                                    width={220}
                                    cell={DueStatusCell} 
                                    filter={"numeric"}
                                />
                                <Column
                                    field="statusDescription"
                                    title={getTranslatedLabel(`${localizationKey}.status`, "Status")}
                                    width={140}
                                />
                                <Column field="amount" title={getTranslatedLabel(`${localizationKey}.amount`, "Amount")} width={130} />
                                <Column field="projectName" title={getTranslatedLabel(`${localizationKey}.projectName`, "Project")} width={180} />
                                <Column
                                    field="costCenterDescription"
                                    title={getTranslatedLabel(`${localizationKey}.costCenterDescription`, "Cost Center")}
                                    width={180}
                                />
                                <Column field="comments" title={getTranslatedLabel(`${localizationKey}.comments`, "Comments")} width={200} />
                            </KendoGrid>
                            {isFetching && <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, "Loading Payments...")} />}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}