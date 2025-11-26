<Typography variant="h6" sx={{ pl: 2 }}>
    {getTranslatedLabel(`${localizationKey}.total`, "Total:")}{" "}
    <span style={{ fontWeight: "bold", color: "red", marginLeft: "10px" }}>
    {isTotalLoading ? "..." : iTotal !== null ? iTotal.toFixed(2) : "—"}
  </span>
</Typography>

{/* REFACTOR: New Outstanding Amount row with smart styling */}
<Typography variant="h6" sx={{ pl: 2, mt: 1 }}>
    {getTranslatedLabel(`${localizationKey}.outstanding-amount`, "Outstanding Amount:")}{" "}
    <span style={{
        fontWeight: "bold",
        color:
            isTotalLoading ? "inherit" :
                iOutstanding === 0 ? "green" :
                    iOutstanding === iTotal ? "red" : "orange",
        marginLeft: "10px"
    }}>
    {isTotalLoading
        ? "..."
        : iOutstanding !== null
            ? iOutstanding.toFixed(2)
            : "—"
    }
        {iOutstanding === 0 && !isTotalLoading && (
            <strong style={{ color: "green", marginLeft: "8px" }}>
                ({getTranslatedLabel(`${localizationKey}.fully-paid`, "Fully Paid")})
            </strong>
        )}
  </span>
</Typography>