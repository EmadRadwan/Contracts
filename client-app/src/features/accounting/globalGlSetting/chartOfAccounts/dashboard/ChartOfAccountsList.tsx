import React, { useEffect, useState } from "react";
import { Grid, Paper, Dialog, DialogTitle, DialogContent, DialogActions, TextField } from "@mui/material";
import GlSettingsMenu from "../../menu/GlSettingsMenu";
import AccountingMenu from "../../../invoice/menu/AccountingMenu";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { toast } from "react-toastify";

import AccountForm from "../form/AccountForm";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import { GlAccount } from "../../../../../app/models/accounting/globalGlSettings";
import { State } from "@progress/kendo-data-query";
import {
  useCreateGlAccountMutation,
  useFetchGlobalGlAccountsQuery,
  useFetchTopLevelGlobalGlAccountsQuery
} from "../../../../../app/store/apis/accounting/globalGlSettingsApi";
import { useAppSelector } from "../../../../../app/store/configureStore";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";

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

const ChartOfAccountsList = () => {
  const { language } = useAppSelector(state => state.localization);
  const { getTranslatedLabel } = useTranslationHelper();

  // -----------------------------------------------------------------
  // View State: list vs form
  // -----------------------------------------------------------------
  const [editMode, setEditMode] = useState<0 | 1 | 2>(0); // 0=list, 1=create, 2=edit
  const [selectedAccount, setSelectedAccount] = useState<GlAccount | undefined>(undefined);
  const [similarModalOpen, setSimilarModalOpen] = useState<boolean>(false);
  const [similarAccountName, setSimilarAccountName] = useState<string>("");
  const [similarParentAccount, setSimilarParentAccount] = useState<GlAccount | null>(null);

  // -----------------------------------------------------------------
  // Data
  // -----------------------------------------------------------------
  const [dataState, setDataState] = useState<State>({ take: 20, skip: 0 });
  const { data: flatGlAccounts, isFetching: isFlatFetching } = useFetchGlobalGlAccountsQuery({ ...dataState });
  const { data: topLevelAccounts, isFetching: isTopFetching } = useFetchTopLevelGlobalGlAccountsQuery(undefined);

  const [hierarchicalAccounts, setHierarchicalAccounts] = useState<GlAccount[]>([]);
  const [createGlAccount, { isLoading: isCreating }] = useCreateGlAccountMutation();

  useEffect(() => {
    if (topLevelAccounts) {
      setHierarchicalAccounts(topLevelAccounts);
    }
  }, [topLevelAccounts]);

  const getMessage = (code: string) => {
    return messages[language]?.[code] || messages["en"]?.[code] || code;
  };

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

  const handleApiError = (error: any, defaultMessage: string) => {
    const errorCode = error?.data?.errorCode || "DEFAULT";
    const errorMessage = error?.data?.title || defaultMessage;
    const localizedMessage = messages[language]?.[errorCode] || errorMessage || defaultMessage;
    toast.error(localizedMessage);
    console.error(error);
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
    };
    try {
      const result = await createGlAccount(newAccount).unwrap();

      toast.success(getMessage("GL_ACCOUNT_CREATED"));

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
    } catch (e) {
      handleApiError(e, getMessage("GL_ACCOUNT_SAVE_FAILED"));
    }
    handleSimilarModalClose();
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

                  <Column field="glAccountId" title={getTranslatedLabel("accounting.glAccount.list.accountId", "Account ID")} cell={AccountIdCell} width={120} />
                  <Column field="accountName" title={getTranslatedLabel("accounting.glAccount.list.accountName", "Account Name")} />
                  {/* <Column field="accountNameArabic" title={getTranslatedLabel("accounting.glAccount.list.accountNameArabic", "Name (Arabic)")} width={300} /> */}
                  <Column field="parentGlAccountId" title={getTranslatedLabel("accounting.glAccount.list.parentId", "Parent ID")} width={120} />
                  <Column field="glAccountTypeId" title={getTranslatedLabel("accounting.glAccount.list.type", "Type")} width={180} />
                  <Column field="glAccountClassId" title={getTranslatedLabel("accounting.glAccount.list.class", "Class")} width={180} />
                  <Column field="glReportDescription" title={getTranslatedLabel("accounting.glAccount.list.report", "Report")} width={180} />
                  <Column field="glClassCourseDescription" title={getTranslatedLabel("accounting.glAccount.list.classCourse", "Class Course")} width={180} />
                  <Column field="glSubClassDescription" title={getTranslatedLabel("accounting.glAccount.list.subClass", "Sub Class")} width={180} />
                  <Column field="glSubClass2Description" title={getTranslatedLabel("accounting.glAccount.list.subClass2", "Sub Class 2")} width={180} />
                  <Column field="glAccountCourseLabelDescription" title={getTranslatedLabel("accounting.glAccount.list.courseLabel", "Course Label")} width={180} />
                  <Column cell={CreateSimilarAccountCell} width={170} />
                </KendoGrid>

                {(isFlatFetching || isTopFetching) && <LoadingComponent />}
          </Grid>
        </Grid>
      </Paper>

      {/* Create Similar Account Modal */}
      <Dialog open={similarModalOpen} onClose={handleSimilarModalClose} maxWidth="sm" fullWidth>
        <DialogTitle>
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

export default ChartOfAccountsList;