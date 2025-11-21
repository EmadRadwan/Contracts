import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import {SalesRequest} from "../../../../../app/models/order/SalesRequest";
import {useMemo} from "react";
import {Button, Grid, Typography} from "@mui/material";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridRowProps,
    GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import {Product} from "../../../../../app/models/product/product";
import {PaymentPlanExcel} from "../report/PaymentPlanExcel";

function PaymentPlanModal({
                              onClose,
                              salesRequest,
                              apartment,
                          }: {
    onClose: () => void;
    salesRequest: SalesRequest;
    apartment?: Product;
}) {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "sales.request.paymentPlan";


    const installments = useMemo(() => {
        const {
            totalPrice = 0,
            advancePayment = 0,
            numberOfInstallments = 0,
            dateOfFirstInstallment,           // ← CORRECT FIELD NAME
            monthsBetweenInstallments = 0,
        } = salesRequest;

        if (
            !totalPrice ||
            !advancePayment ||
            advancePayment >= totalPrice ||
            !numberOfInstallments ||
            !dateOfFirstInstallment ||         // ← now correctly checked
            !monthsBetweenInstallments
        ) {
            return [];
        }

        const remaining = totalPrice - advancePayment;
        const installmentAmount = remaining / numberOfInstallments;

        const result: Array<{
            installmentNumber: number;
            dueDate: Date;
            amount: number;
        }> = [];

        let currentDate = new Date(dateOfFirstInstallment); // ← use correct var
        for (let i = 1; i <= numberOfInstallments; i++) {
            result.push({
                installmentNumber: i,
                dueDate: new Date(currentDate),
                amount: installmentAmount,
            });
            currentDate.setMonth(currentDate.getMonth() + monthsBetweenInstallments);
        }

        return result;
    }, [salesRequest]);
    
    // If no installments, show message
    if (installments.length === 0) {
        return (
            <Grid container padding={4} justifyContent="center">
                <Typography>
                    {getTranslatedLabel(
                        `${localizationKey}.noInstallments`,
                        "No payment plan available. Advance payment covers full amount or required fields are missing."
                    )}
                </Typography>
                <Grid item xs={12} sx={{mt: 2, textAlign: "center"}}>
                    <Button onClick={onClose} color="primary" variant="contained">
                        {getTranslatedLabel(`${localizationKey}.close`, "Close")}
                    </Button>
                </Grid>
            </Grid>
        );
    }
    
    

    return (
        <Grid container padding={2}>
            <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>
                    {getTranslatedLabel(`${localizationKey}.title`, "Payment Plan Schedule")}
                </Typography>
                
            </Grid>

            {apartment && (
                <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                        {apartment.productName} -{' '}
                        {getTranslatedLabel(`${localizationKey}.totalPrice`, 'Total')}:{' '}
                        {salesRequest.totalPrice}
                    </Typography>
                </Grid>
            )}
            <Grid item xs={12}>
                <div className="div-container">
                    <KendoGrid
                        style={{height: "300px"}}
                        data={installments}
                        sortable={true}
                        resizable={true}
                    >
                        <Column
                            field="installmentNumber"
                            title={getTranslatedLabel(`${localizationKey}.columns.installmentNumber`, "#")}
                            width={80}
                        />
                        <Column
                            field="dueDate"
                            title={getTranslatedLabel(`${localizationKey}.columns.dueDate`, "Due Date")}
                            width={150}
                            format="{0: dd/MM/yyyy}"
                        />
                        <Column
                            field="amount"
                            title={getTranslatedLabel(`${localizationKey}.columns.amount`, "Amount")}
                            width={120}
                            format="{0:n2}"
                        />
                    </KendoGrid>
                </div>
            </Grid>
            <PaymentPlanExcel
                salesRequest={salesRequest}
                apartment={apartment}
                getTranslatedLabel={getTranslatedLabel}
            />
        </Grid>
    );
}

export default PaymentPlanModal;