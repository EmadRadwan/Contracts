import React, {useMemo, useState} from 'react';
import { useAppSelector, useFetchPartyGlAccountsQuery } from '../../../../../app/store/configureStore';
import { router } from '../../../../../app/router/Routes';
import {CompositeFilterDescriptor, filterBy, orderBy, SortDescriptor, State} from '@progress/kendo-data-query';
import GlAccountDefaults from './GlAccountDefaults';
import { Grid, Paper } from '@mui/material';
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column, GridFilterChangeEvent,
    GridPageChangeEvent,
    GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import PartyGlAccountsForm from '../../form/PartyGlAccountsForm';
import { useTranslationHelper } from '../../../../../app/hooks/useTranslationHelper';  // ← Add this import

const PartyGlAccounts = () => {
    const { getTranslatedLabel } = useTranslationHelper();  // ← Hook for translations

    const selectedAccountingCompanyId = useAppSelector(
        (state) => state.accountingSharedUi.selectedAccountingCompanyId
    );

    if (!selectedAccountingCompanyId) {
        router.navigate("/orgGl");
    }

    const initialSort: Array<SortDescriptor> = [
        { field: "partyId", dir: "desc" },
    ];

    const { data: partyGlAccounts } = useFetchPartyGlAccountsQuery(selectedAccountingCompanyId, {
        skip: selectedAccountingCompanyId === undefined,
    });

    console.log(partyGlAccounts);

    const [sort, setSort] = React.useState(initialSort);

    const initialDataState: State = { skip: 0, take: 9 };
    const [page, setPage] = React.useState<any>(initialDataState);

    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const [filter, setFilter] = useState<CompositeFilterDescriptor | undefined>(undefined);
    const onFilterChange = (event: GridFilterChangeEvent) => {
        setFilter(event.filter);
    };
    // Then compute the filtered + sorted + paged data
    const processedData = useMemo(() => {
        const filtered = filterBy(partyGlAccounts ?? [], filter);

        const sorted = orderBy(filtered, sort);

        return sorted.slice(page.skip, page.skip + page.take);
    }, [partyGlAccounts, filter, sort, page.skip, page.take]);

// Also update total for correct pager
    const total = filter ? filterBy(partyGlAccounts ?? [], filter)?.length : partyGlAccounts?.length ?? 0;


    return (
        <>
            <GlAccountDefaults />

            <Grid container padding={2} columnSpacing={1} justifyContent={"center"}>
                <Grid item xs={8}>
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <PartyGlAccountsForm
                            selectedAccountingCompanyId={selectedAccountingCompanyId}
                        />

                        <div className="div-container">
                            <KendoGrid
                                data={processedData}
                                sortable={true}
                                resizable={true}
                                filterable={true}              // ← enable filter row
                                filter={filter}                // ← controlled filter
                                onFilterChange={onFilterChange}

                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => {
                                    setSort(e.sort);
                                }}
                                skip={page.skip}
                                take={page.take}
                                total={total}
                                pageable={true}
                                onPageChange={pageChange}
                            >
                                <Column
                                    field="partyDescription"
                                    title={getTranslatedLabel(
                                        "accounting.partyGlAccounts.grid.party",
                                        "Party"
                                    )}
                                    width={200}
                                />
                                <Column
                                    field="glAccountName"
                                    title={getTranslatedLabel(
                                        "accounting.partyGlAccounts.grid.glAccountName",
                                        "GL Account Name"
                                    )}
                                    width={350}
                                />
                                <Column
                                    field="glAccountTypeDescription"
                                    title={getTranslatedLabel(
                                        "accounting.partyGlAccounts.grid.glAccountType",
                                        "GL Account Type"
                                    )}
                                    width={250}
                                />
                                <Column
                                    field="roleDescription"
                                    title={getTranslatedLabel(
                                        "accounting.partyGlAccounts.grid.roleDescription",
                                        "Role Type"
                                    )}
                                    width={150}
                                />
                                <Column
                                    field="parentGlAccountName"
                                    title={getTranslatedLabel(
                                        "accounting.partyGlAccounts.grid.parentGlAccountName",
                                        "Parent Account"
                                    )}
                                    width={150}
                                />
                                <Column
                                    field="partyId"
                                    title={getTranslatedLabel(
                                        "accounting.partyGlAccounts.grid.partyId",
                                        "Party Id"
                                    )}
                                    width={150}
                                />
                                {/* <Column cell={CommandCell} width="auto" /> */}
                            </KendoGrid>
                        </div>
                    </Paper>
                </Grid>
            </Grid>
        </>
    );
};

export default PartyGlAccounts;