import React, { useCallback, useEffect, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import Button from "@mui/material/Button";
import { Grid, Paper } from "@mui/material";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import { useLocation, useNavigate } from "react-router-dom";
import {
    resetCertificateUi,
    setCertificateFormEditMode,
    setCurrentCertificateType,
    setSelectedCertificate
} from "../slice/certificateUiSlice";
import {useAppDispatch, useAppSelector} from "../../../app/store/configureStore";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {useFetchProjectCertificatesQuery} from "../../../app/store/apis/projectsApi";
import ProjectMenu from "../menu/ProjectMenu";
import ProjectCertificateForm from "../form/ProjectCertificateForm";


interface ProjectCertificate {
  workEffortId: string;
  projectNum: string;
  projectName: string;
  partyId: string;
  partyName: string; // From Party navigation
  description: string;
  estimatedStartDate: string;
  estimatedCompletionDate: string;
  statusDescription: string;
}

export default function ProjectCertificatesList() {
  const [certificates, setCertificates] = useState<DataResult>({ data: [], total: 0 });
  const [dataState, setDataState] = useState<State>({ take: 6, skip: 0 });
  const {  selectedCertificate, certificateFormEditMode } = useAppSelector((state) => state.certificateUi);
  const { getTranslatedLabel } = useTranslationHelper();
  const location = useLocation();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const [certificate, setCertificate] = useState<ProjectCertificate | undefined>(undefined);

  const { data, isFetching } = useFetchProjectCertificatesQuery({ ...dataState });

  useEffect(() => {
    if (data) {
      const adjustedData = data.data.map((item: ProjectCertificate) => ({
        ...item,
        estimatedStartDate: item.estimatedStartDate ? new Date(item.estimatedStartDate).toLocaleDateString("en-GB") : "",
        estimatedCompletionDate: item.estimatedCompletionDate ? new Date(item.estimatedCompletionDate).toLocaleDateString("en-GB") : "",
      }));
      setCertificates({ data: adjustedData, total: data.total });
    }
  }, [data]);
  

  useEffect(() => {
    if (selectedCertificate && certificates.data.length) {
      handleSelectCertificate(selectedCertificate.workEffortId);
    }
  }, [selectedCertificate, certificates]);

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    setDataState(e.dataState);
  };

  const handleSelectCertificate = useCallback(
    (workEffortId: string, statusDescription?: string, partyId?: string) => {
      const selectedCert: ProjectCertificate | undefined = certificates.data.find(
        (cert: any) => cert.workEffortId === workEffortId
      );
      const status = statusDescription ?? selectedCert?.statusDescription;
      const party = partyId ?? selectedCert?.partyId;

      setCertificate(selectedCert);
      dispatch(setSelectedCertificate(selectedCert));
      dispatch(setCurrentCertificateType("PROJECT_CERTIFICATE"));

      // Set edit mode based on status
      if (status === "CREATED") {
        dispatch(setCertificateFormEditMode(2));
      } else if (status === "APPROVED") {
        dispatch(setCertificateFormEditMode(3));
      } else if (status === "COMPLETED") {
        dispatch(setCertificateFormEditMode(4));
      }
    },
    [dispatch, certificates.data]
  );

  const cancelEdit = useCallback(() => {
    setCertificate(undefined);
    dispatch(setCertificateFormEditMode(0));
    dispatch(resetCertificateUi());
  }, [dispatch]);

  const handleMenuSelect = useCallback(
      (e: MenuSelectEvent) => {
        if (e.item.data === "procurements") {
          dispatch(resetCertificateUi());
          dispatch(setCurrentCertificateType("PROCUREMENT_CERTIFICATE"));
          dispatch(setCertificateFormEditMode(1));
        } else if (e.item.data === "contracting") {
          dispatch(resetCertificateUi());
          dispatch(setCurrentCertificateType("CONTRACTING_CERTIFICATE"));
          dispatch(setCertificateFormEditMode(1));
        }
      },
      [dispatch]
  );

  const ProjectNumberCell = (props: any) => {
    const field = props.field || "";
    const value = props.dataItem[field];
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
      <td
        className={props.className}
        style={{ ...props.style, color: "blue" }}
        colSpan={props.colSpan}
        role="gridcell"
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
        {...navigationAttributes}
      >
        <Button
          onClick={() =>
            handleSelectCertificate(
              props.dataItem.workEffortId,
              props.dataItem.statusDescription,
              props.dataItem.partyId
            )
          }
        >
          {props.dataItem.projectNum}
        </Button>
      </td>
    );
  };

  if (certificateFormEditMode > 0) {
    return (
      <ProjectCertificateForm
        selectedCertificate={certificate}
        cancelEdit={cancelEdit}
        editMode={certificateFormEditMode}
      />
    );
  }

  return (
    <>
      <ProjectMenu />
      <Paper elevation={5} className="div-container-withBorderCurved">
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={4}>
            <Menu onSelect={handleMenuSelect}>
              <MenuItem key="newCertificate" text={getTranslatedLabel("certificate.list.new", "New Certificate")}>
                <MenuItem
                    key="procurements"
                    text={getTranslatedLabel("certificate.list.procurements", "Procurements")}
                    data="procurements"
                />
                <MenuItem
                    key="contracting"
                    text={getTranslatedLabel("certificate.list.contracting", "Contracting")}
                    data="contracting"
                />
              </MenuItem>
            </Menu>
          </Grid>
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid
                style={{ height: "65vh" }}
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={true}
                {...dataState}
                data={certificates ? certificates : { data: [], total: 0 }}
                onDataStateChange={dataStateChange}
              >
                <Column
                  field="projectNum"
                  title={getTranslatedLabel("certificate.list.projectNum", "Project Number")}
                  width={150}
                  locked={false}
                  cell={ProjectNumberCell}
                />
                <Column
                  field="projectName"
                  title={getTranslatedLabel("certificate.list.projectName", "Project Name")}
                />
                <Column
                  field="partyId"
                  title={getTranslatedLabel("certificate.list.partyId", "Party ID")}
                />
                <Column
                  field="partyName"
                  title={getTranslatedLabel("certificate.list.partyName", "Party Name")}
                />
                <Column
                  field="description"
                  title={getTranslatedLabel("certificate.list.description", "Certificate Description")}
                />
                <Column
                  field="estimatedStartDate"
                  title={getTranslatedLabel("certificate.list.fromDate", "From Date")}
                  format="{0: dd/MM/yyyy}"
                />
                <Column
                  field="estimatedCompletionDate"
                  title={getTranslatedLabel("certificate.list.toDate", "To Date")}
                  format="{0: dd/MM/yyyy}"
                />
              </KendoGrid>
              {isFetching && (
                <LoadingComponent message={getTranslatedLabel("certificate.list.loading", "Loading Certificates...")} />
              )}
            </div>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
};