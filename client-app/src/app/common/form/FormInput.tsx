import React from 'react';
import {FieldRenderProps, FieldWrapper} from '@progress/kendo-react-form';
import {Input} from '@progress/kendo-react-inputs';
import {Error, Label} from '@progress/kendo-react-labels';
import {Notification, NotificationGroup} from "@progress/kendo-react-notification";


const FormInput = (fieldRenderProps: FieldRenderProps) => {
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
            <Label editorId={id} editorValid={valid} editorDisabled={disabled} optional={optional}>{label}</Label>
            <div className={'k-form-field-wrap'}>
                <Input
                    valid={valid}
                    type={type}
                    id={id}
                    autoComplete={autoComplete}
                    disabled={disabled}
                    ariaDescribedBy={`${hintId} ${errorId}`}
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
                
            </div>
        </FieldWrapper>
    );
};

export default FormInput;