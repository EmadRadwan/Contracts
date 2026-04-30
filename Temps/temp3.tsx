const { data: advancesResponse, isLoading: isAdvancesLoading  } = useFetchPayrollAdvancesQuery({
    invoiceDate,
    organizationPartyId: companyId
}, {
    skip: !invoiceDate || !companyId,
    refetchOnMountOrArgChange: true
});   // Keep this