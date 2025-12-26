import React, { useState } from "react";
import {
    Button,
    Grid,
    Typography,
    Box,
} from "@mui/material";
import {
    Form,
    FormElement,
    Field,
    FormRenderProps,
} from "@progress/kendo-react-form";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import { requiredValidator } from "../../../../../app/common/form/Validators";

interface Props {
    basePrice: number;
    onClose: () => void;
    onApply: (totalDiscountAmount: number, finalPrice: number) => void;
}

const ApartmentPriceCalculatorModal: React.FC<Props> = ({ basePrice, onClose, onApply }) => {
    const [result, setResult] = useState<{
        afterFirstDiscount: number;
        afterSecondDiscount: number;
        finalPrice: number;
    } | null>(null);

    const handleSubmit = (data: any) => {
        const firstDiscountPct = Number(data.values.firstDiscountPct || 0);
        const secondDiscountPct = Number(data.values.secondDiscountPct || 0);

        const afterFirst = basePrice * (1 - firstDiscountPct);
        const final = afterFirst * (1 - secondDiscountPct);

        setResult({
            afterFirstDiscount: afterFirst,
            afterSecondDiscount: final,
            finalPrice: final,
        });
    };

    // REFACTOR: Added "Apply Discount" button that sends data back to parent form
    // Purpose: Close the loop – user can calculate and directly apply discount
    // Why it improves: Eliminates manual re-entry, reduces errors
    const handleApply = () => {
        if (!result) return;
        const totalDiscount = basePrice - result.finalPrice;
        onApply(totalDiscount, result.finalPrice);
    };

    return (
        <Grid container spacing={3} padding={3}>
            <Grid item xs={12}>
                <Typography variant="h5" gutterBottom>
                    حاسبة سعر الشقة بعد الخصومات
                </Typography>
                <Typography variant="body2" color="text.secondary">
                    السعر الأساسي: {basePrice.toLocaleString("ar-EG")} ج.م
                </Typography>
            </Grid>

            <Grid item xs={12}>
                <Form
                    initialValues={{
                        firstDiscountPct: 0.05,
                        secondDiscountPct: 0,
                    }}
                    onSubmitClick={handleSubmit}
                    render={(formRenderProps: FormRenderProps) => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2}>
                                    <Grid item xs={12} md={6}>
                                        <Field
                                            id="firstDiscountPct"
                                            name="firstDiscountPct"
                                            label="الخصم الأول (%)"
                                            component={FormNumericTextBox}
                                            format="p2"
                                            min={0}
                                            max={1}
                                            validator={requiredValidator}
                                        />
                                    </Grid>

                                    <Grid item xs={12} md={6}>
                                        <Field
                                            id="secondDiscountPct"
                                            name="secondDiscountPct"
                                            label="الخصم الثاني (%) - اختياري"
                                            hint="اتركه صفرًا إذا لم يكن هناك خصم ثاني"
                                            component={FormNumericTextBox}
                                            format="p2"
                                            min={0}
                                            max={1}
                                        />
                                    </Grid>

                                    <Grid item xs={12}>
                                        <Box display="flex" gap={2} mt={2}>
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                type="submit"
                                                disabled={!formRenderProps.valid}
                                            >
                                                احسب
                                            </Button>
                                            <Button variant="outlined" onClick={onClose}>
                                                إغلاق
                                            </Button>
                                        </Box>
                                    </Grid>
                                </Grid>
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Grid>

            {result && (
                <>
                    <Grid item xs={12}>
                        <Typography variant="h6" gutterBottom>
                            نتائج الحساب
                        </Typography>
                    </Grid>

                    <Grid container spacing={3}>
                        <Grid item xs={12} sm={6}>
                            <Box bgcolor="#f5f5f5" p={2} borderRadius={2}>
                                <Typography variant="caption" color="text.secondary">
                                    السعر بعد الخصم الأول
                                </Typography>
                                <Typography variant="h5" color="primary" fontWeight="bold">
                                    {result.afterFirstDiscount.toLocaleString("ar-EG")} ج.م
                                </Typography>
                            </Box>
                        </Grid>

                        <Grid item xs={12} sm={6}>
                            <Box bgcolor="#e8f5e8" p={2} borderRadius={2}>
                                <Typography variant="caption" color="text.secondary">
                                    السعر النهائي
                                </Typography>
                                <Typography variant="h5" color="success.main" fontWeight="bold">
                                    {result.finalPrice.toLocaleString("ar-EG")} ج.م
                                </Typography>
                            </Box>
                        </Grid>

                        <Grid item xs={12}>
                            <Box display="flex" justifyContent="flex-end" gap={2} mt={2}>
                                {/* REFACTOR: Apply button sends discount back to main form */}
                                <Button
                                    variant="contained"
                                    color="success"
                                    onClick={handleApply}
                                >
                                    تطبيق الخصم
                                </Button>
                                <Button variant="outlined" onClick={onClose}>
                                    إلغاء
                                </Button>
                            </Box>
                        </Grid>
                    </Grid>
                </>
            )}
        </Grid>
    );
};

export default ApartmentPriceCalculatorModal;