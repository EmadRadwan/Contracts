import React, { useState, useCallback } from 'react';
import {
    Autocomplete,
    TextField,
    Chip,
    Box,
    Typography,
    CircularProgress
} from '@mui/material';
import PersonIcon from '@mui/icons-material/Person';
import { useFetchLeadsLovQuery } from '../../../app/store/configureStore';
import { LeadLov } from '../models/lead';
import { SalesOpportunityLead } from '../models/salesOpportunity';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';

interface LeadPickerProps {
    label?: string;
    value: SalesOpportunityLead[];
    onChange: (leads: SalesOpportunityLead[]) => void;
    multiple?: boolean;
    placeholder?: string;
    disabled?: boolean;
}

const LeadPicker: React.FC<LeadPickerProps> = ({
    label,
    value,
    onChange,
    multiple = true,
    placeholder,
    disabled = false
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [inputValue, setInputValue] = useState('');
    const [open, setOpen] = useState(false);

    const defaultLabel = label || getTranslatedLabel('crm.leadPicker.label', 'Leads');
    const defaultPlaceholder = placeholder || getTranslatedLabel('crm.leadPicker.placeholder', 'Search leads...');

    // Fetch leads - query runs when dropdown is open
    const { data: leads, isLoading, isFetching } = useFetchLeadsLovQuery(
        { search: inputValue || undefined, take: 20 },
        { skip: !open }
    );

    const handleInputChange = useCallback((
        _event: React.SyntheticEvent,
        newInputValue: string,
        reason: string
    ) => {
        // Only update input when user is typing, not when selecting/clearing
        if (reason === 'input') {
            setInputValue(newInputValue);
        }
    }, []);

    const handleChange = useCallback((_: any, newValue: LeadLov | LeadLov[] | null) => {
        if (multiple) {
            onChange([])
            onChange([{
                partyId: (newValue as LeadLov[])[newValue?.length - 1].partyId,
                partyName: (newValue as LeadLov[])[newValue?.length - 1].fullName,
                email: (newValue as LeadLov[])[newValue?.length - 1].email,
                phone: (newValue as LeadLov[])[newValue?.length - 1].phone,
                dataSourceId: (newValue as LeadLov[])[newValue?.length - 1].dataSourceId,
                roleTypeId: 'LEAD_CONTACT'
            }]);
        } else {
            const lead = newValue as LeadLov | null;
            if (lead) {
                onChange([{
                    partyId: lead.partyId,
                    partyName: lead.fullName,
                    email: lead.email,
                    phone: lead.phone,
                    dataSourceId: lead.dataSourceId,
                    roleTypeId: 'LEAD_CONTACT'
                }]);
            } else {
                onChange([]);
            }
        }
    }, [multiple, onChange]);

    // Convert SalesOpportunityLead[] to LeadLov[] for the Autocomplete value
    const selectedValues: LeadLov[] = value.map(v => ({
        partyId: v.partyId!,
        fullName: v.partyName || v.fullName,
        email: v.email,
        phone: v.phone
    }));

    const loading = isLoading || isFetching;

    return (
        <Autocomplete
            multiple={multiple}
            options={leads || []}
            loading={loading}
            disabled={disabled}
            // value={multiple ? selectedValues : ([...selectedValues[0]] || [])}
            value={selectedValues || []}
            inputValue={inputValue}
            open={open}
            onOpen={() => setOpen(true)}
            onClose={() => setOpen(false)}
            onChange={handleChange}
            onInputChange={handleInputChange}
            getOptionLabel={(option: LeadLov) => option.fullName || option.partyId || ''}
            isOptionEqualToValue={(option, val) => option.partyId === val.partyId}
            filterOptions={(x) => x} // Disable client-side filtering, server handles it
            noOptionsText={loading ? getTranslatedLabel('crm.leadPicker.loading', 'Loading...') : getTranslatedLabel('crm.leadPicker.noLeads', 'No leads found')}
            renderInput={(params) => (
                <TextField
                    {...params}
                    label={defaultLabel}
                    placeholder={defaultPlaceholder}
                    direction={'rtl'}
                    InputProps={{
                        ...params.InputProps,
                        endAdornment: (
                            <>
                                {loading ? <CircularProgress color="inherit" size={20} /> : null}
                                {params.InputProps.endAdornment}
                            </>
                        ),
                    }}
                />
            )}
            renderOption={(props, option) => {
                const { key, ...restProps } = props as any;
                return (
                    <Box component="li" key={option.partyId} {...restProps} sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                        <PersonIcon fontSize="small" color="action" />
                        <Box>
                            <Typography variant="body2">{option.fullName || option.partyId}</Typography>
                            {option.email && (
                                <Typography variant="caption" color="text.secondary">
                                    {option.email}
                                </Typography>
                            )}
                        </Box>
                    </Box>
                );
            }}
            renderTags={(tagValue, getTagProps) =>
                tagValue.map((option, index) => (
                    <Chip
                        {...getTagProps({ index })}
                        key={option.partyId}
                        label={option.fullName || option.partyId}
                        icon={<PersonIcon />}
                        size="medium"
                        sx={{
                            px: "1.2em",
                            
                        }}
                    />
                ))
            }
        />
    );
};

export default LeadPicker;
