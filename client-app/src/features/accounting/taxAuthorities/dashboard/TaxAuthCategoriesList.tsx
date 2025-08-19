import { Button, Grid, Typography } from '@mui/material'
import { orderBy, SortDescriptor, State } from '@progress/kendo-data-query';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridRowProps,
    GridSortChangeEvent
} from "@progress/kendo-react-grid";
import React, { useEffect, useState } from 'react'
import { handleDatesArray } from '../../../../app/util/utils';

interface Props {
    taxCategories: any[]
    onClose: () => void
}

const TaxAuthCategoriesList = ({taxCategories, onClose}: Props) => {
    const initialSort: Array<SortDescriptor> = [
        {field: "taxAuthorityRateSeqId", dir: "asc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 10};
    const [page, setPage] = React.useState<any>(initialDataState);
    const [data, setData] = useState<any[]| null>(taxCategories)
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    useEffect(() => {
        if (taxCategories && taxCategories.length > 0) {
            const adjustedData = handleDatesArray(taxCategories)
            setData(adjustedData)
        }
    }, [taxCategories])

  return <>
    <Grid container padding={2} columnSpacing={1}>
        <Grid item xs={8}>
            <Typography sx={{p: 1}} variant="h6">
                Tax Authority Categories
            </Typography>
        </Grid>
        <Grid container>
                <div className="div-container">
                    <KendoGrid 
                        style={{ height: "65vh" }}
                        data={orderBy(data ? data : [], sort).slice(page.skip, page.take + page.skip)}
                        sortable={true}
                        sort={sort}
                        onSortChange={(e: GridSortChangeEvent) => {
                            setSort(e.sort);
                        }}
                        skip={page.skip}
                        take={page.take}
                        total={data ? data.length : 0}
                        pageable={true}
                        onPageChange={pageChange}
                        resizable={true}
                    >
                        <Column field='productCategoryId' title='Category Id' />
                        <Column field='productCategoryName' title='Name' />
                    </KendoGrid>
                </div>
        </Grid>
    </Grid>
    <Grid item xs={2}>
        <Button onClick={() => onClose()} color="error" variant="contained">
            Close
        </Button>
    </Grid>
  </>
}

export default TaxAuthCategoriesList