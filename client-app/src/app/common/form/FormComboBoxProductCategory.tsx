import * as React from 'react';
import { FieldRenderProps, FieldWrapper } from '@progress/kendo-react-form';
import { Label } from '@progress/kendo-react-labels';
import { ComboBox } from '@progress/kendo-react-dropdowns';
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";

export const FormComboBoxProductCategory = (fieldRenderProps: FieldRenderProps) => {
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
        ...others
    } = fieldRenderProps;

    const editorRef = React.useRef(null);
    const [focused, setFocused] = React.useState(false);

    const showValidationMessage = !focused && touched && validationMessage;
    const showHint = !showValidationMessage && focused && hint;
    const hintId = showHint ? `${id}_hint` : "";
    const errorId = showValidationMessage ? `${id}_error` : "";
    const labelId = label ? `${id}_label` : '';

    const position = {
        bottomRight: {
            bottom: 0,
            right: 0,
            alignItems: "flex-end",
        },
    } as any;

    const handleOnFocus = React.useCallback(
        () => {
            onFocus();
            setFocused(true);
        },
        [onFocus]
    );

    const handleOnBlur = React.useCallback(
        () => {
            onBlur();
            setFocused(false);
        },
        [onBlur]
    );

    const onChangeHandler = React.useCallback(
        (event: any) => {
            const val = event.value;
            if (val && typeof val === 'object') {
                onChange({ value: val[dataItemKey] });
            } else {
                onChange({ value: val });
            }
        },
        [onChange, dataItemKey]
    );

    const selectedValue = data ? data.find((item: any) => item[dataItemKey] === value) : null;

    return (
        <FieldWrapper style={wrapperStyle}>
            <Label
                id={labelId}
                editorRef={editorRef}
                editorId={id}
                editorValid={valid}
                editorDisabled={disabled}
            >
                {label}
            </Label>

            <ComboBox
                ariaLabelledBy={labelId}
                ariaDescribedBy={`${hintId} ${errorId}`}
                ref={editorRef}
                valid={valid}
                id={id}
                disabled={disabled}
                data={data || []}
                dataItemKey={dataItemKey}
                textField={textField}
                value={selectedValue || value}
                onChange={onChangeHandler}
                onFocus={handleOnFocus}
                onBlur={handleOnBlur}
                filterable={true}
                popupSettings={{ appendTo: document.body }}
                {...others}
            />
            {
                showHint &&
                <NotificationGroup style={position.bottomRight}>
                    <Notification type={{ style: 'info', icon: true }} closable={false}>
                        <span>{hint}</span>
                    </Notification>
                </NotificationGroup>
            }
        </FieldWrapper>
    );
};

export const MemoizedFormComboBoxProductCategory = React.memo(FormComboBoxProductCategory);
