const getNavItemStyles = (isSelected: boolean) => ({
    color: isSelected ? theme.palette.primary.main : 'inherit',
    textDecoration: "none",
    typography: "h6",
    "&:hover": { color: "grey.500", borderRadius: "3rem" },
    fontWeight: isSelected ? "bold" : "normal",
    display: 'flex',
    alignItems: 'center',
    // --- CHANGED VALUES BELOW ---
    padding: { xs: '4px 8px', xl: '6px 12px' }, // Responsive padding
    minWidth: { xs: '110px', xl: '130px' },    // Smaller min-width on laptops
    // ----------------------------
    justifyContent: 'center',
    textAlign: 'center',
    whiteSpace: 'nowrap',
    fontSize: { xs: '0.75rem', xl: '0.875rem' } // Slightly smaller font on laptops
});