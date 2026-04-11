import React, {useCallback, useEffect, useMemo, useRef, useState} from "react";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {Box, Button, Collapse, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Grid, IconButton, Menu, MenuItem, Paper, Typography} from "@mui/material";
import {Ribbon, RibbonContainer} from "react-ribbons";
import useMultiPaymentCertificate from "../hook/useMultiPaymentCertificate";
import {FormInitialValues, MultiPaymentCertificate, MultiPaymentItem} from "../../../app/models/project/MultiPaymentCertificate";
import {useAppSelector} from "../../../app/store/configureStore";
import MultiPaymentItemsList from "../dashboard/MultiPaymentItemsList";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import AccountingMenu from "../../accounting/invoice/menu/AccountingMenu";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import {requiredValidator} from "../../../app/common/form/Validators";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import FormInput from "../../../app/common/form/FormInput";

import {MemoizedFormComboBox2} from "../../../app/common/form/FormComboBox2";
import {useFetchGlAccountOrgCashOrEquivalentLovQuery, useFetchWorkEffortAcctTransEntriesQuery} from "../../../app/store/apis";
import {MultiPaymentCertificateExcel} from "../report/MultiPaymentCertificateExcel";
import {FormComboBoxVirtualParty} from "../../../app/common/form/FormComboBoxVirtualParty";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import CreatePartyModalForm from "../../parties/form/CreatePartyModalForm";


interface CertificateActionsMenuProps {
    workEffortId: string | undefined;
    currentStatusId: string | undefined;
    handleApprove: () => void;
    handleReset: () => void;
    disabled: boolean;
}

const CertificateActionsMenu: React.FC<CertificateActionsMenuProps> = ({
                                                                           workEffortId,
                                                                           currentStatusId,
                                                                           handleApprove,
                                                                           handleReset,
                                                                           disabled,
                                                                       }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.multiPaymentCertificate.form";
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const [resetDialogOpen, setResetDialogOpen] = useState(false);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const handleResetClick = () => {
        setResetDialogOpen(true);
        handleClose();
    };

    const handleConfirmReset = () => {
        handleReset();
        setResetDialogOpen(false);
    };

    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleClick}
                disabled={disabled || !workEffortId}
                sx={{mt: 2, mr: 2}}
            >
                {getTranslatedLabel(`${localizationKey}.actions`, "Actions")}
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                anchorOrigin={{vertical: "bottom", horizontal: "right"}}
                transformOrigin={{vertical: "top", horizontal: "right"}}
            >
                <MenuItem
                    onClick={() => {
                        handleApprove();
                        handleClose();
                    }}
                    disabled={currentStatusId === "WEPR_APPROVED"}
                >
                    {getTranslatedLabel(`${localizationKey}.approve`, "Approve")}
                </MenuItem>
                <MenuItem
                    onClick={handleResetClick}
                    disabled={currentStatusId !== "WEPR_APPROVED"}
                    sx={{ color: '#d32f2f' }}
                >
                    {getTranslatedLabel(`${localizationKey}.resetConfirm`, "Reset Certificate")}
                </MenuItem>
            </Menu>

            <Dialog
                open={resetDialogOpen}
                onClose={() => setResetDialogOpen(false)}
                aria-labelledby="reset-certificate-dialog-title"
                aria-describedby="reset-certificate-dialog-description"
            >
                <DialogTitle id="reset-certificate-dialog-title">
                    {getTranslatedLabel(
                        `${localizationKey}.reset.dialogTitle`,
                        "Confirm Certificate Reset"
                    )}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText id="reset-certificate-dialog-description">
                        {getTranslatedLabel(
                            `${localizationKey}.reset.dialogMessage`,
                            "Are you sure you want to reset certificate {workEffortId}?\n\nThis will:\n• Delete accounting transactions\n• Reset status to Created\n\nThis action cannot be undone."
                        ).replace("{workEffortId}", workEffortId || "")}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setResetDialogOpen(false)}>
                        {getTranslatedLabel("global.cancel", "Cancel")}
                    </Button>
                    <Button
                        onClick={handleConfirmReset}
                        color="error"
                        variant="contained"
                        autoFocus
                    >
                        {getTranslatedLabel(`${localizationKey}.reset.confirm`, "Reset Certificate")}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};

