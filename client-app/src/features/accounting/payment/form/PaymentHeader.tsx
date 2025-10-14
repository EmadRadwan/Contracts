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
        const statuses = {
            1: { key: "new", backgroundColor: "green", foreColor: "#ffffff" },
            2: { key: "not-paid", backgroundColor: "yellow", foreColor: "#000000" },
            3: { key: "received", backgroundColor: "green", foreColor: "#ffffff" },
            4: { key: "sent", backgroundColor: "blue", foreColor: "#ffffff" },
            5: { key: "confirmed", backgroundColor: "yellow", foreColor: "#000000" },
            6: { key: "cancelled", backgroundColor: "red", foreColor: "#ffffff" },
            default: { key: "unknown", backgroundColor: "gray", foreColor: "#ffffff" },
        };
        const statusKey = statuses[formEditMode] || statuses.default;
        return {
            label: getTranslatedLabel(`${localizationKey}.statuses.${statusKey.key}`, statusKey.key),
            backgroundColor: statusKey.backgroundColor,
            foreColor: statusKey.foreColor,
        };
    }, [formEditMode, getTranslatedLabel]);

    const title = useMemo(() => {
        if (payment) {
            return `${getTranslatedLabel(`${localizationKey}.title`, "Payment No:")} ${payment.paymentId}`;
        }
        return paymentType === 1
            ? getTranslatedLabel(`${localizationKey}.new-incoming`, "New Incoming Payment")
            : getTranslatedLabel(`${localizationKey}.new-outgoing`, "New Outgoing Payment");
    }, [payment, paymentType, getTranslatedLabel, localizationKey]);


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
                    {title}
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