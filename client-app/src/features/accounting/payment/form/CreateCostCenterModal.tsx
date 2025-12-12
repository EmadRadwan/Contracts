// src/features/accounting/costcenter/CreateCostCenterModal.tsx
import React, { useState } from 'react';
import { Button, Grid, Typography } from '@mui/material';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {requiredValidator} from "../../../../app/common/form/Validators";
import FormInput from "../../../../app/common/form/FormInput";
import {useCreateCostCenterMutation} from "../../../../app/store/apis/accounting/paymentTypesApi";
import {FormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList2";

interface Props {
    open: boolean;
    onClose: () => void;
    isOutPayment: boolean;
}

const isOutOptions = [
    { value: 'Y', text: 'نعم (للصرف)' },
    { value: 'N', text: 'لا (للقبض)' },
];

export default function CreateCostCenterModal({ open, onClose, onCreated, isOutPayment }: Props) {
    const [createCostCenter, { isLoading }] = useCreateCostCenterMutation();
    const [submitError, setSubmitError] = useState<string | null>(null);

    const handleSubmit = async (data: any) => {
        setSubmitError(null);
        try {
            const result = await createCostCenter({
                description: data.description.trim(),
                isOutPayment: isOutPayment ? 'Y' : 'N',
            }).unwrap();
            
            onClose();
        } catch (err: any) {
            setSubmitError(err?.data?.message || 'فشل إنشاء مركز التكلفة');
        }
    };

    return (
        <ModalContainer show={open} onClose={onClose} width={500}>
            <Typography variant="h6" gutterBottom>إضافة مركز تكلفة جديد</Typography>

            <Form
                onSubmit={handleSubmit}
                render={(formRenderProps) => (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Field
                                name="description"
                                label="الوصف *"
                                component={FormInput}
                                validator={requiredValidator}
                            />

                            <Field
                                name="isOutPayment"
                                label="للصرف؟"
                                component={FormDropDownList}
                                data={isOutOptions}
                                dataItemKey="value"
                                textField="text"
                                defaultValue={isOutPayment ? 'Y' : 'N'}
                                //disabled
                            />

                            {submitError && (
                                <Grid item xs={12}>
                                    <Typography color="error">{submitError}</Typography>
                                </Grid>
                            )}

                            <div className="k-form-buttons" style={{ marginTop: 20 }}>
                                <Button
                                    type="submit"
                                    variant="contained"
                                    disabled={!formRenderProps.valid || isLoading}
                                >
                                    إنشاء
                                </Button>
                                <Button onClick={onClose} variant="outlined" sx={{ ml: 1 }}>
                                    إلغاء
                                </Button>
                            </div>

                            {isLoading && <LoadingComponent message="جاري الإنشاء..." />}
                        </fieldset>
                    </FormElement>
                )}
            />
        </ModalContainer>
    );
}