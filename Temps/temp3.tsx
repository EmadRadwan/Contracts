// BillingAccountsList.tsx
import React, { useEffect, useState } from "react";
// ... other imports

function BillingAccountsList() {
    const [editMode, setEditMode] = useState(0);           // 0=list, 1=create, 2=edit
    const [selectedAccount, setSelectedAccount] = useState<BillingAccount | undefined>(undefined);

    // ── NEW: Menu callback to force back to list ──────────────────────
    const handleMenuSelect = () => {
        setEditMode(0);
        setSelectedAccount(undefined);
    };
    // ─────────────────────────────────────────────────────────────────────

    // ... rest of your existing code (data fetching, startEdit, cancelEdit, etc.)

    // ── RENDER FORM FIRST ─────────────────────────────────────
    if (editMode > 0) {
        return (
            <BillingAccountForm
                billingAccount={editMode === 1 ? undefined : selectedAccount}
                editMode={editMode}
                cancelEdit={cancelEdit}
                onBillingAccountCreated={handleCreated}
            />
        );
    }

    // ── RENDER LIST ─────────────────────────────────────
    return (
        <>
            {/* ← Pass the callback exactly like SalesRequestsList does */}
            <AccountingMenu
                selectedMenuItem="/billingAccounts"
                onMenuSelect={handleMenuSelect}   {/* ← THIS IS THE KEY */}
            />

            <Paper elevation={5} className="div-container-withBorderCurved">
                <KendoGrid
                    // ... your grid config
                >
                    <GridToolbar>
                        <Button
                            variant="outlined"
                            color="secondary"
                            onClick={() => startEdit()}
                        >
                            {getTranslatedLabel("accounting.billingAccounts.list.new", "إنشاء حساب أجل جديد")}
                        </Button>
                    </GridToolbar>

                    {/* your columns */}
                </KendoGrid>

                {isFetching && <LoadingComponent message="جاري تحميل حسابات الأجل..." />}
            </Paper>
        </>
    );
}

export default BillingAccountsList;