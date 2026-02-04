// PartyGlAccountsForm.tsx

// Add this constant near the top of the component (or in a shared constants file)
const ALLOWED_GL_ACCOUNT_TYPES = [
    "ACCOUNTS_PAYABLE",
    "ACCOUNTS_RECEIVABLE",
    "OWNERS_EQUITY",
];

// Then filter the data before passing to the Field
const filteredGlAccountTypes = React.useMemo(() =>
        (glAccountTypes ?? []).filter(type =>
            ALLOWED_GL_ACCOUNT_TYPES.includes(type.glAccountTypeId)
        ),
    [glAccountTypes]
);

// ...

<Field
    name={"glAccountTypeId"}
    id={"glAccountTypeId"}
    label={getTranslatedLabel(
        "accounting.partyGlAccountsForm.glAccountType",
        "GL Account Type"
    )}
    component={MemoizedFormComboBox2}
    data={filteredGlAccountTypes}           // ← use the filtered array here
    dataItemKey={"glAccountTypeId"}
    textField={"description"}
    validator={requiredValidator}
/>