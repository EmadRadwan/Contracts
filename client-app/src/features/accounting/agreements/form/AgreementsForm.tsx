import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Button, Grid, Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import FormInput from "../../../../app/common/form/FormInput";
import { Agreement } from "../../../../app/models/accounting/agreement";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import {
  useFetchAgreementTypesQuery,
  useFetchRoleTypesQuery,
} from "../../../../app/store/apis";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import AgreementsMenu from "../menu/AgreementsMenu";
import { Box } from "@mui/system";
import { FormComboBoxVirtualPartyWithEmployees } from "../../../../app/common/form/FormComboBoxVirtualPartyWithEmployee";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { FormComboBoxVirtualFinishedManufacturingProduct } from "../../../../app/common/form/FormComboBoxVirtualFinishedManufacturingProduct";

interface AgreementsFormProps {
  editMode: number;
  cancelEdit: () => void;
  selectedAgreement?: Agreement;
}

const AgreementsForm = ({
  editMode,
  cancelEdit,
  selectedAgreement,
}: AgreementsFormProps) => {
  const { data: roleTypes } = useFetchRoleTypesQuery(undefined);
  const { data: agreementTypes } = useFetchAgreementTypesQuery(undefined);
  const onSubmit = (data: any) => {
    console.log(data);
    if (!data.isValid) return;
  };
  return (
    <>
      <AccountingMenu selectedMenuItem={"/agreements"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid
          container
          spacing={2}
          alignItems={"center"}
          sx={{ position: "relative" }}
        >
          <Grid item xs={7}>
            <Box display="flex" justifyContent="space-between">
              <Typography
                variant="h6"
                sx={{
                  color: editMode === 1 ? "green" : "black",
                  fontWeight: "bold",
                  paddingLeft: 3,
                  fontSize: "18px",
                }}
              >
                {editMode === 1
                  ? "New Agreement"
                  : `Agreement: ${selectedAgreement?.agreementId}`}
              </Typography>
            </Box>
          </Grid>
          {editMode > 1 && (
            <Grid item xs={5}>
              <AgreementsMenu />
            </Grid>
          )}
        </Grid>
        <Form
          initialValues={
            editMode === 1
              ? undefined
              : selectedAgreement
                ? selectedAgreement
                : undefined
          }
          onSubmit={(values) => onSubmit(values)}
          render={() => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid container spacing={2}>
                  <Grid item xs={4}>
                    <Field
                      component={
                        FormComboBoxVirtualFinishedManufacturingProduct
                      }
                      id={"productId"}
                      name={"productId"}
                      label={"Product"}
                      validator={requiredValidator}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      component={FormComboBoxVirtualPartyWithEmployees}
                      id={"fromPartyId"}
                      name={"fromPartyId"}
                      label={"From Party"}
                      validator={requiredValidator}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      component={FormComboBoxVirtualPartyWithEmployees}
                      id={"toPartyId"}
                      name={"toPartyId"}
                      label={"To Party"}
                      validator={requiredValidator}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      component={MemoizedFormDropDownList2}
                      id={"roleTypeIdFrom"}
                      name={"roleTypeIdFrom"}
                      label={"Role Type From"}
                      data={roleTypes ?? []}
                      dataItemKey={"roleTypeId"}
                      textField={"description"}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      component={MemoizedFormDropDownList2}
                      id={"roleTypeIdTo"}
                      name={"roleTypeIdTo"}
                      label={"Role Type To"}
                      data={roleTypes ?? []}
                      dataItemKey={"roleTypeId"}
                      textField={"description"}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      component={MemoizedFormDropDownList}
                      id={"agreementTypeId"}
                      name={"agreementTypeId"}
                      label={"Agreement Type"}
                      data={agreementTypes ?? []}
                      dataItemKey={"agreementTypeId"}
                      textField={"description"}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      component={FormDatePicker}
                      id={"agreementDate"}
                      name={"agreementDate"}
                      label={"Agreement Date"}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      component={FormDatePicker}
                      id={"fromDate"}
                      name={"fromDate"}
                      label={"From Date"}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      component={FormDatePicker}
                      id={"thruDate"}
                      name={"thruDate"}
                      label={"Thru Date"}
                    />
                  </Grid>
                </Grid>
              </fieldset>
              <div className="k-form-buttons">
                <Grid item container spacing={2}>
                  <Grid item xs={1}>
                    <Button variant="contained" type={"submit"} color="success">
                      Submit
                    </Button>
                  </Grid>
                  <Grid item xs={3}>
                    <Button
                      onClick={() => cancelEdit()}
                      color="error"
                      variant="contained"
                    >
                      Cancel
                    </Button>
                  </Grid>
                </Grid>
              </div>
            </FormElement>
          )}
        />
      </Paper>
    </>
  );
};

export default AgreementsForm;
