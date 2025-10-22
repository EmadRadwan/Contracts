import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import { Button, Grid, Paper } from "@mui/material";
import MultiPaymentCertificateForm from "../form/MultiPaymentCertificateForm";
import { useAppDispatch } from "../../../app/store/configureStore";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { MultiPaymentCertificate } from "../../../app/models/project/MultiPaymentCertificate";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useFetchMultiPaymentCertificatesQuery } from "../../../app/store/apis/multiPaymentCertificateApi";
import AccountingMenu from "../../accounting/invoice/menu/AccountingMenu";
import {handleDatesArray} from "../../../app/util/utils";

export default function MultiPaymentCertificatesList() {
    const [certificates, setCertificates] = useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = useState<State>({ take: 6, skip: 0 });
    const [formEditMode, setFormEditMode] = useState<number>(0);
    const [viewMode, setViewMode] = useState<"list" | "form">("list");
    const { getTranslatedLabel } = useTranslationHelper();
    const [paymentCertificate, setPaymentCertificate] = useState<MultiPaymentCertificate | null>(null);
    const dispatch = useAppDispatch();
    const { data, isFetching } = useFetchMultiPaymentCertificatesQuery({ ...dataState });

    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setCertificates({ data: adjustedData, total: data.total });
        }
    }, [data]);

    useEffect(() => {
        if (viewMode === "list") {
            setPaymentCertificate(null);
            setFormEditMode(0); // REFACTOR: Ensure editMode is reset when returning to list view
        }
    }, [viewMode]);
    
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const handleSelectCertificate = useCallback(
        (workEffortId?: string) => {
            if (!workEffortId) return;
            const selectedCert = certificates.data.find(
                (cert: MultiPaymentCertificate) => cert.workEffortId === workEffortId
            );
            if (!selectedCert) return;
            setPaymentCertificate(selectedCert);
            setFormEditMode(2);
            setViewMode("form");
        },
        [certificates.data]
    );

    const handleCreateNew = useCallback(() => {
        setPaymentCertificate(null);
        setFormEditMode(1);
        setViewMode("form");
    }, []);

    const cancelEdit = useCallback(() => {
        setPaymentCertificate(null);
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
                        handleSelectCertificate(props.dataItem.workEffortId)
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
                setEditMode={setFormEditMode}
                setParentCertificate={setPaymentCertificate}
            />
        );
    }

    const columnWidths = {
        workEffortId: 150,
        date: 150,
        description: 250,
        paymentMethod: 200,
        chequeNumber: 150,
        chequeDate: 150,
    };

    return (
        <>
            <AccountingMenu selectedMenuItem={"/multiPaymentCertificates"} />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <KendoGrid
                            style={{ height: "65vh" }}
                            scrollable="scrollable"
                            resizable={true}
                            filterable={true}
                            sortable={true}
                            pageable={true}
                            {...dataState}
                            data={certificates ? certificates : { data: [], total: 0 }}
                            onDataStateChange={dataStateChange}
                        >
                            <GridToolbar>
                                <Button
                                    variant="contained"
                                    color="primary"
                                    onClick={handleCreateNew}
                                    style={{ margin: "5px" }}
                                >
                                    {getTranslatedLabel(
                                        "accounting.multiPaymentCertificate.list.createNew",
                                        "Create New Certificate"
                                    )}
                                </Button>
                            </GridToolbar>
                            <Column
                                field="workEffortId"
                                title={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.workEffortId",
                                    "Certificate ID"
                                )}
                                width={columnWidths.workEffortId}
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
                                field="paymentMethodDescription"
                                title={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.paymentMethod",
                                    "Payment Method"
                                )}
                                width={columnWidths.paymentMethod}
                            />
                            <Column
                                field="chequeNumber"
                                title={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.chequeNumber",
                                    "Cheque Number"
                                )}
                                width={columnWidths.chequeNumber}
                            />
                            <Column
                                field="chequeDate"
                                title={getTranslatedLabel(
                                    "accounting.multiPaymentCertificate.list.chequeDate",
                                    "Cheque Date"
                                )}
                                format="{0: dd/MM/yyyy}"
                                width={columnWidths.chequeDate}
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