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
import MermaidChart from "../../../manufacturing/dashboard/MermaidChart";

interface Props {
  companyId?: string | undefined;
}

const OrganizationChartOfAccountsList = ({ companyId }: Props) => {
  const DetailComponent = (props: GridDetailRowProps) => {
    const {text, items} = props.dataItem
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
          <Column field="glAccountTypeId" title="Account Type" width={160} />
          <Column field="glAccountClassId" title="Account Class" width={160} />
          <Column field="glResourceTypeId" title="Resource Type" width={130} />
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
  const [diagramAccounts, setDiagramAccounts] = React.useState<GlAccount[]>([]);
  const [diagramText, setDiagramText] = useState("");

  // Updated diagram generator that builds a tree from a flat array.
  const generateChartOfAccountsDiagram = (flatAccounts: GlAccount[]): string => {
    // Build a lookup map with an items property to hold children.
    const accountMap: { [id: string]: GlAccount & { items: GlAccount[] } } = {};
    flatAccounts.forEach(acc => {
      // Ensure we have an items array.
      accountMap[acc.glAccountId] = { ...acc, items: [] };
    });

    // Build the tree: for each account, if it has a parentGlAccountId and that parent exists,
    // then add it to that parent's items collection; otherwise, it's a root.
    const roots: (GlAccount & { items: GlAccount[] })[] = [];
    flatAccounts.forEach(acc => {
      if (acc.parentGlAccountId && accountMap[acc.parentGlAccountId]) {
        accountMap[acc.parentGlAccountId].items.push(accountMap[acc.glAccountId]);
      } else {
        roots.push(accountMap[acc.glAccountId]);
      }
    });

    // Now, generate the Mermaid diagram text.
    const diagramLines: string[] = [];
    diagramLines.push("flowchart TD"); // top-down layout

    // Recursive helper to process each account in the tree.
    function processAccount(account: GlAccount & { items: GlAccount[] }, parentId?: string) {
      // Create a safe node id by removing any non-alphanumeric characters.
      const nodeId = `acc_${account.glAccountId.replace(/[^A-Za-z0-9_]/g, "_")}`;
      // Use the account's name (from accountName) and wrap it in a span.
      diagramLines.push(`  ${nodeId}[<span>${account.accountName}</span>]`);
      if (parentId) {
        diagramLines.push(`  ${parentId} --> ${nodeId}`);
      }
      // Process children recursively.
      if (account.items && account.items.length > 0) {
        account.items.forEach(child => processAccount(child, nodeId));
      }
    }

    roots.forEach(root => processAccount(root));
    const diagram = diagramLines.join("\n");
    console.log("Generated Diagram:", diagram);
    return diagram;
  };





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

  useEffect(() => {
    //console.log("Fetched data:", data);
    if (diagramAccounts && diagramAccounts?.length > 0) {
      const diag = generateChartOfAccountsDiagram(diagramAccounts);
      console.log("Generated diagram:", diag);
      setDiagramText(diag);
    } else {
      setDiagramText("");
    }
  }, [diagramAccounts]);


  
  console.log('data', data?.data);
  console.log('diagram', diagramText);
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
  
 useEffect(() => {
    if (fullCompanyChartOfAccounts) {
      setDiagramAccounts(fullCompanyChartOfAccounts);
    }
  }, [fullCompanyChartOfAccounts]);

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
              <StyledTab label="Diagram" value={"3"} />
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
              <Column field="text" title="Account Name" width={320} />
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
            </Grid>
          </TabPanel>
          <TabPanel value="3">
            <Typography variant="h6" sx={{ mb: 2 }}>
              Chart of Accounts Diagram
            </Typography>
            {diagramText ? (
                <MermaidChart chart={diagramText} />
            ) : (
                <Typography>No diagram available.</Typography>
            )}
          </TabPanel>
        </TabContext>
        {isFetching && <LoadingComponent message="Loading Accounts..." />}
      </div>
    </>
  );
};

export default OrganizationChartOfAccountsList;
