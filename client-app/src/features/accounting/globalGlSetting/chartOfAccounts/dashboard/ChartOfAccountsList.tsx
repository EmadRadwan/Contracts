import React, { useEffect, useState } from "react";
import { Grid, Paper } from "@mui/material";
import GlSettingsMenu from "../../menu/GlSettingsMenu";
import AccountingMenu from "../../../invoice/menu/AccountingMenu";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import Box from '@mui/material/Box';
import TabContext from '@mui/lab/TabContext';
import TabPanel from '@mui/lab/TabPanel';
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { toast } from "react-toastify";

import AccountForm from "../form/AccountForm";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import { GlAccount } from "../../../../../app/models/accounting/globalGlSettings";
import { State } from "@progress/kendo-data-query";
import {
  useFetchGlobalGlAccountsQuery,
  useFetchTopLevelGlobalGlAccountsQuery
} from "../../../../../app/store/apis/accounting/globalGlSettingsApi";
import { StyledTabs } from "../../../../../app/components/StyledTabs";
import { StyledTab } from "../../../../../app/components/StyledTab";
import { useAppSelector } from "../../../../../app/store/configureStore";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";

const ChartOfAccountsList = () => {
  const { language } = useAppSelector(state => state.localization);
  const { getTranslatedLabel } = useTranslationHelper();

  // -----------------------------------------------------------------
  // View State: list vs form
  // -----------------------------------------------------------------
  const [editMode, setEditMode] = useState<0 | 1 | 2>(0); // 0=list, 1=create, 2=edit
  const [selectedAccount, setSelectedAccount] = useState<GlAccount | undefined>(undefined);

  // -----------------------------------------------------------------
  // Data
  // -----------------------------------------------------------------
  const [dataState, setDataState] = useState<State>({ take: 20, skip: 0 });
  const { data: flatGlAccounts, isFetching: isFlatFetching } = useFetchGlobalGlAccountsQuery({ ...dataState });
  const { data: topLevelAccounts, isFetching: isTopFetching } = useFetchTopLevelGlobalGlAccountsQuery(undefined);

  const [hierarchicalAccounts, setHierarchicalAccounts] = useState<GlAccount[]>([]);

  useEffect(() => {
    if (topLevelAccounts) {
      setHierarchicalAccounts(topLevelAccounts);
    }
  }, [topLevelAccounts]);

  // -----------------------------------------------------------------
  // Handlers – same pattern as SalesRequest
  // -----------------------------------------------------------------
  const startCreate = () => {
    setSelectedAccount(undefined);
    setEditMode(1);
  };

  const startEdit = (account: GlAccount) => {
    setSelectedAccount(account);
    setEditMode(2);
  };

  const cancelEdit = () => {
    setEditMode(0);
    setSelectedAccount(undefined);
  };

  const handleAccountCreated = (newAccount: GlAccount) => {
    // toast.success("Account created successfully");
    setSelectedAccount(newAccount);
    setEditMode(2); // switch to edit mode
    // Optional: refetch top-level or flat list if needed
  };

  const handleAccountUpdated = (updatedAccount: GlAccount) => {
    
    setSelectedAccount(updatedAccount);
    // No need to change mode — stays in edit
  };

  // -----------------------------------------------------------------
  // Custom Cell
  // -----------------------------------------------------------------
  const AccountIdCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
        <td {...navigationAttributes} style={{ color: "blue", cursor: "pointer" }}>
          <Button variant="text" onClick={() => startEdit(props.dataItem)}>
            {props.dataItem.glAccountId}
          </Button>
        </td>
    );
  };

  // -----------------------------------------------------------------
  // Render Form or List
  // -----------------------------------------------------------------
  if (editMode > 0) {
    return (
        <AccountForm
            account={editMode === 1 ? undefined : selectedAccount}
            editMode={editMode}
            cancelEdit={cancelEdit}
            onAccountCreated={handleAccountCreated}
            onAccountUpdated={handleAccountUpdated}
        />
    );
  }

  // -----------------------------------------------------------------
  // List View
  // -----------------------------------------------------------------
  return (
      <>
        <AccountingMenu selectedMenuItem="/globalGL" />
        <Paper elevation={5} className="div-container-withBorderCurved">
          <GlSettingsMenu selectedMenuItem="chartOfAccounts" />

          <Grid container spacing={2} sx={{ p: 2 }}>
            <Grid item xs={12}>
              <Button
                  variant="contained"
                  color="success"
                  onClick={startCreate}
                  sx={{ mb: 2 }}
              >
                {getTranslatedLabel("accounting.glAccount.list.createNew", "Create New Account")}
              </Button>
            </Grid>

            <Grid item xs={12}>
              <TabContext value="1">
                <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                  <StyledTabs value="1">
                    <StyledTab label={getTranslatedLabel("accounting.glAccount.list.title", "Chart of Accounts")} value="1" />
                  </StyledTabs>
                </Box>

                <TabPanel value="1">
                  <KendoGrid
                      style={{ height: "70vh" }}
                      data={flatGlAccounts?.data ?? []}
                      total={flatGlAccounts?.total ?? 0}
                      {...dataState}
                      onDataStateChange={(e) => setDataState(e.dataState)}
                      sortable
                      filterable
                      pageable
                      resizable
                  >
                    <GridToolbar>
                      <strong>{getTranslatedLabel("accounting.glAccount.list.globalTitle", "Global Chart of Accounts")}</strong>
                    </GridToolbar>

                    <Column field="glAccountId" title={getTranslatedLabel("accounting.glAccount.list.accountId", "Account ID")} cell={AccountIdCell} width={140} />
                    <Column field="accountName" title={getTranslatedLabel("accounting.glAccount.list.accountName", "Account Name")} />
                    {/* <Column field="accountNameArabic" title={getTranslatedLabel("accounting.glAccount.list.accountNameArabic", "Name (Arabic)")} width={300} /> */}
                    <Column field="parentGlAccountId" title={getTranslatedLabel("accounting.glAccount.list.parentId", "Parent ID")} width={140} />
                    <Column field="glAccountTypeId" title={getTranslatedLabel("accounting.glAccount.list.type", "Type")} width={180} />
                    <Column field="glAccountClassId" title={getTranslatedLabel("accounting.glAccount.list.class", "Class")} width={180} />
                    <Column field="glResourceTypeId" title={getTranslatedLabel("accounting.glAccount.list.resourceType", "Resource Type")} width={160} />
                  </KendoGrid>

                  {(isFlatFetching || isTopFetching) && <LoadingComponent />}
                </TabPanel>
              </TabContext>
            </Grid>
          </Grid>
        </Paper>
      </>
  );
};

export default ChartOfAccountsList;