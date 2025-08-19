import React from 'react';
import {FieldRenderProps, FieldWrapper} from '@progress/kendo-react-form';
import {TextArea} from '@progress/kendo-react-inputs';
import {Error, Label} from '@progress/kendo-react-labels';
import {Notification, NotificationGroup} from "@progress/kendo-react-notification";

export const FormTextArea = (fieldRenderProps: FieldRenderProps) => {
    const {
        validationMessage,
        touched,
        value,
        onFocus,
        onBlur,
        label,
        id,
        valid,
        disabled,
        hint,
        type,
        optional,
        autoComplete,
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
            <Label editorId={id} editorValid={valid} optional={optional}>{label}</Label>
            <TextArea
                valid={valid}
                id={id}
                disabled={disabled}
                ariaDescribedBy={`${hintId} ${errorId}`}
                value={value || ""}
                onFocus={handleOnFocus}
                onBlur={handleOnBlur}
                {...others}
            />
            {
                showHint &&
                <NotificationGroup style={position.bottomRight}>
                    <Notification type={{style: 'info', icon: true}} closable={false}>
                        <span>{hint}</span>
                    </Notification>
                </NotificationGroup>
            }
            {
                showValidationMessage &&
                <Error id={errorId}>{validationMessage}</Error>
            }
        </FieldWrapper>
    );
};

export default FormTextArea;