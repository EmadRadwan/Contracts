import {Grid as MuiGrid, Typography} from "@mui/material";
import {Grid as KendoGrid, GridColumn} from "@progress/kendo-react-grid";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {Payment} from "../../../../app/models/accounting/payment";


interface NotAppliedPayment {
    toPaymentId: string;
    amount: number;
}

interface NotAppliedPaymentsGridProps {
    payment: Payment | undefined;
    notAppliedPayments: NotAppliedPayment[];
    isLoading: boolean;
}

const NotAppliedPaymentsGrid: React.FC<NotAppliedPaymentsGridProps> = ({
                                                                           payment,
                                                                           notAppliedPayments,
                                                                           isLoading,
                                                                       }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.payments.applications";

    if (isLoading) {
        return (
            <LoadingComponent
                message={getTranslatedLabel(`${localizationKey}.loadingPayments`, "Loading payments...")}
            />
        );
    }

    if (!notAppliedPayments.length) return null;

    return (
        <MuiGrid item xs={12}>
            <Typography variant="h5">
                {getTranslatedLabel(`${localizationKey}.notAppliedPayments`, "Payments Not Yet Applied")}
            </Typography>
            <KendoGrid
                data={notAppliedPayments}
                rowHeight={40}
                className="kendo-grid-alternate"
            >
                <GridColumn
                    field="toPaymentId"
                    title={getTranslatedLabel(`${localizationKey}.toPaymentId`, "To Payment ID")}
                />
                <GridColumn
                    field="amount"
                    title={getTranslatedLabel(`${localizationKey}.amount`, "Amount")}
                    format="{0:c}"
                    cell={(props) => (
                        <td>{props.dataItem.amount.toFixed(2)} {payment?.currencyUomId}</td>
                    )}
                />
            </KendoGrid>
        </MuiGrid>
    );
};

export default NotAppliedPaymentsGrid;