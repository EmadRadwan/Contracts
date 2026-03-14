import React from 'react';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GridDataStateChangeEvent,
    GridCellProps
} from '@progress/kendo-react-grid';
import { State, process } from '@progress/kendo-data-query';
import { Grid, Typography, Chip, Box } from '@mui/material';
import Button from '@mui/material/Button';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import { useFetchLeadsQuery } from '../../../app/store/configureStore';
import { Lead } from '../models/lead';
import LoadingComponent from '../../../app/layout/LoadingComponent';

interface LeadsListProps {
    onCreateNew: () => void;
    onEditLead: (lead: Lead) => void;
}

const LeadsList: React.FC<LeadsListProps> = ({ onCreateNew, onEditLead }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads.list';

    const { data: leads, isLoading } = useFetchLeadsQuery();

    const [dataState, setDataState] = React.useState<State>({
        take: 10,
        skip: 0,
        sort: [{ field: 'fullName', dir: 'asc' }]
    });

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const processedData = leads
        ? process(leads, dataState)
        : { data: [], total: 0 };

    // Custom cell for name (clickable)
    const NameCell = (props: GridCellProps) => {
        return (
            <td className={props.className} style={props.style}>
                <Button
                    variant="text"
                    color="primary"
                    onClick={() => onEditLead(props.dataItem)}
                    sx={{ textTransform: 'none', justifyContent: 'flex-start', p: 0 }}
                >
                    {props.dataItem.fullName || `${props.dataItem.firstName} ${props.dataItem.lastName}`.trim()}
                </Button>
            </td>
        );
    };

    // Custom cell for email
    const EmailCell = (props: GridCellProps) => {
        const email = props.dataItem.email;
        return (
            <td className={props.className} style={props.style}>
                {email ? (
                    <a href={`mailto:${email}`} style={{ color: 'inherit' }}>
                        {email}
                    </a>
                ) : (
                    <Typography variant="caption" color="text.disabled">-</Typography>
                )}
            </td>
        );
    };

    // Custom cell for phone
    const PhoneCell = (props: GridCellProps) => {
        const phone = props.dataItem.phone || props.dataItem.mobilePhone;
        return (
            <td className={props.className} style={props.style}>
                {phone ? (
                    <a href={`tel:${phone}`} style={{ color: 'inherit' }}>
                        {phone}
                    </a>
                ) : (
                    <Typography variant="caption" color="text.disabled">-</Typography>
                )}
            </td>
        );
    };

    // Custom cell for status
    const StatusCell = (props: GridCellProps) => {
        const status = props.dataItem.statusDescription || props.dataItem.statusId;
        return (
            <td className={props.className} style={props.style}>
                {status ? (
                    <Chip
                        label={status}
                        size="small"
                        color={props.dataItem.statusId === 'PARTY_ENABLED' ? 'success' : 'default'}
                        variant="outlined"
                    />
                ) : (
                    <Typography variant="caption" color="text.disabled">-</Typography>
                )}
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

    // Custom cell for address
    const AddressCell = (props: GridCellProps) => {
        const { address1, city, countryGeoId } = props.dataItem;
        const parts = [address1, city, countryGeoId].filter(Boolean);

        return (
            <td className={props.className} style={props.style}>
                {parts.length > 0 ? (
                    <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                        {parts.join(', ')}
                    </Typography>
                ) : (
                    <Typography variant="caption" color="text.disabled">-</Typography>
                )}
            </td>
        );
    };

    if (isLoading) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading leads...')} />;
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
                                {getTranslatedLabel(`${localizationKey}.title`, 'Leads')}
                            </Typography>
                        </Grid>
                    </Grid>
                </GridToolbar>

                <Column
                    field="fullName"
                    title={getTranslatedLabel(`${localizationKey}.name`, 'Name')}
                    cell={NameCell}
                    width={200}
                    locked={true}
                />
                <Column
                    field="email"
                    title={getTranslatedLabel(`${localizationKey}.email`, 'Email')}
                    cell={EmailCell}
                />
                <Column
                    field="phone"
                    title={getTranslatedLabel(`${localizationKey}.phone`, 'Phone')}
                    cell={PhoneCell}
                    width={150}
                />
                <Column
                    field="address1"
                    title={getTranslatedLabel(`${localizationKey}.address`, 'Address')}
                    cell={AddressCell}
                    width={250}
                />
                <Column
                    field="statusDescription"
                    title={getTranslatedLabel(`${localizationKey}.status`, 'Status')}
                    cell={StatusCell}
                    width={120}
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
