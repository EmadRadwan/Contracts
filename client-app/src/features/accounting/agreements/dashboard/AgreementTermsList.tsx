import { useAppSelector } from "../../../../app/store/configureStore";
import { useFetchAgreementTermsQuery } from "../../../../app/store/apis";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import AgreementsMenu from "../menu/AgreementsMenu";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import {
  Grid as KendoGrid,
  GridColumn as Column,
} from "@progress/kendo-react-grid";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { Navigate, useNavigate } from "react-router";

const AgreementTermsList = () => {
  const navigate = useNavigate();
  const { selectedAgreement } = useAppSelector(
    (state) => state.accountingSharedUi
  );

  if (!selectedAgreement) {
    return <Navigate to="/agreements" />;
  }

  function handleBackClick() {
    navigate("/agreements", { state: { myStateProp: "bar" } });
  }

  const { data: agreementTerms, isFetching } = useFetchAgreementTermsQuery(
    selectedAgreement?.agreementId!,
    {
      skip: !selectedAgreement,
    }
  );
  console.log(agreementTerms);
  return (
    <>
      <AccountingMenu selectedMenuItem="/agreements" />
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
                  fontWeight: "bold",
                  paddingLeft: 3,
                  fontSize: "18px",
                }}
              >
                {`Agreement Terms for Agreement: ${selectedAgreement?.agreementId}`}
              </Typography>
            </Box>
          </Grid>

          <Grid item xs={5}>
            <AgreementsMenu />
          </Grid>
        </Grid>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid style={{ height: "45vh" }} data={agreementTerms ?? []} resizable>
                {/* <GridToolbar>
                  <Grid container>
                    <Grid item xs={4}>
                      <Button
                        color={"secondary"}
                        variant="outlined"
                        onClick={() => setEditMode(1)}
                      >
                        Create Agreement
                      </Button>
                    </Grid>
                  </Grid>
                </GridToolbar> */}
                <Column field="agreementTermId" title="Agreement Term" />
                <Column field="description" title="Description" />
                <Column field="termTypeDescription" title="Term Type" width={230}/>
                <Column field="agreementItemSeqId" title="Agreement Item" />
                <Column
                  field="invoiceItemTypeDescription"
                  title="Invoice Item Type"
                  width={230}
                />
                <Column field="termDays" title="Term Duration (Days)" />
                <Column field="fromDate" title="From" />
                <Column field="thruDate" title="To" />
              </KendoGrid>
              {isFetching && (
                <LoadingComponent message="Loading Agreement Terms..." />
              )}
              <Grid item xs={3} paddingTop={1}>
                <Button
                  variant="contained"
                  color="error"
                  onClick={handleBackClick}
                >
                  Back
                </Button>
              </Grid>
            </div>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
};

export default AgreementTermsList;
