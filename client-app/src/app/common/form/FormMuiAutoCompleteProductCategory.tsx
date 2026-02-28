import * as React from 'react';
import { FieldRenderProps, FieldWrapper } from '@progress/kendo-react-form';
import { Label } from '@progress/kendo-react-labels';
import { Autocomplete, TextField } from '@mui/material';

export const FormMuiAutoCompleteProductCategory = (fieldRenderProps: FieldRenderProps) => {
    const {
        validationMessage,
        touched,
        onFocus,
        onBlur,
        label,
        id,
        valid,
        disabled,
        hint,
        wrapperStyle,
        value,
        data,
        dataItemKey,
        textField,
        onChange,
    } = fieldRenderProps;

    const showValidationMessage = touched && validationMessage;
    const labelId = label ? `${id}_label` : '';

    const handleOnChange = (event: any, newValue: any) => {
        if (newValue && typeof newValue === 'object') {
            onChange({ value: newValue[dataItemKey] });
        } else {
            onChange({ value: null });
        }
    };

    const selectedValue = data ? data.find((item: any) => item[dataItemKey] === value) : null;

    return (
        <FieldWrapper style={wrapperStyle}>
            <Label
                id={labelId}
                editorId={id}
                editorValid={valid}
                editorDisabled={disabled}
                style={{ marginBottom: '8px', display: 'block' }}
            >
                {label}
            </Label>
            <Autocomplete
                id={id}
                options={data || []}
                getOptionLabel={(option) => option[textField] || ''}
                value={selectedValue || null}
                onChange={handleOnChange}
                disabled={disabled}
                onFocus={onFocus}
                onBlur={onBlur}
                renderInput={(params) => (
                    <TextField
                        {...params}
                        error={!!showValidationMessage}
                        helperText={showValidationMessage}
                        size="small"
                        variant="outlined"
                    />
                )}
                isOptionEqualToValue={(option, val) => option[dataItemKey] === val[dataItemKey]}
            />
        </FieldWrapper>
    );
};

export const MemoizedFormMuiAutoCompleteProductCategory = React.memo(FormMuiAutoCompleteProductCategory);
