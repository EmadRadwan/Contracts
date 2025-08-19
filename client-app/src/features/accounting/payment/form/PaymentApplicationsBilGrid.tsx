import {Button, Grid as MuiGrid, Typography} from "@mui/material";
import {Grid as KendoGrid, GridColumn} from "@progress/kendo-react-grid";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";


const PaymentApplicationsBilGrid: React.FC<PaymentApplicationGridProps> = ({
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
                {getTranslatedLabel(`${localizationKey}.appliedBillingAccounts`, "Applied Billing Accounts")}
            </Typography>
            <div className={disabled ? "grid-disabled" : "grid-normal"}>
                <KendoGrid
                    data={paymentApplications}
                    rowHeight={40}
                    className="kendo-grid-alternate"
                >
                    <GridColumn
                        field="billingAccountId"
                        title={getTranslatedLabel(`${localizationKey}.billingAccountId`, "Billing Account ID")}
                    />
                    <GridColumn
                        field="amountApplied"
                        title={getTranslatedLabel(`${localizationKey}.amountApplied`, "Amount Applied")}
                        format="{0:c}"
                        cell={(props) => (
                            <td>{props.dataItem.amountApplied.toFixed(2)} {payment?.currencyUomId}</td>
                        )}
                    />
                    <GridColumn
                        title={getTranslatedLabel("general.actions", "Actions")}
                        cell={(props) => (
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
                        )}
                    />
                </KendoGrid>
            </div>
        </MuiGrid>
    );
};

export default PaymentApplicationsBilGrid;