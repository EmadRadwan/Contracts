import React from 'react';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GRID_COL_INDEX_ATTRIBUTE,
    GridDataStateChangeEvent,
    GridCellProps
} from '@progress/kendo-react-grid';
import { State, process } from '@progress/kendo-data-query';
import { useTableKeyboardNavigation } from '@progress/kendo-react-data-tools';
import { Grid, Box, Chip, Typography } from '@mui/material';
import Button from '@mui/material/Button';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import { useFetchOpportunitiesQuery } from '../../../app/store/configureStore';
import { SalesOpportunity } from '../models/salesOpportunity';
import LoadingComponent from '../../../app/layout/LoadingComponent';

interface LeadsListProps {
    onCreateNew: () => void;
    onEditOpportunity: (opportunity: SalesOpportunity) => void;
}

const LeadsList: React.FC<LeadsListProps> = ({ onCreateNew, onEditOpportunity }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads.list';

    const { data: opportunities, isLoading, refetch } = useFetchOpportunitiesQuery();

    const [dataState, setDataState] = React.useState<State>({
        take: 10,
        skip: 0,
        sort: [{ field: 'createdStamp', dir: 'desc' }]
    });

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const processedData = opportunities
        ? process(opportunities, dataState)
        : { data: [], total: 0 };

    // Custom cell for opportunity name (clickable)
    const NameCell = (props: GridCellProps) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ ...props.style }}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button
                    variant="text"
                    color="primary"
                    onClick={() => onEditOpportunity(props.dataItem)}
                    sx={{ textTransform: 'none', justifyContent: 'flex-start', p: 0 }}
                >
                    {props.dataItem.opportunityName}
                </Button>
            </td>
        );
    };

    // Custom cell for amount
    const AmountCell = (props: GridCellProps) => {
        const amount = props.dataItem.estimatedAmount || 0;
        const currency = props.dataItem.currencyUomId || 'USD';
        const formatted = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency,
            minimumFractionDigits: 0
        }).format(amount);

        return (
            <td className={props.className} style={props.style}>
                <Typography variant="body2" fontWeight="medium" color="success.main">
                    {formatted}
                </Typography>
            </td>
        );
    };

    // Custom cell for stage
    const StageCell = (props: GridCellProps) => {
        return (
            <td className={props.className} style={props.style}>
                <Chip
                    label={props.dataItem.opportunityStageName || props.dataItem.opportunityStageId}
                    size="small"
                    color="primary"
                    variant="outlined"
                />
            </td>
        );
    };

    // Custom cell for probability
    const ProbabilityCell = (props: GridCellProps) => {
        const probability = props.dataItem.estimatedProbability || 0;
        let color: 'error' | 'warning' | 'success' = 'error';
        if (probability >= 70) color = 'success';
        else if (probability >= 40) color = 'warning';

        return (
            <td className={props.className} style={props.style}>
                <Chip
                    label={`${probability}%`}
                    size="small"
                    color={color}
                    sx={{ minWidth: 50 }}
                />
            </td>
        );
    };

    // Custom cell for date
    const DateCell = (props: GridCellProps) => {
        const field = props.field || '';
        const value = props.dataItem[field];
        const formatted = value ? new Date(value).toLocaleDateString() : '-';

        return (
            <td className={props.className} style={props.style}>
                {formatted}
            </td>
        );
    };

    // Custom cell for leads
    const LeadsCell = (props: GridCellProps) => {
        const leads = props.dataItem.leads || [];
        return (
            <td className={props.className} style={props.style}>
                {leads.length > 0 ? (
                    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                        {leads.slice(0, 2).map((c: any, idx: number) => (
                            <Chip
                                key={idx}
                                label={c.partyName}
                                size="small"
                                variant="outlined"
                                sx={{ maxWidth: 100 }}
                            />
                        ))}
                        {leads.length > 2 && (
                            <Chip
                                label={`+${leads.length - 2}`}
                                size="small"
                                color="default"
                            />
                        )}
                    </Box>
                ) : (
                    <Typography variant="caption" color="text.disabled">-</Typography>
                )}
            </td>
        );
    };

    if (isLoading) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading opportunities...')} />;
    }

    return (
        <div className="div-container">
            <KendoGrid
                style={{ height: '75vh', width: '100%' }}
                data={processedData}
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={{
                    pageSizes: [10, 20, 50],
                    buttonCount: 5
                }}
                {...dataState}
                onDataStateChange={dataStateChange}
            >
                <GridToolbar>
                    <Grid container justifyContent="space-between" alignItems="center">
                        <Grid item>
                            <Typography variant="h6">
                                {getTranslatedLabel(`${localizationKey}.title`, 'Sales Opportunities')}
                            </Typography>
                        </Grid>
                        <Grid item>
                            <Button
                                variant="outlined"
                                color="secondary"
                                onClick={onCreateNew}
                            >
                                {getTranslatedLabel(`${localizationKey}.createNew`, 'Create New Opportunity')}
                            </Button>
                        </Grid>
                    </Grid>
                </GridToolbar>

                <Column
                    field="opportunityName"
                    title={getTranslatedLabel(`${localizationKey}.name`, 'Name')}
                    cell={NameCell}
                    width={250}
                    locked={true}
                />
                <Column
                    field="estimatedAmount"
                    title={getTranslatedLabel(`${localizationKey}.amount`, 'Value')}
                    cell={AmountCell}
                    width={120}
                />
                <Column
                    field="opportunityStageName"
                    title={getTranslatedLabel(`${localizationKey}.stage`, 'Stage')}
                    cell={StageCell}
                    width={130}
                />
                <Column
                    field="estimatedProbability"
                    title={getTranslatedLabel(`${localizationKey}.probability`, 'Probability')}
                    cell={ProbabilityCell}
                    width={100}
                />
                <Column
                    field="ownerName"
                    title={getTranslatedLabel(`${localizationKey}.owner`, 'Owner')}
                    width={150}
                />
                <Column
                    field="estimatedCloseDate"
                    title={getTranslatedLabel(`${localizationKey}.closeDate`, 'Close Date')}
                    cell={DateCell}
                    width={120}
                />
                <Column
                    field="leads"
                    title={getTranslatedLabel(`${localizationKey}.leads`, 'Leads')}
                    cell={LeadsCell}
                    width={200}
                    filterable={false}
                    sortable={false}
                />
                <Column
                    field="createdStamp"
                    title={getTranslatedLabel(`${localizationKey}.created`, 'Created')}
                    cell={DateCell}
                    width={120}
                />
            </KendoGrid>
        </div>
    );
};

export default LeadsList;
