import { useRef, useState } from "react";
import { useFetchTermTypesQuery } from "../../../../../app/store/apis/termTypesApi";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { OrderTerm } from "../../../../../app/models/order/orderTerm";
import { Button, Grid } from "@mui/material";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import FormInput from "../../../../../app/common/form/FormInput";
import { FormDropDownTreeTermTypes } from "../../../../../app/common/form/FormDropDownTreeTermTypes";
import { useAppDispatch } from "../../../../../app/store/configureStore";
import {
  orderTermsEntities,
  setUiOrderTerms,
} from "../../../slice/orderTermsUiSlice";
import { useSelector } from "react-redux";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";

interface OrderTermFormProps {
  orderId?: string;
  selectedTerm?: OrderTerm;
  onClose: () => void;
}

const OrderTermForm = ({
  orderId,
  onClose,
  selectedTerm,
}: OrderTermFormProps) => {
  const { data: termTypes } = useFetchTermTypesQuery(undefined);
  const dispatch = useAppDispatch();
  const [formKey, setFormKey] = useState(Date.now());
  const orderUiTerms = useSelector(orderTermsEntities);
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "order.so.terms";

  // State to store the selected text value from the dropdown.
  const [selectedTermText, setSelectedTermText] = useState("");

  // Recursive helper function to search for the 'text' value given a termTypeId.
  const findTextByTermTypeId = (
    data: any[],
    termTypeId: string
  ): string | null => {
    for (const item of data) {
      if (item.termTypeId === termTypeId) {
        return item.text;
      }
      if (item.items && item.items.length > 0) {
        const result = findTextByTermTypeId(item.items, termTypeId);
        if (result) {
          return result;
        }
      }
    }
    return null;
  };

  // Handle change and search for the selected node's text
  const handleTermTypeChange = (event: any) => {
    // event.value is assumed to be the selected termTypeId.
    const selectedId = event.value;
    if (termTypes && selectedId) {
      const text = findTextByTermTypeId(termTypes, selectedId);
      if (text) {
        setSelectedTermText(text);
      }
    }
    // Return the event to ensure that the form's internal state updates as expected.
    return event;
  };

  const handleSubmit = (data: any) => {
    if (!data.values.termDays && !data.values.termValue) return;
    if (!data.isValid) return;

    const startingIndex = orderUiTerms ? orderUiTerms.length + 1 : 0;
    const newOrderTerm: OrderTerm = {
      orderId: "DUMMY_ID",
      orderTermSeqId: startingIndex.toString().padStart(2, "0"),
      orderItemSeqId: "_NA_",
      termTypeId: data.values.termTypeId,
      termDays: data.values.termDays,
      termValue: data.values.termValue,
      // Use the found text value from the state.
      termTypeName: selectedTermText,
      description: data.values.description,
      isNewTerm: "Y",
    };

    dispatch(setUiOrderTerms(newOrderTerm));
    setFormKey(Date.now());
  };

  return (
    <Form
      initialValues={selectedTerm ?? undefined}
      key={formKey}
      onSubmitClick={(values) => handleSubmit(values)}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid
              container
              alignItems={"start"}
              justifyContent="flex-start"
              spacing={1}
            >
              <Grid item xs={8}>
                {termTypes && termTypes.length > 0 && (
                  <Field
                    id="termTypeId"
                    name="termTypeId"
                    label={getTranslatedLabel(
                      `${localizationKey}.type`,
                      "Term Type"
                    )}
                    component={FormDropDownTreeTermTypes}
                    dataItemKey="termTypeId"
                    selectField={"selected"}
                    expandField={"expanded"}
                    textField="text"
                    data={termTypes}
                    // Attach the custom onChange handler.
                    onChange={handleTermTypeChange}
                  />
                )}
              </Grid>
              <Grid item container xs={12} spacing={2}>
                <Grid item xs={6}>
                  <Field
                    id="termDays"
                    name="termDays"
                    label={getTranslatedLabel(
                      `${localizationKey}.days`,
                      "Term Days"
                    )}
                    component={FormNumericTextBox}
                    min={0}
                  />
                </Grid>
                <Grid item xs={6}>
                  <Field
                    id="termValue"
                    name="termValue"
                    label={getTranslatedLabel(
                      `${localizationKey}.termValue`,
                      "Term Value"
                    )}
                    component={FormNumericTextBox}
                    min={0}
                  />
                </Grid>
              </Grid>
              <Grid item container xs={12} spacing={2}>
                <Grid item xs={6}>
                  <Field
                    id="description"
                    name="description"
                    label={getTranslatedLabel(
                      `${localizationKey}.description`,
                      "Description"
                    )}
                    component={FormInput}
                  />
                </Grid>
              </Grid>
              <Grid item>
                <Button
                  sx={{ mt: 1 }}
                  type="submit"
                  color="success"
                  variant="contained"
                >
                  {getTranslatedLabel("general.add", "Add")}
                </Button>
              </Grid>
              <Grid item>
                <Button
                  sx={{ mt: 1 }}
                  onClick={onClose}
                  color="error"
                  variant="contained"
                >
                  {getTranslatedLabel("general.cancel", "Cancel")}
                </Button>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
      )}
    />
  );
};

export default OrderTermForm;
