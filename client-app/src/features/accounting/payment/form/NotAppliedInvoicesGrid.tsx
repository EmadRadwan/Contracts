import {Grid as MuiGrid, Typography} from "@mui/material";
import {Grid as KendoGrid, GridColumn} from "@progress/kendo-react-grid";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {Payment} from "../../../../app/models/accounting/payment";
import React from "react";
import {handleDatesArray} from "../../../../app/util/utils";
import {DataResult} from "@progress/kendo-data-query";


interface NotAppliedInvoice {
    invoiceId: string;
    amount: number;
    amountApplied: number;        // already applied from other payments
    amountToApply: number;
}

interface NotAppliedInvoicesGridProps {
    payment: Payment | undefined;
    notAppliedInvoices: NotAppliedInvoice[];
    isLoading: boolean;
}

const NotAppliedInvoicesGrid: React.FC<NotAppliedInvoicesGridProps> = ({
                                                                           payment,
                                                                           notAppliedInvoices,
                                                                           isLoading,
                                                                       }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.payments.applications";
    const [invoices, setInvoices] = React.useState<[]>([]);
    
    React.useEffect(() => {
        if (notAppliedInvoices) {
            const adjustedData = handleDatesArray(notAppliedInvoices);
            setInvoices(adjustedData);
        }
    }, [notAppliedInvoices]);
    
    console.log(invoices);

    if (isLoading) {
        return (
            <LoadingComponent
                message={getTranslatedLabel(`${localizationKey}.loadingInvoices`, "Loading invoices...")}
            />
        );
    }

    if (!notAppliedInvoices.length) return null;

    return (
        <MuiGrid item xs={12}>
            <Typography variant="h5">
                {getTranslatedLabel(`${localizationKey}.notAppliedInvoices`, "Invoices Not Yet Applied")}
            </Typography>
            <KendoGrid
                data={invoices}
                rowHeight={40}
                className="kendo-grid-alternate"
            >
                <GridColumn
                    field="invoiceId"
                    title={getTranslatedLabel(`${localizationKey}.invoiceId`, "Invoice ID")}
                />
                <GridColumn
                    field="invoiceDate"
                    title={getTranslatedLabel(`${localizationKey}.invoiceDate`, "Invoice Date")}
                    format="{0: dd/MM/yyyy}"
                />
                <GridColumn
                    field="amount"
                    title={getTranslatedLabel(`${localizationKey}.amount`, "Amount")}
                    format="{0:c}"
                    cell={(props) => (
                        <td>{props.dataItem.amount.toFixed(2)} {payment?.currencyUomId}</td>
                    )}
                />
                <GridColumn
                    title={getTranslatedLabel(`${localizationKey}.alreadyApplied`, "Applied")}
                    cell={({dataItem}) => (
                        <td>{dataItem.amountApplied.toFixed(2)} {payment?.currencyUomId}</td>
                    )}
                />

                <GridColumn
                    title={getTranslatedLabel(`${localizationKey}.remaining`, "Remaining")}
                    cell={({dataItem}) => (
                        <td>{dataItem.amountToApply.toFixed(2)} {payment?.currencyUomId}</td>
                    )}
                />
            </KendoGrid>
        </MuiGrid>
    );
};

export default NotAppliedInvoicesGrid;