import React, { useState, useRef, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Form, FormElement } from '@progress/kendo-react-form';
import { Paper, Box, Button } from '@mui/material';
import { toast } from 'react-toastify';
import { useAppDispatch } from '../../../../app/store/configureStore';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { Return, ReturnItem, ReturnAdjustment } from '../../../../app/models/order/return';
import useReturn from '../../hook/useReturn';
import useReturnItems  from '../../hook/useReturnItems';
import LoadingComponent from '../../../../app/layout/LoadingComponent';
import OrderMenu from '../../menu/OrderMenu';
import ModalContainer from '../../../../app/common/modals/ModalContainer';
import { ReturnHeader } from '../../form/return/ReturnHeader';
import { ReturnGrid } from './ReturnGrid';
import { ReturnTotals } from './ReturnTotals';
import { LoadOrderItemsForm } from '../../form/return/LoadOrderItemsForm';
import ReturnItemForm from '../../form/return/ReturnItemForm';

// Purpose: Use useParams to get returnId and manage navigation independently
// Why: Decouples from EditReturn, aligning with OFBiz's separate screens
export interface ReturnRow {
    type: 'item' | 'adjustment';
    item?: ReturnItem;
    adjustment?: ReturnAdjustment;
    index: number;
    isNew?: boolean;
}

