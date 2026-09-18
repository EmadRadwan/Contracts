import {Button, Grid as MuiGrid, Typography} from "@mui/material";
import {Grid as KendoGrid, GridColumn} from "@progress/kendo-react-grid";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import { formatNumber } from "../../../../app/util/utils";


const PaymentApplicationsPayGrid: React.FC<PaymentApplicationGridProps> = ({
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
                {getTranslatedLabel(`${localizationKey}.appliedPayments`, "Applied Payments")}
            </Typography>
            <div className={disabled ? "grid-disabled" : "grid-normal"}>
                <KendoGrid scrollable="scrollable"
                    data={paymentApplications}
                    rowHeight={40}
                    className="kendo-grid-alternate"
                >
                    <GridColumn
                        field="toPaymentId"
                        title={getTranslatedLabel(`${localizationKey}.toPaymentId`, "To Payment ID")}
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

export default PaymentApplicationsPayGrid;