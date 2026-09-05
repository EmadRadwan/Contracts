import React from 'react';
import { Box } from '@mui/material';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import { FormSection } from './FormSection';

interface OrderItemsSectionProps {
    memoizedSalesOrderItemsList: React.ReactNode;
    formEditMode: number;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

export const OrderItemsSection: React.FC<OrderItemsSectionProps> = ({
    memoizedSalesOrderItemsList,
    formEditMode,
    getTranslatedLabel,
}) => {
    const localizationKey = 'order.so.form';

    return (
        <FormSection
            title={getTranslatedLabel(`${localizationKey}.items`, 'Order Items')}
            description={getTranslatedLabel(
                `${localizationKey}.itemsDesc`,
                'Products and quantities in this order'
            )}
            icon={<ShoppingCartIcon />}
            defaultExpanded={true}
        >
            <Box sx={{ width: '100%' }}>
                {memoizedSalesOrderItemsList}
            </Box>
        </FormSection>
    );
};

export default OrderItemsSection;