interface Props {
    selectedCertificate?: MultiPaymentCertificate;
    editMode: number; // 1: add, 2: edit
    cancelEdit: () => void;
    setEditMode: (mode: number) => void;
    setParentCertificate: (certificate: MultiPaymentCertificate | null) => void;
}

export default function MultiPaymentCertificateForm({
                                                        selectedCertificate,
                                                        editMode,
                                                        cancelEdit,
                                                        setEditMode,
                                                        setParentCertificate
                                                    }: Props) {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "projects.multiPaymentCertificate.form";
    const [isFormCollapsed, setIsFormCollapsed] = useState(false);
    const {language} = useAppSelector((state) => state.localization);
    const [showNewCustomer, setShowNewCustomer] = useState(false);

    const [formKey, setFormKey] = useState<number>(1);
    const formRef = useRef<any>(null);
    const {user} = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";
    const companyName = user?.organizationPartyName || "";
    const {
        data: glAccounts,
        isLoading: isLoadingGlAccounts
    } = useFetchGlAccountOrgCashOrEquivalentLovQuery(companyId, {
        skip: !companyId,
    });

    


    const {
        certificate,
        setCertificate,
        items,
        addItem,
        updateItem,
        deleteItem,
        handleCreate,
        handleUpdate, setItems, handleApprove, itemsVersion,
        handleDelete, handleReset,
        isLoading: apiLoading,
    } = useMultiPaymentCertificate({
        selectedCertificate,
        editMode,
        setFormKey,
        setEditMode, setParentCertificate
    });

    const { data: acctTransEntryData, isFetching: isFetchingTransactions, refetch: refetchTransactions } = useFetchWorkEffortAcctTransEntriesQuery(certificate?.workEffortId, {
        skip: !certificate?.workEffortId,
    });

    // Whenever items change → update hidden field
    useEffect(() => {
        formRef.current?.onChange("_itemsVersion", { value: itemsVersion });
    }, [itemsVersion]);

    const initialValues = useMemo((): FormInitialValues => {
        // REFACTOR: Use a type guard to safely access properties of source
        const source = certificate || selectedCertificate;
        const defaultValues: FormInitialValues = {
            workEffortId: "",
            date: new Date(),
            description: "",
            currentStatusId: "WEPR_CREATED",
            statusDescription: "Created",
            statusDescriptionArabic: "تم الإنشاء",
            glaccountId: "",
            partyIdEmployee: null,
            notes: "",
        };

        // REFACTOR: Only override default values if source exists and has valid properties
        if (source) {
            return {
                ...defaultValues,
                workEffortId: source.workEffortId || defaultValues.workEffortId,
                date: source.date ? new Date(source.date) : defaultValues.date,
                description: source.description || defaultValues.description,
                currentStatusId: source.currentStatusId || defaultValues.currentStatusId,
                statusDescription: source.statusDescription || defaultValues.statusDescription,
                statusDescriptionArabic: source.statusDescriptionArabic || defaultValues.statusDescriptionArabic,
                glAccountId: source.glAccountId || defaultValues.glAccountId,
                partyIdEmployee: source.partyIdEmployee ? { "fromPartyId": source.partyIdEmployee, "fromPartyName": source.partyName || source.partyIdEmployee } : defaultValues.partyIdEmployee,
                notes: source.notes || defaultValues.notes,
            };
        }

        return defaultValues;
    }, [certificate, selectedCertificate]);
    
    const renderSwitchStatus = useCallback(() => {
        const status = certificate?.currentStatusId || "WEPR_CREATED";
        if (certificate?.statusDescription && certificate?.statusDescriptionArabic) {
            return {
                label: language === "ar" ? certificate.statusDescriptionArabic : certificate.statusDescription,
                backgroundColor: status === "WEPR_CREATED" ? "blue" : "green",
                foreColor: "#ffffff",
            };
        }
        const statusLabels: { [key: string]: { en: string; ar: string } } = {
            WEPR_CREATED: {en: "Created", ar: "تم الإنشاء"},
            WEPR_APPROVED: {en: "Approved", ar: "تمت الموافقة"},
        };
        return {
            label: language === "ar" ? statusLabels[status]?.ar || "غير معروف" : statusLabels[status]?.en || "Unknown",
            backgroundColor: status === "WEPR_CREATED" ? "blue" : "green",
            foreColor: "#ffffff",
        };
    }, [certificate, language]);


    const handleCancelForm = useCallback(() => {
        setCertificate(undefined);
        setItems([]);
        setFormKey((prev) => prev + 1);
        setParentCertificate(null);
        cancelEdit();
    }, [setCertificate, setItems, cancelEdit, setParentCertificate]);

    const handleSubmit = useCallback((values: any) => {
        const serializedValues: MultiPaymentCertificate = {
            workEffortId: certificate?.workEffortId || values.workEffortId || "",
            date: values.date instanceof Date ? values.date.toISOString() : new Date().toISOString(),
            description: values.description || "",
            glAccountId: values.glAccountId || "",
            partyIdEmployee: values.partyIdEmployee?.fromPartyId,
            notes: values.notes || "",
            items,
        };
        if (editMode === 1) {
            handleCreate({
                values: serializedValues,
                isValid: formRef.current?.isValid(),
            }).then((result) => {
                if (result.success && result.certificate) {
                    setCertificate(result.certificate);
                    setEditMode(2);
                    setParentCertificate(result.certificate);
                }
            });
        } else {
            handleUpdate({
                values: serializedValues,
                isValid: formRef.current?.isValid(),
            }).then((result) => {
                if (result.success && result.certificate) {
                    setCertificate(result.certificate);
                    setParentCertificate(result.certificate);
                    setFormKey((prev) => prev + 1);
                }
            });
        }
    }, [handleCreate, handleUpdate, editMode, items, setEditMode, setParentCertificate, certificate]);


    const titleText = editMode === 1
        ? getTranslatedLabel(`${localizationKey}.title`, "New Multi-Payment Certificate")
        : `${getTranslatedLabel(`${localizationKey}.title`, "Multi Payment Certificate")} ${selectedCertificate?.workEffortId || ""}`;

    const handleApproveCertificate = useCallback(() => {
        if (!certificate?.workEffortId) {
            return;
        }
        handleApprove({
            workEffortId: certificate.workEffortId,
            isValid: formRef.current?.isValid() && items.length > 0,
        }).then((result) => {
            if (result.success && result.certificate) {
                setEditMode(3); // Per your preference for post-approval
                setParentCertificate(result.certificate);
                refetchTransactions();
            }
        });
    }, [certificate, handleApprove, items, setEditMode, setParentCertificate]);

    const handleResetCertificate = useCallback(() => {
        if (!certificate?.workEffortId) {
            return;
        }
        handleReset(certificate.workEffortId).then((result) => {
            if (result.success && result.certificate) {
                setEditMode(2);
                setParentCertificate(result.certificate);
                refetchTransactions();
            }
        });
    }, [certificate, handleReset, setEditMode, setParentCertificate]);


    const status = renderSwitchStatus();


    return (
        <>
            <AccountingMenu selectedMenuItem="/multi-payment-certificates"/>
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2} alignItems="center">
                    <Grid item xs={11}>
                        <Box display="flex" alignItems="center" justifyContent="space-between" sx={{paddingLeft: 3}}>
                            <Typography variant="h6">
                                {titleText}
                            </Typography>
                            <IconButton onClick={() => setIsFormCollapsed(!isFormCollapsed)}>
                                {isFormCollapsed ? <ExpandMoreIcon/> : <ExpandLessIcon/>}
                            </IconButton>
                            {(editMode === 2 || editMode === 3) && (
                                <CertificateActionsMenu
                                    workEffortId={certificate?.workEffortId}
                                    currentStatusId={certificate?.currentStatusId}
                                    handleApprove={handleApproveCertificate}
                                    handleReset={handleResetCertificate}
                                    disabled={apiLoading || items.length === 0}
                                />
                            )}
                        </Box>
                    </Grid>
                    <Grid item xs={1}>
                        {(editMode === 2 || editMode === 3) && (
                            <RibbonContainer>
                                <Ribbon
                                    side={language === "ar" ? "left" : "right"}
                                    type="corner"
                                    size="large"
                                    backgroundColor={status.backgroundColor}
                                    color={status.foreColor}
                                    fontFamily="sans-serif"
                                >
                                    {status.label}
                                </Ribbon>
                            </RibbonContainer>

                        )}
                    </Grid>
                </Grid>
                <Collapse in={!isFormCollapsed}>
                    <Form
                        ref={formRef}
                        initialValues={{...initialValues, _itemsVersion: 0}}
                        key={formKey}
                        onSubmit={handleSubmit}
                        render={(formRenderProps) => (
                            <>
                                {/* Hidden field that Kendo watches */}
                                <Field name="_itemsVersion" type="hidden" component="input"/>

                                <FormElement>
                                    <fieldset className="k-form-fieldset">
                                        <Grid container spacing={2}
                                              sx={{display: 'flex', flexDirection: 'row', alignItems: 'flex-end'}}>
                                            <Field name="workEffortId" component="input" type="hidden"/>
                                            <Grid item xs={2}>
                                                <Field
                                                    id="date"
                                                    name="date"
                                                    label={getTranslatedLabel(`${localizationKey}.date`, "Date *")}
                                                    component={FormDatePicker}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Field
                                                    id="glAccountId"
                                                    name="glAccountId"
                                                    label={getTranslatedLabel(`${localizationKey}.glAccount`, "Cost Center")}
                                                    component={MemoizedFormComboBox2}
                                                    data={glAccounts || []}
                                                    dataItemKey="glAccountId"
                                                    textField="accountName"
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="description"
                                                    name="description"
                                                    label={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                                    component={FormInput}
                                                />
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Field
                                                    id="partyIdEmployee"
                                                    name="partyIdEmployee"
                                                    component={FormComboBoxVirtualParty}
                                                    label={getTranslatedLabel(`${localizationKey}.paymentTo`, "Payment To")}
                                                    valueField="fromPartyId"
                                                    textField="fromPartyName"
                                                    //validator={requiredValidator}
                                                />
                                            </Grid>

                                            <Grid item xs={1}>
                                                <Button
                                                    color="secondary"
                                                    onClick={() => setShowNewCustomer(true)}
                                                    variant="outlined"
                                                >
                                                    +
                                                </Button>
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Field
                                                    id="notes"
                                                    name="notes"
                                                    label={getTranslatedLabel(`${localizationKey}.referenceNum`, "Reference Number")}
                                                    component={FormInput}
                                                />
                                            </Grid>

                                            
                                        </Grid><Grid container item spacing={2} sx={{
                                        display: 'flex',
                                        flexDirection: 'row',
                                        justifyContent: 'flex-start',
                                        alignItems: 'flex-end',
                                        mt: 2
                                    }}>
                                        {(editMode === 1 || (editMode === 2 && certificate?.currentStatusId !== "WEPR_APPROVED")) && (
                                            <Grid item>
                                                <Button
                                                    type="submit"
                                                    variant="contained"
                                                    disabled={!formRenderProps.valid || !formRenderProps.modified || apiLoading}
                                                    sx={{mr: 2}}
                                                >
                                                    {getTranslatedLabel(
                                                        `${localizationKey}.${editMode === 1 ? "create" : "update"}`,
                                                        editMode === 1 ? "Create Certificate" : "Update Certificate"
                                                    )}
                                                </Button>
                                            </Grid>
                                        )}
                                        {(editMode === 2 || editMode === 3) && certificate && (
                                            <Grid item>
                                                <MultiPaymentCertificateExcel
                                                    companyName={companyName}
                                                    certificate={certificate}
                                                    items={items}
                                                    transactions={acctTransEntryData || []}
                                                    getTranslatedLabel={getTranslatedLabel}
                                                    isFetching={isFetchingTransactions}
                                                    language={language}
                                                />
                                            </Grid>
                                        )}
                                        <Grid item>
                                            <Button
                                                onClick={handleCancelForm}
                                                color="error"
                                                variant="contained"
                                                disabled={apiLoading}
                                            >
                                                {getTranslatedLabel("general.cancel", "Cancel")}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                        
                                    </fieldset>
                                </FormElement>
                                
                            </>
                        )}
                    />
                </Collapse>
                <MultiPaymentItemsList
                    workEffortId={certificate?.workEffortId || ""}
                    items={items}
                    addItem={addItem}
                    updateItem={updateItem}
                    deleteItem={deleteItem}
                />
                {showNewCustomer && (
                    <ModalContainer
                        show={showNewCustomer}
                        onClose={() => setShowNewCustomer(false)}
                        width={500}
                    >
                        <CreatePartyModalForm
                            onClose={() => setShowNewCustomer(false)}
                        />
                    </ModalContainer>
                )}
            </Paper>
        </>
    );
}