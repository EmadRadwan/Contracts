import React, { useState } from "react";
import FacilityMenu from "../menu/FacilityMenu";
import { Grid, Paper } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { useFetchFinishedProductFacilitiesQuery, useFetchPicklistDisplayInfoQuery } from "../../../app/store/apis";
import LoadingComponent from "../../../app/layout/LoadingComponent";

const ManagePicklists = () => {
  const { data: facilities } =
    useFetchFinishedProductFacilitiesQuery(undefined);
  const [selectedFacility, setSelectedFacility] = useState<string | undefined>(
    undefined
  );
  const {data: picklists, isFetching, isLoading} = useFetchPicklistDisplayInfoQuery(selectedFacility!, {
    skip: !selectedFacility
  })
  console.log(picklists)
  return (
    <>
      <FacilityMenu />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item container xs={12} ml={1}>
            <Grid item xs={4}>
              <Form
                render={() => (
                  <FormElement>
                    <Field
                      id="facilityId"
                      name="facilityId"
                      label="Facility"
                      component={MemoizedFormDropDownList2}
                      data={facilities ?? []}
                      dataItemKey="facilityId"
                      textField="facilityName"
                      onChange={(e) => setSelectedFacility(e.value)}
                    />
                  </FormElement>
                )}
              />
            </Grid>
          </Grid>
        </Grid>
        {(isLoading || isFetching) && (
            <LoadingComponent message="Loading Picklists..." />
        )}
      </Paper>
    </>
  );
};

export default ManagePicklists;
