import React, { useEffect, useState } from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper } from "@mui/material";
import {
    useAppDispatch,
    useFetchTaxAuthoritiesQuery,
} from "../../../../app/store/configureStore";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { handleDatesArray } from "../../../../app/util/utils";
import { useLocation } from "react-router-dom";
import { DataResult, State } from "@progress/kendo-data-query";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { TaxAuthority  } from "../../../../app/models/accounting/taxAuthority";
import { useFetchTaxAuthorityCategoriesListQuery, useFetchTaxAuthorityProductsListQuery } from "../../../../app/store/apis";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import TaxAuthorityRateProductsList from "./TaxAuthorityRateProductsList";
import TaxAuthCategoriesList from "./TaxAuthCategoriesList";
import TaxAuthorityForm from "../form/TaxAuthorityForm";

function TaxAuthoritiesList() {
    const [editMode, setEditMode] = useState(0);
    const location = useLocation();
    const dispatch = useAppDispatch();
    const [showProducts, setShowProducts] = useState(false);
    const [showCategories, setShowCategories] = useState(false);
    const [selectedTaxAuth, setSelectedTaxAuth] = useState<{taxAuthPartyId: string, taxAuthGeoId: string} | undefined>(undefined);
    const [dataState, setDataState] = React.useState<State>({ take: 9, skip: 0 });
    const [taxAuthorities, setTaxAuthorities] = React.useState<DataResult>({
        data: [],
        total: 0,
    });
    const [selectedAuthority, setSelectedAuthority] = useState<TaxAuthority | undefined>(undefined)

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };
    const { data, isFetching } = useFetchTaxAuthoritiesQuery({ ...dataState });
    const {data:  taxProducts} = useFetchTaxAuthorityProductsListQuery({...selectedTaxAuth}!, {
        skip: selectedTaxAuth === undefined
    }) 
    const {data:  taxCategories} = useFetchTaxAuthorityCategoriesListQuery({...selectedTaxAuth}!, {
        skip: selectedTaxAuth === undefined
    }) 
    

    useEffect(() => {
        console.log(taxCategories)
    }, [taxCategories])

    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setTaxAuthorities({ data: adjustedData, total: data.total });
        }
    }, [data]);

    function handleSelectTaxAuthorities(taxAuthorityId: string) {
        const selectedTaxAuthority: TaxAuthority  | undefined = data?.data?.find(
            (taxAuth: any) => taxAuthorityId === taxAuth.taxAuthGeoId
        );
        setSelectedAuthority(selectedTaxAuthority)
        setEditMode(2);
    }

    function cancelEdit() {
        setSelectedAuthority(undefined)
        setEditMode(0);
    }

    const TaxAuthorityIdCell = (props: any) => {
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
                    onClick={() => handleSelectTaxAuthorities(props.dataItem.taxAuthGeoId)}
                >
                    {props.dataItem.taxAuthGeoId}
                </Button>
            </td>
        );
    };

    const TaxAuthProductsCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        const isDisabled = props.dataItem.taxAuthGeoId.includes("NA")
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
                    onClick={() => handleShowModal(props.dataItem, 1)}
                    disabled={isDisabled}
                >
                    Rate Products
                </Button>
            </td>
        );
    }

    const TaxAuthCategoryCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        const isDisabled = props.dataItem.taxAuthGeoId.includes("NA")
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
                    onClick={() => handleShowModal(props.dataItem, 2)}
                    disabled={isDisabled}
                >
                    Categories
                </Button>
            </td>
        );
    }

    const handleShowModal = (dataItem: any, selectedOption: number) => {
        const {taxAuthGeoId, taxAuthPartyId} = dataItem
        setSelectedTaxAuth({
            taxAuthGeoId,
            taxAuthPartyId
        })
        switch (selectedOption) {
            case 1: 
                setShowProducts(true)
                break;
            case 2:
                setShowCategories(true)
                break;
        }
    }

    const handleCloseModal = () => {
        setSelectedTaxAuth(undefined)
        setShowProducts(false)
        setShowCategories(false)
    }

    /*
          if (editMode) {
              return <ProductForm product={selectedProduct} cancelEdit={cancelEdit} editMode={editMode}/>
          }*/

    if (editMode > 0) {
        return <TaxAuthorityForm selectedTaxAuth={selectedAuthority} cancelEdit={cancelEdit} editMode={editMode} />
    }

    return (
        <>
        {showProducts && (
            <ModalContainer 
                onClose={handleCloseModal}
                show={showProducts}
                width={1100}
            >
                <TaxAuthorityRateProductsList taxProducts={taxProducts!} onClose={handleCloseModal} />
            </ModalContainer>
        )}
        {showCategories && (
            <ModalContainer 
                onClose={handleCloseModal}
                show={showCategories}
                width={1100}
            >
                <TaxAuthCategoriesList taxCategories={taxCategories!} onClose={handleCloseModal} />
            </ModalContainer>
        )}
            <AccountingMenu selectedMenuItem={"/taxAuthorities"} />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{ height: "70vh" }}
                                data={
                                    taxAuthorities ? taxAuthorities : { data: [], total: data!.total }
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
                                            <Button color={"secondary"} variant="outlined" onClick={() => setEditMode(1)}>
                                                Create Tax Authority
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </GridToolbar>
                                <Column
                                    field="taxAuthGeoId"
                                    title="TaxAuthority Geo Id"
                                    cell={TaxAuthorityIdCell}
                                    width={250}
                                    locked={true}
                                />
                                <Column
                                    field="taxAuthPartyName"
                                    title="Tax Authority"
                                    width={180}
                                />
                                <Column
                                    cell={TaxAuthProductsCell}
                                    title="Tax Authority Products"
                                    width={180}
                                />
                                <Column
                                    cell={TaxAuthCategoryCell}
                                    title="Tax Authority Categories"
                                    width={180}
                                />
                            </KendoGrid>
                            {isFetching && (
                                <LoadingComponent message="Loading Tax Authorities..." />
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}

export default TaxAuthoritiesList;
