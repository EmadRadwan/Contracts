const handleSubmitData = async (data: any) => {
    try {
        const flattened = flattenComboValues(data);

        // REFACTOR: Include ReserveRequestId when in edit mode
        // Purpose: Allows backend to identify which record to update
        const payload = {
            reserveRequestDto: {
                ...(editMode === 2 && { reserveRequestId: reserveRequest?.reserveRequestId }),
                ...flattened
            }
        };

        if (editMode === 2) {
            const updated = await updateRR(payload).unwrap();
            toast.success(getTranslatedLabel("reserveRequest.updated", "Reserve request updated"));
            onReserveRequestUpdated?.(updated as ReserveRequest);
        } else {
            const created = await createRR(payload).unwrap();
            toast.success(getTranslatedLabel("reserveRequest.created", "Reserve request created"));
            onReserveRequestCreated?.(created as ReserveRequest);
        }
    } catch (error: any) {
        // ... error handling
    }
};