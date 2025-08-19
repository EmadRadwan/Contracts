import { useEffect, useState } from "react";
import OrganizationGlSettingsMenuNavContainer from "../menu/OrganizationGlSettingsMenu";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Button, Grid, Paper } from "@mui/material";
import { useAppSelector } from "../../../../app/store/configureStore";
import { router } from "../../../../app/router/Routes";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { useCloseTimePeriodMutation, useFetchCustomTimePeriodsLovQuery } from "../../../../app/store/apis/accounting/customTimePeriodsApi";
import { CustomTimePeriod } from "../../../../app/models/accounting/customTimePeriod";
import { handleDatesArray } from "../../../../app/util/utils";
import OrgSetupTimePeriodForm from "../form/OrgSetupTimePeriodForm";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";

const OrgSetupTimePeriodList = () => {
  const companyId = useAppSelector(
    (state) => state.accountingSharedUi.selectedAccountingCompanyId
  );
  if (!companyId) {
    router.navigate("/orgGl");
  }
  const {getTranslatedLabel} = useTranslationHelper()

  const [timePeriods, setTimePeriods] = useState<CustomTimePeriod[]>([]);
  const [selectedTimePeriod, setSelectedTimePeriod] = useState<
    CustomTimePeriod | undefined
  >(undefined);
  const [editMode, setEditMode] = useState<number>(0);

  const { data } = useFetchCustomTimePeriodsLovQuery(undefined);
  useEffect(() => {
    if (data) {
      setTimePeriods(handleDatesArray(data));
    }
  }, [data]);
  const [closePeriod, {isLoading}] = useCloseTimePeriodMutation()

  const handleSelectTimePeriod = (customTimePeriodId: any) => {
    const selected = timePeriods?.find(
      (t: CustomTimePeriod) => t.customTimePeriodId === customTimePeriodId
    );
    if (selected) {
      setSelectedTimePeriod(selected);
      setEditMode(2);
    }
  };

  const handleCloseTimePeriod = async (customTimePeriodId: string) => {
    try {
      let res = await closePeriod(customTimePeriodId);
      if (res.error) {
        toast.error(res.error.data.title)
        return
      }
      toast.success("Time Period closed successfully")
    } catch (e) {
      console.error(e)
      toast.error("Something went wrong")
    }
  }

  const TimePeriodIdCell = (props: any) => {
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
          onClick={() => {
            handleSelectTimePeriod(props.dataItem.customTimePeriodId);
          }}
        >
          {props.dataItem.customTimePeriodId}
        </Button>
      </td>
    );
  };

  const CloseTimePeriodCell = (props: any) => {
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
          onClick={() => {
            handleCloseTimePeriod(props.dataItem.customTimePeriodId)
          }}
        >
          {getTranslatedLabel("general.close", "Close")}
        </Button>
      </td>
    );
  };


  if (editMode > 0) {
    return <OrgSetupTimePeriodForm selectedTimePeriod={selectedTimePeriod} editMode={editMode} onClose={() => setEditMode(0)} />
  }
  return (
    <>
      <AccountingMenu selectedMenuItem={"/orgGL"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <OrganizationGlSettingsMenuNavContainer />
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid
                style={{ height: "65vh", flex: 1 }}
                resizable={true}
                data={timePeriods ?? []}
              >
                <GridToolbar>
                  <Grid container>
                    <Grid item xs={4}>
                      <Button
                      color={"secondary"}
                        variant="outlined"
                        onClick={() => setEditMode(1)}
                      >
                        Create Custom Time Period
                      </Button>
                    </Grid>
                  </Grid>
                </GridToolbar>
                <Column
                  field="customTimePeriodId"
                  title="Time Period Id"
                  width={120}
                  locked
                  cell={TimePeriodIdCell}
                />
                <Column field="parentPeriodId" title="Parent Time Period Id" />
                <Column field="periodTypeDescription" title="Period Type" />
                <Column field="periodNum" title="Period Num" />
                <Column field="periodName" title="Period Name" width={160} />
                <Column field="fromDate" title="From Date" format="{0: dd/MM/yyyy}" />
                <Column field="thruDate" title="Thru Date" format="{0: dd/MM/yyyy}" />
                <Column cell={CloseTimePeriodCell} />
              </KendoGrid>
            </div>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
};

export default OrgSetupTimePeriodList;
