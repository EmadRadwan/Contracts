import React, { useEffect, useState } from "react";
import { Grid, Paper, Typography } from "@mui/material";
import GlSettingsMenu from "../../menu/GlSettingsMenu";
import AccountingMenu from "../../../invoice/menu/AccountingMenu";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridExpandChangeEvent,
  GridDetailRowProps,
  GridToolbar,
} from "@progress/kendo-react-grid";
import Box from '@mui/material/Box';
import TabContext from '@mui/lab/TabContext';
import TabPanel from '@mui/lab/TabPanel';
import Button from "@mui/material/Button";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { toast } from "react-toastify";
import AccountForm from "../form/AccountForm";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import { GlAccount } from "../../../../../app/models/accounting/globalGlSettings";
import { State } from "@progress/kendo-data-query";
import { useFetchGlobalGlAccountsQuery, useFetchTopLevelGlobalGlAccountsQuery } from "../../../../../app/store/apis/accounting/globalGlSettingsApi";
import { StyledTabs } from "../../../../../app/components/StyledTabs";
import { StyledTab } from "../../../../../app/components/StyledTab";
import { useAppSelector } from "../../../../../app/store/configureStore";

const ChartOfAccountsList = () => {
  const {language} = useAppSelector(state => state.localization)

  const DetailComponent = (props: GridDetailRowProps) => {
    const {glAccountId, accountName, children} = props.dataItem
    
    if (children) {
      return (
        <KendoGrid data={children} detail={DetailComponent}
        expandField="expanded"
        onExpandChange={expandChange}>
          <GridToolbar>
            <Typography variant="body1">
              {language === "ar" ? `الحسابات المتفرعة من حساب : ${accountName} (${glAccountId})` : `Child Accounts for ${accountName} (${glAccountId})`}
            </Typography>
          </GridToolbar>
          <Column
            field="glAccountId"
            title="Account Number"
            width={120}
            cell={AccountDescriptionCell}
          />
          <Column field="accountName" title="Account Name" width={320} />
          <Column
            field="parentAccountName"
            title="Parent Account Name"
            width={270}
          />
          <Column field="glAccountTypeId" title="Account Type" width={160} />
          <Column field="glAccountClassId" title="Account Class" width={160} />
          <Column field="glResourceTypeId" title="Resource Type" width={130} />
        </KendoGrid>
      );
    }
  };

  const [account, setAccount] = useState<any>(undefined);
  const [editMode, setEditMode] = useState<number>(0);
  const initialDataState : State = {take: 10, skip: 0}
  const [dataState, setDataState] = React.useState<State>(initialDataState);
  const [value, setValue] = React.useState('1')
  const [accounts, setAccounts] = React.useState<GlAccount[]>([]);

  const handleChange = (event: any, newValue: string) => {
    setValue(newValue);
  };

  const {data: glAccounts, isFetching} = useFetchTopLevelGlobalGlAccountsQuery(undefined)
  const {data: flatGlAccounts, isFetching: isFlatGlFetching} = useFetchGlobalGlAccountsQuery({...dataState})

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    console.log("dataStateChange", e.dataState);
    setDataState(e.dataState);
  };

  const findAndModifyChild = (accountId: string, accounts: GlAccount[], expandedValue: boolean): GlAccount[] => {
    return accounts.map((account: GlAccount) => {
      // If the current account matches the accountId, update its expanded property
      if (account.glAccountId === accountId && account.children && account.children.length > 0) {
        return { ...account, expanded: expandedValue };
      }
      // If the account has children, recursively search and update within the children
      if (account.children) {
        return { ...account, children: findAndModifyChild(accountId, account.children, expandedValue) };
      }
      // Return the account unchanged if no match
      return account;
    });
  };

  const expandChange = (event: GridExpandChangeEvent) => {
    const selectedAccountId = event.dataItem.glAccountId
    let modifiedAccounts = findAndModifyChild(selectedAccountId, accounts, event.value)
    if (modifiedAccounts) {
      setAccounts(modifiedAccounts)
    } 
  };

  useEffect(() => {
    if (glAccounts && glAccounts.length > 0) {
      setAccounts(glAccounts)
    }
  }, [glAccounts])

  // const onAssign = (assignment: any) => {
  //   console.log(assignment.values);
  //   toast.success("assignment in console");
  //   setCreateAssignmentShow(false);
  // };

  const handleSelectAccount = (account: any) => {
    const selected = glAccounts?.find(
      (a: GlAccount) => a.glAccountId === account.glAccountId
    );
    console.log(selected);
    setAccount(account);
    setEditMode(2);
  };

  const handleEditAccount = (data: any) => {
    console.log(data.values);
    toast.success("account info in console");
    setEditMode(0);
  };

  const AccountDescriptionCell = (props: any) => {
    // const field = props.field || "";
    // const value = props.dataItem[field];
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
      <td
        className={props.className}
        style={{ ...props.style, color: "blue" }}
        colSpan={props.colSpan}
        role={"gridcell"}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{
          [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
        }}
        {...navigationAttributes}
      >
        <Button
          onClick={() => {
            console.log(props.dataItem);
            handleSelectAccount(props.dataItem);
          }}
        >
          {props.dataItem.glAccountId}
        </Button>
      </td>
    );
  };

  if (editMode > 0) {
    return (
      <AccountForm
        selectedAccount={account}
        editMode={editMode}
        onSubmit={handleEditAccount}
        cancelEdit={() => setEditMode(0)}
      />
    );
  }

  return (
    <>
      <AccountingMenu selectedMenuItem={"/globalGL"} />

      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <GlSettingsMenu selectedMenuItem={"chartOfAccounts"} />
        <Grid container spacing={1} alignItems={"center"}>
        <TabContext value={value}>
          <Box sx={{ display: "flex", typography: 'body1', ml: 2, mt: 1 }}>
              <StyledTabs onChange={handleChange} value={value} >
                <StyledTab label="Structured Accounts" value={"1"}/>
                <StyledTab label="List of Accounts" value={"2"}/>
              </StyledTabs>
          </Box>
            <TabPanel value={"1"}>
              <Grid item xs={12}>
                <div className="div-container">
                  <KendoGrid
                    style={{ height: "65vh", flex: 1 }}
                    resizable={true}
                    filterable={true}
                    sortable={true}
                    pageable={true}
                    detail={DetailComponent}
                    expandField="expanded"
                    onExpandChange={expandChange}
                    data={accounts ?? []}
                    reorderable={true}
                  >
                    <Column
                      field="glAccountId"
                      title="Account Number"
                      width={120}
                      cell={AccountDescriptionCell}
                    />
                    <Column field="accountName" title="Account Name" width={320} />
                    <Column
                      field="parentAccountName"
                      title="Parent Account Name"
                      width={270}
                    />
                    <Column
                      field="glAccountTypeId"
                      title="Account Type"
                      width={160}
                    />
                    <Column
                      field="glAccountClassId"
                      title="Account Class"
                      width={160}
                    />
                    <Column
                      field="glResourceTypeId"
                      title="Resource Type"
                      width={130}
                    />
                  </KendoGrid>
                </div>
              </Grid>
            </TabPanel>
            <TabPanel value={"2"}>
              <Grid item xs={12}>
                <div className="div-container">
                  <KendoGrid
                    style={{ height: "65vh", flex: 1 }}
                    resizable={true}
                    filterable={true}
                    sortable={true}
                    pageable={true}
                    {...dataState}
                    data={flatGlAccounts ? flatGlAccounts : {data: [], total: 0}}
                    total={flatGlAccounts ? flatGlAccounts.total : 0}
                    onDataStateChange={dataStateChange}
                    reorderable={true}
                  >
                    <Column
                      field="glAccountId"
                      title="Account Number"
                      width={120}
                      cell={AccountDescriptionCell}
                    />
                    <Column field="accountName" title="Account Name" width={320} />
                    <Column
                      field="parentAccountName"
                      title="Parent Account Name"
                      width={270}
                    />
                    <Column
                      field="glAccountTypeId"
                      title="Account Type"
                      width={160}
                    />
                    <Column
                      field="glAccountClassId"
                      title="Account Class"
                      width={160}
                    />
                    <Column
                      field="glResourceTypeId"
                      title="Resource Type"
                      width={130}
                    />
                  </KendoGrid>
                  {isFetching && <LoadingComponent message="Loading Accounts..." />}
                </div>
              </Grid>
            </TabPanel>
        </TabContext>
        </Grid>
        {(isFetching || isFlatGlFetching) && <LoadingComponent message="Loading Accounts..." />}
      </Paper>
    </>
  );
};

export default ChartOfAccountsList;
