import { useState, useEffect } from 'react';
import { Box, Typography, CircularProgress, Button, FormControl, InputLabel, Select, MenuItem } from '@mui/material';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useGetBomInventoryItemsQuery, useReserveBomWithSelectedItemsMutation } from "../../../app/store/apis";

interface Props {
    workEffortId: string;
    onClose: () => void;
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

interface Selection {
    productId: string;
    inventoryItemId: string;
}

export default function ReserveBomModal({ workEffortId, onClose }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const [selections, setSelections] = useState<Selection[]>([]);
    const [error, setError] = useState<string | null>(null);

    const { data: components = [], isLoading, error: fetchError } = useGetBomInventoryItemsQuery(workEffortId);
    const [reserveBom, { isLoading: isReserving, error: mutationError }] = useReserveBomWithSelectedItemsMutation();

    // Purpose: Store user-selected inventoryItemIds without editable quantities
    // Benefit: Simplifies state by using fixed estimatedQuantity from components
    useEffect(() => {
        if (components.length) {
            const initialSelections = components.reduce((acc: Selection[], item: BomInventoryItem) => {
                if (!acc.some(sel => sel.productId === item.productId)) {
                    acc.push({
                        productId: item.productId,
                        inventoryItemId: item.inventoryItemId,
                    });
                }
                return acc;
            }, []);
            setSelections(initialSelections);
        }
    }, [components]);

    // Purpose: Update selections for inventoryItemId, removing quantity handling
    // Benefit: Streamlines interaction to focus on inventory item selection
    const handleSelectionChange = (productId: string, inventoryItemId: string) => {
        setSelections(prev =>
            prev.map(sel =>
                sel.productId === productId ? { ...sel, inventoryItemId } : sel
            )
        );
    };

    // Purpose: Send user-selected inventoryItemIds with fixed quantities to reserveBom endpoint
    // Benefit: Aligns with REST API and backend handler, removes quantity editing
    const handleSubmit = async () => {
        setError(null);
        const invalidSelections = selections.some(sel => !sel.inventoryItemId);
        if (invalidSelections) {
            setError('Please select an inventory item for each component.');
            return;
        }

        try {
            const response = await reserveBom({
                reserveBomParams: {
                    workEffortId,
                    items: selections.map(sel => {
                        const component = components.find(c => c.productId === sel.productId);
                        return {
                            productId: sel.productId,
                            inventoryItemId: sel.inventoryItemId,
                            quantity: component?.estimatedQuantity || 0,
                        };
                    }),
                    isAdditionalMaterials: false, // REFACTOR: Explicitly set to false for full BOM reservation
                },
            }).unwrap();

            if (response.success) {
                onClose();
            } else {
                setError(response.message);
            }
        } catch (err: any) {
            setError(err.data?.message || 'Failed to reserve materials');
        }
    };

    // Purpose: Organizes data for rendering one dropdown per product
    // Benefit: Simplifies UI and ensures one selection per BOM component
    const groupedComponents = components.reduce((acc: { [key: string]: BomInventoryItem[] }, item) => {
        acc[item.productId] = acc[item.productId] || [];
        acc[item.productId].push(item);
        return acc;
    }, {});

    // Purpose: Maintains compact layout with two dropdowns per row
    // Benefit: Preserves existing UI structure for readability
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
                        <Box key={index} sx={{ display: 'flex', gap: 2, my: 2, flexWrap: 'wrap' }}>
                            {pair.map(([productId, items]) => {
                                const selection = selections.find(sel => sel.productId === productId);
                                // Purpose: Visually indicate when multiple color options are available
                                // Benefit: Alerts users to choose from multiple inventory items
                                const hasMultipleItems = items.length > 1;
                                const highlightStyle = hasMultipleItems
                                    ? { border: '2px solid #FFD700', backgroundColor: '#FFF9C4' }
                                    : {};

                                return (
                                    <Box key={productId} sx={{ flex: '1 1 45%', minWidth: 200 }}>
                                        <FormControl fullWidth sx={highlightStyle}>
                                            <InputLabel>{`Product: ${items[0].productName}`}</InputLabel>
                                            <Select
                                                value={selection?.inventoryItemId || ''}
                                                onChange={e => handleSelectionChange(productId, e.target.value)}
                                                label={`Product: ${items[0].productName}`}
                                            >
                                                <MenuItem value="">Select Inventory Item</MenuItem>
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
                                );
                            })}
                            {pair.length === 1 && <Box sx={{ flex: '1 1 45%', minWidth: 200 }} />}
                        </Box>
                    ))}
                    {error && <Typography color="error">{error}</Typography>}
                    {mutationError && <Typography color="error">{mutationError.message}</Typography>}
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