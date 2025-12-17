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
import { toast } from "react-toastify";
import { DataResult, State } from "@progress/kendo-data-query";
import {
  useCreateAndAssignGlAccountToOrganizationMutation,
  useFetchFullChartOfAccountsQuery,
  useFetchOrganizationGlChartOfAccountsQuery
} from "../../../../app/store/apis/accounting/organizationGlChartOfAccountsApi";
import { handleDatesArray } from "../../../../app/util/utils";

import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { StyledTab } from "../../../../app/components/StyledTab";
import { StyledTabs } from "../../../../app/components/StyledTabs";
import {
  useFetchGlAccountOrganizationHierarchyLovQuery,
  useFetchOrgChartOfAccountsLovQuery
} from "../../../../app/store/apis";
import { GlAccount } from "../../../../app/models/accounting/globalGlSettings";
import { TabContext, TabPanel } from "@mui/lab";
import { Box, Grid, Typography, Dialog, DialogTitle, DialogContent, DialogActions, TextField } from "@mui/material";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { ChartOfAccountsExcel } from "../report/ChartOfAccountsExcel";
import { useAppSelector } from "../../../../app/store/configureStore";

interface Props {
  companyId?: string | undefined;
}

const messages: Record<string, Record<string, string>> = {
  en: {
    // Success messages
    GL_ACCOUNT_CREATED: "GL Account created successfully.",
    // Error messages
    USER_NOT_FOUND: "Unauthorized: User not found.",
    GL_ACCOUNT_ID_REQUIRED: "GL Account ID is required.",
    GL_ACCOUNT_NOT_FOUND: "The specified GL Account could not be found.",
    GL_ACCOUNT_SAVE_FAILED: "Failed to save the GL Account.",
    GL_ACCOUNT_CREATE_FAILED: "Failed to create the GL Account.",
    ACCOUNT_CODE_GENERATION_FAILED: "Failed to generate a unique account code.",
    GL_ACCOUNT_INVALID_PARENT: "Cannot assign account as child to itself.",
    ALREADY_EXISTS: "Record already exists.",
    UNEXPECTED_ERROR: "An unexpected error occurred. Please try again.",
    DEFAULT: "An unexpected error occurred. Please try again.",
  },
  ar: {
    // Success messages
    GL_ACCOUNT_CREATED: "تم إنشاء حساب دفتر الأستاذ بنجاح.",
    // Error messages
    USER_NOT_FOUND: "غير مصرح: المستخدم غير موجود.",
    GL_ACCOUNT_ID_REQUIRED: "معرف حساب دفتر الأستاذ مطلوب.",
    GL_ACCOUNT_NOT_FOUND: "حساب دفتر الأستاذ المحدد غير موجود.",
    GL_ACCOUNT_SAVE_FAILED: "فشل في حفظ حساب دفتر الأستاذ.",
    GL_ACCOUNT_CREATE_FAILED: "فشل في إنشاء حساب دفتر الأستاذ.",
    ACCOUNT_CODE_GENERATION_FAILED: "فشل في إنشاء رمز حساب فريد.",
    GL_ACCOUNT_INVALID_PARENT: "لا يمكن تعيين الحساب كتابع لنفسه.",
    ALREADY_EXISTS: "السجل موجود بالفعل.",
    UNEXPECTED_ERROR: "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
    DEFAULT: "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
  },
};

