// BillingAccountsList.tsx
import React, { useEffect, useState } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { Grid, Paper } from "@mui/material";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { handleDatesArray } from "../../../../app/util/utils";
import { State } from "@progress/kendo-data-query";

import AccountingMenu from "../../invoice/menu/AccountingMenu";
import BillingAccountForm from "../form/BillingAccountForm";
import { useFetchBillingAccountsQuery } from "../../../../app/store/apis";
import { BillingAccount } from "../../../../app/models/accounting/billingAccount";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

function BillingAccountsList() {
    const [editMode, setEditMode] = useState(0); // 0=list, 1=create, 2=edit
    const [selectedAccount, setSelectedAccount] = useState<BillingAccount | undefined>(undefined);

    const { getTranslatedLabel } = useTranslationHelper();

    const [dataState, setDataState] = useState<State>({ take: 9, skip: 0 });
    const { data, isFetching } = useFetchBillingAccountsQuery(dataState);
    const [gridData, setGridData] = useState({ data: [], total: 0 });

    useEffect(() => {
        if (data) {
            setGridData({ data: handleDatesArray(data.data), total: data.total });
        }
    }, [data]);

    const startEdit = (account?: BillingAccount) => {
        setSelectedAccount(account);
        setEditMode(account ? 2 : 1);
    };

    const cancelEdit = () => {
        setSelectedAccount(undefined);
        setEditMode(0);
    };

    const handleCreated = (created: BillingAccount) => {
        setSelectedAccount(created);
        setEditMode(2); // stay in form, now in edit mode
    };

    const IdCell = (props: any) => (
        <td style={{ color: "blue", cursor: "pointer" }}>
            <Button onClick={() => startEdit(props.dataItem)} variant="text">
                {props.dataItem.billingAccountId}
            </Button>
        </td>
    );

    // ── RENDER FORM ─────────────────────────────────────
    if (editMode > 0) {
        return (
            <BillingAccountForm
                billingAccount={editMode === 1 ? undefined : selectedAccount}
                editMode={editMode}
                cancelEdit={cancelEdit}
                onBillingAccountCreated={handleCreated}
            />
        );
    }

    // ── RENDER LIST ─────────────────────────────────────
    return (
        <>
            <AccountingMenu selectedMenuItem="/billingAccounts" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <KendoGrid
                    style={{ height: "65vh" }}
                    data={gridData}
                    total={gridData.total}
                    resizable
                    filterable
                    sortable
                    pageable
                    {...dataState}
                    onDataStateChange={(e) => setDataState(e.dataState)}
                >
                    <GridToolbar>
                        <Button
                            variant="outlined"
                            color="secondary"
                            onClick={() => startEdit()}
                        >
                            {getTranslatedLabel("accounting.billingAccounts.list.new", "إنشاء حساب أجل جديد")}
                        </Button>
                    </GridToolbar>

                    <Column field="billingAccountId" title="رقم الحساب" cell={IdCell} width={130} />
                    <Column field="partyName" title="اسم العميل" width={280} />
                    <Column field="projectName" title="المشروع" width={220} />
                    <Column field="accountLimit" title="حد الحساب" format="{0:n2}" />
                    <Column field="fromDate" title="من تاريخ" format="{0:dd/MM/yyyy}" />
                    <Column field="thruDate" title="إلى تاريخ" format="{0:dd/MM/yyyy}" />
                </KendoGrid>

                {isFetching && <LoadingComponent message="جاري تحميل حسابات الأجل..." />}
            </Paper>
        </>
    );
}

export default BillingAccountsList;