import React from 'react';
import {FieldRenderProps, FieldWrapper} from '@progress/kendo-react-form';
import {NumericTextBox} from '@progress/kendo-react-inputs';
import {Label} from '@progress/kendo-react-labels';
import {Notification, NotificationGroup} from "@progress/kendo-react-notification";


const FormNumericTextBox = (fieldRenderProps: FieldRenderProps) => {
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
        type,
        optional,
        autoComplete, min,
        ...others
    } = fieldRenderProps;
    const [focused, setFocused] = React.useState(false);

    const showValidationMessage = !focused && touched && validationMessage;
    const showHint = !showValidationMessage && focused && hint;
    const hintId = showHint ? `${id}_hint` : "";
    const errorId = showValidationMessage ? `${id}_error` : "";
    const position = {
        topLeft: {
            top: 0,
            left: 0,
            alignItems: "flex-start",
        },
        topCenter: {
            top: 0,
            left: "50%",
            transform: "translateX(-50%)",
        },
        topRight: {
            top: 0,
            right: 0,
            alignItems: "flex-end",
        },
        bottomLeft: {
            bottom: 0,
            left: 0,
            alignItems: "flex-start",
        },
        bottomCenter: {
            bottom: 0,
            left: "50%",
            transform: "translateX(-50%)",
        },
        bottomRight: {
            bottom: 0,
            right: 0,
            alignItems: "flex-end",
        },
    };
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

    return (
        <FieldWrapper>
            <Label editorId={id} editorValid={valid} editorDisabled={disabled}>
                {label}</Label>
            <NumericTextBox
                ariaDescribedBy={`${hintId} ${errorId}`}
                valid={valid}
                id={id}
                disabled={disabled}
                onFocus={handleOnFocus}
                onBlur={handleOnBlur}
                min={min}
                {...others}
            />
            {
                showHint &&
                <NotificationGroup style={position.bottomRight}>
                    <Notification type={{style: 'info', icon: false}} closable={false}>
                        <span>{hint}</span>
                    </Notification>
                </NotificationGroup>
            }

        </FieldWrapper>
    );
};

export default FormNumericTextBox;