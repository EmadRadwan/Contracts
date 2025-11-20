/* ------------------------------------------------------------------
   FIX: Vertical spacing – page content not glued to header
   ------------------------------------------------------------------ */
.party-financial-history {
    width: 100%;
    min-height: 100vh;
    direction: ltr;
    margin-top: 3rem;          /* ← adjust 2rem–4rem to your taste */
    padding: 0 1.5rem;         /* keep horizontal padding consistent */
}

/* ------------------------------------------------------------------
   FIX: Space between Excel button and financial summary numbers
   ------------------------------------------------------------------ */
.financial-summary-card {
    background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
    border-left: 5px solid #1976d2;
    transition: transform 0.2s ease, box-shadow 0.2s ease;
    padding-bottom: 1rem !important;   /* ensures inner spacing */
}

/* Space after the row that contains the Excel button */
.financial-summary-card > .MuiGrid-container:first-of-type {
    margin-bottom: 2.5rem !important;
    padding-bottom: 1.5rem;
    border-bottom: 1px solid #e0e0e0;
}