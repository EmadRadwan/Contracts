const processedData = useMemo(() => {
    if (!enrichedPayments || !Array.isArray(enrichedPayments) || enrichedPayments.length === 0) {
        return { data: [], total: 0 };
    }

    try {
        return process(enrichedPayments, dataState);
    } catch (err) {
        console.error("Kendo process error:", err);
        return { data: enrichedPayments, total: enrichedPayments.length }; // fallback
    }
}, [enrichedPayments, dataState]);