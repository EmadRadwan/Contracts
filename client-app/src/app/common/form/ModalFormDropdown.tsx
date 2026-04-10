import React from 'react';
import { FieldRenderProps } from '@progress/kendo-react-form';
import {
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    FormHelperText,
    SelectChangeEvent,
} from '@mui/material';

interface ModalFormDropdownProps extends FieldRenderProps {
    label?: string;
    placeholder?: string;
}

export const ModalFormDropdown = (props: ModalFormDropdownProps) => {
    const {
        value,
        onChange,
        name,
        data = [],
        label,
        placeholder = "Select an option",
        valid = true,
        validationMessage,
        touched,
        disabled,
        dataItemKey = "id",
        textField = "description",
    } = props;

    const handleChange = (event: SelectChangeEvent) => {
        const selectedValue = event.target.value;
        const selectedItem = data.find((item: any) => item[dataItemKey] === selectedValue);
        onChange({ value: selectedItem ? selectedItem[dataItemKey] : null });
    };

    const hasError = touched && !valid && !!validationMessage;
    const displayValue = value || '';

    return (
        <FormControl 
            fullWidth 
            error={hasError} 
            disabled={disabled}
            size="small"
            sx={{ mt: 1 }}
        >
            {label && <InputLabel id={`${name}-label`}>{label}</InputLabel>}

            <Select
                labelId={`${name}-label`}
                value={displayValue}
                label={label}
                onChange={handleChange}
                // displayEmpty
                sx={{
                    '& .MuiSelect-select': {
                        py: 1.2,
                    }
                }}
            >
                <MenuItem value="" disabled>
                    {placeholder}
                </MenuItem>

                {data.map((item: any, index: number) => (
                    <MenuItem key={index} value={item[dataItemKey]}>
                        {item[textField]}
                    </MenuItem>
                ))}
            </Select>

            {hasError && (
                <FormHelperText sx={{ ml: 0.5 }}>
                    {validationMessage}
                </FormHelperText>
            )}
        </FormControl>
    );
};

export const MemoizedModalFormDropdown = React.memo(ModalFormDropdown);