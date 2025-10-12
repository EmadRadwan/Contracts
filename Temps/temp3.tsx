const GlAccountCell = useCallback(
    ({ dataItem }: GridCellProps) => {
        const glAccountId = dataItem.debitGlAccountId || dataItem.creditGlAccountId;
        const glAccount = glAccounts?.find((acc) => acc.glAccountId === glAccountId);

        // REFACTOR: Combine glAccountId and glAccountTypeDescription in the cell display
        // Purpose: Display both glAccountId and glAccountTypeDescription for better context
        // Improvement: Enhances readability by showing the account description alongside the ID, while preserving the clickable functionality
        const displayText = glAccount
            ? `${glAccount.glAccountId} - ${glAccount.glAccountTypeDescription || 'N/A'}`
            : glAccountId
                ? `${glAccountId} - N/A`
                : "-";

        return (
            <td
                style={{ cursor: "pointer", color: "#1976d2" }}
                onClick={() => handleEditEntry(dataItem.id)}
            >
                {displayText}
            </td>
        );
    },
    [glAccounts, handleEditEntry]
);