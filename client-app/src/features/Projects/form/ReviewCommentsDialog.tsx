import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Button,
    Typography,
} from "@mui/material";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {useState} from "react";

// REFACTOR: Added new dialog component to collect optional comments before review actions
// Purpose: Improves UX by only asking for comments when the user actually triggers a review action
// Improvement: Keeps the main form clean, avoids unnecessary fields, and ensures comments are fresh
export const ReviewCommentsDialog: React.FC<{
    open: boolean;
    action: string;
    onClose: () => void;
    onConfirm: (comments: string) => void;
}> = ({ open, action, onClose, onConfirm }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [comments, setComments] = useState("");

    const handleConfirm = () => {
        onConfirm(comments.trim());
        setComments(""); // reset for next time
        onClose();
    };

    const handleCancel = () => {
        setComments("");
        onClose();
    };

    const title =
        action === "MarkReadyForApproval"
            ? getTranslatedLabel("projects.certificate.markAsReadyForApproval", "Mark as Ready for Approval")
            : getTranslatedLabel("projects.certificate.markAsRequiresEdit", "Mark as Requires Editing");

    return (
        <Dialog open={open} onClose={handleCancel} maxWidth="sm" fullWidth>
            <DialogTitle>{title}</DialogTitle>
            <DialogContent>
                <Typography variant="body2" sx={{ mb: 2 }}>
                    {getTranslatedLabel(
                        "projects.certificate.reviewCommentsHint",
                        "You can optionally add comments for this status change."
                    )}
                </Typography>
                <TextField
                    autoFocus
                    multiline
                    rows={4}
                    fullWidth
                    variant="outlined"
                    placeholder={getTranslatedLabel("projects.certificate.commentsPlaceholder", "Enter comments...")}
                    value={comments}
                    onChange={(e) => setComments(e.target.value)}
                />
            </DialogContent>
            <DialogActions>
                <Button onClick={handleCancel} color="inherit">
                    {getTranslatedLabel("common.cancel", "Cancel")}
                </Button>
                <Button onClick={handleConfirm} variant="contained" color="primary">
                    {getTranslatedLabel("common.confirm", "Confirm")}
                </Button>
            </DialogActions>
        </Dialog>
    );
};