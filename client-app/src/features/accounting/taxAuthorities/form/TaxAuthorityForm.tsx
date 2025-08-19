import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import React from "react";
import { TaxAuthority } from "../../../../app/models/accounting/taxAuthority";
import {
  useAppSelector,
  useFetchCountriesQuery,
} from "../../../../app/store/configureStore";
import { geoSelectors } from "../../../../app/common/slice/geoSlice";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";

interface TaxAuthorityFormProps {
  cancelEdit: () => void;
  selectedTaxAuth?: TaxAuthority;
  editMode: number;
}

const TaxAuthorityForm = ({
  cancelEdit,
  selectedTaxAuth,
  editMode,
}: TaxAuthorityFormProps) => {
  const { data: countries } = useFetchCountriesQuery(undefined);
  console.log(countries);

  const handleSubmit = (values: any) => {};

  return (
    <>
      <AccountingMenu selectedMenuItem={"/taxAuthorities"} />

      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container spacing={2}>
          <Grid item xs={6}>
            <Box display="flex" justifyContent="space-between">
              <Typography
                sx={{ p: 2 }}
                variant="h4"
                color={!selectedTaxAuth ? "green" : "black"}
              >
                {selectedTaxAuth
                  ? selectedTaxAuth?.taxAuthPartyName
                  : "New Tax Authority"}{" "}
              </Typography>
            </Box>
          </Grid>
        </Grid>
        <Form
          initialValues={selectedTaxAuth}
          onSubmitClick={(values) => handleSubmit(values)}
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid container spacing={2} alignItems={"flex-start"} direction={"column"}>
                  <Grid item xs={4}>
                    <Field
                      name={"taxAuthGeoId"}
                      id={"taxAuthGeoId"}
                      label={"Tax Auth Geo"}
                      component={MemoizedFormDropDownList2}
                      data={countries ?? []}
                      dataItemKey={"geoId"}
                      textField={"geoName"}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      name={"requireTaxIdForExemption"}
                      id={"requireTaxIdForExemption"}
                      label={"Require Tax Id For Exemption"}
                      component={MemoizedFormDropDownList2}
                      data={[
                        { value: "Y", label: "Yes" },
                        { value: "N", label: "No" },
                      ]}
                      dataItemKey={"value"}
                      textField={"label"}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <Field
                      name={"includeTaxInPrice"}
                      id={"includeTaxInPrice"}
                      label={"Include Tax In Price"}
                      component={MemoizedFormDropDownList2}
                      data={[
                        { value: "Y", label: "Yes" },
                        { value: "N", label: "No" },
                      ]}
                      dataItemKey={"value"}
                      textField={"label"}
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

export default TaxAuthorityForm;
