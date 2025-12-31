import React, {useCallback, useEffect, useMemo, useRef, useState} from "react";
import {Field, Form, FormElement, FormRenderProps} from "@progress/kendo-react-form";
import {Box, Button, Collapse, Grid, IconButton, Menu, MenuItem, Paper, Skeleton, Typography} from "@mui/material";
import {Ribbon, RibbonContainer} from "react-ribbons";
import useMultiPaymentCertificate from "../hook/useMultiPaymentCertificate";
import {FormInitialValues, MultiPaymentCertificate} from "../../../app/models/project/MultiPaymentCertificate";
import {RootState, useAppSelector} from "../../../app/store/configureStore";
import MultiPaymentItemsList from "../dashboard/MultiPaymentItemsList";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import AccountingMenu from "../../accounting/invoice/menu/AccountingMenu";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import {requiredValidator} from "../../../app/common/form/Validators";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import FormInput from "../../../app/common/form/FormInput";

import {MemoizedFormComboBox2} from "../../../app/common/form/FormComboBox2";
import {
    useFetchGlAccountOrganizationHierarchyLovQuery,
    useFetchGlAccountOrgCashOrEquivalentLovQuery
} from "../../../app/store/apis";


interface CertificateActionsMenuProps {
    workEffortId: string | undefined;
    currentStatusId: string | undefined;
    handleApprove: () => void;
    disabled: boolean;
}

const CertificateActionsMenu: React.FC<CertificateActionsMenuProps> = ({
                                                                           workEffortId,
                                                                           currentStatusId,
                                                                           handleApprove,
                                                                           disabled,
                                                                       }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.multiPaymentCertificate.form";
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleClick}
                disabled={disabled || !workEffortId || currentStatusId === "WEPR_APPROVED"}
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
                    disabled={!workEffortId || currentStatusId === "WEPR_APPROVED"}
                >
                    {getTranslatedLabel(`${localizationKey}.approve`, "Approve")}
                </MenuItem>
            </Menu>
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

export default function MultiPaymentCertificateForm({selectedCertificate, editMode, cancelEdit, setEditMode, setParentCertificate}: Props) {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "projects.multiPaymentCertificate.form";
    const [isFormCollapsed, setIsFormCollapsed] = useState(false);
    const {language} = useAppSelector((state) => state.localization);

    const [formKey, setFormKey] = useState<number>(1);
    const formRef = useRef<any>(null);
    const {user} = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";
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
        handleUpdate, setItems, handleApprove,
        isLoading: apiLoading,
    } = useMultiPaymentCertificate({
        selectedCertificate,
        editMode,
        setFormKey,
        setEditMode, setParentCertificate
    });

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
            }
        });
    }, [certificate, handleApprove, items, setEditMode, setParentCertificate]);



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
                            {editMode === 2 && (
                                <CertificateActionsMenu
                                    workEffortId={certificate?.workEffortId}
                                    currentStatusId={certificate?.currentStatusId}
                                    handleApprove={handleApproveCertificate}
                                    disabled={apiLoading || items.length === 0}
                                />
                            )}
                        </Box>
                    </Grid>
                    <Grid item xs={1}>
                        {editMode === 2 && (
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
                        initialValues={initialValues}
                        key={formKey}
                        onSubmit={handleSubmit}
                        render={(formRenderProps: FormRenderProps) => (
                            <FormElement>
                                <fieldset className="k-form-fieldset">
                                    <Grid container spacing={2}
                                          sx={{display: 'flex', flexDirection: 'row', alignItems: 'center'}}>
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
                                        
                                        <Grid item xs={3}>
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
                                        
                                        <Grid container item spacing={2} sx={{
                                            display: 'flex',
                                            flexDirection: 'row',
                                            justifyContent: 'flex-start',
                                            mt: 2
                                        }}>
                                            {(editMode === 1 || (editMode === 2 && certificate?.currentStatusId !== "WEPR_APPROVED")) && (
                                                <Grid item>
                                                    <Button
                                                        type="submit"
                                                        variant="contained"
                                                        disabled={!formRenderProps.valid || !formRenderProps.modified || apiLoading}
                                                        sx={{ mr: 2 }}
                                                    >
                                                        {getTranslatedLabel(
                                                            `${localizationKey}.${editMode === 1 ? "create" : "update"}`,
                                                            editMode === 1 ? "Create Certificate" : "Update Certificate"
                                                        )}
                                                    </Button>
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
                                    </Grid>
                                </fieldset>
                            </FormElement>
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
            </Paper>
        </>
    );
}