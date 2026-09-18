import {Grid as MuiGrid, Typography} from "@mui/material";
import {Grid as KendoGrid, GridColumn} from "@progress/kendo-react-grid";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {Payment} from "../../../../app/models/accounting/payment";
import React from "react";
import {handleDatesArray, formatNumber} from "../../../../app/util/utils";
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
            <KendoGrid scrollable="scrollable"
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
                    cells={{ data: (props) => (
                        <td>{formatNumber(props.dataItem.amount)} {payment?.currencyUomId}</td>
                    ) }}
                />
                <GridColumn
                    title={getTranslatedLabel(`${localizationKey}.alreadyApplied`, "Applied")}
                    cells={{ data: ({dataItem}) => (
                        <td>{formatNumber(dataItem.amountApplied)} {payment?.currencyUomId}</td>
                    ) }}
                />

                <GridColumn
                    title={getTranslatedLabel(`${localizationKey}.remaining`, "Remaining")}
                    cells={{ data: ({dataItem}) => (
                        <td>{formatNumber(dataItem.amountToApply)} {payment?.currencyUomId}</td>
                    ) }}
                />
            </KendoGrid>
        </MuiGrid>
    );
};

export default NotAppliedInvoicesGrid;