import {useMemo} from "react";
import {Grid as MuiGrid, Paper, Typography} from "@mui/material";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {Payment} from "../../../../app/models/accounting/payment";
import {
    useFetchPaymentApplicationsForPaymentQuery,
    useRemovePaymentApplicationMutation
} from "../../../../app/store/apis";
import {useFetchNotListedInvoicesQuery} from "../../../../app/store/apis/invoice/invoicesApi";
import PaymentApplicationsInvGrid from "./PaymentApplicationsInvGrid";
import NotAppliedInvoicesGrid from "./NotAppliedInvoicesGrid";
import AddPaymentApplicationForm from "./AddPaymentApplicationForm";


interface EditPaymentApplicationsProps {
    payment: Payment | undefined;
    paymentId: string;
    onClose: () => void;
}

const EditPaymentApplications: React.FC<EditPaymentApplicationsProps> = ({payment, paymentId, onClose}) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.payments.applications";

    const {data: paymentApplications = [], isLoading: isApplicationsLoading} =
        useFetchPaymentApplicationsForPaymentQuery(paymentId);
    const {data: notAppliedInvoices = [], isLoading: isInvoicesLoading} =
        useFetchNotListedInvoicesQuery(paymentId, {
            skip: !paymentId || !payment?.amount,
        });
    const [removePaymentApplication, {isLoading: isRemoving}] =
        useRemovePaymentApplicationMutation();

    // REFACTOR: Move calculations to useMemo for performance
    // Purpose: Optimizes rendering by memoizing derived data
    const appliedAmount = useMemo(() => {
        return paymentApplications.reduce(
            (sum: number, app: PaymentApplication) => sum + (app.amountApplied || 0),
            0
        );
    }, [paymentApplications]);

    const notAppliedAmount = useMemo(() => {
        return payment ? payment.amount - appliedAmount : 0;
    }, [payment, appliedAmount]);

    const {
        paymentApplicationsInv,
        paymentApplicationsPay,
        paymentApplicationsBil,
        paymentApplicationsTax,
    } = useMemo(
        () => ({
            paymentApplicationsInv: paymentApplications.filter((app) => app.invoiceId),
            paymentApplicationsPay: paymentApplications.filter((app) => app.toPaymentId),
            paymentApplicationsBil: paymentApplications.filter((app) => app.billingAccountId),
            paymentApplicationsTax: paymentApplications.filter((app) => app.taxAuthGeoId),
        }),
        [paymentApplications]
    );

    const nonEditableStatuses = ["PMNT_RECEIVED", "PMNT_SENT", "PMNT_CONFIRMED"];
    const isFormDisabled = payment && nonEditableStatuses.includes(payment.statusId);

    const handleRemove = async (paymentApplicationId: string) => {
        if (isFormDisabled) return;
        try {
            await removePaymentApplication({paymentApplicationId}).unwrap();
        } catch (error) {
            console.error("Failed to remove payment application:", error);
        }
    };

    const handleApplyPayment = (values: any) => {
        if (isFormDisabled) return;
        // TODO: Implement API call to apply payment
        console.log("Applying payment:", values);
    };

    if (isApplicationsLoading) {
        return (
            <LoadingComponent
                message={getTranslatedLabel(`${localizationKey}.loading`, "Loading payment applications...")}
            />
        );
    }

    if (!payment) {
        return (
            <Typography variant="h6" sx={{pl: 2}}>
                {getTranslatedLabel(`${localizationKey}.noPayment`, "No payment found.")}
            </Typography>
        );
    }

    return (
        <Paper elevation={5} sx={{p: 2, mt: 2}}>
            <MuiGrid container spacing={2}>
                <MuiGrid item xs={12}>
                    <Typography variant="h4">
                        {getTranslatedLabel(`${localizationKey}.title`, "Payment Applications")}
                    </Typography>
                    <Typography variant="h6">
                        {getTranslatedLabel(`${localizationKey}.amountTotal`, "Total Amount")}:{" "}
                        {payment.amount.toFixed(2)} {payment.currencyUomId}
                    </Typography>
                    <Typography variant="h6">
                        {getTranslatedLabel(`${localizationKey}.amountNotApplied`, "Amount Not Applied")}:{" "}
                        {notAppliedAmount.toFixed(2)} {payment.currencyUomId}
                    </Typography>
                </MuiGrid>

                {paymentApplications.length === 0 ? (
                    <MuiGrid item xs={12}>
                        <Typography variant="h6">
                            {getTranslatedLabel(`${localizationKey}.noApplications`, "No payment applications found.")}
                        </Typography>
                    </MuiGrid>
                ) : (
                    <>
                        <PaymentApplicationsInvGrid
                            payment={payment}
                            paymentApplications={paymentApplicationsInv}
                            isRemoving={isRemoving}
                            handleRemove={handleRemove}
                            disabled={isFormDisabled}
                        />
                    </>
                )}

                {notAppliedAmount > 0 && (
                    <>
                        <NotAppliedInvoicesGrid
                            payment={payment}
                            notAppliedInvoices={notAppliedInvoices}
                            isLoading={isInvoicesLoading}
                        />
                        
                        <AddPaymentApplicationForm
                            payment={payment}
                            notAppliedInvoices={notAppliedInvoices}
                            notAppliedAmount={notAppliedAmount}
                            disabled={isFormDisabled}
                            onSubmit={handleApplyPayment}
                            onCancel={onClose}
                        />
                    </>
                )}
            </MuiGrid>
        </Paper>
    );
};

export default EditPaymentApplications;