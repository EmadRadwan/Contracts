import React, { useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridDetailRowProps,
  GridExpandChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";

import { DataResult, State } from "@progress/kendo-data-query";
import {
  useFetchFullChartOfAccountsQuery,
  useFetchOrganizationGlChartOfAccountsQuery
} from "../../../../app/store/apis/accounting/organizationGlChartOfAccountsApi";
import { handleDatesArray } from "../../../../app/util/utils";

import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { StyledTab } from "../../../../app/components/StyledTab";
import { StyledTabs } from "../../../../app/components/StyledTabs";
import { useFetchOrgChartOfAccountsLovQuery } from "../../../../app/store/apis";
import { GlAccount } from "../../../../app/models/accounting/globalGlSettings";
import { TabContext, TabPanel } from "@mui/lab";
import { Box, Grid, Typography } from "@mui/material";

interface Props {
  companyId?: string | undefined;
}

const OrganizationChartOfAccountsList = ({ companyId }: Props) => {

  
  const DetailComponent = (props: GridDetailRowProps) => {
    const {text, items} = props.dataItem
    console.log('text', text)

    console.log(props.dataItem);
    if (items) {
      return (
        <KendoGrid
          data={items}
          detail={DetailComponent}
          expandField="expanded"
          onExpandChange={expandChange}
        >
          <GridToolbar>
            <Typography variant="body1">
              Child Accounts for {text}
            </Typography>
          </GridToolbar>
          <Column
            field="glAccountId"
            title="Account Number"
            width={120}
            cell={AccountDescriptionCell}
          />
          <Column field="text" title="Account Name" width={320} />
          <Column
            field="parentAccountName"
            title="Parent Account Name"
            width={270}
          />
        </KendoGrid>
      );
    }
  };

  const [createAssignmentShow, setCreateAssignmentShow] = useState(false);
  const [account, setAccount] = useState<any>(undefined);
  const [editMode, setEditMode] = useState<number>(0);
  const [glAccounts, setGlAccounts] = React.useState<DataResult>({
    data: [],
    total: 0,
  });
  const [value, setValue] = React.useState("1");
  const [accounts, setAccounts] = React.useState<GlAccount[]>([]);




  const handleChange = (event: any, newValue: string) => {
    setValue(newValue);
  };

  const [dataState, setDataState] = React.useState<State>({ take: 8, skip: 0 });

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    console.log("dataStateChange", e.dataState);
    setDataState(e.dataState);
  };

  const { data, isFetching } = useFetchOrganizationGlChartOfAccountsQuery(
    { companyId, dataState },
    { skip: companyId === undefined }
  );
  
  const { data:  fullCompanyChartOfAccounts } = useFetchFullChartOfAccountsQuery(
    { companyId},
    { skip: companyId === undefined }
  );

 
  
  console.log('data', data?.data);
  console.log('fullCompanyChartOfAccounts', fullCompanyChartOfAccounts);

  const { data: structuredGlAccounts, isFetching: isStructuredGlFetching } =
    useFetchOrgChartOfAccountsLovQuery(undefined, {
      skip: companyId === undefined,
    });

  useEffect(() => {
    if (structuredGlAccounts && structuredGlAccounts?.length > 0) {
      setAccounts(structuredGlAccounts);
    }
  }, [structuredGlAccounts]);

  useEffect(() => {
    if (data) {
      const adjustedData = handleDatesArray(data.data);
      setGlAccounts({ data: adjustedData, total: data.total });
    }
  }, [data]);
  


  const findAndModifyChild = (
    accountId: string,
    accounts: GlAccount[],
    expandedValue: boolean
  ): GlAccount[] => {
    return accounts.map((account: GlAccount) => {
      // If the current account matches the accountId, update its expanded property
      if (account.glAccountId === accountId) {
        return { ...account, expanded: expandedValue };
      }
      // If the account has children, recursively search and update within the children
      if (account.items) {
        return {
          ...account,
          items: findAndModifyChild(
            accountId,
            account.items,
            expandedValue
          ),
        };
      }
      // Return the account unchanged if no match
      return account;
    });
  };

  const expandChange = (event: GridExpandChangeEvent) => {
    const selectedAccountId = event.dataItem.glAccountId;
    let modifiedAccounts = findAndModifyChild(
      selectedAccountId,
      accounts,
      event.value
    );
    if (modifiedAccounts) {
      setAccounts(modifiedAccounts);
    }
  };

  function handleSelectGlAccount(glAccountId: string) {
    const selectedGlAccount: GlAccount | undefined = data?.data?.find(
      (glAccount: GlAccount) => glAccountId === glAccount.glAccountId
    );

    setEditMode(2);
  }

  const AccountDescriptionCell = (props: any) => {
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
        {...{
          [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
        }}
        {...navigationAttributes}
      >
        <Button
          onClick={() => {
            console.log(props.dataItem);
            handleSelectGlAccount(props.dataItem.glAccountId);
          }}
        >
          {props.dataItem.glAccountId}
        </Button>
      </td>
    );
  };
  

  return (
    <>
      <div className="div-container">
        <TabContext value={value}>
          <Box sx={{ display: "flex", typography: "body1", ml: 2, mt: 1 }}>
            <StyledTabs onChange={handleChange} value={value}>
              <StyledTab label="Accounts Tree" value={"1"} />
              <StyledTab label="List of Accounts" value={"2"} />
            </StyledTabs>
          </Box>
          <TabPanel value="1">
            <KendoGrid
              style={{ height: "65vh", flex: 1 }}
              resizable={true}
              sortable={true}
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
              <Column field="text" title="Account Name" width={400} />
              <Column
                field="parentAccountName"
                title="Parent Account Name"
                width={400}
              />
              
              
            </KendoGrid>
          </TabPanel>
          <TabPanel value="2">
            <Grid item xs={12}>
              <KendoGrid
                style={{ height: "55vh", flex: 1 }}
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={true}
                {...dataState}
                data={glAccounts ? glAccounts : { data: [], total: 77 }}
                onDataStateChange={dataStateChange}
                reorderable={true}
              >
                <Column
                  field="glAccountId"
                  title="Account Number"
                  width={120}
                  cell={AccountDescriptionCell}
                />
                <Column field="accountName" title="Account Name" width={400} />
                <Column
                  field="parentAccountName"
                  title="Parent Account Name"
                  width={400}
                />
               

              </KendoGrid>
            </Grid>
          </TabPanel>
          <TabPanel value="3">
            <Typography variant="h6" sx={{ mb: 2 }}>
              Chart of Accounts Diagram
            </Typography>
          </TabPanel>
        </TabContext>
        {isFetching && <LoadingComponent message="Loading Accounts..." />}
      </div>
    </>
  );
};

export default OrganizationChartOfAccountsList;
