import {
  useAppDispatch,
  useAppSelector,
  useFetchOrderAdjustmentTypesQuery,
} from "../../../../app/store/configureStore";
import React, { Fragment, useEffect, useState } from "react";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { OrderAdjustment } from "../../../../app/models/order/orderAdjustment";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { setUiOrderAdjustments } from "../../slice/orderAdjustmentsUiSlice";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import {orderItemSubTotal} from "../../slice/orderSelectors";
import { v4 as uuidv4 } from 'uuid';
import {setNeedsTaxRecalculation} from "../../slice/sharedOrderUiSlice";
import {useSelector} from "react-redux";

interface Props {
  orderItem?: any;
  orderAdjustment?: OrderAdjustment;
  editMode: number; // 1: Add, 2: Edit
  onClose: () => void;
}

interface OrderAdjustmentType {
  orderAdjustmentTypeId: string;
  description: string;
}

export default function OrderItemAdjustmentForm({
                                                  orderItem,
                                                  orderAdjustment,
                                                  editMode,
                                                  onClose,
                                                }: Props) {
  const [oAdjustment, setOAdjustment] = useState<OrderAdjustment | undefined>(orderAdjustment);
  const { data: orderAdjustmentTypesData } = useFetchOrderAdjustmentTypesQuery(undefined);
  const [buttonFlag, setButtonFlag] = useState(false);
  const dispatch = useAppDispatch();
  const MyForm = React.useRef<any>();
  const itemSubTotal: number = useAppSelector(orderItemSubTotal);
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "order.so.items.adj.form";
  const allAdjustments = useAppSelector((state) => state.orderAdjustmentsUi.orderAdjustments.entities);
  const allAvailableAdjustments = Object.values(allAdjustments) as OrderAdjustment[];
  const addTax = useSelector((state: any) => state.sharedOrderUi.addTax);

  // REFACTOR: Default to DISCOUNT or first available type with type safety
  const defaultOrderAdjustmentType = orderAdjustmentTypesData?.find(
      (x: OrderAdjustmentType) => x.orderAdjustmentTypeId === "DISCOUNT_ADJUSTMENT"
  ) || orderAdjustmentTypesData?.[0];

  useEffect(() => {
    const initialFormValues: OrderAdjustment = {
      orderAdjustmentId: undefined, // Let reducer generate key
      orderAdjustmentTypeId: defaultOrderAdjustmentType?.orderAdjustmentTypeId || "",
    };

    if (orderAdjustment && editMode === 2) {
      setOAdjustment(orderAdjustment);
    } else if (orderAdjustmentTypesData && editMode === 1) {
      setOAdjustment(initialFormValues);
    }
  }, [defaultOrderAdjustmentType, editMode, orderAdjustment, orderAdjustmentTypesData]);

  // REFACTOR: Enhanced error handling with specific messages
  // Purpose: Provide clearer feedback for different failure scenarios
  // Why: Improves debugging and user experience
  const handleError = (error: any, defaultMessage: string) => {
    const message = error?.data?.message || error?.message || defaultMessage;
    console.error("Error:", JSON.stringify(error, null, 2));
    toast.error(message);
  };

  async function handleSubmitData(data: OrderAdjustment) {
    setButtonFlag(true);
    let newOrderAdjustment: OrderAdjustment;

    try {
      if (editMode === 2) {
        newOrderAdjustment = {
          ...oAdjustment!,
          ...data,
        };
      } else {
        newOrderAdjustment = {
          orderAdjustmentId: uuidv4(),
          amount: data.amount,
          correspondingProductId: orderItem?.productId,
          correspondingProductName: orderItem?.productName,
          orderAdjustmentTypeDescription: orderAdjustmentTypesData!.find(
              (x: OrderAdjustmentType) => x.orderAdjustmentTypeId === data.orderAdjustmentTypeId
          )?.description,
          isAdjustmentDeleted: false,
          orderAdjustmentTypeId: data.orderAdjustmentTypeId,
          orderId: orderItem?.orderId,
          orderItemSeqId: orderItem?.orderItemSeqId, // Item-level adjustment
          isManual: "Y",
          sourcePercentage: data.sourcePercentage,
        };
      }

      // REFACTOR: Negate amount for DISCOUNT and PROMOTION_ADJUSTMENT
      // Purpose: Ensure consistent adjustment amount logic
      if (
          newOrderAdjustment.orderAdjustmentTypeId === "PROMOTION_ADJUSTMENT" ||
          newOrderAdjustment.orderAdjustmentTypeId === "DISCOUNT_ADJUSTMENT"
      ) {
        newOrderAdjustment.amount = -Math.abs(newOrderAdjustment.amount!);
      }

      // REFACTOR: Append or update adjustments
      // Purpose: Update Redux store with new or edited adjustment
      const updatedAdjustments: OrderAdjustment[] =
          editMode === 1
              ? [...allAvailableAdjustments, newOrderAdjustment]
              : allAvailableAdjustments.map((item: OrderAdjustment) =>
                  item.orderAdjustmentId === newOrderAdjustment.orderAdjustmentId
                      ? newOrderAdjustment
                      : item
              );

      if (addTax) {
        dispatch(setNeedsTaxRecalculation(true));
      }
      
      dispatch(setUiOrderAdjustments(updatedAdjustments));

      
      onClose();
    } catch (error) {
      handleError(error, editMode === 2 ? "Failed to update adjustment" : "Failed to create adjustment");
      setButtonFlag(false);
      return;
    }

    setButtonFlag(false);
  }

  const percentageValidator = (value: any) => {
    if (value === null || value === undefined || value === "") {
      return getTranslatedLabel(
          `${localizationKey}.validation.percentRequired`,
          "Percentage is required"
      );
    } else if (value < 0 || value > 100) {
      return getTranslatedLabel(
          `${localizationKey}.validation.percentRange`,
          "Percentage should be between 0 and 100"
      );
    }
    return "";
  };

  const onAmountChange = React.useCallback(
      (event) => {
        if (!event.value) return;
        const newPercentage = (event.value / itemSubTotal) * 100;
        MyForm.current.onChange("sourcePercentage", { value: newPercentage });
      },
      [itemSubTotal]
  );

  const onPercentageChange = React.useCallback(
      (event) => {
        if (!event.value) return;
        const newAmount = (event.value / 100) * itemSubTotal;
        MyForm.current.onChange("amount", { value: newAmount });
      },
      [itemSubTotal]
  );
  
  console.log('orderAdjustmentTypesData:', orderAdjustmentTypesData)

  return (
      <Fragment>
        <Form
            initialValues={oAdjustment}
            key={JSON.stringify(oAdjustment)}
            ref={MyForm}
            onSubmit={(values) => handleSubmitData(values as OrderAdjustment)}
            render={(formRenderProps) => (
                <FormElement>
                  <fieldset className={"k-form-fieldset"}>
                    <Field
                        id={"orderAdjustmentTypeId"}
                        name={"orderAdjustmentTypeId"}
                        label={getTranslatedLabel(
                            `${localizationKey}.type`,
                            "Adjustment Type *"
                        )}
                        component={MemoizedFormDropDownList}
                        dataItemKey={"orderAdjustmentTypeId"}
                        textField={"description"}
                        data={orderAdjustmentTypesData || []}
                        validator={requiredValidator}
                        disabled={editMode === 2}
                    />

                    <Field
                        id={"amount"}
                        format="n0"
                        name={"amount"}
                        min={0.1}
                        label={getTranslatedLabel(
                            `${localizationKey}.amount`,
                            "Adjustment Amount *"
                        )}
                        component={FormNumericTextBox}
                        onChange={onAmountChange}
                        validator={requiredValidator}
                    />

                    <Field
                        id={"sourcePercentage"}
                        format="n2"
                        name={"sourcePercentage"}
                        label={getTranslatedLabel(
                            `${localizationKey}.percent`,
                            "Adjustment Percent *"
                        )}
                        component={FormNumericTextBox}
                        validator={percentageValidator}
                        onChange={onPercentageChange}
                    />

                    <div className="k-form-buttons">
                      <Grid container rowSpacing={2}>
                        <Grid item xs={3}>
                          <Button
                              variant="contained"
                              type={"submit"}
                              color="success"
                              disabled={!formRenderProps.allowSubmit || buttonFlag}
                          >
                            {editMode === 2
                                ? getTranslatedLabel(`general.update`, "Update")
                                : getTranslatedLabel(`general.add`, "Add")}
                          </Button>
                        </Grid>
                        <Grid item xs={2}>
                          <Button
                              onClick={() => onClose()}
                              color="error"
                              variant="contained"
                          >
                            {getTranslatedLabel("general.cancel", "Cancel")}
                          </Button>
                        </Grid>
                      </Grid>
                    </div>
                  </fieldset>
                </FormElement>
            )}
        />
      </Fragment>
  );
}