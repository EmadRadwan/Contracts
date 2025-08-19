import AccountingMenu from "../../invoice/menu/AccountingMenu"
import {Grid, Paper, Typography} from "@mui/material"
import React, {useState} from "react";
import {State} from "@progress/kendo-data-query";
import {GRID_COL_INDEX_ATTRIBUTE, GridDataStateChangeEvent, GridToolbar, Grid as KendoGrid, GridColumn as Column} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {useFetchInternalAccountingOrganizationsQuery} from "../../../../app/store/apis";
import OrgGlSettingsMenu from "../../globalGlSetting/menu/OrgGlSettingsMenu";
import {router} from "../../../../app/router/Routes";
import {useAppDispatch} from "../../../../app/store/configureStore";
import {setSelectedAccountingCompanyId, setSelectedAccountingCompanyName} from "../../slice/accountingSharedUiSlice";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";

const OrganizationGlSettingsList = () => {
    const [editMode, setEditMode] = useState<number>(0);
    const {getTranslatedLabel} = useTranslationHelper()
    const localizationKey = "accounting.orgGL.dashboard"
    const [dataState, setDataState] = React.useState<State>({take: 8, skip: 0});

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const {data, isFetching} = useFetchInternalAccountingOrganizationsQuery({...dataState});

    const dispatch = useAppDispatch();

    const CompanyNameCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
      <td
        className={props.className}
        style={{ ...props.style, fontSize: "1.15rem", fontWeight: "bold" }}
        colSpan={props.colSpan}
        role={"gridcell"}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{
          [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
        }}
        {...navigationAttributes}
      >
        {props.dataItem.partyName}
      </td>
    );
    }

    const SetupCell = (props: any) => {
        const {dataItem, setup} = props;

        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() => setup(dataItem)}
                >
                    {getTranslatedLabel(`general.setup`, "Setup")}
                </Button>
            </td>
        );
    };

    const setup = (dataItem: any) => {
        dispatch(setSelectedAccountingCompanyId(dataItem.partyId));
        dispatch(setSelectedAccountingCompanyName(dataItem.partyName));
        router.navigate("/orgChartOfAccount");
    };

    const AccountingCell = (props: any) => {
        const {dataItem, accounting} = props;

        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() => accounting(dataItem)}
                >
                    {getTranslatedLabel(`general.accounting`, "Accounting")}
                </Button>
            </td>
        );
    };

    const accounting = (dataItem: any) => {
        dispatch(setSelectedAccountingCompanyId(dataItem.partyId));
        dispatch(setSelectedAccountingCompanyName(dataItem.partyName));
        router.navigate("/orgAccountingSummary");
    };

    return (
        <>
            <AccountingMenu selectedMenuItem={'/orgGL'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <OrgGlSettingsMenu/>
                <Grid container spacing={1} alignItems={"center"}>
                    <Grid item xs={12}>
                        <Typography sx={{p: 2}} variant='h5'>{getTranslatedLabel(`${localizationKey}.title`, "Companies")}</Typography>
                    </Grid>
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid style={{flex: 1}}
                                       {...dataState}
                                       data={data ? data : {data: [], total: 77}}
                                       onDataStateChange={dataStateChange}
                                       reorderable={true}>
                                <GridToolbar>
                                    <Grid container>
                                        <Grid item xs={12}>
                                            <Button color={"secondary"} onClick={() => setEditMode(1)}
                                                    variant="outlined">
                                                {getTranslatedLabel(`${localizationKey}.new`, "Create New Accounting Company")}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </GridToolbar>
                                <Column
                                    field="partyName"
                                    cell={CompanyNameCell}
                                    title={getTranslatedLabel(`${localizationKey}.company`, "Company")}
                                    width={320}
                                />
                                <Column
                                    field="partyId"
                                    title=" "
                                    width={120}
                                    cell={(props) => <SetupCell {...props} setup={setup}/>}
                                />
                                <Column
                                    field="partyId"
                                    title=" "
                                    width={120}
                                    cell={(props) => <AccountingCell {...props} accounting={accounting}/>}
                                />


                            </KendoGrid>
                            {isFetching && <LoadingComponent message="Loading Companies..."/>}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>

    );
}

export default OrganizationGlSettingsList