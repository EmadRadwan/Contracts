import { Fragment, useEffect, useState } from 'react';
import { Grid as KendoGrid, GridColumn as Column, GridPageChangeEvent, GridSortChangeEvent, GridToolbar } from '@progress/kendo-react-grid';
import { orderBy, SortDescriptor, State } from '@progress/kendo-data-query';
import { useFetchProductionRunTasksSimpleQuery } from '../../../app/store/apis';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import { handleDatesArray } from "../../../app/util/utils";
import { ProductionRunRoutingTask } from "../../../app/models/manufacturing/productionRunRoutingTask";
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import { Grid, Typography } from '@mui/material';
import { useAppSelector } from '../../../app/store/configureStore';

interface Props {
    productionRunTasksData?: [] | undefined;
}

const millisToMinutes = (millis: number) => (millis / 60000).toFixed(2);

export default function ProductionRunTasksListSimple({ productionRunTasksData }: Props) {
    const initialSort: Array<SortDescriptor> = [{ field: 'sequenceNum', dir: 'asc' }];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = { skip: 0, take: 10 };
    const [page, setPage] = useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => setPage(event.page);
    const {getTranslatedLabel} = useTranslationHelper()
    const {language} = useAppSelector(state => state.localization)
    
    const TimeDisplayCell = (props: any) => (
        <td><span dir={language === "ar" ? 'ltr' : 'rtl'}>{props.dataItem[props.field]} {getTranslatedLabel("general.ms", "ms")}</span> / <span dir='ltr'>{millisToMinutes(props.dataItem[props.field])} {getTranslatedLabel("general.mins", "mins")}</span></td>
    );

    const fixedColumns = [
        { field: "sequenceNum", title: getTranslatedLabel("manufacturing.jobshop.tasks.seqNumber", "Seq. Number"), width: 100 },
        { field: "workEffortName", title: getTranslatedLabel("manufacturing.jobshop.tasks.routingTaskName", "Routing Task Name"), width: 250 },
        { field: "fixedAssetName", title: getTranslatedLabel("manufacturing.jobshop.tasks.fixedAsset", "Fixed Asset"), width: 200 },
        { field: "estimatedStartDate", title: getTranslatedLabel("manufacturing.jobshop.tasks.startDate", "Start Date"), width: 150, format: "{0: dd/MM/yyyy}" },
        { field: "estimatedCompletionDate", title: getTranslatedLabel("manufacturing.jobshop.tasks.estimatedCompletionDate", "Estimated Completion Date"), width: 200, format: "{0: dd/MM/yyyy}" },
        { field: "estimatedSetupMillis", title: getTranslatedLabel("manufacturing.jobshop.tasks.estimatedSetupTime", "Estimated Setup Time"), width: 200, cell: TimeDisplayCell },
        { field: "estimatedMilliSeconds", title: getTranslatedLabel("manufacturing.jobshop.tasks.estimatedRunTime", "Estimated Run Time"), width: 200, cell: TimeDisplayCell },
    ];

    return (
        <Fragment>
            <KendoGrid
                data={orderBy(productionRunTasksData ?? [], sort).slice(page.skip, page.take + page.skip)}
                onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                skip={page.skip}
                take={page.take}
                total={productionRunTasksData ? productionRunTasksData.length : 0}
                pageable={true}
                onPageChange={pageChange}
                resizable={true}
            >
                <GridToolbar>
                    <Grid container justifyContent={"center"}>
                        <Grid item xs={12}>
                            <Typography
                                color="primary"
                                sx={{ fontSize: "18px", color: "blue", fontWeight: "bold", textAlign: "center" }}
                                variant="h6"
                            >
                                {getTranslatedLabel(
                                    "manufacturing.jobshop.tasks.title",
                                    "Tasks"
                                )}
                            </Typography>
                        </Grid>
                    </Grid>
                </GridToolbar>
                {fixedColumns.map((column) => (
                    <Column
                        key={column.field}
                        field={column.field}
                        title={column.title}
                        width={column.width}
                        format={column.format}
                        cell={column.cell}
                    />
                ))}
            </KendoGrid>
        </Fragment>
    );
}
