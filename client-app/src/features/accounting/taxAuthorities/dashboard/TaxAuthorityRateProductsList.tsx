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
    taxProducts: any[]
    onClose: () => void
}

const TaxAuthorityRateProductsList = ({taxProducts, onClose}: Props) => {
    const initialSort: Array<SortDescriptor> = [
        {field: "taxAuthorityRateSeqId", dir: "asc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 10};
    const [page, setPage] = React.useState<any>(initialDataState);
    const [data, setData] = useState<any[]| null>(taxProducts)
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    useEffect(() => {
        if (taxProducts && taxProducts.length > 0) {
            const adjustedData = handleDatesArray(taxProducts)
            setData(adjustedData)
        }
    }, [taxProducts])

  return <>
    <Grid container padding={2} columnSpacing={1}>
        <Grid item xs={8}>
            <Typography sx={{p: 1}} variant="h6">
                Tax Authority Rate Products
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
                        <Column field='taxAuthorityRateTypeDescription' title='Description' />
                        <Column field='productStoreId' title='Product Store' />
                        <Column field='minItemPrice' title='Min Price' />
                        <Column field='minPurchase' title='Min Quantity' />
                        <Column field='taxShipping' title='Tax Shipping' />
                        <Column field='taxPercentage' title='Percentage' />
                        <Column field='taxPromotions' title='Promotions' />
                        <Column field='fromDate' title='From Date' format='{0: dd/MM/yyyy}' />
                        <Column field='thruDate' title='Thru Date' format='{0: dd/MM/yyyy}' />
                        <Column field='isTaxInShippingPrice' title='Is Tax In Shipping Price' />
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

export default TaxAuthorityRateProductsList