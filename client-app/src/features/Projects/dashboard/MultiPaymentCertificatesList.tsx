import React, {useCallback, useEffect, useMemo, useState} from "react";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent, GridToolbar,
} from "@progress/kendo-react-grid";
import {DataResult, State} from "@progress/kendo-data-query";
import {Button, Grid, Paper} from "@mui/material";
import MultiPaymentCertificateForm from "../form/MultiPaymentCertificateForm";
import {useAppDispatch} from "../../../app/store/configureStore";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {MultiPaymentCertificate} from "../../../app/models/project/MultiPaymentCertificate";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {useFetchMultiPaymentCertificatesQuery} from "../../../app/store/apis/multiPaymentCertificateApi";
import AccountingMenu from "../../accounting/invoice/menu/AccountingMenu";

export default function MultiPaymentCertificatesList() {
    const [certificates, setCertificates] = useState<DataResult>({data: [], total: 0});
    const [dataState, setDataState] = useState<State>({take: 6, skip: 0});
    const [formEditMode, setFormEditMode] = useState<number>(0);
    const [viewMode, setViewMode] = useState<"list" | "form">("list");
    const {getTranslatedLabel} = useTranslationHelper();
    const [paymentCertificate, setPaymentCertificate] = useState<MultiPaymentCertificate | null>(null);
    const dispatch = useAppDispatch();
    const {data, isFetching} = useFetchMultiPaymentCertificatesQuery({...dataState});


    useEffect(() => {
        if (data) {
            const adjustedData = data.data.map((item: MultiPaymentCertificate) => ({
                ...item,
                date: item.date ? new Date(item.date).toLocaleDateString("en-GB") : "",
            }));
            setCertificates({data: adjustedData, total: data.total});
        }
    }, [data]);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const handleSelectCertificate = useCallback(
        (certificateId?: string) => {
            if (!certificateId) return;
            const selectedCert = certificates.data.find(
                (cert: MultiPaymentCertificate) => cert.certificateId === certificateId
            );
            if (!selectedCert) return;
            setPaymentCertificate(selectedCert);
            setFormEditMode(1);
            setViewMode("form");
        },
        [certificates.data, dispatch]
    );


    const handleCreateNew = useCallback(() => {
        setFormEditMode(1);
        setViewMode("form");
    }, [dispatch]);

    const cancelEdit = useCallback(() => {
        setFormEditMode(0);
        setViewMode("list");
    }, []);

    const CertificateNumberCell = (props: any) => {
        const field = props.field || "";
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: "blue"}}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{[GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex}}
                {...navigationAttributes}
            >
                <Button
                    onClick={() =>
                        handleSelectCertificate(
                            props.dataItem.certificateId,
                            props.dataItem.currentStatusId
                        )
                    }
                >
                    {value}
                </Button>
            </td>
        );
    };

    if (viewMode === "form" && formEditMode > 0) {
        return (
            <MultiPaymentCertificateForm
                selectedCertificate={paymentCertificate}
                cancelEdit={cancelEdit}
                editMode={formEditMode}
            />
        );
    }

    const columnWidths = {
        certificateNumber: 150,
        date: 150,
        description: 250,
        paymentMethod: 200,
        totalAmount: 120,
    };

    return (
        <>
            <AccountingMenu selectedMenuItem={"/multiPaymentCertificates"} />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <KendoGrid
                            style={{height: "65vh"}}
                            scrollable="scrollable"
                            resizable={true}
                            filterable={true}
                            sortable={true}
                            pageable={true}
                            {...dataState}
                            data={certificates ? certificates : {data: [], total: 0}}
                            onDataStateChange={dataStateChange}
                        >
                            <GridToolbar>
                                <Button
                                    variant="contained"
                                    color="primary"
                                    onClick={handleCreateNew}
                                    style={{margin: "5px"}}
                                >
                                    {getTranslatedLabel(
                                        "accounting.multiPaymentCertificate.list.createNew",
                                        "Create New Certificate"
                                    )}
                                </Button>
                            </GridToolbar>
                            <Column
                                field="certificateNumber"
                                title={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.certificateNumber",
                                    "Certificate Number"
                                )}
                                width={columnWidths.certificateNumber}
                                cell={CertificateNumberCell}
                            />
                            <Column
                                field="date"
                                title={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.date",
                                    "Date"
                                )}
                                format="{0: dd/MM/yyyy}"
                                width={columnWidths.date}
                            />
                            <Column
                                field="description"
                                title={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.description",
                                    "Description"
                                )}
                                width={columnWidths.description}
                            />
                            <Column
                                field="paymentMethod"
                                title={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.paymentMethod",
                                    "Payment Method"
                                )}
                                width={columnWidths.paymentMethod}
                            />
                            <Column
                                field="totalAmount"
                                title={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.totalAmount",
                                    "Total Amount"
                                )}
                                width={columnWidths.totalAmount}
                                format="{0:c}"
                            />
                        </KendoGrid>
                        {isFetching && (
                            <LoadingComponent
                                message={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.loading",
                                    "Loading Certificates..."
                                )}
                            />
                        )}
                    </Grid>
                </Grid>
            </Paper>
        </>
        
);
}