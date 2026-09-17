import {Link} from "react-router-dom";
import {Button, Grid as MuiGrid, Typography} from "@mui/material";
import {Grid as KendoGrid, GridColumn} from "@progress/kendo-react-grid";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import { formatNumber } from "../../../../app/util/utils";


const PaymentApplicationsInvGrid: React.FC<PaymentApplicationGridProps> = ({
                                                                               payment,
                                                                               paymentApplications,
                                                                               isRemoving,
                                                                               handleRemove,
                                                                               disabled,
                                                                           }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.payments.applications";

    if (!paymentApplications.length) return null;

    return (
        <MuiGrid item xs={12}>
            <Typography variant="h5">
                {getTranslatedLabel(`${localizationKey}.appliedInvoices`, "Applied Invoices")}
            </Typography>
            <div className={disabled ? "grid-disabled" : "grid-normal"}>
                <KendoGrid
                    data={paymentApplications}
                    rowHeight={40}
                    className="kendo-grid-alternate"
                >
                    <GridColumn
                        field="invoiceId"
                        title={getTranslatedLabel(`${localizationKey}.invoiceId`, "Invoice ID")}
                        cells={{ data: (props) => (
                            <td>
                                <Link to={`/invoices/view/${props.dataItem.invoiceId}`}>
                                    [{props.dataItem.invoiceId}]
                                </Link>
                            </td>
                        ) }}
                    />
                    <GridColumn
                        field="invoiceItemSeqId"
                        title={getTranslatedLabel(`${localizationKey}.invoiceItemSeqId`, "Item Sequence")}
                    />
                    <GridColumn
                        field="amountApplied"
                        title={getTranslatedLabel(`${localizationKey}.amountApplied`, "Amount Applied")}
                        format="{0:c}"
                        cells={{ data: (props) => (
                            <td>{formatNumber(props.dataItem.amountApplied)} {payment?.currencyUomId}</td>
                        ) }}
                    />
                    <GridColumn
                        title={getTranslatedLabel("general.actions", "Actions")}
                        cells={{ data: (props) => (
                            <td>
                                <Button
                                    variant="outlined"
                                    color="error"
                                    size="small"
                                    onClick={() => handleRemove(props.dataItem.paymentApplicationId)}
                                    disabled={isRemoving || disabled}
                                >
                                    {getTranslatedLabel("general.remove", "Remove")}
                                </Button>
                            </td>
                        ) }}
                    />
                </KendoGrid>
            </div>
        </MuiGrid>
    );
};

export default PaymentApplicationsInvGrid;
