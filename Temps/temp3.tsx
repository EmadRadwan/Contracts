<GridToolbar>
    <Typography variant="body1">
        {getTranslatedLabel(`${localizationKey}.debits`, "Debits total: ")}
        <Box component="span" fontWeight="bold" color="success.main">
            {formatCurrency(data.postedDebitsTotal)}
        </Box>
    </Typography>

    <Typography variant="body1">
        {getTranslatedLabel(`${localizationKey}.credits`, "Credits total: ")}
        <Box component="span" fontWeight="bold" color="error.main">
            {formatCurrency(data.postedCreditsTotal)}
        </Box>
    </Typography>
</GridToolbar>