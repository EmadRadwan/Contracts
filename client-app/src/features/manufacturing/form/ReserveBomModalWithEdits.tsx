import { useState, useEffect } from 'react';
import { Box, Typography, CircularProgress, Button, FormControl, InputLabel, Select, MenuItem, TextField } from '@mui/material';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useGetBomInventoryItemsWithEditsQuery, useReserveBomWithSelectedItemsMutation } from "../../../app/store/apis";
import { useAppSelector } from "../../../app/store/configureStore";

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
  quantity: number;
}

export default function ReserveBomModalWithEdits({ workEffortId, onClose }: Props) {
  const { getTranslatedLabel } = useTranslationHelper();
  const [selections, setSelections] = useState<Selection[]>([]);
  const [error, setError] = useState<string | null>(null);
  const { language } = useAppSelector(state => state.localization);

  const { data: components = [], isLoading, error: fetchError } = useGetBomInventoryItemsWithEditsQuery(workEffortId);
  const [reserveBom, { isLoading: isReserving, error: mutationError }] = useReserveBomWithSelectedItemsMutation();

  // REFACTOR: Initialize selections with quantity field
  // Purpose: Set default quantity to 0 for user input
  // Benefit: Enables flexible material reservations
  useEffect(() => {
    if (components.length) {
      const initialSelections = components.reduce((acc: Selection[], item: BomInventoryItem) => {
        if (!acc.some(sel => sel.productId === item.productId)) {
          acc.push({
            productId: item.productId,
            inventoryItemId: item.inventoryItemId,
            quantity: 0,
          });
        }
        return acc;
      }, []);
      setSelections(initialSelections);
    }
  }, [components]);

  // REFACTOR: Handle inventory item selection
  // Purpose: Update inventoryItemId for a product
  // Benefit: Maintains selection logic
  const handleSelectionChange = (productId: string, inventoryItemId: string) => {
    setSelections(prev =>
        prev.map(sel =>
            sel.productId === productId ? { ...sel, inventoryItemId } : sel
        )
    );
  };

  // REFACTOR: Handle quantity input changes
  // Purpose: Update quantity in selections and validate input
  // Benefit: Ensures valid quantities are submitted
  const handleQuantityChange = (productId: string, value: string) => {
    const quantity = parseFloat(value);
    setError(null);
    setSelections(prev =>
        prev.map(sel =>
            sel.productId === productId
                ? { ...sel, quantity: isNaN(quantity) || quantity < 0 ? 0 : quantity }
                : sel
        )
    );
  };

  // REFACTOR: Validate and submit selections
  // Purpose: Allow submission if at least one product has a valid inventory item and quantity
  // Benefit: Supports selective reservations while ensuring valid data
  const handleSubmit = async () => {
    setError(null);

    // Filter selections with quantity > 0
    const validSelections = selections.filter(
        sel => sel.quantity > 0 && sel.inventoryItemId
    );

    // Check if at least one valid selection exists
    if (validSelections.length === 0) {
      setError('Please select at least one inventory item with a valid quantity (positive and not exceeding available stock).');
      return;
    }

    // Validate only selections with quantity > 0
    const invalidSelections = validSelections.some(
        sel =>
            !sel.inventoryItemId ||
            sel.quantity <= 0 ||
            sel.quantity > (components.find(c => c.productId === sel.productId)?.availableToPromiseTotal || 0)
    );

    if (invalidSelections) {
      setError('Please ensure all selected quantities are positive and do not exceed available stock.');
      return;
    }

    try {
      // Submit only valid selections
      const response = await reserveBom({
        reserveBomParams: {
          workEffortId,
          items: validSelections.map(sel => ({
            productId: sel.productId,
            inventoryItemId: sel.inventoryItemId,
            quantity: sel.quantity,
          })),
          isAdditionalMaterials: true, // REFACTOR: Set flag for additional materials
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

  // REFACTOR: Group components by productId for dropdowns
  // Purpose: Organize data for one dropdown and quantity input per product
  // Benefit: Simplifies UI
  const groupedComponents = components.reduce((acc: { [key: string]: BomInventoryItem[] }, item) => {
    acc[item.productId] = acc[item.productId] || [];
    acc[item.productId].push(item);
    return acc;
  }, {});

  // REFACTOR: Create triplets of products for layout
  // Purpose: Group products into sets of three per row to minimize vertical space
  // Benefit: Ensures consistent three-per-row layout
  const productTriplets = [];
  const productEntries = Object.entries(groupedComponents);
  for (let i = 0; i < productEntries.length; i += 3) {
    productTriplets.push(productEntries.slice(i, i + 3));
  }

  return (
      <Box sx={{ p: 3 }}>
        <Typography variant="h6">
          {getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.reserveAdditionalMaterials", "Reserve Additional Materials")}
        </Typography>
        {isLoading && <CircularProgress />}
        {fetchError && <Typography color="error">{fetchError.message || 'Failed to fetch BOM components'}</Typography>}
        {!isLoading && !fetchError && (
            <>
              {productTriplets.map((triplet, index) => (
                  // REFACTOR: Optimize row container for three items
                  // Purpose: Set full width and max-width to prevent premature wrapping
                  // Benefit: Ensures three controls fit per row by maximizing available space
                  <Box
                      key={index}
                      sx={{
                        display: 'flex',
                        gap: 2,
                        my: 2,
                        width: '100%',
                        maxWidth: '1200px',
                        flexWrap: 'wrap',
                      }}
                  >
                    {triplet.map(([productId, items]) => {
                      const selection = selections.find(sel => sel.productId === productId);
                      const hasMultipleItems = items.length > 1;
                      const highlightStyle = hasMultipleItems
                          ? { border: '2px solid #FFD700', backgroundColor: '#FFF9C4' }
                          : {};
                      return (
                          // REFACTOR: Enforce three controls per row
                          // Purpose: Use calc(33.33% - 16px) to account for gap and ensure three items fit
                          // Benefit: Prevents wrapping to fewer than three items unless screen is very narrow
                          <Box
                              key={productId}
                              sx={{
                                flex: '0 0 calc(33.33% - 16px)',
                                minWidth: 180,
                                boxSizing: 'border-box',
                              }}
                          >
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
                            <TextField
                                label="Quantity to Reserve"
                                type="number"
                                value={selection?.quantity || ''}
                                onChange={e => handleQuantityChange(productId, e.target.value)}
                                fullWidth
                                inputProps={{ min: 0, step: 1 }}
                                sx={{ mt: 1 }}
                                error={
                                    selection?.quantity >
                                    (components.find(c => c.productId === productId)?.availableToPromiseTotal || 0)
                                }
                                helperText={
                                  selection?.quantity >
                                  (components.find(c => c.productId === productId)?.availableToPromiseTotal || 0)
                                      ? 'Quantity exceeds available stock'
                                      : `Available: ${components.find(c => c.productId === productId)?.availableToPromiseTotal || 0}`
                                }
                            />
                            <Typography variant="caption" sx={{ mt: 1, display: 'block' }}>
                              Estimated Quantity Needed: {items[0].estimatedQuantity}
                            </Typography>
                          </Box>
                      );
                    })}
                    {triplet.length < 3 &&
                        Array.from({ length: 3 - triplet.length }).map((_, idx) => (
                            <Box
                                key={`placeholder-${idx}`}
                                sx={{
                                  flex: '0 0 calc(33.33% - 16px)',
                                  minWidth: 180,
                                  boxSizing: 'border-box',
                                }}
                            />
                        ))}
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