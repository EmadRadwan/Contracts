import React from "react";
import { Button, Grid, Typography, Box } from "@mui/material";
import ModalContainer from "../../../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";

interface DefaultPercentagesModalProps {
    open: boolean;
    onClose: () => void;
    advancePercent: number;
    maintenancePercent: number;
    onSave: (advance: number, maintenance: number) => void;
}

const DefaultPercentagesModal: React.FC<DefaultPercentagesModalProps> = ({
                                                                             open,
                                                                             onClose,
                                                                             advancePercent,
                                                                             maintenancePercent,
                                                                             onSave,
                                                                         }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [localAdvance, setLocalAdvance] = React.useState(advancePercent * 100);
    const [localMaintenance, setLocalMaintenance] = React.useState(maintenancePercent * 100);

    const handleSave = () => {
        onSave(localAdvance / 100, localMaintenance / 100);
        onClose();
    };

    return (
        <ModalContainer show={open} onClose={onClose} width={500}>
            <Box p={3}>
                <Typography variant="h6" gutterBottom>
                    {getTranslatedLabel("salesRequest.form.defaults.title", "Default Calculation Percentages")}
                </Typography>
                <Grid container spacing={3} mt={1}>
                    <Grid item xs={12}>
                        <FormNumericTextBox
                            label={getTranslatedLabel("salesRequest.form.defaults.advance", "Default Advance Payment (%)")}
                            value={localAdvance}
                            onChange={(e) => setLocalAdvance(e.value ?? 0)}
                            min={0}
                            max={100}
                            format="n2"
                        />
                    </Grid>
                    <Grid item xs={12}>
                        <FormNumericTextBox
                            label={getTranslatedLabel("salesRequest.form.defaults.maintenance", "Default Maintenance Deposit (%)")}
                            value={localMaintenance}
                            onChange={(e) => setLocalMaintenance(e.value ?? 0)}
                            min={0}
                            max={100}
                            format="n2"
                        />
                    </Grid>
                    <Grid item xs={12}>
                        <Box display="flex" gap={2} justifyContent="flex-end">
                            <Button onClick={onClose} color="inherit">
                                {getTranslatedLabel("general.cancel", "Cancel")}
                            </Button>
                            <Button onClick={handleSave} variant="contained" color="primary">
                                {getTranslatedLabel("general.save", "Save")}
                            </Button>
                        </Box>
                    </Grid>
                </Grid>
            </Box>
        </ModalContainer>
    );
};

export default DefaultPercentagesModal;