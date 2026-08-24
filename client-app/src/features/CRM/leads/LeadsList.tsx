import React from 'react';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GridDataStateChangeEvent,
    GridCellProps
} from '@progress/kendo-react-grid';
import { State, process } from '@progress/kendo-data-query';
import { Grid, Typography, Chip, Box, Checkbox } from '@mui/material';
import Button from '@mui/material/Button';
import PersonIcon from '@mui/icons-material/Person';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import { useFetchLeadsQuery, useAppSelector } from '../../../app/store/configureStore';
import { Lead } from '../models/lead';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import { useLocation, useNavigate } from 'react-router-dom';
import AssignLeadModal from './AssignLeadModal';
import BulkAssignLeadsModal from './BulkAssignLeadsModal';

interface LeadsListProps {
    onCreateNew: () => void;
    onEditLead: (lead: Lead) => void;
}

const LeadsList: React.FC<LeadsListProps> = ({ onCreateNew, onEditLead }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads.list';

    const location = useLocation();
    const navigate = useNavigate();

    // Assignment is the CRM Admin's job - hide the controls for everyone else.
    // The server enforces this too; this only keeps the UI honest.
    const { user } = useAppSelector((state) => state.account);
    const canAssign = (user?.roles || []).includes('CRM_Leads_Assign');
    const highlightedLeadId = location.state?.duplicateLeadId ?? null;
    const highlightedLeadIdRef = React.useRef<string | null>(highlightedLeadId);

    React.useEffect(() => {
        if (highlightedLeadId) {
            navigate(location.pathname, { replace: true, state: {} });
        }
    }, []);

    const [dataState, setDataState] = React.useState<State>({
        take: 10,
        skip: 0,
        sort: [{ field: 'partyId', dir: 'asc' }],
        filter: highlightedLeadId
            ? {
                logic: 'and',
                filters: [{ field: 'partyId', operator: 'eq', value: highlightedLeadId }],
            }
            : undefined,
    });

    const { data: leads, isLoading } = useFetchLeadsQuery(dataState);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    // Assignment modal state
    const [assignOpen, setAssignOpen] = React.useState(false);
    const [leadToAssign, setLeadToAssign] = React.useState<Lead | undefined>(undefined);

    const handleOpenAssign = (lead: Lead) => {
        setLeadToAssign(lead);
        setAssignOpen(true);
    };

    const handleCloseAssign = () => {
        setAssignOpen(false);
        setLeadToAssign(undefined);
    };

    // Bulk selection. Selection is held as a Set of partyIds rather than a flag
    // on the row, because the grid is server-paged - the row objects are
    // replaced on every fetch, so a per-row flag would not survive paging.
    const [selectedIds, setSelectedIds] = React.useState<Set<string>>(new Set());
    const [bulkAssignOpen, setBulkAssignOpen] = React.useState(false);

    const toggleRow = (partyId: string) => {
        setSelectedIds((prev) => {
            const next = new Set(prev);
            if (next.has(partyId)) {
                next.delete(partyId);
            } else {
                next.add(partyId);
            }
            return next;
        });
    };

    const clearSelection = () => setSelectedIds(new Set());

    const processedData = leads || { data: [], total: 0 };

    React.useEffect(() => {
        if (
            highlightedLeadIdRef.current &&
            !isLoading &&
            processedData.data.length === 1
        ) {
            onEditLead(processedData.data[0] as Lead);
            highlightedLeadIdRef.current = null
        }
    }, [isLoading, processedData.data]);

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
        const { address1, city, countryGeoId, address2 } = props.dataItem;
        const parts = [address1, address2, city, countryGeoId].filter(Boolean);

        return (
            <td className={props.className} style={props.style}>
                {parts.length > 0 ? (
                    <Typography variant="body2" noWrap sx={{ maxWidth: 400 }}>
                        {parts.join(', ')}
                    </Typography>
                ) : (
                    <Typography variant="caption" color="text.disabled">-</Typography>
                )}
            </td>
        );
    };

    // Custom cell for the assigned owner
    const OwnerCell = (props: GridCellProps) => {
        const ownerName = props.dataItem.ownerName;
        return (
            <td className={props.className} style={props.style}>
                {ownerName ? (
                    <Chip
                        icon={<PersonIcon />}
                        label={ownerName}
                        size="small"
                        color="primary"
                        variant="outlined"
                    />
                ) : (
                    <Chip
                        label={getTranslatedLabel(`${localizationKey}.unassigned`, 'Unassigned')}
                        size="small"
                        variant="outlined"
                    />
                )}
            </td>
        );
    };

    // Assign / reassign action
    const AssignCell = (props: GridCellProps) => {
        return (
            <td className={props.className} style={props.style}>
                <Button
                    variant="outlined"
                    size="small"
                    onClick={() => handleOpenAssign(props.dataItem)}
                >
                    {props.dataItem.ownerPartyId
                        ? getTranslatedLabel(`${localizationKey}.reassign`, 'Reassign')
                        : getTranslatedLabel(`${localizationKey}.assign`, 'Assign')}
                </Button>
            </td>
        );
    };

    // Select-all applies to the current page only, which is what the user can see.
    const pageIds: string[] = processedData.data.map((d: any) => d.partyId).filter(Boolean);
    const allPageSelected = pageIds.length > 0 && pageIds.every((id) => selectedIds.has(id));
    const somePageSelected = pageIds.some((id) => selectedIds.has(id)) && !allPageSelected;

    const togglePage = () => {
        setSelectedIds((prev) => {
            const next = new Set(prev);
            if (allPageSelected) {
                pageIds.forEach((id) => next.delete(id));
            } else {
                pageIds.forEach((id) => next.add(id));
            }
            return next;
        });
    };

    const SelectCell = (props: GridCellProps) => {
        const partyId = props.dataItem.partyId;
        return (
            <td className={props.className} style={props.style}>
                <Checkbox
                    size="small"
                    checked={selectedIds.has(partyId)}
                    onChange={() => toggleRow(partyId)}
                />
            </td>
        );
    };

    if (isLoading) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loadingLeads`, 'Loading leads...')} />;
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
                total={processedData.total}
                onDataStateChange={dataStateChange}
            >
                {canAssign && selectedIds.size > 0 && (
                    <GridToolbar>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
                            <Typography variant="body2">
                                {getTranslatedLabel(`${localizationKey}.selectedCount`, '{0} selected')
                                    .replace('{0}', String(selectedIds.size))}
                            </Typography>
                            <Button
                                variant="contained"
                                size="small"
                                onClick={() => setBulkAssignOpen(true)}
                            >
                                {getTranslatedLabel(`${localizationKey}.assignSelected`, 'Assign selected')}
                            </Button>
                            <Button variant="text" size="small" onClick={clearSelection}>
                                {getTranslatedLabel(`${localizationKey}.clearSelection`, 'Clear')}
                            </Button>
                        </Box>
                    </GridToolbar>
                )}
                {canAssign && (
                    <Column
                        width={50}
                        filterable={false}
                        sortable={false}
                        cell={SelectCell}
                        headerCell={() => (
                            <Checkbox
                                size="small"
                                checked={allPageSelected}
                                indeterminate={somePageSelected}
                                onChange={togglePage}
                            />
                        )}
                    />
                )}

                {/* <Column
                    field="partyId"
                    title=" "
                    width={130}
                    headerCell={() => null}

                /> */}
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
                    width={300}
                />
                <Column
                    field="phone"
                    title={getTranslatedLabel(`${localizationKey}.phone`, 'Phone')}
                    cell={PhoneCell}
                    width={200}
                />
                <Column
                    field="ownerName"
                    title={getTranslatedLabel(`${localizationKey}.owner`, 'Assigned To')}
                    cell={OwnerCell}
                    width={200}
                />
                <Column
                    field="address1"
                    title={getTranslatedLabel(`${localizationKey}.address`, 'Address')}
                    cell={AddressCell}

                />
                {canAssign && (
                    <Column
                        title={getTranslatedLabel(`${localizationKey}.actions`, 'Actions')}
                        cell={AssignCell}
                        width={140}
                        filterable={false}
                        sortable={false}
                    />
                )}
            </KendoGrid>

            <AssignLeadModal
                open={assignOpen}
                onClose={handleCloseAssign}
                lead={leadToAssign}
            />

            <BulkAssignLeadsModal
                open={bulkAssignOpen}
                onClose={() => setBulkAssignOpen(false)}
                leadPartyIds={Array.from(selectedIds)}
                onAssigned={clearSelection}
            />
        </div>
    );
};

export default LeadsList;
