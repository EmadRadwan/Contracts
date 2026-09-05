import React from 'react';
import { Grid, Box } from '@mui/material';
import { Field, FormRenderProps } from '@progress/kendo-react-form';
import Button from '@mui/material/Button';
import { FormComboBoxVirtualCustomer } from '../../../../../../app/common/form/FormComboBoxVirtualCustomer';
import { MemoizedFormDropDownList2 } from '../../../../../../app/common/form/MemoizedFormDropDownList2';
import { MemoizedFormCheckBox } from '../../../../../../app/common/form/FormCheckBox';
import FormTextArea from '../../../../../../app/common/form/FormTextArea';
import { requiredValidator } from '../../../../../../app/common/form/Validators';
import { FormSection } from './FormSection';
import PersonIcon from '@mui/icons-material/Person';
import AddIcon from '@mui/icons-material/Add';

interface CustomerInformationSectionProps {
    formRenderProps: FormRenderProps;
    formEditMode: number;
    onCustomerChange: (e: any) => void;
    showNewCustomer: boolean;
    setShowNewCustomer: (show: boolean) => void;
    currencies?: any[];
    isTaxLoading?: boolean;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

export const CustomerInformationSection: React.FC<CustomerInformationSectionProps> = ({
    formRenderProps,
    formEditMode,
    onCustomerChange,
    showNewCustomer,
    setShowNewCustomer,
    currencies = [],
    isTaxLoading = false,
    getTranslatedLabel,
}) => {
    const localizationKey = 'order.so.form';

    return (
        <FormSection
            title={getTranslatedLabel(`${localizationKey}.customerInfo`, 'Customer Information')}
            description={getTranslatedLabel(
                `${localizationKey}.customerInfoDesc`,
                'Customer details and order settings'
            )}
            icon={<PersonIcon />}
            defaultExpanded={true}
        >
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                {/* Customer Selection Section */}
                <Box sx={{
                    p: 2,
                    backgroundColor: '#FAFAFA',
                    borderRadius: '8px',
                    display: 'flex', flexDirection: 'row', gap: 3,
                    border: '1px solid #E8E8E8'
                }}>
                    <Grid container spacing={2} alignItems="flex-start">
                        <Grid item xs={12} sx={{ display: 'flex', alignItems: 'flex-end', gap: 1 }}>
                            <Box sx={{ flex: 1 }}>
                                <Field
                                    id="fromPartyId"
                                    name="fromPartyId"
                                    label={getTranslatedLabel(`${localizationKey}.customer`, 'Customer')}
                                    component={FormComboBoxVirtualCustomer}
                                    autoComplete="off"
                                    disabled={formEditMode > 1}
                                    onChange={onCustomerChange}
                                    validator={requiredValidator}
                                />
                            </Box>
                            <Button
                                onClick={() => setShowNewCustomer(true)}
                                disabled={formEditMode > 1}
                                sx={{
                                    minWidth: '44px',
                                    width: '44px',
                                    height: '44px',
                                    padding: 0,
                                    backgroundColor: '#09419A',
                                    color: '#FFFFFF',
                                    borderRadius: '4px',
                                    '&:hover': {
                                        backgroundColor: '#062E6F'
                                    },
                                    '&:disabled': {
                                        backgroundColor: '#D0D0D0',
                                        color: '#808080'
                                    }
                                }}
                            >
                                <AddIcon sx={{ fontSize: '24px' }} />
                            </Button>
                        </Grid>
                    </Grid>

                    <Grid container spacing={2} alignItems="flex-end">
                        <Grid item xs={12} sm={formEditMode === 1 ? 6 : 12}>
                            <Field
                                id="currencyUomId"
                                name="currencyUomId"
                                component={MemoizedFormDropDownList2}
                                data={currencies}
                                label={getTranslatedLabel(`${localizationKey}.currency`, 'Currency')}
                                dataItemKey="currencyUomId"
                                disabled={formEditMode > 1}
                                textField="description"
                            />
                        </Grid>

                        {formEditMode === 1 && (
                            <Grid item xs={12} sm={6}>
                                <Box sx={{ display: 'flex', alignItems: 'center', height: '48px' }}>
                                    <Field
                                        id="addTax"
                                        name="addTax"
                                        label={getTranslatedLabel(`${localizationKey}.addTax`, 'Add Tax')}
                                        component={MemoizedFormCheckBox}
                                        disabled={isTaxLoading}
                                    />
                                </Box>
                            </Grid>
                        )}
                    </Grid>
                </Box>

                {/* Remarks Section */}
                <Box sx={{
                    p: 2,
                    backgroundColor: '#FAFAFA',
                    borderRadius: '8px',
                    border: '1px solid #E8E8E8'
                }}>
                    <Grid container spacing={2}>
                        <Grid item xs={12} sm={6}>
                            <Field
                                id="customerRemarks"
                                name="customerRemarks"
                                label={getTranslatedLabel(`${localizationKey}.customer-remarks`, 'Customer Remarks')}
                                component={FormTextArea}
                                autoComplete="off"
                                disabled={formEditMode > 2}
                            />
                        </Grid>

                        <Grid item xs={12} sm={6}>
                            <Field
                                id="internalRemarks"
                                name="internalRemarks"
                                label={getTranslatedLabel(`${localizationKey}.internal-remarks`, 'Internal Remarks')}
                                component={FormTextArea}
                                autoComplete="off"
                                disabled={formEditMode > 2}
                            />
                        </Grid>
                    </Grid>
                </Box>
            </Box>
        </FormSection>
    );
};

export default CustomerInformationSection;
