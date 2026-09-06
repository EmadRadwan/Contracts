import React from 'react';
import { Grid, Box, Typography } from '@mui/material';
import { Menu, MenuItem } from '@progress/kendo-react-layout';
import { RibbonContainer, Ribbon } from 'react-ribbons';

interface OrderHeaderSectionProps {
    order: any;
    formEditMode: number;
    status: any;
    language: string;
    handleMenuSelect: (e: any) => void;
    showQuickShip: boolean;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

export const OrderHeaderSection: React.FC<OrderHeaderSectionProps> = ({
    order,
    formEditMode,
    status,
    language,
    handleMenuSelect,
    showQuickShip,
    getTranslatedLabel,
}) => {
    const localizationKey = 'order.so.form';

    return (
        <RibbonContainer>
            <Grid
                container
                alignItems="center"
                sx={{ position: 'relative', mb: 2 }}
                spacing={2}
            >
                {/* Title Section */}
                <Grid item xs={12} md={10}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Typography
                            sx={{
                                fontWeight: 'bold',
                                fontSize: '20px',
                                color: formEditMode === 1 ? '#2E7D32' : '#212121',
                            }}
                            variant="h5"
                        >
                            {order && order?.orderId
                                ? `${getTranslatedLabel(`${localizationKey}.orderNo`, 'Sales Order No: ')} ${order?.orderId}`
                                : `${getTranslatedLabel(`${localizationKey}.new`, 'New Sales Order')}`}
                        </Typography>
                    </Box>
                </Grid>

                {/* Actions Menu */}
                <Grid item xs={12} md={1}>
                    <Menu onSelect={handleMenuSelect}>
                        <MenuItem
                            text={getTranslatedLabel('general.actions', 'Actions')}
                        >
                            <MenuItem
                                text={getTranslatedLabel(`${localizationKey}.actions.new`, 'New Order')}
                                data="new"
                            />
                            {formEditMode === 3 && showQuickShip && (
                                <MenuItem
                                    text={getTranslatedLabel(`${localizationKey}.actions.ship`, 'Quick Ship Order')}
                                    data="ship"
                                />
                            )}
                        </MenuItem>
                    </Menu>
                </Grid>

                {/* Status Ribbon */}
                {formEditMode > 1 && (
                    <Grid item xs={12} md={1}>
                        <Ribbon
                            side={language === 'ar' ? 'left' : 'right'}
                            type="corner"
                            size="large"
                            withStripes
                            backgroundColor={status.backgroundColor}
                            color={status.foreColor}
                            fontFamily="sans-serif"
                        >
                            <Typography
                                variant="h4"
                                sx={{
                                    fontSize: language === 'ar' ? '1.1rem' : '0.9rem',
                                    fontWeight: 600,
                                }}
                            >
                                {order.statusDescription}
                            </Typography>
                        </Ribbon>
                    </Grid>
                )}
            </Grid>
        </RibbonContainer>
    );
};

export default OrderHeaderSection;
