import React, { useEffect, useState } from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Button } from "@mui/material";
import {
    useAppDispatch,
    useAppSelector,
    useFetchBillingAccountsQuery,
} from "../../../../app/store/configureStore";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { handleDatesArray } from "../../../../app/util/utils";
import { useLocation } from "react-router-dom";
import { DataResult, State } from "@progress/kendo-data-query";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { BillingAccount } from "../../../../app/models/accounting/billingAccount";
import BillingAccountForm from "../form/BillingAccountForm";
import { setSelectedBillingAccount } from "../../slice/accountingSharedUiSlice";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

function BillingAccountsList() {
    const [editMode, setEditMode] = useState(0); // 0 = list, 1 = create, 2 = edit/view
    const location = useLocation();
    const dispatch = useAppDispatch();
    const { selectedBillingAccount } = useAppSelector((state) => state.accountingSharedUi);
    const { getTranslatedLabel } = useTranslationHelper();

    const [dataState, setDataState] = useState<State>({
        take: 10,
        skip: 0,
    });

    const [billingAccounts, setBillingAccounts] = useState<DataResult>({
        data: [],
        total: 0,
    });

    const { data, isFetching } = useFetchBillingAccountsQuery({ ...dataState });

    // Sync fetched data with local state (date formatting)
    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setBillingAccounts({
                data: adjustedData,
                total: data.total,
            });
        }
    }, [data]);

    // Auto-select account if passed via navigation state (e.g. from another page)
    useEffect(() => {
        if (
            location.state?.selectedBillingAccountId &&
            billingAccounts.data.length > 0
        ) {
            const id = location.state.selectedBillingAccountId;
            const account = billingAccounts.data.find(
                (item: any) => item.billingAccountId === id
            );
            if (account) {
                dispatch(
                    setSelectedBillingAccount({
                        ...account,
                        partyId: {
                            fromPartyId: account.partyId,
                            fromPartyName: account.partyName,
                        },
                    })
                );
                setEditMode(2);
            }
        }
    }, [location.state, billingAccounts.data, dispatch]);

    const handleSelectBillingAccount = (billingAccountId: string) => {
        const selectedAccount = billingAccounts.data.find(
            (acc: any) => acc.billingAccountId === billingAccountId
        );
        if (selectedAccount) {
            dispatch(
                setSelectedBillingAccount({
                    ...selectedAccount,
                    partyId: {
                        fromPartyId: selectedAccount.partyId,
                        fromPartyName: selectedAccount.partyName,
                    },
                })
            );
            setEditMode(2);
        }
    };

    const cancelEdit = () => {
        dispatch(setSelectedBillingAccount(undefined));
        setEditMode(0);
    };

    // Clickable ID cell
    const BillingAccountIdCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);

        return (
            <td
                {...navigationAttributes}
                style={{ ...props.style, cursor: "pointer" }}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                aria-colindex={props.ariaColumnIndex}
            >
                <Button
                    variant="text"
                    color="primary"
                    onClick={() => handleSelectBillingAccount(props.dataItem.billingAccountId)}
                    sx={{ fontWeight: "bold" }}
                >
                    {props.dataItem.billingAccountId}
                </Button>
            </td>
        );
    };

    // Show form if in create/edit mode
    if (editMode > 0 || selectedBillingAccount) {
        return (
            <BillingAccountForm
                selectedBillingAccount={selectedBillingAccount}
                editMode={editMode || 2}
                onClose={cancelEdit}
            />
        );
    }

    return (
        <>
            <AccountingMenu selectedMenuItem="/billingAccounts" />

            <Paper elevation={5} className="div-container-withBorderCurved" sx={{ p: 2 }}>
                <KendoGrid
                    style={{ height: "70vh", width: "100%" }}
                    data={billingAccounts}
                    total={billingAccounts.total}
                    {...dataState}
                    onDataStateChange={(e: GridDataStateChangeEvent) => {
                        setDataState(e.dataState);
                    }}
                    sortable
                    filterable
                    pageable={{ buttonCount: 5, pageSizes: [10, 20, 50, 100] }}
                    resizable
                >
                    <GridToolbar>
                        <Button
                            variant="contained"
                            color="success"
                            onClick={() => setEditMode(1)}
                            startIcon={<span>+</span>}
                        >
                            {getTranslatedLabel(
                                "accounting.billingAccounts.list.new",
                                "إنشاء حساب أجل جديد"
                            )}
                        </Button>
                    </GridToolbar>

                    <Column
                        field="billingAccountId"
                        title={getTranslatedLabel(
                            "accounting.billingAccounts.list.accountId",
                            "رقم الحساب"
                        )}
                        cell={BillingAccountIdCell}
                        width={160}
                    />

                    <Column
                        field="partyName"
                        title={getTranslatedLabel(
                            "accounting.billingAccounts.list.party",
                            "اسم العميل"
                        )}
                        width={280}
                    />

                    <Column
                        field="projectName"
                        title={getTranslatedLabel(
                            "accounting.billingAccounts.list.project",
                            "المشروع"
                        )}
                        width={220}
                    />

                    <Column
                        field="accountLimit"
                        title={getTranslatedLabel(
                            "accounting.billingAccounts.list.limit",
                            "حد الحساب"
                        )}
                        format="{0:n2}"
                        width={160}
                    />

                    <Column
                        field="accountCurrencyUomDescription"
                        title={getTranslatedLabel(
                            "accounting.billingAccounts.list.currency",
                            "العملة"
                        )}
                        width={140}
                    />

                    <Column
                        field="availableBalance"
                        title={getTranslatedLabel(
                            "accounting.billingAccounts.list.balance",
                            "الرصيد المتاح"
                        )}
                        format="{0:n2}"
                        width={180}
                        cell={(props) => (
                            <td style={{ textAlign: "right", fontWeight: "bold", color: props.dataItem.availableBalance < 0 ? "red" : "green" }}>
                                {new Intl.NumberFormat("ar-EG", {
                                    style: "currency",
                                    currency: props.dataItem.accountCurrencyUomId || "EGP",
                                }).format(props.dataItem.availableBalance || 0))}
                            </td>
                        )}
                    />

                    <Column
                        field="fromDate"
                        title={getTranslatedLabel(
                            "accounting.billingAccounts.list.fromDate",
                            "من تاريخ"
                        )}
                        format="{0:dd/MM/yyyy}"
                        width={140}
                    />

                    <Column
                        field="thruDate"
                        title={getTranslatedLabel(
                            "accounting.billingAccounts.list.thruDate",
                            "إلى تاريخ"
                        )}
                        format="{0:dd/MM/yyyy}"
                        width={140}
                    />
                </KendoGrid>

                {isFetching && (
                    <LoadingComponent message={getTranslatedLabel("general.loading", "جاري تحميل حسابات الأجل...")} />
                )}
            </Paper>
        </>
    );
}

export default BillingAccountsList;