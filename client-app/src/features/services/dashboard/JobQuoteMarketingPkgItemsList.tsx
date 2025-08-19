import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent
} from "@progress/kendo-react-grid";
import {useAppSelector} from "../../../app/store/configureStore";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";

interface Props {
    onClose: () => void;
}

export default function JobQuoteMarketingPkgItemsList({onClose}: Props) {
    const initialSort: Array<SortDescriptor> = [
        {field: "partyId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const relatedRecords: any = useAppSelector(state => state.quoteItemsUi.relatedRecords);


    return (
        <React.Fragment>
            <Grid container columnSpacing={1}>
                <Grid container>
                    <div className="div-container">
                        <KendoGrid style={{height: "300px"}}
                                   data={orderBy(relatedRecords ? relatedRecords : [], sort).slice(page.skip, page.take + page.skip)}
                                   sortable={true}
                                   sort={sort}
                                   onSortChange={(e: GridSortChangeEvent) => {
                                       setSort(e.sort);
                                   }}
                                   skip={page.skip}
                                   take={page.take}
                                   total={relatedRecords ? relatedRecords.length : 0}
                                   pageable={true}
                                   onPageChange={pageChange}

                        >
                            <Column field="productName" title="Product Name" width={220}/>
                            <Column field="facilityName" title="Facility" width={160}/>
                            <Column field="quantityOnHandTotal" title="QOH" width={80}/>
                            <Column field="availableToPromiseTotal" title="ATP" width={80}/>
                            <Column field="price" title="Price" width={80}/>

                        </KendoGrid>
                    </div>
                </Grid>


            </Grid>
            <Grid item xs={2}>
                <Button onClick={() => onClose()} variant="contained" color="error">
                    Close
                </Button>
            </Grid>
        </React.Fragment>
    );
}
