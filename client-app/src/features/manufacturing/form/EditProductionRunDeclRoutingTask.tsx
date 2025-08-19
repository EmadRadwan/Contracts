import { useState } from "react";
import {
  Box,
  Button,
  CircularProgress,
  Grid,
  Paper,
  Typography,
} from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import FormTextArea from "../../../app/common/form/FormTextArea";
import { WorkEffort } from "../../../app/models/manufacturing/workEffort";
import { useUpdateProductionRunTaskMutation } from "../../../app/store/apis";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useAppSelector } from "../../../app/store/configureStore";

interface Props {
  workEffort: WorkEffort | undefined;
  closeModal: () => void;
  productRunId: string;
}

// Utility function to convert minutes to milliseconds
const minutesToMillis = (minutes: number) => minutes * 60000;
// Utility function to convert milliseconds to minutes
const millisToMinutes = (millis: number) => (millis / 60000).toFixed(2);

export default function EditProductionRunDeclRoutingTask({
  workEffort,
  closeModal,
  productRunId,
}: Props) {
  const [productionRun, setProductionRun] = useState<WorkEffort | undefined>(
    workEffort
  );
  const [updateProductionRunTask, { isLoading, isError, error }] =
    useUpdateProductionRunTaskMutation();
  const { getTranslatedLabel } = useTranslationHelper();
  const {language} = useAppSelector(state => state.localization)

  const handleSubmit = async (data: any) => {
    if (!data.isValid) {
      return false;
    }

    // Conversion from minutes to milliseconds before submission
    const convertedData = {
      productionRunId: productRunId,
      productionRunTaskId: productionRun?.workEffortId,
      fromDate: data.values.actualStartDate,
      toDate: data.values.actualCompletionDate,
      addSetupTime: minutesToMillis(data.values.addSetupTime), // Conversion here
      addTaskTime: minutesToMillis(data.values.addTaskTime), // Conversion here
      addQuantityProduced: data.values.addQuantityProduced,
      addQuantityRejected: data.values.addQuantityRejected,
      comments: data.values.noteId,
    };

    try {
      await updateProductionRunTask(convertedData).unwrap();
      closeModal();
    } catch (err) {
      console.error("Failed to update production run task:", err);
    }
  };

  console.log("workEffort", workEffort);

  return (
    <>
      <Paper elevation={5} className="div-container-withBorderCurved">
        <Form
          initialValues={productionRun}
          onSubmitClick={(values) => handleSubmit(values)}
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid
                  container
                  spacing={2}
                  alignItems={"start"}
                  sx={{ height: "75vh" }}
                  dir={language === "ar" ? "rtl" : "ltr"}
                >
                  <Grid container item xs={12}>
                    <Box display="flex" justifyContent={"start"}>
                      <Typography sx={{ minWidth: "230px" }} variant="h6">
                        {getTranslatedLabel(
                          "manufacturing.jobshop.prodruntasks.edit.taskNumber",
                          "Task Number"
                        )}
                      </Typography>
                      <Typography
                        sx={{ fontWeight: "bold", marginLeft: 5 }}
                        color="black"
                        variant="h6"
                      >
                        {productionRun?.workEffortId}
                      </Typography>
                    </Box>
                  </Grid>

                  <Grid
                    container
                    item
                    xs={12}
                    sx={{ mb: 1, paddingTop: "0 !important" }}
                  >
                    <Grid item xs={6}>
                      <Box display="flex" justifyContent={"start"}>
                        <Typography sx={{ minWidth: "230px" }} variant="h6">
                          {getTranslatedLabel(
                            "manufacturing.jobshop.prodruntasks.edit.seqNumber",
                            "Seq Number"
                          )}
                        </Typography>
                        <Typography
                          sx={{ fontWeight: "bold", marginLeft: 5 }}
                          color="black"
                          variant="h6"
                        >
                          {productionRun?.sequenceNum}
                        </Typography>
                      </Box>
                      <Box display="flex" justifyContent={"start"}>
                        <Typography sx={{ minWidth: "230px" }} variant="h6">
                          {getTranslatedLabel(
                            "manufacturing.jobshop.prodruntasks.edit.taskName",
                            "Task Name"
                          )}
                        </Typography>
                        <Typography
                          sx={{ fontWeight: "bold", marginLeft: 5 }}
                          color="black"
                          variant="h6"
                        >
                          {productionRun?.workEffortName}
                        </Typography>
                      </Box>
                    </Grid>

                    <Grid item xs={6}>
                      <Box display="flex" justifyContent={"start"}>
                        <Typography sx={{ minWidth: "230px" }} variant="h6">
                          {getTranslatedLabel(
                            "manufacturing.jobshop.prodruntasks.edit.description",
                            "Description"
                          )}
                        </Typography>
                        <Typography
                          sx={{ fontWeight: "bold", marginLeft: 5 }}
                          color="black"
                          variant="h6"
                        >
                          {productionRun?.description}
                        </Typography>
                      </Box>
                      <Box display="flex" justifyContent={"start"}>
                        <Typography sx={{ minWidth: "220px" }} variant="h6">
                          {getTranslatedLabel(
                            "manufacturing.jobshop.prodruntasks.edit.calculatedCompletionDate",
                            "Calculated Completion Date"
                          )}
                        </Typography>
                        <Typography
                          sx={{ fontWeight: "bold", marginLeft: 5 }}
                          color="black"
                          variant="h6"
                        >
                          {productionRun?.estimatedCompletionDate?.toDateString()}
                        </Typography>
                      </Box>
                    </Grid>
                  </Grid>
                  <Grid container item xs={12}>
                    <Grid container item xs={7}>
                      <Grid container item spacing={2} alignItems={"center"}>
                        <Box
                          display="flex"
                          sx={{ paddingLeft: 4 }}
                          alignItems={"flex-end"}
                          dir={language === "ar" ? "rtl" : "ltr"}
                        >
                          <Typography sx={{ minWidth: "220px" }} variant="h6">
                            {getTranslatedLabel(
                              "manufacturing.jobshop.prodruntasks.edit.fromDate",
                              "From Date"
                            )}
                          </Typography>
                          <Field
                            id={"actualStartDate"}
                            name={"actualStartDate"}
                            component={FormDatePicker}
                          />
                        </Box>

                        <Box
                          display="flex"
                          sx={{ paddingLeft: 4 }}
                          alignItems={"flex-end"}
                          dir={language === "ar" ? "rtl" : "ltr"}
                        >
                          <Typography sx={{ minWidth: "220px" }} variant="h6">
                            {getTranslatedLabel(
                              "manufacturing.jobshop.prodruntasks.edit.toDate",
                              "To Date"
                            )}
                          </Typography>
                          <Field
                            id={"actualCompletionDate"}
                            name={"actualCompletionDate"}
                            component={FormDatePicker}
                          />
                        </Box>

                        <Box
                          sx={{
                            border: 1,
                            borderRadius: 3,
                            borderColor: "black",
                            marginLeft: 3,
                            p: 1,
                            my: 1,
                          }}
                        >
                          <Box display="flex" alignItems={"flex-end"}>
                            <Typography sx={{ minWidth: "220px" }} variant="h6">
                              {getTranslatedLabel(
                                "manufacturing.jobshop.prodruntasks.edit.estimatedSetupTime",
                                "Estimated Setup Time"
                              )}
                            </Typography>
                            <Typography
                              sx={{ fontWeight: "bold", marginLeft: 1 }}
                              color="black"
                              variant="h6"
                            >
                              <span dir={language === "ar" ? 'ltr' : ''}>{productionRun?.estimatedSetupMillis} {getTranslatedLabel("general.ms", "ms")}</span> /{" "}
                              <span dir={language === "ar" ? 'ltr' : ''}>{millisToMinutes(productionRun?.estimatedSetupMillis)} {getTranslatedLabel("general.mins", "mins")}</span>
                            </Typography>
                          </Box>

                          <Box display="flex" alignItems={"flex-end"}>
                            <Typography sx={{ minWidth: "220px" }} variant="h6">
                              {getTranslatedLabel(
                                "manufacturing.jobshop.prodruntasks.edit.actualSetupTime",
                                "Actual Setup Time"
                              )}
                            </Typography>
                            {productionRun?.actualSetupMillis && (
                              <Typography
                                sx={{ fontWeight: "bold", marginLeft: 1 }}
                                color="black"
                                variant="h6"
                              >
                                <span dir={language === "ar" ? 'ltr' : ''}>{productionRun?.actualSetupMillis} {getTranslatedLabel("general.ms", "ms")}</span> /{" "}
                                <span dir={language === "ar" ? 'ltr' : ''}>{millisToMinutes(productionRun?.actualSetupMillis)}{" "}{getTranslatedLabel("general.mins", "mins")}</span>
                              </Typography>
                            )}
                          </Box>

                          <Box display="flex" alignItems={"flex-end"}>
                            <Typography sx={{ minWidth: "220px" }} variant="h6">
                              {getTranslatedLabel(
                                "manufacturing.jobshop.prodruntasks.edit.addActualSetupTime",
                                "Add Actual Setup Time (in minutes)"
                              )}
                            </Typography>
                            <Field
                              id={"addSetupTime"}
                              name={"addSetupTime"}
                              component={FormNumericTextBox}
                              min={0}
                            />
                          </Box>
                        </Box>

                        <Box
                          sx={{
                            border: 1,
                            borderRadius: 3,
                            borderColor: "black",
                            marginLeft: 3,
                            p: 1,
                            my: 1,
                          }}
                        >
                          <Box display="flex" alignItems={"flex-end"}>
                            <Typography sx={{ minWidth: "220px" }} variant="h6">
                              {getTranslatedLabel(
                                "manufacturing.jobshop.prodruntasks.edit.estimatedUnitRunTime",
                                "Estimated Unit Run Time"
                              )}
                            </Typography>
                            <Typography
                              sx={{ fontWeight: "bold", marginLeft: 1 }}
                              color="black"
                              variant="h6"
                            >
                              <span dir={language === "ar" ? 'ltr' : ''}>{productionRun?.estimatedMilliSeconds} {getTranslatedLabel("general.ms", "ms")}</span> /{" "}
                              <span dir={language === "ar" ? 'ltr' : ''}>{millisToMinutes(productionRun?.estimatedMilliSeconds)}{" "}{getTranslatedLabel("general.mins", "mins")}</span>
                            </Typography>
                          </Box>
                          <Box display="flex" alignItems={"flex-end"}>
                            <Typography sx={{ minWidth: "220px" }} variant="h6">
                              {getTranslatedLabel(
                                "manufacturing.jobshop.prodruntasks.edit.actualTaskTime",
                                "Actual Task Time"
                              )}
                            </Typography>
                            {productionRun?.actualMilliSeconds && (
                              <Typography
                                sx={{ fontWeight: "bold", marginLeft: 1 }}
                                color="black"
                                variant="h6"
                              >
                                <span dir={language === "ar" ? 'ltr' : ''}>{productionRun?.actualMilliSeconds} {getTranslatedLabel("general.ms", "ms")}</span> /{" "}
                                <span dir={language === "ar" ? 'ltr' : ''}>{millisToMinutes(productionRun?.actualMilliSeconds)}{" "}{getTranslatedLabel("general.mins", "mins")}</span>
                              </Typography>
                            )}
                          </Box>
                          <Box display="flex" alignItems={"flex-end"}>
                            <Typography sx={{ minWidth: "220px" }} variant="h6">
                              {getTranslatedLabel(
                                "manufacturing.jobshop.prodruntasks.edit.addActualTaskTime",
                                "Add Actual Task Time (in Minutes)"
                              )}
                            </Typography>
                            <Field
                              id={"addTaskTime"}
                              name={"addTaskTime"}
                              component={FormNumericTextBox}
                              min={0}
                            />
                          </Box>
                        </Box>
                      </Grid>
                    </Grid>

                    <Grid container item xs={5} sx={{ mt: 3 }}>
                      <Grid container spacing={2} alignItems={"center"}>
                        <Box display="flex" flexDirection="column">
                          <Box display="flex" sx={{ paddingLeft: 1 }}>
                            <Typography sx={{ minWidth: "180px" }} variant="h6">
                              {getTranslatedLabel(
                                "manufacturing.jobshop.prodruntasks.edit.qtyToProduce",
                                "Qty To Produce"
                              )}
                            </Typography>
                            <Typography
                              sx={{ fontWeight: "bold", marginLeft: 1 }}
                              color="black"
                              variant="h6"
                            >
                              {productionRun?.quantityToProduce}
                            </Typography>
                          </Box>

                          <Box
                            sx={{
                              border: 1,
                              borderRadius: 3,
                              borderColor: "black",
                              py: 1,
                              pr: 1,
                              my: 1,
                            }}
                          >
                            <Box
                              display="flex"
                              sx={{ paddingLeft: 1 }}
                              alignItems={"flex-end"}
                            >
                              <Typography
                                sx={{ minWidth: "180px" }}
                                variant="h6"
                              >
                                {getTranslatedLabel(
                                  "manufacturing.jobshop.prodruntasks.edit.qtyProduced",
                                  "Qty Produced"
                                )}
                              </Typography>
                              <Typography
                                sx={{ fontWeight: "bold", marginLeft: 1 }}
                                color="black"
                                variant="h6"
                              >
                                {productionRun?.quantityProduced}
                              </Typography>
                            </Box>
                            <Box
                              display="flex"
                              sx={{ paddingLeft: 1 }}
                              alignItems={"flex-end"}
                            >
                              <Typography
                                sx={{ minWidth: "180px" }}
                                variant="h6"
                              >
                                {getTranslatedLabel(
                                  "manufacturing.jobshop.prodruntasks.edit.addQtyProduced",
                                  "Add Qty Produced"
                                )}
                              </Typography>
                              <Field
                                id={"addQuantityProduced"}
                                name={"addQuantityProduced"}
                                component={FormNumericTextBox}
                                min={0}
                              />
                            </Box>
                          </Box>

                          <Box
                            sx={{
                              border: 1,
                              borderRadius: 3,
                              borderColor: "black",
                              py: 1,
                              pr: 1,
                              my: 1,
                            }}
                          >
                            <Box
                              display="flex"
                              sx={{ paddingLeft: 1 }}
                              alignItems={"flex-end"}
                            >
                              <Typography
                                sx={{ minWidth: "180px" }}
                                variant="h6"
                              >
                                {getTranslatedLabel(
                                  "manufacturing.jobshop.prodruntasks.edit.qtyRejected",
                                  "Qty Rejected"
                                )}
                              </Typography>
                              <Typography
                                sx={{ fontWeight: "bold", marginLeft: 1 }}
                                color="black"
                                variant="h6"
                              >
                                {productionRun?.quantityRejected}
                              </Typography>
                            </Box>
                            <Box
                              display="flex"
                              sx={{ paddingLeft: 1 }}
                              alignItems={"flex-end"}
                            >
                              <Typography
                                sx={{ minWidth: "180px" }}
                                variant="h6"
                              >
                                {getTranslatedLabel(
                                  "manufacturing.jobshop.prodruntasks.edit.addQtyRejected",
                                  "Add Qty Rejected"
                                )}
                              </Typography>
                              <Field
                                id={"addQuantityRejected"}
                                name={"addQuantityRejected"}
                                component={FormNumericTextBox}
                                min={0}
                              />
                            </Box>
                          </Box>

                          <Box
                            display="flex"
                            sx={{ paddingLeft: 1 }}
                            alignItems={"flex-end"}
                          >
                            <Typography sx={{ minWidth: "180px" }} variant="h6">
                              {getTranslatedLabel(
                                "manufacturing.jobshop.prodruntasks.edit.comment",
                                "Comment"
                              )}
                            </Typography>
                            <Field
                              id={"noteId"}
                              name={"noteId"}
                              component={FormTextArea}
                            />
                          </Box>
                        </Box>
                      </Grid>
                    </Grid>
                  </Grid>
                </Grid>

                {/* Buttons */}
                <Box sx={{ marginTop: 4 }}>
                  <Grid container spacing={2}>
                    <Grid item>
                      <Button
                        color="success"
                        type={"submit"}
                        variant="contained"
                        disabled={!formRenderProps.allowSubmit || isLoading}
                      >
                        {isLoading ? (
                          <CircularProgress size={24} />
                        ) : (
                          getTranslatedLabel(
                            "general.submit",
                            "Submit"
                          )
                        )}
                      </Button>
                    </Grid>
                    <Grid item>
                      <Button
                        onClick={closeModal}
                        color="error"
                        variant="contained"
                        disabled={isLoading}
                      >
                        {getTranslatedLabel(
                          "general.cancel",
                          "Cancel"
                        )}
                      </Button>
                    </Grid>
                  </Grid>

                  {isLoading && (
                    <LoadingComponent
                      message={getTranslatedLabel(
                        "manufacturing.jobshop.prodruntasks.edit.processing",
                        "Processing Production Run Task Update..."
                      )}
                    />
                  )}

                  {isError && (
                    <Typography color="error">
                      {error?.data?.message ||
                        getTranslatedLabel(
                          "manufacturing.jobshop.prodruntasks.edit.error",
                          "An error occurred while updating the task"
                        )}
                    </Typography>
                  )}
                </Box>
              </fieldset>
            </FormElement>
          )}
        />
      </Paper>
    </>
  );
}
