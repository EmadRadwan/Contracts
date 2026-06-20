import React, { useMemo, useState } from "react";
import { Button, Grid, Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../../app/common/form/MemoizedFormDropDownList";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import { requiredValidator } from "../../../../../app/common/form/Validators";
import { toast } from "react-toastify";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import { ProjectCommissionRate } from "../../../../../app/models/orders/projectCommissionRate";
import {
    useAddProjectCommissionRateMutation,
    useUpdateProjectCommissionRateMutation,
} from "../../../../../app/store/apis/projectsApi";
import { FormComboBoxVirtualProject } from "../../../../../app/common/form/FormComboBoxVirtualProject";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";

const saleTypeOptions = [
    { saleTypeId: "COMM_SALE_DIRECT", description: "بيع مباشر" },
    { saleTypeId: "COMM_SALE_PERSONAL", description: "بيع شخصي" },
    { saleTypeId: "COMM_SALE_INDIRECT", description: "بيع غير مباشر" },
];

interface Props {
    rate?: ProjectCommissionRate;
    editMode: number;
    cancelEdit: () => void;
}

export default function ProjectCommissionRateForm({ rate, editMode, cancelEdit }: Props) {
    const [addRate, { isLoading: isCreating }] = useAddProjectCommissionRateMutation();
    const [updateRate, { isLoading: isUpdating }] = useUpdateProjectCommissionRateMutation();
    const [buttonFlag, setButtonFlag] = useState(false);
    const { getTranslatedLabel } = useTranslationHelper();

    const initialValues = useMemo(() => {
        if (editMode !== 2 || !rate) {
            return {
                projectId: null,
                saleTypeId: null,
                salesRepPercent: 0,
                managerPercent: 0,
                externalCompanyPercent: null,
                externalSalesRepPercent: null,
                externalManagerPercent: null,
            };
        }

        return {
            projectCommissionRateId: rate.projectCommissionRateId,
            projectId: rate.projectId
                ? { projectId: rate.projectId, projectName: rate.projectName ?? "", facilityId: "" }
                : null,
            saleTypeId: rate.saleTypeId,
            salesRepPercent: rate.salesRepPercent,
            managerPercent: rate.managerPercent,
            externalCompanyPercent: rate.externalCompanyPercent ?? null,
            externalSalesRepPercent: rate.externalSalesRepPercent ?? null,
            externalManagerPercent: rate.externalManagerPercent ?? null,
        };
    }, [editMode, rate]);

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        const isIndirect = data.saleTypeId === "COMM_SALE_INDIRECT";

        const payload: Partial<ProjectCommissionRate> = {
            projectId: data.projectId?.projectId ?? data.projectId,
            saleTypeId: data.saleTypeId,
            salesRepPercent: data.salesRepPercent ?? 0,
            managerPercent: data.managerPercent ?? 0,
            externalCompanyPercent: isIndirect ? (data.externalCompanyPercent ?? null) : null,
            externalSalesRepPercent: isIndirect ? (data.externalSalesRepPercent ?? null) : null,
            externalManagerPercent: isIndirect ? (data.externalManagerPercent ?? null) : null,
        };

        try {
            if (editMode === 2) {
                await updateRate({ ...payload, projectCommissionRateId: rate!.projectCommissionRateId }).unwrap();
                toast.success(getTranslatedLabel("commissionRate.form.updateSuccess", "Commission rate updated successfully"));
            } else {
                await addRate(payload).unwrap();
                toast.success(getTranslatedLabel("commissionRate.form.createSuccess", "Commission rate created successfully"));
            }
            cancelEdit();
        } catch {
            toast.error(getTranslatedLabel("commissionRate.form.error", "Failed to save commission rate"));
        } finally {
            setButtonFlag(false);
        }
    }

    return (
        <>
            <SalesRequestMenu selectedMenuItem="project-commission-rates" />
            <Paper elevation={5} className="div-container-withBorderCurved" style={{ padding: "16px" }}>
                {editMode === 1 && (
                    <Typography variant="h4" color="green" sx={{ mb: 2 }}>
                        {getTranslatedLabel("commissionRate.form.new", "New Commission Rate")}
                    </Typography>
                )}
                {editMode === 2 && rate && (
                    <Typography variant="h4" color="black" sx={{ mb: 2 }}>
                        {getTranslatedLabel("commissionRate.form.edit", `Edit Commission Rate (${rate.projectCommissionRateId})`)}
                    </Typography>
                )}
                <Form
                    key={rate?.projectCommissionRateId ?? "new"}
                    initialValues={initialValues}
                    onSubmit={handleSubmitData}
                    render={(formRenderProps) => {
                        const isIndirect = formRenderProps.valueGetter("saleTypeId") === "COMM_SALE_INDIRECT";

                        return (
                            <FormElement>
                                <fieldset className="k-form-fieldset">
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} md={5}>
                                            <Field
                                                id="projectId"
                                                name="projectId"
                                                label={getTranslatedLabel("commissionRate.form.project", "Project")}
                                                component={FormComboBoxVirtualProject}
                                                validator={requiredValidator}
                                                disabled={editMode === 2}
                                            />
                                        </Grid>
                                    </Grid>
                                    <Grid container spacing={2} sx={{ mt: 1 }}>
                                        <Grid item xs={12} md={4}>
                                            <Field
                                                id="saleTypeId"
                                                name="saleTypeId"
                                                label={getTranslatedLabel("commissionRate.form.saleType", "Sale Type")}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="saleTypeId"
                                                textField="description"
                                                data={saleTypeOptions}
                                                validator={requiredValidator}
                                                disabled={editMode === 2}
                                            />
                                        </Grid>
                                    </Grid>
                                    <Grid container spacing={2} sx={{ mt: 1 }}>
                                        <Grid item xs={6} md={3}>
                                            <Field
                                                id="salesRepPercent"
                                                name="salesRepPercent"
                                                label={getTranslatedLabel("commissionRate.form.salesRepPercent", "Sales Rep %")}
                                                component={FormNumericTextBox}
                                                min={0}
                                                max={100}
                                                format="n4"
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={6} md={3}>
                                            <Field
                                                id="managerPercent"
                                                name="managerPercent"
                                                label={getTranslatedLabel("commissionRate.form.managerPercent", "Manager %")}
                                                component={FormNumericTextBox}
                                                min={0}
                                                max={100}
                                                format="n4"
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                    </Grid>
                                    {isIndirect && (
                                        <Grid container spacing={2} sx={{ mt: 1 }}>
                                            <Grid item xs={6} md={3}>
                                                <Field
                                                    id="externalCompanyPercent"
                                                    name="externalCompanyPercent"
                                                    label={getTranslatedLabel("commissionRate.form.extCompanyPercent", "Ext. Company %")}
                                                    component={FormNumericTextBox}
                                                    min={0}
                                                    max={100}
                                                    format="n4"
                                                />
                                            </Grid>
                                            <Grid item xs={6} md={3}>
                                                <Field
                                                    id="externalSalesRepPercent"
                                                    name="externalSalesRepPercent"
                                                    label={getTranslatedLabel("commissionRate.form.extSalesRepPercent", "Ext. Sales Rep %")}
                                                    component={FormNumericTextBox}
                                                    min={0}
                                                    max={100}
                                                    format="n4"
                                                />
                                            </Grid>
                                            <Grid item xs={6} md={3}>
                                                <Field
                                                    id="externalManagerPercent"
                                                    name="externalManagerPercent"
                                                    label={getTranslatedLabel("commissionRate.form.extManagerPercent", "Ext. Manager %")}
                                                    component={FormNumericTextBox}
                                                    min={0}
                                                    max={100}
                                                    format="n4"
                                                />
                                            </Grid>
                                        </Grid>
                                    )}
                                    <div className="k-form-buttons" style={{ marginTop: "16px" }}>
                                        <Grid container spacing={1}>
                                            <Grid item xs={1}>
                                                <Button
                                                    type="submit"
                                                    color="success"
                                                    variant="contained"
                                                    disabled={!formRenderProps.allowSubmit || buttonFlag}
                                                >
                                                    {getTranslatedLabel("commissionRate.form.submit", "Submit")}
                                                </Button>
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Button onClick={cancelEdit} variant="contained" color="error">
                                                    {getTranslatedLabel("commissionRate.form.cancel", "Cancel")}
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </div>
                                    {buttonFlag && (
                                        <Grid container spacing={2} sx={{ mt: 1 }}>
                                            <Grid item xs={4}>
                                                <LoadingComponent
                                                    message={getTranslatedLabel("commissionRate.form.processing", "Processing...")}
                                                />
                                            </Grid>
                                        </Grid>
                                    )}
                                </fieldset>
                            </FormElement>
                        );
                    }}
                />
            </Paper>
        </>
    );
}
