import { useState } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    CircularProgress,
} from "@mui/material";
import { toast } from "react-toastify";
import agent from "../../api/agent";

export type PartyRole = "SALES_REP" | "SALES_MANAGER" | "BROKER";

interface Props {
    open: boolean;
    role: PartyRole;
    onClose: () => void;
    onCreated: (party: { fromPartyId: string; fromPartyName: string }) => void;
}

const ROLE_LABELS: Record<PartyRole, string> = {
    SALES_REP: "مندوب مبيعات",
    SALES_MANAGER: "مدير مبيعات",
    BROKER: "وسيط",
};

const CREATE_FN: Record<PartyRole, (data: any) => Promise<any>> = {
    SALES_REP: (data) => agent.Parties.createSalesRep(data),
    SALES_MANAGER: (data) => agent.Parties.createSalesManager(data),
    BROKER: (data) => agent.Parties.createBroker(data),
};

export default function QuickCreatePartyDialog({ open, role, onClose, onCreated }: Props) {
    const [name, setName] = useState("");
    const [phone, setPhone] = useState("");
    const [loading, setLoading] = useState(false);

    async function handleCreate() {
        if (!name.trim()) return;
        setLoading(true);
        try {
            const result = await CREATE_FN[role]({ groupName: name.trim(), mobileContactNumber: phone.trim() || undefined });
            const created = result.data ?? result;
            onCreated({ fromPartyId: created.partyId, fromPartyName: name.trim() });
            toast.success(`تم إنشاء ${ROLE_LABELS[role]} بنجاح`);
            setName("");
            setPhone("");
            onClose();
        } catch {
            toast.error(`فشل إنشاء ${ROLE_LABELS[role]}`);
        } finally {
            setLoading(false);
        }
    }

    function handleClose() {
        setName("");
        setPhone("");
        onClose();
    }

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
            <DialogTitle>إنشاء {ROLE_LABELS[role]} جديد</DialogTitle>
            <DialogContent>
                <TextField
                    autoFocus
                    margin="dense"
                    label="الاسم"
                    fullWidth
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    onKeyDown={(e) => e.key === "Enter" && handleCreate()}
                />
                <TextField
                    margin="dense"
                    label="رقم الجوال (اختياري)"
                    fullWidth
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                />
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose} color="error">إلغاء</Button>
                <Button
                    onClick={handleCreate}
                    variant="contained"
                    color="success"
                    disabled={!name.trim() || loading}
                    startIcon={loading ? <CircularProgress size={16} /> : null}
                >
                    إنشاء
                </Button>
            </DialogActions>
        </Dialog>
    );
}
