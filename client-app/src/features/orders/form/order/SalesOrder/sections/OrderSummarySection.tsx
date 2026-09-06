import React, { useState } from 'react';
import { Grid, Box, Typography, Button, useTheme, useMediaQuery } from '@mui/material';
import { Field, FormRenderProps } from '@progress/kendo-react-form';
import { FormRadioGroup } from '../../../../../../app/common/form/FormRadioGroup';
import { Popover } from '@progress/kendo-react-tooltip';
import { Link, NavLink } from 'react-router-dom';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import { FormSection } from './FormSection';

interface OrderSummarySectionProps {
    memoizedOrderTotals: React.ReactNode;
    formRenderProps: FormRenderProps;
    formEditMode: number;
    invoiceId?: string;
    paymentId?: string;
    finalPaymentMethodTypes?: any[];
    paymentMethodLabel: string;
    isOrderApprovedOrBillingAccountPresent: boolean;
    billingAccount?: any[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

export const OrderSummarySection: React.FC<OrderSummarySectionProps> = ({
    memoizedOrderTotals,
    formRenderProps,
    formEditMode,
    invoiceId,
    paymentId,
    finalPaymentMethodTypes = [],
    paymentMethodLabel,
    isOrderApprovedOrBillingAccountPresent,
    billingAccount = [],
    getTranslatedLabel,
}) => {
    const localizationKey = 'order.so.form';
    const [showPopover, setShowPopover] = useState(false);
    const popoverAnchorRef = React.useRef<HTMLButtonElement | null>(null);
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

    return (
        <FormSection
            title={getTranslatedLabel(`${localizationKey}.summary`, 'Order Summary')}
            description={getTranslatedLabel(
                `${localizationKey}.summaryDesc`,
                'Totals, payment method, and order status'
            )}
            icon={<AccountBalanceIcon />}
            defaultExpanded={true}
        >
            <Grid container spacing={3}>
                {/* Order Totals */}
                <Grid item xs={12} md={6}>
                    <Box sx={{ p: 2, backgroundColor: '#F8F9FA', borderRadius: '8px' }}>
                        {memoizedOrderTotals}
                    </Box>
                </Grid>

                {/* Payment & Status Information */}
                <Grid item xs={12} md={6}>
                    <Grid container spacing={2}>
                        {/* Payment Method */}
                        {formEditMode < 3 ? (
                            <Grid item xs={12}>
                                <Field
                                    id="paymentMethodTypeId"
                                    name="paymentMethodTypeId"
                                    label={getTranslatedLabel(`${localizationKey}.pmt`, 'Payment Method Type')}
                                    component={FormRadioGroup}
                                    disabled={formEditMode > 2}
                                    layout="vertical"
                                    data={finalPaymentMethodTypes}
                                    onChange={() => {
                                        formRenderProps.onChange('billingAccountId', {
                                            value: null,
                                        });
                                    }}
                                />
                            </Grid>
                        ) : (
                            <Grid item xs={12}>
                                <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                                    {getTranslatedLabel(`${localizationKey}.pmt`, 'Payment Method')}
                                </Typography>
                                <Typography variant="body2">
                                    {paymentMethodLabel || 'N/A'}
                                </Typography>
                            </Grid>
                        )}

                        {/* Invoice Link */}
                        {invoiceId && formEditMode !== 1 && (
                            <Grid item xs={12}>
                                <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                                    {getTranslatedLabel('general.invoiceNo', 'Invoice No:')}
                                </Typography>
                                <Link to={`/invoices/${invoiceId}`} style={{ color: '#09419A' }}>
                                    <Typography
                                        variant="body2"
                                        sx={{
                                            color: '#09419A',
                                            textDecoration: 'underline',
                                            cursor: 'pointer',
                                        }}
                                    >
                                        {invoiceId}
                                    </Typography>
                                </Link>
                            </Grid>
                        )}

                        {/* Payment Link */}
                        {paymentId && formEditMode !== 1 && (
                            <Grid item xs={12}>
                                <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
                                    {getTranslatedLabel('general.paymentNo', 'Payment No:')}
                                </Typography>
                                <NavLink
                                    to="/payments"
                                    state={{ selectedPaymentId: paymentId }}
                                    style={{ color: '#09419A' }}
                                >
                                    <Typography
                                        variant="body2"
                                        sx={{
                                            color: '#09419A',
                                            textDecoration: 'underline',
                                            cursor: 'pointer',
                                        }}
                                    >
                                        {paymentId}
                                    </Typography>
                                </NavLink>
                            </Grid>
                        )}

                        {/* Billing Account Balance */}
                        {isOrderApprovedOrBillingAccountPresent && (
                            <Grid item xs={12}>
                                <Button
                                    ref={popoverAnchorRef}
                                    variant="contained"
                                    color="primary"
                                    onClick={() => setShowPopover(!showPopover)}
                                    size={isMobile ? 'small' : 'medium'}
                                    fullWidth={isMobile}
                                >
                                    {getTranslatedLabel(
                                        `${localizationKey}.showBillingBalance`,
                                        'Show Billing Account Balance'
                                    )}
                                </Button>
                                <Popover
                                    show={showPopover}
                                    anchor={popoverAnchorRef.current}
                                    position="bottom"
                                >
                                    <Box p={2} width={250}>
                                        {billingAccount?.map((ba: any) => (
                                            <Box key={ba.billingAccountId} sx={{ mb: 2 }}>
                                                <Typography
                                                    variant="subtitle2"
                                                    sx={{ fontWeight: 600, mb: 0.5 }}
                                                >
                                                    {getTranslatedLabel('general.accountLimit', 'Account Limit:')}
                                                </Typography>
                                                <Typography variant="body2">
                                                    {ba.accountLimit.toLocaleString()} {ba.accountCurrencyUomId}
                                                </Typography>

                                                <Typography
                                                    variant="subtitle2"
                                                    sx={{ fontWeight: 600, mt: 1, mb: 0.5 }}
                                                >
                                                    {getTranslatedLabel('general.accountBalance', 'Account Balance:')}
                                                </Typography>
                                                <Typography variant="body2">
                                                    {ba.accountBalance.toLocaleString()} {ba.accountCurrencyUomId}
                                                </Typography>
                                            </Box>
                                        ))}
                                    </Box>
                                </Popover>
                            </Grid>
                        )}
                    </Grid>
                </Grid>
            </Grid>
        </FormSection>
    );
};

export default OrderSummarySection;
