import React, { useState } from "react";
import { useAppSelector } from "../../../../app/store/configureStore";
import CatalogMenu from "../../menu/CatalogMenu";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import FormInput from "../../../../app/common/form/FormInput";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import LoadingComponent from "../../../../app/layout/LoadingComponent";

interface ProductStoreFormProps {
  editMode: number;
  cancelEdit: () => void;
}

const ProductStoreForm = ({ editMode, cancelEdit }: ProductStoreFormProps) => {
  const { selectedProductStore } = useAppSelector(
    (state) => state.productStoreUi
  );
  console.log(selectedProductStore);
  const [buttonFlag, setButtonFlag] = useState(false);
  const { getTranslatedLabel } = useTranslationHelper();
  const handleSubmitData = (values: any) => {
    console.log(values);
  }
  return (
    <>
      <CatalogMenu selectedMenuItem='/products' selectedMenuItem='/products' />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container spacing={2}>
          <Grid item xs={5}>
            <Box display="flex" justifyContent="space-between">
              <Typography
                color={selectedProductStore?.storeName ? "black" : "green"}
                sx={{ p: 2 }}
                variant="h4"
              >
                {" "}
                {selectedProductStore?.storeName
                  ? selectedProductStore?.storeName
                  : getTranslatedLabel(
                      "product.store.form.new",
                      "New Product"
                    )}{" "}
              </Typography>
            </Box>
          </Grid>
        </Grid>
        <Form
          initialValues={editMode === 2 ? selectedProductStore : undefined}
          onSubmit={(values) => handleSubmitData(values)}
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid container spacing={2}>
                  <Grid item xs={5}>
                    <Field
                      id={"storeName"}
                      name={"storeName"}
                      label={getTranslatedLabel(
                        "product.stores.form.name",
                        "Store Name *"
                      )}
                      component={FormInput}
                      autoComplete={"off"}
                      validator={requiredValidator}
                    />
                  </Grid>
                </Grid>

                <div className="k-form-buttons">
                  <Grid container rowSpacing={2}>
                    <Grid item xs={1}>
                      <Button
                        variant="contained"
                        type={"submit"}
                        color="success"
                        disabled={!formRenderProps.allowSubmit || buttonFlag}
                      >
                        {getTranslatedLabel("general.submit", "Submit")}
                      </Button>
                    </Grid>
                    <Grid item xs={1}>
                      <Button
                        onClick={cancelEdit}
                        color="error"
                        variant="contained"
                      >
                        {getTranslatedLabel("general.cancel", "Cancel")}
                      </Button>
                    </Grid>
                  </Grid>
                </div>

                {buttonFlag && (
                  <LoadingComponent
                    message={getTranslatedLabel(
                      "product.stores.form.processing",
                      "Processing Product..."
                    )}
                  />
                )}
              </fieldset>
            </FormElement>
          )}
        />
      </Paper>
    </>
  );
};

export default ProductStoreForm;
