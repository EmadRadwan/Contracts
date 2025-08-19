import React, { useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Typography } from "@mui/material";
import {
  useAppDispatch,
  useAppSelector,
  useFetchAgreementsQuery,
} from "../../../../app/store/configureStore";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { handleDatesArray, handleDatesObject } from "../../../../app/util/utils";
import { useLocation } from "react-router-dom";
import { DataResult, State } from "@progress/kendo-data-query";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Agreement } from "../../../../app/models/accounting/agreement";
import AgreementsForm from "../form/AgreementsForm";
import { setSelectedAgreement } from "../../slice/accountingSharedUiSlice";

function AgreementsList() {
  const [editMode, setEditMode] = useState(0);
  const location = useLocation();
  const dispatch = useAppDispatch();
  const [show, setShow] = useState(false);
  const [dataState, setDataState] = React.useState<State>({ take: 9, skip: 0 });
  const [agreements, setAgreements] = React.useState<DataResult>({
    data: [],
    total: 0,
  });
  const {selectedAgreement} = useAppSelector(state => state.accountingSharedUi)

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    setDataState(e.dataState);
  };
  const { data, error, isFetching } = useFetchAgreementsQuery({ ...dataState });

  useEffect(() => {
    if (data) {
      const adjustedData = handleDatesArray(data.data);
      setAgreements({ data: adjustedData, total: data.total });
    }
  }, [data]);

  function handleSelectAgreements(agreementId: string) {
    const selectedAgreement: Agreement | undefined = data?.data?.find(
      (agreement: any) => agreementId === agreement.agreementId
    );
    if (selectedAgreement) {
      let agreementToDispatch = {
        ...selectedAgreement,
        fromPartyId: {
          fromPartyId: selectedAgreement.partyIdFrom,
          fromPartyName: selectedAgreement.partyIdFromName,
        },
        toPartyId: {
          toPartyId: selectedAgreement.partyIdTo,
          toPartyName: selectedAgreement.partyIdFromName,
        }
      }
      dispatch(setSelectedAgreement(handleDatesObject(agreementToDispatch)))
      // setSelectedAgreement({
      //   ...selectedAgreement,
      //   fromPartyId: {
      //     fromPartyId: selectedAgreement.partyIdFrom,
      //     fromPartyName: selectedAgreement.partyIdFromName,
      //   },
      //   toPartyId: {
      //     toPartyId: selectedAgreement.partyIdTo,
      //     toPartyName: selectedAgreement.partyIdFromName,
      //   }
      // });
      setEditMode(2);
    }
  }

  function cancelEdit() {
    setSelectedAgreement(undefined);
    dispatch(setSelectedAgreement(undefined))
    setEditMode(0);
  }

  const AgreementIdCell = (props: any) => {
    const field = props.field || "";
    const value = props.dataItem[field];
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
      <td
        className={props.className}
        style={{ ...props.style, color: "blue" }}
        colSpan={props.colSpan}
        role={"gridcell"}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{
          [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
        }}
        {...navigationAttributes}
      >
        <Button
          onClick={() => handleSelectAgreements(props.dataItem.agreementId)}
        >
          {props.dataItem.agreementId}
        </Button>
      </td>
    );
  };

  if (editMode > 0) {
    return (
      <AgreementsForm
        editMode={editMode}
        cancelEdit={cancelEdit}
        selectedAgreement={selectedAgreement}
      />
    );
  }
  if (location.state?.myStateProp === 'bar' && selectedAgreement) {
    return (
      <AgreementsForm
        editMode={2}
        cancelEdit={cancelEdit}
        selectedAgreement={selectedAgreement}
      />
    );
  }

  return (
    <>
      <AccountingMenu selectedMenuItem={"/agreements"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid
                style={{ height: "70vh" }}
                data={
                  agreements ? agreements : { data: [], total: data!.total }
                }
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={true}
                {...dataState}
                onDataStateChange={dataStateChange}
              >
                <GridToolbar>
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
                </GridToolbar>
                <Column
                  field="agreementId"
                  title="Agreement Id"
                  cell={AgreementIdCell}
                  width={210}
                  locked={true}
                />
                <Column field="partyIdFromName" title="Party From" />
                <Column field="partyIdToName" title="Party To" />
                <Column
                  field="roleTypeIdFromDescription"
                  title="Role Type From"
                />
                <Column
                  field="roleTypeIdToDescription"
                  title="Role Type To"
                  width={180}
                />
                <Column
                  field="agreementTypeIdDescription"
                  title="Agreement Type"
                />
                <Column
                  field="description"
                  title="Agreement Description"
                  width={180}
                />
                <Column
                  field="fromDate"
                  title="From"
                  format="{0: dd/MM/yyyy}"
                />
                <Column
                  field="thruDate"
                  title="To"
                  format="{0: dd/MM/yyyy}"
                />
              </KendoGrid>
              {isFetching && (
                <LoadingComponent message="Loading Agreements..." />
              )}
            </div>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
}

export default AgreementsList;
