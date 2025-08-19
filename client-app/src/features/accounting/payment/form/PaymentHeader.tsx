import {Ribbon} from "react-ribbons";
import {Grid, Typography} from "@mui/material";
import {useMemo} from "react";
import {Payment} from "../../../../app/models/accounting/payment";




interface PaymentHeaderProps {
    payment?: Payment;
    paymentType: number;
    formEditMode: number;
    language: string;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const PaymentHeader: React.FC<PaymentHeaderProps> = ({
                                                         payment,
                                                         paymentType,
                                                         formEditMode,
                                                         language,
                                                         getTranslatedLabel,
                                                     }) => {
    const localizationKey = "accounting.payments.form";
    const status = useMemo(() => {
        switch (formEditMode) {
            case 1: return { label: "New", backgroundColor: "green", foreColor: "#ffffff" };
            case 2: return { label: "Not Paid", backgroundColor: "yellow", foreColor: "#000000" };
            case 3: return { label: "Received", backgroundColor: "green", foreColor: "#ffffff" };
            case 4: return { label: "Sent", backgroundColor: "blue", foreColor: "#ffffff" };
            case 5: return { label: "Confirmed", backgroundColor: "yellow", foreColor: "#000000" };
            case 6: return { label: "Cancelled", backgroundColor: "red", foreColor: "#ffffff" };
            default: return { label: "Unknown", backgroundColor: "gray", foreColor: "#ffffff" };
        }
    }, [formEditMode]);

    return (
        <Grid container spacing={2} alignItems="center">
            <Grid item xs={11}>
                <Typography
                    sx={{
                        fontWeight: "bold",
                        paddingLeft: 3,
                        fontSize: "18px",
                        color: formEditMode === 1 ? "green" : "black",
                    }}
                    variant="h6"
                >
                    {payment
                        ? `${getTranslatedLabel(`${localizationKey}.title`, "Payment No:")} ${payment.paymentId}`
                        : paymentType === 1
                            ? getTranslatedLabel(`${localizationKey}.new-incoming`, "New Incoming Payment")
                            : getTranslatedLabel(`${localizationKey}.new-outgoing`, "New Outgoing Payment")}
                </Typography>
            </Grid>
            <Grid item xs={1}>
                {formEditMode > 1 && (
                    <Ribbon
                        side={language === "ar" ? "left" : "right"}
                        type="corner"
                        size="large"
                        withStripes
                        backgroundColor={status.backgroundColor}
                        color={status.foreColor}
                        fontFamily="sans-serif"
                    >
                        {status.label}
                    </Ribbon>
                )}
            </Grid>
        </Grid>
    );
};

export default PaymentHeader;