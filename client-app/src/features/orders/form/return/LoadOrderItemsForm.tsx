// src/features/orders/return/components/LoadOrderItemsForm.tsx
import React from 'react';
import { Form, FormElement, Field } from '@progress/kendo-react-form';
import { Grid, Typography, Button } from '@mui/material';
import { FormDropDownList } from '../../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../../app/common/form/Validators';
import { GetTranslatedLabel } from '../../../../app/hooks/useTranslationHelper';
import { Return, PartyOrder } from '../../../../app/models/order/return';

// REFACTOR: Extract load order items form into a separate component
// Purpose: Isolate form logic for loading order items
// Why: Improves readability and reusability
interface LoadOrderItemsFormProps {
    partyOrders?: PartyOrder[];
    returnHeader?: Return;
    handleLoadItemsSubmit: (data: any) => Promise<boolean>;
    getTranslatedLabel: GetTranslatedLabel;
}

export const LoadOrderItemsForm: React.FC<LoadOrderItemsFormProps> = ({
                                                                          partyOrders,
                                                                          returnHeader,
                                                                          handleLoadItemsSubmit,
                                                                          getTranslatedLabel,
                                                                      }) => {
    
    console.log('partyOrders:', partyOrders);
    const formattedPartyOrders = partyOrders?.map(order => ({
        ...order,
        displayText: `${order.orderId} - ${new Date(order.orderDate).toLocaleDateString('en-GB')}`,
    })) || [];
    return (
        <Form
            initialValues={{ orderId: '' }}
            onSubmitClick={handleLoadItemsSubmit}
            render={() => (
                <FormElement>
                    <Grid container spacing={2} sx={{ mt: 2 }}>
                        <Grid item xs={12}>
                            <Typography variant="h3">{getTranslatedLabel('Order', 'ReturnItems')}</Typography>
                        </Grid>
                        {formattedPartyOrders.length ? (
                            <Grid item xs={3}>
                                <Field
                                    name="orderId"
                                    label={getTranslatedLabel('Order', 'OrderId')}
                                    component={FormDropDownList}
                                    data={formattedPartyOrders}
                                    dataItemKey="orderId"
                                    // REFACTOR: Use displayText for dropdown display
                                    // Purpose: Show orderId - DD/MM/YYYY in the dropdown
                                    // Why: Improves readability for users
                                    textField="displayText"
                                    validator={requiredValidator}
                                />
                            </Grid>
                        ) : (
                            <>
                                <Grid item xs={3}>
                                    <Typography>
                                        {getTranslatedLabel('Order', 'NoOrderFoundForParty')}: {returnHeader?.fromPartyId || returnHeader?.toPartyId}
                                    </Typography>
                                </Grid>
                                <Grid item xs={3}>
                                    <Field
                                        name="orderId"
                                        label={getTranslatedLabel('Order', 'OrderId')}
                                        component="input"
                                        type="text"
                                        size="20"
                                        validator={requiredValidator}
                                    />
                                </Grid>
                            </>
                        )}
                        <Grid item xs={2}>
                            <Button type="submit" color="primary" variant="contained">
                                {getTranslatedLabel('Order', 'Load Order Items')}
                            </Button>
                        </Grid>
                    </Grid>
                </FormElement>
            )}
        />
    );
};