const OrganizationChartOfAccountsList = ({ companyId }: Props) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const { language } = useAppSelector(state => state.localization);


  const DetailComponent = (props: GridDetailRowProps) => {
    const { text, items } = props.dataItem
    console.log('text', text)

    console.log(props.dataItem);
    if (items) {
      return (
        <KendoGrid
          data={items}
          detail={DetailComponent}
          expandField="expanded"
          onExpandChange={expandChange}
          resizable={true}
        >
          <GridToolbar>
            <Typography variant="body1">
              {getTranslatedLabel("accounting.glAccount.list.childAccountsFor", "Child Accounts for")} {text}
            </Typography>
          </GridToolbar>
          <Column
            field="glAccountId"
            title={getTranslatedLabel("accounting.glAccount.list.accountNumber", "Account Number")}
            width={120}
            cell={AccountDescriptionCell}
          />
          <Column field="text" title={getTranslatedLabel("accounting.glAccount.list.accountName", "Account Name")} width={320} />
          <Column
            field="parentAccountName"
            title={getTranslatedLabel("accounting.glAccount.list.parentAccountName", "Parent Account Name")}
            width={270}
          />
        </KendoGrid>
      );
    }
  };

  const [createAssignmentShow, setCreateAssignmentShow] = useState(false);
  const [selectedAccount, setSelectedAccount] = useState<GlAccount | undefined>(undefined);
  const [editMode, setEditMode] = useState<number>(0);
  const [glAccounts, setGlAccounts] = React.useState<DataResult>({
    data: [],
    total: 0,
  });
  const [value, setValue] = React.useState("1");
  const [accounts, setAccounts] = React.useState<GlAccount[]>([]);
  const [similarModalOpen, setSimilarModalOpen] = useState<boolean>(false);
  const [similarAccountName, setSimilarAccountName] = useState<string>("");
  const [similarParentAccount, setSimilarParentAccount] = useState<GlAccount | null>(null);

  const [createAndAssignGlAccount] = useCreateAndAssignGlAccountToOrganizationMutation();

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



  console.log('data', data?.data);

  const { data: structuredGlAccounts, isFetching: isStructuredGlFetching } =
    useFetchGlAccountOrganizationHierarchyLovQuery(companyId, {
      skip: companyId === undefined,
    });

  const {refetch} = useFetchOrgChartOfAccountsLovQuery(undefined);

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

  const CreateSimilarAccountCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
      <td {...navigationAttributes}>
        <Button variant="contained" onClick={() => {
          setSimilarParentAccount(props.dataItem);
          setSimilarAccountName("");
          setSimilarModalOpen(true);
        }}>
          {getTranslatedLabel("accounting.glAccount.list.similar", "Create account from")}
        </Button>
      </td>
    );
  };

  const getMessage = (code: string) => {
    return messages[language]?.[code] || messages["en"]?.[code] || code;
  };

  const handleSimilarModalClose = () => {
    setSimilarModalOpen(false);
    setSimilarAccountName("");
    setSimilarParentAccount(null);
  };

  const handleCreateSimilarAccount = async () => {
    if (!similarAccountName.trim() || !similarParentAccount) {
      toast.error(getTranslatedLabel("accounting.glAccount.modal.nameRequired", "Account name is required"));
      return;
    }
    // Set up the account creation with the parent's settings
    const newAccount = {
      ...similarParentAccount,
      glAccountId: "",
      accountCode: "",
      accountName: similarAccountName.trim(),
      parentGlAccountId: similarParentAccount.parentGlAccountId,
      companyId
    };
    try {
      const result = await createAndAssignGlAccount(newAccount).unwrap();

      toast.success(getMessage("GL_ACCOUNT_CREATED"));
      refetch()

      const createdAccount: GlAccount = {
        ...result,
        glAccountId: result.glAccountId,
        accountCode: result.accountCode,
        accountName: result.accountName,
        description: result.description,
        glAccountTypeId: result.glAccountTypeId,
        glAccountClassId: result.glAccountClassId,
        glResourceTypeId: result.glResourceTypeId,
        parentGlAccountId: result.parentGlAccountId,
      };

      handleAccountCreated(createdAccount);
    } catch (err) {
      handleApiError(err, getMessage("GL_ACCOUNT_SAVE_FAILED"));
    }
    handleSimilarModalClose();
  };

  const handleAccountCreated = (newAccount: GlAccount) => {
    // toast.success("Account created successfully");
    setSelectedAccount(newAccount);
    setEditMode(2); // switch to edit mode
    // Optional: refetch top-level or flat list if needed
  };

  const handleApiError = (error: any, defaultMessage: string) => {
    const errorCode = error?.data?.errorCode || "DEFAULT";
    const errorMessage = error?.data?.title || defaultMessage;
    const localizedMessage = messages[language]?.[errorCode] || errorMessage || defaultMessage;
    toast.error(localizedMessage);
    console.error(error);
  };


  return (
    <>
      <div className="div-container">
        <TabContext value={value}>
          <Box sx={{ display: "flex", typography: "body1", ml: 2, mt: 1 }}>
            <StyledTabs onChange={handleChange} value={value}>
              <StyledTab label={getTranslatedLabel("accounting.glAccount.tabs.accountsTree", "Accounts Tree")} value={"1"} />
              <StyledTab label={getTranslatedLabel("accounting.glAccount.tabs.listOfAccounts", "List of Accounts")} value={"2"} />
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
              <GridToolbar>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 4 }}>
                  <Typography variant="body1">{getTranslatedLabel("accounting.glAccount.list.chartOfAccounts", "Chart of Accounts")}</Typography>
                  <ChartOfAccountsExcel
                    accounts={accounts}
                    companyId={companyId}
                    getTranslatedLabel={getTranslatedLabel}
                  />
                </Box>
              </GridToolbar>
              <Column
                field="glAccountId"
                title={getTranslatedLabel("accounting.glAccount.list.accountNumber", "Account Number")}
                width={120}
                cell={AccountDescriptionCell}
              />
              <Column field="text" title={getTranslatedLabel("accounting.glAccount.list.accountName", "Account Name")} width={400} />
              <Column
                field="parentAccountName"
                title={getTranslatedLabel("accounting.glAccount.list.parentAccountName", "Parent Account Name")}
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
                  title={getTranslatedLabel("accounting.glAccount.list.accountNumber", "Account Number")}
                  width={120}
                  cell={AccountDescriptionCell}
                />
                <Column field="accountName" title={getTranslatedLabel("accounting.glAccount.list.accountName", "Account Name")} width={400} />
                <Column
                  field="parentAccountName"
                  title={getTranslatedLabel("accounting.glAccount.list.parentAccountName", "Parent Account Name")}
                  width={400}
                />
                <Column cell={CreateSimilarAccountCell} width={170} />

              </KendoGrid>
            </Grid>
          </TabPanel>

        </TabContext>
        {isFetching && <LoadingComponent message={getTranslatedLabel("accounting.glAccount.list.loadingAccounts", "Loading Accounts...")} />}
      </div>

      <Dialog open={similarModalOpen} onClose={handleSimilarModalClose} maxWidth="sm" fullWidth>
        <DialogTitle textAlign="center">
          {getTranslatedLabel("accounting.glAccount.modal.title", "Create Similar Account")}
        </DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label={getTranslatedLabel("accounting.glAccount.modal.enterName", "Enter new account name")}
            type="text"
            fullWidth
            variant="outlined"
            value={similarAccountName}
            onChange={(e) => setSimilarAccountName(e.target.value)}
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleSimilarModalClose} variant="outlined">
            {getTranslatedLabel("general.cancel", "Cancel")}
          </Button>
          <Button onClick={handleCreateSimilarAccount} variant="contained" color="primary">
            {getTranslatedLabel("general.create", "Create")}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default OrganizationChartOfAccountsList;
