import { useState, useEffect } from 'react';
import { Box, Typography, CircularProgress, Button, FormControl, InputLabel, Select, MenuItem } from '@mui/material';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useGetBomInventoryItemsQuery, useReserveProductionRunTaskMutation } from "../../../app/store/apis";

interface Props {
    workEffortId: string;
    onClose: () => void;
    language: string;
    reserveTaskQoh: (dataItem: { workEffortId: string }) => Promise<void>;
}

interface BomInventoryItem {
    productId: string;
    estimatedQuantity: number;
    inventoryItemId: string;
    availableToPromiseTotal: number;
    productFeatureId: string | null;
    colorDescription: string;
    productName: string;
}

export default function ReserveBomModal({ workEffortId, onClose, language, reserveTaskQoh }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const [selections, setSelections] = useState<{ [productId: string]: string }>({});
    const [error, setError] = useState<string | null>(null);
    const { data: components = [], isLoading, error: fetchError } = useGetBomInventoryItemsQuery(workEffortId);
    const [reserveProductionRunTask, { isLoading: isReserving }] = useReserveProductionRunTaskMutation();

    useEffect(() => {
        if (components.length) {
            const initialSelections = components.reduce((acc: { [key: string]: string }, item: BomInventoryItem) => {
                if (!acc[item.productId]) acc[item.productId] = item.inventoryItemId;
                return acc;
            }, {});
            setSelections(initialSelections);
        }
    }, [components]);

    const handleSelectionChange = (productId: string, inventoryItemId: string) => {
        setSelections(prev => ({ ...prev, [productId]: inventoryItemId }));
    };

    const handleSubmit = async () => {
        setError(null);
        const componentsPayload = Object.entries(selections).map(([productId, inventoryItemId]) => {
            const component = components.find(c => c.productId === productId && c.inventoryItemId === inventoryItemId);
            return {
                productId,
                inventoryItemId,
                quantity: component?.estimatedQuantity || 0,
            };
        });

        try {
            await reserveProductionRunTask({
                workEffortId,
                requireInventory: 'Y',
                components: componentsPayload,
            }).unwrap();
            await reserveTaskQoh({ workEffortId });
            onClose();
        } catch (err: any) {
            setError(err.message || 'Failed to reserve materials');
        }
    };

    // REFACTOR: Group components by productId for dropdowns.
    // Purpose: Organizes data for displaying inventory items per product.
    // Benefit: Simplifies rendering of dropdowns for each product.
    const groupedComponents = components.reduce((acc: { [key: string]: BomInventoryItem[] }, item) => {
        acc[item.productId] = acc[item.productId] || [];
        acc[item.productId].push(item);
        return acc;
    }, {});

    // REFACTOR: Create pairs of products for two dropdowns per row.
    // Purpose: Arranges 7 products into rows with two dropdowns each (last row with one).
    // Benefit: Improves UI by reducing vertical space and enhancing readability.
    const productPairs = [];
    const productEntries = Object.entries(groupedComponents);
    for (let i = 0; i < productEntries.length; i += 2) {
        productPairs.push(productEntries.slice(i, i + 2));
    }

    return (
        <Box sx={{ p: 3 }}>
            <Typography variant="h6">
                {getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.selectBomColors", "Select BOM Colors")}
            </Typography>
            {isLoading && <CircularProgress />}
            {fetchError && <Typography color="error">{fetchError.message || 'Failed to fetch BOM components'}</Typography>}
            {!isLoading && !fetchError && (
                <>
                    {productPairs.map((pair, index) => (
                        // REFACTOR: Use flexbox to display two dropdowns per row.
                        // Purpose: Places two products side by side to optimize space for 7 BOM elements.
                        // Benefit: Reduces scrolling and improves user experience with compact layout.
                        <Box key={index} sx={{ display: 'flex', gap: 2, my: 2, flexWrap: 'wrap' }}>
                            {pair.map(([productId, items]) => (
                                <Box key={productId} sx={{ flex: '1 1 45%', minWidth: 200 }}>
                                    <FormControl fullWidth>
                                        <InputLabel>{`Product: ${items[0].productName}`}</InputLabel>
                                        <Select
                                            value={selections[productId] || ''}
                                            onChange={e => handleSelectionChange(productId, e.target.value)}
                                            label={`Product: ${items[0].productName}`}
                                        >
                                            {items.map(item => (
                                                <MenuItem key={item.inventoryItemId} value={item.inventoryItemId}>
                                                    {item.colorDescription} (Available: {item.availableToPromiseTotal})
                                                </MenuItem>
                                            ))}
                                        </Select>
                                    </FormControl>
                                    <Typography variant="caption" sx={{ mt: 1, display: 'block' }}>
                                        Quantity Needed: {items[0].estimatedQuantity}
                                    </Typography>
                                </Box>
                            ))}
                            {/* Add empty placeholder for odd number of products */}
                            {pair.length === 1 && (
                                <Box sx={{ flex: '1 1 45%', minWidth: 200 }} />
                            )}
                        </Box>
                    ))}
                    {error && <Typography color="error">{error}</Typography>}
                    <Box sx={{ mt: 2, display: 'flex', justifyContent: 'flex-end' }}>
                        <Button onClick={onClose} disabled={isLoading || isReserving} sx={{ mr: 1 }}>
                            {getTranslatedLabel("general.cancel", "Cancel")}
                        </Button>
                        <Button
                            variant="contained"
                            color="primary"
                            onClick={handleSubmit}
                            disabled={isLoading || isReserving || !components.length}
                        >
                            {isReserving ? (
                                <>
                                    <CircularProgress size={20} />
                                    <span>{getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.reservingBOM", "Reserving BOM")}</span>
                                </>
                            ) : (
                                getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.reserveBOM", "Reserve BOM")
                            )}
                        </Button>
                    </Box>
                </>
            )}
        </Box>
    );
}