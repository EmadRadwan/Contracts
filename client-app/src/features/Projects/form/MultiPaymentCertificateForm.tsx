import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {Field, Form, FormElement, FormRenderProps} from "@progress/kendo-react-form";
import {Box, Button, Collapse, Grid, IconButton, Paper, Typography} from "@mui/material";
import { RibbonContainer } from "react-ribbons";
import useMultiPaymentCertificate from "../hook/useMultiPaymentCertificate";
import { v4 as uuidv4 } from "uuid";
import {MultiPaymentCertificate} from "../../../app/models/project/MultiPaymentCertificate";
import {useAppDispatch, useAppSelector, useFetchPaymentMethodsQuery} from "../../../app/store/configureStore";
import MultiPaymentItemsList from "../dashboard/MultiPaymentItemsList";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import AccountingMenu from "../../accounting/invoice/menu/AccountingMenu";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {requiredValidator} from "../../../app/common/form/Validators";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import FormInput from "../../../app/common/form/FormInput";

interface Props {
    selectedCertificate?: MultiPaymentCertificate;
    editMode: number; // 1: add, 2: edit
    cancelEdit: () => void;
}

export default function MultiPaymentCertificateForm({ selectedCertificate, editMode, cancelEdit }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.multiPaymentCertificate.form";
    const { language } = useAppSelector((state) => state.localization);
    const { data: paymentMethods, isLoading: paymentMethodsLoading } = useFetchPaymentMethodsQuery(undefined);
    const [isFormCollapsed, setIsFormCollapsed] = useState(false);

    const [formKey, setFormKey] = useState<number>(1);
    const formRef = useRef<any>(null);

    const filteredPaymentMethods = useMemo(() => {
        return paymentMethods?.filter(
            (method) => method.paymentMethodTypeId === "COMPANY_CHECK" || method.paymentMethodTypeId === "CASH"
        ) || [];
    }, [paymentMethods]);
    
    const {
        certificate,
        setCertificate,
        handleCreate,
        handleUpdate,
        isLoading: apiLoading,
    } = useMultiPaymentCertificate({
        selectedCertificate,
        editMode,
        setFormKey,
    });

    const initialValues = useMemo(() => ({
        workEffortId: selectedCertificate?.workEffortId || "",
        code: selectedCertificate?.code || "", // Auto-generated in backend, read-only
        date: selectedCertificate?.date ? new Date(selectedCertificate.date) : new Date(),
        description: selectedCertificate?.description || "",
        paymentMethodId: selectedCertificate?.paymentMethodId || "",
        chequeNumber: selectedCertificate?.chequeNumber || "",
        chequeDate: selectedCertificate?.chequeDate ? new Date(selectedCertificate.chequeDate) : null,
    }), [selectedCertificate]);


    useEffect(() => {
        if (selectedCertificate) {
            setCertificate(selectedCertificate);
        } else {
            setCertificate(undefined);
        }
    }, [selectedCertificate, setCertificate]);

    const handleCancelForm = useCallback(() => {
        setCertificate(undefined);
        setFormKey((prev) => prev + 1);
        cancelEdit();
    }, [setCertificate, cancelEdit]);

    const handleSubmit = useCallback((values: any) => {
        const serializedValues: MultiPaymentCertificate = {
            workEffortId: values.workEffortId || uuidv4(),
            code: values.code || "",
            date: values.date instanceof Date ? values.date.toISOString() : new Date().toISOString(),
            description: values.description || "",
            paymentMethodId: values.paymentMethodId || "",
            chequeNumber: values.chequeNumber || "",
            chequeDate: values.chequeDate instanceof Date ? values.chequeDate.toISOString() : null,
        };
        if (editMode === 1) {
            handleCreate({
                values: serializedValues,
                isValid: formRef.current?.isValid(),
                menuItem: "Create Certificate",
            });
        } else {
            handleUpdate({
                values: serializedValues,
                isValid: formRef.current?.isValid(),
                menuItem: "Update Certificate",
            });
        }
    }, [handleCreate, handleUpdate, editMode]);


    const titleText = editMode === 1
        ? getTranslatedLabel(`${localizationKey}.title`, "New Multi-Payment Certificate")
        : `${getTranslatedLabel(`${localizationKey}.title`, "Multi Payment Certificate")} ${selectedCertificate?.workEffortId || ""}`;



    return (
        <RibbonContainer>
            <AccountingMenu selectedMenuItem="/multi-payment-certificates" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2} alignItems="center">
                    <Grid item xs={11}>
                        <Box display="flex" alignItems="center" justifyContent="space-between" sx={{ paddingLeft: 3 }}>
                            <Typography variant="h6">
                                {titleText}
                            </Typography>
                            <IconButton onClick={() => setIsFormCollapsed(!isFormCollapsed)}>
                                {isFormCollapsed ? <ExpandMoreIcon /> : <ExpandLessIcon />}
                            </IconButton>
                        </Box>
                    </Grid>
                    <Grid item xs={1}>
                        {/* Placeholder for potential Ribbon status display, if needed in future */}
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
                                    <Grid container spacing={2} sx={{ display: 'flex', flexDirection: 'row', alignItems: 'center' }}>
                                        <Field name="workEffortId" component="input" type="hidden" />
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
                                                id="paymentMethodId"
                                                name="paymentMethodId"
                                                label={getTranslatedLabel(`${localizationKey}.paymentMethod`, "Payment Method *")}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="paymentMethodId"
                                                textField="description"
                                                data={filteredPaymentMethods}
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
                                        {formRenderProps.valueGetter('paymentMethodId') !== 'CASH' && (
                                            <>
                                                <Grid item xs={1}>
                                                    <Field
                                                        id="chequeNumber"
                                                        name="chequeNumber"
                                                        label={getTranslatedLabel(`${localizationKey}.chequeNumber`, "Cheque Number")}
                                                        component={FormInput}
                                                    />
                                                </Grid>
                                                <Grid item xs={2}>
                                                    <Field
                                                        id="chequeDate"
                                                        name="chequeDate"
                                                        label={getTranslatedLabel(`${localizationKey}.chequeDate`, "Cheque Date")}
                                                        component={FormDatePicker}
                                                    />
                                                </Grid>
                                            </>
                                        )}
                                        <Grid container item spacing={2} sx={{ display: 'flex', flexDirection: 'row', justifyContent: 'flex-start', mt: 2 }}>
                                            <Grid item>
                                                <Button
                                                    type="submit"
                                                    variant="contained"
                                                    disabled={!formRenderProps.valid}
                                                    sx={{ mr: 2 }} // REFACTOR: Added margin-right for spacing between buttons
                                                >
                                                    {getTranslatedLabel(
                                                        `${localizationKey}.${editMode === 1 ? "create" : "update"}`,
                                                        editMode === 1 ? "Create Certificate" : "Update Certificate"
                                                    )}
                                                </Button>
                                            </Grid>
                                            <Grid item>
                                                <Button
                                                    onClick={handleCancelForm}
                                                    color="error"
                                                    variant="contained"
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
                <MultiPaymentItemsList workEffortId={certificate?.workEffortId || ""} />
            </Paper>
        </RibbonContainer>
    );
}