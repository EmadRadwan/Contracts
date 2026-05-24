React.useEffect(() => {
    if (data) {
        const adjustedData = handleDatesArray(data.data).map((item: any) => ({
            ...item,
            // New field: Other Party ID (the one that is NOT the Company)
            otherPartyId: item.isDisbursement
                ? item.partyIdTo          // Outgoing → To Party
                : item.partyIdFrom,       // Incoming → From Party
        }));

        setPayments({ data: adjustedData, total: data.total });
    }
}, [data]);