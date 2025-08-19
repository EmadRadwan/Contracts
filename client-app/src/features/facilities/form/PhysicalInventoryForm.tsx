import { Button, Grid } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { requiredValidator } from "../../../app/common/form/Validators";
import { useFetchFacilitiesQuery } from "../../../app/store/configureStore";
import { FormComboBoxVirtualGetPhysicalInventoryProductsLovProduct } from "../../../app/common/form/FormComboBoxVirtualGetPhysicalInventoryProductsLovProduct";
import { MemoizedFormDropDownList } from "../../../app/common/form/MemoizedFormDropDownList";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

interface PhysicalInventoryFormProps {
  onSubmit: (values: any) => void;
}

const PhysicalInventoryForm = ({ onSubmit }: PhysicalInventoryFormProps) => {
  const { data: facilities } = useFetchFacilitiesQuery(undefined);
  const {getTranslatedLabel} = useTranslationHelper()
  
  return (
    <Form
      initialValues={undefined}
      onSubmit={(values) => onSubmit(values)}
      render={() => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2}>
              <Grid container item xs={12} spacing={2}>
                <Grid item xs={4}>
                  <Field
                    id={"facilityId"}
                    name={"facilityId"}
                    label={getTranslatedLabel("facility.physical.facility", "Facility")}
                    component={MemoizedFormDropDownList}
                    autoComplete={"off"}
                    data={facilities ?? []}
                    dataItemKey={"facilityId"}
                    textField={"facilityName"}
                  />
                </Grid>
                <Grid item xs={4}>
                  <Field
                    id={"productId"}
                    name={"productId"}
                    label={getTranslatedLabel("facility.physical.product", "Product")}
                    component={FormComboBoxVirtualGetPhysicalInventoryProductsLovProduct}
                    autoComplete={"off"}
                    validator={requiredValidator}
                  />
                </Grid>
              </Grid>
            </Grid>
            <Grid container item xs={12} spacing={2} mt={1}>
              <Grid item xs={12}>
                <Button variant="contained" type="submit" color="success">
                  {getTranslatedLabel("general.find", "Find")}
                </Button>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
      )}
    />
  );
};

export default PhysicalInventoryForm;
