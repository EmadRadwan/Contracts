import React from 'react';
import { Menu, MenuItem, MenuSelectEvent } from '@progress/kendo-react-layout';
import { Grid, Typography } from '@mui/material';
import { GetTranslatedLabel } from '../../../../app/hooks/useTranslationHelper';

// REFACTOR: Extract header into a separate component
// Purpose: Isolate header UI (title and menu) for clarity
// Why: Reduces clutter in main component and improves reusability
interface ReturnHeaderProps {
    returnId: string;
    readOnly: boolean;
    handleMenuSelect: (e: MenuSelectEvent) => void;
    getTranslatedLabel: GetTranslatedLabel;
}

export const ReturnHeader: React.FC<ReturnHeaderProps> = ({
                                                              returnId,
                                                              readOnly,
                                                              handleMenuSelect,
                                                              getTranslatedLabel,
                                                          }) => {
    return (
        <Grid container spacing={2}>
            <Grid item xs={11}>
                <Typography color="black" sx={{ ml: 3 }} variant="h3">
                    {getTranslatedLabel('Order', 'OrderReturn')} #{returnId}
                </Typography>
            </Grid>
            <Grid item xs={1}>
                <Menu onSelect={handleMenuSelect}>
                    <MenuItem text={getTranslatedLabel('general.actions', 'Actions')}>
                        {!readOnly && <MenuItem text="Accept Return" />}
                        <MenuItem text="New Return" />
                    </MenuItem>
                </Menu>
            </Grid>
        </Grid>
    );
};