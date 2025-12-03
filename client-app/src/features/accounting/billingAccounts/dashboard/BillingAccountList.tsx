// BillingAccountsList.tsx – fixed version
import React, { useEffect, useState } from "react";
import { Grid as KendoGrid, GridColumn as Column, GridToolbar } from "@progress/kendo-react-grid";
import { Grid, Paper } from "@mui/material";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {handleDatesArray, handleDatesObject, parseDate} from "../../../../app/util/utils";
import { useLocation } from "react-router-dom";
import { State } from "@progress/kendo-data-query";

import AccountingMenu from "../../invoice/menu/AccountingMenu";
import BillingAccountForm from "../form/BillingAccountForm";

import {
    useAppDispatch,
    useAppSelector,
    useFetchBillingAccountsQuery,
} from "../../../../app/store/configureStore";
import { setSelectedBillingAccount } from "../../slice/accountingSharedUiSlice";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { BillingAccount } from "../../../../app/models/accounting/billingAccount";

function BillingAccountsList() {
    const [editMode, setEditMode] = useState(0);
    const location = useLocation();
    const dispatch = useAppDispatch();
    const { selectedBillingAccount } = useAppSelector(state => state.accountingSharedUi);
    const { getTranslatedLabel } = useTranslationHelper();

    const [dataState, setDataState] = useState<State>({ take: 9, skip: 0 });
    const [billingAccounts, setBillingAccounts] = useState({ data: [], total: 0 });

    const { data, isFetching } = useFetchBillingAccountsQuery({ ...dataState });

    // Adjust dates for the grid
    useEffect(() => {
        if (data) {
            const adjusted = handleDatesArray(data.data);
            setBillingAccounts({ data: adjusted, total: data.total });
        }
    }, [data]);

    const dataStateChange = (e: any) => setDataState(e.dataState);

    // ---------- Selection ----------
    const handleSelectBillingAccounts = (billingAccountId: string) => {
        const account = billingAccounts.data.find(
            (a: any) => a.billingAccountId === billingAccountId
        );
        if (account) {
            dispatch(
                setSelectedBillingAccount({
                    ...account,
                    partyId: { fromPartyId: account.partyId, fromPartyName: account.partyName },
                })
            );
            setEditMode(2);
        }
    };

    // ---------- Navigation from other pages ----------
    useEffect(() => {
        if (location.state?.selectedBillingAccountId && billingAccounts.data.length > 0) {
            handleSelectBillingAccounts(location.state.selectedBillingAccountId);
        }
    }, [location.state?.selectedBillingAccountId, billingAccounts.data]);

    const cancelEdit = () => {
        dispatch(setSelectedBillingAccount(undefined));
        setEditMode(0);
    };

    // ---------- Callback after creation (keeps form open) ----------
    const handleBillingAccountCreated = (response: any) => {
        if (response?.isSuccess && response?.value) {
            const raw = response.value;

            const account: BillingAccount = {
                ...raw,

                // Convert all date strings → Date objects
                fromDate: parseDate(raw.fromDate),
                thruDate: parseDate(raw.thruDate),
                createdDate: parseDate(raw.createdDate),

                // Normalize combo box fields
                partyId: {
                    fromPartyId: raw.partyId,
                    fromPartyName: raw.partyName,
                },
                projectId: raw.projectId
                    ? {
                        projectId: raw.projectId,
                        ProjectName: raw.projectName ?? null,
                    }
                    : null,
            };

            dispatch(setSelectedBillingAccount(account));
            setEditMode(2);
        }
    };

    const BillingAccountCell = (props: any) => (
        <td style={{ color: "blue" }}>
            <Button onClick={() => handleSelectBillingAccounts(props.dataItem.billingAccountId)}>
                {props.dataItem.billingAccountId}
            </Button>
        </td>
    );

    // ──────────────────────────────────────────────────────────────
    // 1. If we are in create / edit mode → show ONLY the form
    // 2. Otherwise → show the list
    // ──────────────────────────────────────────────────────────────
    if (editMode > 0) {
        return (
            <BillingAccountForm
                selectedBillingAccount={selectedBillingAccount}
                editMode={editMode}
                onClose={cancelEdit}
                setEditMode={setEditMode}
                onBillingAccountCreated={handleBillingAccountCreated}
            />
        );
    }

    // ──────────────────────────────────────────────────────────────
    // Normal list view
    // ──────────────────────────────────────────────────────────────
    return (
        <>
            <AccountingMenu selectedMenuItem="/billingAccounts" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{ height: "65vh", width: "94vw" }}
                                data={billingAccounts}
                                resizable
                                filterable
                                sortable
                                pageable
                                {...dataState}
                                onDataStateChange={dataStateChange}
                            >
                                <GridToolbar>
                                    <Button
                                        color="secondary"
                                        variant="outlined"
                                        onClick={() => setEditMode(1)}
                                    >
                                        {getTranslatedLabel(
                                            "accounting.billingAccounts.list.new",
                                            "إنشاء حساب أجل جديد"
                                        )}
                                    </Button>
                                </GridToolbar>

                                <Column
                                    field="billingAccountId"
                                    title={getTranslatedLabel("accounting.billingAccounts.list.accountId", "رقم الحساب")}
                                    cell={BillingAccountCell}
                                />
                                <Column
                                    field="partyName"
                                    title={getTranslatedLabel("accounting.billingAccounts.list.party", "اسم العميل")}
                                    width={250}
                                />
                                <Column
                                    field="accountLimit"
                                    title={getTranslatedLabel("accounting.billingAccounts.list.limit", "حد الحساب")}
                                />
                                <Column
                                    field="fromDate"
                                    title={getTranslatedLabel("accounting.billingAccounts.list.fromDate", "من تاريخ")}
                                    format="{0:dd/MM/yyyy}"
                                />
                                <Column
                                    field="thruDate"
                                    title={getTranslatedLabel("accounting.billingAccounts.list.thruDate", "إلى تاريخ")}
                                    format="{0:dd/MM/yyyy}"
                                />
                            </KendoGrid>

                            {isFetching && (
                                <LoadingComponent
                                    message={getTranslatedLabel("general.loading", "جاري تحميل حسابات الأجل...")}
                                />
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}

export default BillingAccountsList;