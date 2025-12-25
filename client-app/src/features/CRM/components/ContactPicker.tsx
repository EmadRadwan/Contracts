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
import { useFetchContactsLovQuery } from '../../../app/store/configureStore';
import { ContactLov } from '../models/contact';
import { SalesOpportunityContact } from '../models/salesOpportunity';

interface ContactPickerProps {
    label?: string;
    value: SalesOpportunityContact[];
    onChange: (contacts: SalesOpportunityContact[]) => void;
    multiple?: boolean;
    placeholder?: string;
    disabled?: boolean;
}

const ContactPicker: React.FC<ContactPickerProps> = ({
    label = 'Contacts',
    value,
    onChange,
    multiple = true,
    placeholder = 'Search contacts...',
    disabled = false
}) => {
    const [inputValue, setInputValue] = useState('');
    const [open, setOpen] = useState(false);

    // Fetch contacts - query runs when dropdown is open
    const { data: contacts, isLoading, isFetching } = useFetchContactsLovQuery(
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

    const handleChange = useCallback((_: any, newValue: ContactLov | ContactLov[] | null) => {
        if (multiple) {
            const selectedContacts = (newValue as ContactLov[] || []).map(c => ({
                partyId: c.partyId,
                partyName: c.fullName,
                email: c.email,
                phone: c.phone,
                roleTypeId: 'LEAD_CONTACT'
            }));
            onChange(selectedContacts);
        } else {
            const contact = newValue as ContactLov | null;
            if (contact) {
                onChange([{
                    partyId: contact.partyId,
                    partyName: contact.fullName,
                    email: contact.email,
                    phone: contact.phone,
                    roleTypeId: 'LEAD_CONTACT'
                }]);
            } else {
                onChange([]);
            }
        }
    }, [multiple, onChange]);

    // Convert SalesOpportunityContact[] to ContactLov[] for the Autocomplete value
    const selectedValues: ContactLov[] = value.map(v => ({
        partyId: v.partyId!,
        fullName: v.partyName,
        email: v.email,
        phone: v.phone
    }));

    const loading = isLoading || isFetching;

    return (
        <Autocomplete
            multiple={multiple}
            options={contacts || []}
            loading={loading}
            disabled={disabled}
            value={multiple ? selectedValues : (selectedValues[0] || null)}
            inputValue={inputValue}
            open={open}
            onOpen={() => setOpen(true)}
            onClose={() => setOpen(false)}
            onChange={handleChange}
            onInputChange={handleInputChange}
            getOptionLabel={(option: ContactLov) => option.fullName || option.partyId || ''}
            isOptionEqualToValue={(option, val) => option.partyId === val.partyId}
            filterOptions={(x) => x} // Disable client-side filtering, server handles it
            noOptionsText={loading ? "Loading..." : "No contacts found"}
            renderInput={(params) => (
                <TextField
                    {...params}
                    label={label}
                    placeholder={placeholder}
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
                        size="small"
                    />
                ))
            }
        />
    );
};

export default ContactPicker;
