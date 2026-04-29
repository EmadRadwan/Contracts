import React, { useState } from 'react';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridToolbar,
    GRID_COL_INDEX_ATTRIBUTE,
    GridCellProps,
    GridDataStateChangeEvent,
} from '@progress/kendo-react-grid';
import { State, process } from '@progress/kendo-data-query';
import { useTableKeyboardNavigation } from '@progress/kendo-react-data-tools';
import { Box, Chip, Typography, Button } from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import { useFetchOpportunitiesQuery } from '../../../app/store/configureStore';
import { SalesOpportunity } from '../models/salesOpportunity';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import AddActionModal from './components/AddActionsModal';

interface SalesOpportunityListProps {
    onCreateNew: () => void;
    onEditOpportunity: (opportunity: SalesOpportunity) => void;
}

const SalesOpportunityList: React.FC<SalesOpportunityListProps> = ({ onCreateNew, onEditOpportunity }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.opportunities';

    const { data: opportunities, isLoading } = useFetchOpportunitiesQuery();

    const [dataState, setDataState] = useState<State>({
        take: 10,
        skip: 0,
        sort: [{ field: 'createdStamp', dir: 'desc' }],
    });

    // Action Modal State
    const [openActionModal, setOpenActionModal] = useState(false);
    const [selectedOpportunityForAction, setSelectedOpportunityForAction] = useState<SalesOpportunity | null>(null);

    const handleOpenAction = (opportunity: SalesOpportunity) => {
        setSelectedOpportunityForAction(opportunity);
        setOpenActionModal(true);
    };

    const handleCloseActionModal = () => {
        setOpenActionModal(false);
        setSelectedOpportunityForAction(null);
    };

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const processedData = opportunities ? process(opportunities, dataState) : { data: [], total: 0 };

    const OpportunityLeadsCell = (props: GridCellProps) => {
            return (
                <td className={props.className} style={props.style}>
                    {props.dataItem.leads[0].partyName}
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
                resizable
                filterable
                sortable
                pageable={{ pageSizes: [10, 20, 50], buttonCount: 5 }}
                {...dataState}
                onDataStateChange={dataStateChange}
            >

                <Column cell={OpportunityLeadsCell} title={getTranslatedLabel(`${localizationKey}.leads`, 'Leads')} />
                <Column field="productName" title={getTranslatedLabel(`projects.certificate.form.project`, 'Project')} />
                <Column field="workEffortName" title={getTranslatedLabel(`${localizationKey}.unit`, 'unit')} />
                <Column field="opportunityStageName" title={getTranslatedLabel(`${localizationKey}.stage`, 'Stage')} />

                {/* Actions Column */}
                <Column
                    
                    width={200}
                    filterable={false}
                    sortable={false}
                    cell={(props: GridCellProps) => (
                        <td className={props.className} style={props.style}>
                            <Box sx={{ display: 'flex', gap: 1 }}>
                                <Button
                                    variant="outlined"
                                    size="small"
                                    onClick={() => onEditOpportunity(props.dataItem)}
                                >
                                    {getTranslatedLabel(`general.edit`, 'Edit')}
                                </Button>
                                <Button
                                    variant="outlined"
                                    size="small"
                                    onClick={() => handleOpenAction(props.dataItem)}
                                >
                                    {getTranslatedLabel(`general.actions`, 'Action')}
                                </Button>
                            </Box>
                        </td>
                    )}
                />
            </KendoGrid>

            {/* Action Modal */}
            <AddActionModal
                open={openActionModal}
                onClose={handleCloseActionModal}
                opportunity={selectedOpportunityForAction}
            />
        </div>
    );
};

export default SalesOpportunityList;