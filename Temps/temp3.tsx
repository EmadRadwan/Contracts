React.useEffect(() => {
    if (data) {
        const adjustedData = handleDatesArray(data.data).map((item: any) => ({
            ...item,
            effectiveDate: item.effectiveDate ? new Date(item.effectiveDate) : null,
            // You can also convert chequeDate, createdStamp etc. if needed
        }));

        setPayments({ data: adjustedData, total: data.total });
    }
}, [data]);