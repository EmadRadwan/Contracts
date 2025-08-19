import React, {useState} from "react";

import {toast} from "react-toastify";
import {InventoryTransfer} from "../../../app/models/facility/inventoryTransfer";
import {
    useAddInventoryTransferMutation,
    useAppDispatch,
    useUpdateInventoryTransferMutation
} from "../../../app/store/configureStore";
import {InventoryItem} from "../../../app/models/facility/inventoryItem";

type UseInventoryTransferProps = {
    selectedMenuItem: string;
    editMode: number;
    inventoryItem?: InventoryItem | undefined;
    selectedInventoryTransfer?: InventoryTransfer;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};
const useInventoryTransfer = ({
                                  selectedMenuItem,
                                  editMode,
                                  selectedInventoryTransfer, setIsLoading, inventoryItem,
                              }: UseInventoryTransferProps) => {

    const dispatch = useAppDispatch();

    const [
        addInventoryTransfer,
        {data: addInventoryTransferResults, error, isLoading},
    ] = useAddInventoryTransferMutation();

    const [updateInventoryTransferTrigger, {
        data: updateInventoryTransferResults,
        isLoading: isUpdateInventoryTransferLoading
    }] =
        useUpdateInventoryTransferMutation();

    const [formEditMode, setFormEditMode] = useState(editMode);
    const [inventoryTransfer, setInventoryTransfer] = useState<InventoryTransfer | undefined>(() => {
        if (selectedInventoryTransfer) {
            return selectedInventoryTransfer; // Use existing inventoryTransfer for edit mode
        } else {
            // New inventoryTransfer mode
            return {
                inventoryTransferId: "",
                statusId: 'REQUESTED',
                inventoryItemId: inventoryItem!.inventoryItemId,
                productName: inventoryItem!.productName,
                facilityId: inventoryItem!.facilityId,
                facilityName: inventoryItem!.facilityName,
                atpQoh: inventoryItem!.availableToPromiseTotal + " " + inventoryItem!.quantityOnHandTotal,
                sendDate: new Date(),
                transferQuantity: 0,
                comments: '',
                inventoryComments: inventoryItem!.comments,
            };
        }
    });

    async function createInventoryTransfer(newInventoryTransfer: InventoryTransfer) {
        try {
            let createdInventoryTransfer: any;
            try {
                createdInventoryTransfer = await addInventoryTransfer(newInventoryTransfer).unwrap();
            } catch (error) {
                toast.error("Failed to create Inventory Transfer");
            }
            if (createdInventoryTransfer) {
                setInventoryTransfer({
                    inventoryTransferId: createdInventoryTransfer.inventoryTransferId,
                    statusId: createdInventoryTransfer.statusId,
                    sendDate: newInventoryTransfer.sendDate,
                    receiveDate: new Date(createdInventoryTransfer.receiveDate),
                    comments: createdInventoryTransfer.comments,
                    transferQuantity: createdInventoryTransfer.transferQuantity,
                    atpQoh: createdInventoryTransfer!.availableToPromiseTotal + " " + createdInventoryTransfer!.quantityOnHandTotal,

                });
                setFormEditMode(2);
                toast.success("InventoryTransfer Created Successfully");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }

    async function updateInventoryTransfer(newInventoryTransfer: InventoryTransfer) {
        try {
            let updatedInventoryTransfer: any;
            try {
                updatedInventoryTransfer = await updateInventoryTransferTrigger(newInventoryTransfer).unwrap();
            } catch (error) {
                toast.error("Failed to update inventoryTransfer");
            }
            if (updatedInventoryTransfer) {
                setInventoryTransfer({
                    inventoryTransferId: updatedInventoryTransfer.inventoryTransferId,
                    statusId: updatedInventoryTransfer.statusId,
                    sendDate: newInventoryTransfer.sendDate,
                    receiveDate: new Date(updatedInventoryTransfer.receiveDate),
                    comments: updatedInventoryTransfer.comments,
                    transferQuantity: updatedInventoryTransfer.transferQuantity,
                    atpQoh: updatedInventoryTransfer!.availableToPromiseTotal + " " + updatedInventoryTransfer!.quantityOnHandTotal,

                });
                toast.success("InventoryTransfer Updated Successfully");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }


    async function handleCreate(inventoryTransfer: InventoryTransfer) {

        const newInventoryTransfer: InventoryTransfer = {
            inventoryTransferId: formEditMode > 1 ? inventoryTransfer!.inventoryTransferId : "",
            statusId: formEditMode > 1 ? inventoryTransfer.statusId : "IXF_REQUESTED",
            inventoryItemId: formEditMode === 1 ? inventoryItem?.inventoryItemId : inventoryTransfer!.inventoryItemId,
            facilityId: inventoryItem!.facilityId,
            facilityIdTo: inventoryTransfer.facilityIdTo,
            sendDate: inventoryTransfer!.sendDate,
            receiveDate: formEditMode > 1 ? inventoryTransfer.receiveDate : null,
            comments: inventoryTransfer.comments,
            transferQuantity: inventoryTransfer.transferQuantity,
        };


        if (!inventoryTransfer.inventoryTransferId) {
            await createInventoryTransfer(newInventoryTransfer);
        } else {
            await updateInventoryTransfer(newInventoryTransfer);
        }


    }

    return {
        formEditMode,
        setFormEditMode,
        inventoryTransfer,
        setInventoryTransfer,
        handleCreate,
    };
};
export default useInventoryTransfer;