export default function OrderReturnItems() {
    const { returnId } = useParams<{ returnId: string }>();
    const navigate = useNavigate();
    const formRef = useRef<Form>(null);
    const [selectedMenuItem, setSelectedMenuItem] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [orderId, setOrderId] = useState<string | undefined>(undefined); // New state for orderId
    const [showModal, setShowModal] = useState(false);
    const [selectedRow, setSelectedRow] = useState<ReturnRow | null>(null);
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();

    // Purpose: Fetch returnHeader for the given returnId
    // Why: Ensures component has necessary data without relying on props
    const { returnHeader, handleCreate } = useReturn({
        selectedMenuItem,
        editMode: 2, // Assume view/edit mode for existing return
        selectedReturn: undefined, // Not passed as prop; fetched via useReturn
        setIsLoading,
    });

    const {
        returnItems,
        returnAdjustments,
        returnTypes,
        returnReasons,
        partyOrders,
        returnableItems,
        isFetching,
        handleLoadOrderItems,
        handleRemoveItem,
        handleRemoveAdjustment,
        handleAcceptReturn,
        handleAddItem,
        handleAddAdjustment,
    } = useReturnItems({
        returnId: returnId || '',
        orderId,
        partyId: returnHeader?.returnHeaderTypeId === 'VENDOR_RETURN'
            ? (typeof returnHeader.toPartyId === 'object' ? returnHeader.toPartyId?.partyId : returnHeader.toPartyId)
            : (typeof returnHeader.fromPartyId === 'object' ? returnHeader.fromPartyId?.partyId : returnHeader.fromPartyId),
        roleTypeId: returnHeader?.returnHeaderTypeId === 'VENDOR_RETURN' ? 'BILL_FROM_VENDOR' : 'PLACING_CUSTOMER',
    });

    const readOnly = returnHeader?.statusId !== 'RETURN_REQUESTED' && returnHeader?.statusId !== 'SUP_RETURN_REQUESTED';

    // Purpose: Include new rows in grid for display
    // Why: Ensures all rows are shown, including newly added ones
    const [newRows, setNewRows] = useState<ReturnRow[]>([]);
    const gridData: ReturnRow[] = [
        ...(returnItems?.map((item, index) => ({ type: 'item' as const, item, index })) || []),
        ...(returnAdjustments?.map((adjustment, index) => ({ type: 'adjustment' as const, adjustment, index: returnItems?.length + index })) || []),
        ...newRows,
    ];

    // Purpose: Include new rows in total calculation
    // Why: Ensures accurate display of returnTotal
    const returnTotal = gridData.reduce((total, row) => {
        if (row.type === 'item' && row.item) {
            const itemSubTotal = (row.item.returnQuantity || 0) * (row.item.returnPrice || 0);
            return total + itemSubTotal;
        } else if (row.type === 'adjustment' && row.adjustment) {
            return total + (row.adjustment.amount || 0);
        }
        return total;
    }, 0);

    const addNewItem = () => {
        const newItem: ReturnItem = {
            returnId: returnId || '',
            returnItemSeqId: `new_${Date.now()}`,
            orderId: '',
            returnItemTypeId: 'RET_PROD_ITEM',
            returnQuantity: 1,
            returnPrice: 0,
            description: '',
            returnReasonId: '',
            returnTypeId: '',
        };
        setSelectedRow({ type: 'item', item: newItem, index: gridData.length, isNew: true });
        setShowModal(true);
    };

    const addNewAdjustment = () => {
        const newAdjustment: ReturnAdjustment = {
            returnId: returnId || '',
            returnAdjustmentId: `new_${Date.now()}`,
            amount: 0,
            description: '',
            returnTypeId: '',
        };
        setSelectedRow({ type: 'adjustment', adjustment: newAdjustment, index: gridData.length, isNew: true });
        setShowModal(true);
    };

    const handleSelectRow = (row: ReturnRow) => {
        setSelectedRow(row);
        setShowModal(true);
    };

    const handleRemoveRow = (dataItem: ReturnRow) => {
        if (dataItem.isNew) {
            setNewRows(newRows.filter(row => row.index !== dataItem.index));
        } else {
            dataItem.type === 'item'
                ? handleRemoveItem(dataItem.item!.returnItemSeqId)
                : handleRemoveAdjustment(dataItem.adjustment!.returnAdjustmentId);
        }
    };

    const handleCloseModal = useCallback(() => {
        setShowModal(false);
        setSelectedRow(null);
    }, []);

    const handleFormSubmit = (data: ReturnItem | ReturnAdjustment) => {
        if (selectedRow?.isNew) {
            const newRow: ReturnRow = {
                type: selectedRow.type,
                [selectedRow.type]: data,
                index: selectedRow.index,
                isNew: true,
            };
            setNewRows([...newRows, newRow]);
            if (selectedRow.type === 'item') {
                handleAddItem(data as ReturnItem);
            } else {
                handleAddAdjustment(data as ReturnAdjustment);
            }
        } else {
            setNewRows(newRows.map(row => (row.index === selectedRow?.index ? { ...row, [row.type]: data } : row)));
        }
        toast.success(`${selectedRow?.type === 'item' ? 'Item' : 'Adjustment'} updated successfully.`);
        handleCloseModal();
    };

    const handleMenuSelect = async (e: MenuSelectEvent) => {
        setSelectedMenuItem(e.item.text);
        if (e.item.text === 'New Return') {
            navigate('/returns/new'); // Navigate to create a new return
        } else if (e.item.text === 'Accept Return') {
            setIsLoading(true);
            await handleAcceptReturn();
            setIsLoading(false);
            navigate(`/returns/${returnId}`); // Navigate back to EditReturn after accepting
        } else if (e.item.text === 'Edit Return Header') {
            navigate(`/returns/${returnId}`); // Navigate to EditReturn
        }
    };

    const handleSubmit = async (data: any) => {
        if (!data.isValid) return false;
        setIsLoading(true);
        const action = selectedMenuItem || 'Update Return';
        await handleCreate({ ...data.values, returnItems: [...(returnItems || []), ...newRows.filter(r => r.type === 'item').map(r => r.item)] }, action);
        setNewRows([]);
        setIsLoading(false);
        navigate(`/returns/${returnId}`); // Navigate back to EditReturn after update
    };

    console.log('orderId from OrderReturnItems', orderId)


    // Purpose: Pass selected orderId to useReturnItems for useGetReturnableItemsQuery
    // Why: Triggers query to fetch returnable items
    const handleLoadItemsSubmit = async (data: any) => {
        if (!data.isValid) return false;
        setIsLoading(true);
        try {
            await handleLoadOrderItems(data.values.orderId);
            setOrderId(data.values.orderId); // Update orderId state
            toast.success(getTranslatedLabel('Order', 'OrderItemsLoaded'));
        } catch (error) {
            toast.error(getTranslatedLabel('Common', 'ErrorLoadingItems'));
        } finally {
            setIsLoading(false);
        }
    };

    if (!returnId) {
        return <LoadingComponent message="Invalid Return ID" />;
    }

    return (
        <>
            {showModal && selectedRow && (
                <ModalContainer show={showModal} onClose={handleCloseModal} width={950}>
                    <ReturnItemForm
                        item={selectedRow.type === 'item' ? selectedRow.item : selectedRow.adjustment}
                        isNew={selectedRow.isNew}
                        type={selectedRow.type}
                        returnTypes={returnTypes}
                        returnReasons={returnReasons}
                        handleSubmit={handleFormSubmit}
                        onClose={handleCloseModal}
                    />
                </ModalContainer>
            )}
            <OrderMenu selectedMenuItem="/returns" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <div className="screenlet">
                    <div className="screenlet-title-bar">
                        <ReturnHeader
                            returnId={returnId}
                            readOnly={readOnly}
                            handleMenuSelect={handleMenuSelect}
                            getTranslatedLabel={getTranslatedLabel}
                        />
                    </div>
                    <div className="screenlet-body">
                        {isFetching ? (
                            <LoadingComponent message="Loading Return Items..." />
                        ) : (
                            <>
                                {!readOnly && (
                                    <Box sx={{ mb: 2 }}>
                                        <Button onClick={addNewItem} color="primary" variant="contained" sx={{ mr: 1 }}>
                                            Add New Item
                                        </Button>
                                        <Button onClick={addNewAdjustment} color="primary" variant="contained">
                                            Add New Adjustment
                                        </Button>
                                    </Box>
                                )}
                                <Form
                                    ref={formRef}
                                    initialValues={{ returnItems: returnItems || [], returnAdjustments: returnAdjustments || [] }}
                                    onSubmitClick={handleSubmit}
                                    render={() => (
                                        <FormElement>
                                            <input type="hidden" name="_useRowSubmit" value="Y" />
                                            <ReturnGrid
                                                gridData={gridData}
                                                readOnly={readOnly}
                                                handleSelectRow={handleSelectRow}
                                                handleRemoveRow={handleRemoveRow}
                                                getTranslatedLabel={getTranslatedLabel}
                                            />
                                            <ReturnTotals
                                                returnableItems={returnableItems}
                                                returnTotal={returnTotal}
                                                getTranslatedLabel={getTranslatedLabel}
                                            />
                                            {!readOnly && gridData.length > 0 && (
                                                <Box sx={{ textAlign: 'right' }}>
                                                    <input type="hidden" name="returnId" value={returnId} />
                                                    <input type="hidden" name="_rowCount" value={gridData.length} />
                                                    <Button type="submit" color="primary" variant="contained">
                                                        {getTranslatedLabel('Common', 'Update')}
                                                    </Button>
                                                </Box>
                                            )}
                                        </FormElement>
                                    )}
                                />
                                {!readOnly && (
                                    <LoadOrderItemsForm
                                        partyOrders={partyOrders}
                                        returnHeader={returnHeader}
                                        handleLoadItemsSubmit={handleLoadItemsSubmit}
                                        getTranslatedLabel={getTranslatedLabel}
                                    />
                                )}
                            </>
                        )}
                    </div>
                </div>
            </Paper>
        </>
    );
}