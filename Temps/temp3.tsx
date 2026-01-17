<Grid item xs={1}>
    {invoice?.statusId && invoice.statusId !== "UNKNOWN" && (
        <Ribbon
            side="left"
            type="corner"
            size="large"
            backgroundColor={status.backgroundColor}
            color={status.foreColor}
            fontFamily="sans-serif"
        >
            {status.label}
        </Ribbon>
    )}
</Grid>