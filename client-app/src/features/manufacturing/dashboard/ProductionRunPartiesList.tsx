import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {Fragment, useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import { useFetchProductionRunPartyAssignmentsQuery} from "../../../app/store/apis";
import {Typography} from "@mui/material";
import {handleDatesArray} from "../../../app/util/utils";


interface Props {
    onClose: () => void;
    productionRunId?: string | undefined;
}


export default function ProductionRunPartiesList({productionRunId}: Props) {
    const initialSort: Array<SortDescriptor> = [
        {field: "partyName", dir: "asc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 10};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const [productionRunParties, setProductionRunParties] = useState<any[]>([]);


    const {data: productionRunPartiesData} = useFetchProductionRunPartyAssignmentsQuery(productionRunId, {skip: productionRunId === undefined});

    useEffect(() => {
        if (productionRunPartiesData) {
            const adjustedData = handleDatesArray(productionRunPartiesData);
            setProductionRunParties(adjustedData);
            
        }
    }, [productionRunPartiesData]);
    
    return <Fragment>

        <KendoGrid 
                   data={orderBy(productionRunParties ? productionRunParties : [], sort).slice(page.skip, page.take + page.skip)}
                   sortable={true}
                   sort={sort}
                   onSortChange={(e: GridSortChangeEvent) => {
                       setSort(e.sort);
                   }}
                   skip={page.skip}
                   take={page.take}
                   total={productionRunParties ? productionRunParties.length : 0}
                   pageable={true}
                   onPageChange={pageChange}
                   resizable={true}
        >
            <GridToolbar>
                <Typography color="primary"
                            sx={{fontSize: '18px', color: 'blue', fontWeight: 'bold'}}
                            variant="h6">
                    Parties
                </Typography>
            </GridToolbar>
            <Column field="workEffortId" title="Routing Task Id" width={200}/>
            <Column field="partyName" title="Party" width={150}/>
            <Column field="roleTypeDescription" title="Role" width={200}/>
            <Column field="statusDescription" title="Status" width={200}/>
            <Column field="fromDate" title="From" width={200} format="{0:MM/dd/yyyy}"/>
            <Column field="thruDate" title="From" width={200} format="{0:MM/dd/yyyy}"/>
            </KendoGrid>

    </Fragment>

}
