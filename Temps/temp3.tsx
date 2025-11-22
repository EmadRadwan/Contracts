const calculateTotals = useCallback(
    (
        valueGetter: FormRenderProps["valueGetter"],
        insuranceModeParam?: "value" | "percentage",
        additionalInsuranceModeParam?: "value" | "percentage"
    ) => {
        const quantity = Number(valueGetter("quantity") || 0);
        const unitPrice = Number(valueGetter("unitPrice") || 0);
        const materialPrice = Number(valueGetter("materialPrice") || 0);
        const laborPrice = Number(valueGetter("laborPrice") || 0);

        let total = 0;
        // quantity × price
        let deserved = 0;       // This is the TRUE value of work done — never reduced by penalties
        let insurance = 0;
        let additionalInsurance = 0;
        let net = 0;
        let discount = 0;
        let transportationExpenses = 0;
        let gratuities = 0;

        // Use passed-in modes first (for live updates when switching radio), fallback to form value
        const insuranceMode = insuranceModeParam ?? (valueGetter("insuranceMode") as "value" | "percentage") ?? "value";
        const additionalInsuranceMode = additionalInsuranceModeParam ?? (valueGetter("additionalInsuranceMode") as "value" | "percentage") ?? "value";

        // ===================================================================
        // 1. WORKMANSHIP CONTRACTING CERTIFICATE
        // ===================================================================
        if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
            const pricePerUnit = materialPrice + laborPrice;
            total = Math.round(quantity * pricePerUnit * 1000) / 1000;

            const achievementPercentage = Number(valueGetter("achievementPercentage") || 0);
            const grossAchieved = Math.round(total * (achievementPercentage / 100) * 1000) / 1000;

            // Deserved = value of work actually performed (before any penalties)
            deserved = grossAchieved;

            const deductions = Number(valueGetter("deductions") || 0);

            // Insurance & Additional Insurance are ALWAYS calculated on the GROSS achieved value
            const insuranceInput = Number(valueGetter("insurance") || 0);
            insurance = insuranceMode === "percentage"
                ? Math.round((insuranceInput / 100) * grossAchieved * 1000) / 1000
                : insuranceInput;

            const addInsInput = Number(valueGetter("additionalInsurance") || 0);
            additionalInsurance = additionalInsuranceMode === "percentage"
                ? Math.round((addInsInput / 100) * grossAchieved * 1000) / 1000
                : addInsInput;

            // Net = deserved − deductions − insurance − additional insurance
            net = Math.max(0, Math.round((deserved - deductions - insurance - additionalInsurance) * 1000) / 1000);

            return {
                total,
                finalTotal: net,
                net,
                deserved,
                insurance,
                discount: 0,
                additionalInsurance,
                transportationExpenses: 0,
                gratuities: 0,
            };
        }

        // ===================================================================
        // 2. SUPPLY PROCUREMENT CERTIFICATE
        // ===================================================================
        if (currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE") {
            total = Math.round(quantity * unitPrice * 1000) / 1000;
            deserved = total; // In supply, deserved = total

            const discountInput = Number(valueGetter("discount") || 0);
            discount = discountMode === "percentage"
                ? Math.round((discountInput / 100) * total * 1000) / 1000
                : discountInput;

            transportationExpenses = Number(valueGetter("transportationExpenses") || 0);
            gratuities = Number(valueGetter("gratuities") || 0);

            net = Math.max(0, Math.round((total - discount + transportationExpenses + gratuities) * 1000) / 1000);

            return {
                total,
                finalTotal: net,
                net,
                deserved,
                insurance: 0,
                discount,
                additionalInsurance: 0,
                transportationExpenses,
                gratuities,
            };
        }

        // ===================================================================
        // 3. COMPANY SUPPLY SALE CERTIFICATE
        // ===================================================================
        if (currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE") {
            total = Math.round(quantity * unitPrice * 1000) / 1000;
            deserved = total;

            transportationExpenses = Number(valueGetter("transportationExpenses") || 0);
            gratuities = Number(valueGetter("gratuities") || 0);

            net = Math.max(0, Math.round((total + transportationExpenses + gratuities) * 1000) / 1000);

            return {
                total,
                finalTotal: net,
                net,
                deserved,
                insurance: 0,
                discount: 0,
                additionalInsurance: 0,
                transportationExpenses,
                gratuities,
            };
        }

        // Fallback (should never happen)
        return {
            total: 0,
            finalTotal: 0,
            net: 0,
            deserved: 0,
            insurance: 0,
            discount: 0,
            additionalInsurance: 0,
            transportationExpenses: 0,
            gratuities: 0,
        };
    },
    [currentCertificateType, discountMode]
);