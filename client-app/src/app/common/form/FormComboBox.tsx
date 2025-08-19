import * as React from "react";

import {FieldRenderProps, FieldWrapper} from "@progress/kendo-react-form";
import {Error, Label} from "@progress/kendo-react-labels";
import {ComboBox} from "@progress/kendo-react-dropdowns";
import {Notification, NotificationGroup} from "@progress/kendo-react-notification";


export const FormComboBox = (fieldRenderProps: FieldRenderProps) => {
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
        name,
        onChange,
        ...others
    } = fieldRenderProps;
    const editorRef = React.useRef(null);
    const [focused, setFocused] = React.useState(false);

    const onChangeHandler = React.useCallback(
        (event) => onChange({value: event.value && event.value[name]}),
        [onChange, name]
    );
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
    const showValidationMessage = !focused && touched && validationMessage;
    const showHint = !showValidationMessage && focused && hint;
    const hintId = showHint ? `${id}_hint` : "";
    const errorId = showValidationMessage ? `${id}_error` : "";
    const labelId = label ? `${id}_label` : '';

    const selectedValue = data!.find((item: any) => item[name] === value);

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
        <FieldWrapper style={wrapperStyle}>
            <Label id={labelId} editorRef={editorRef} editorId={id} editorValid={valid} editorDisabled={disabled}>
                {label}
            </Label>
            <ComboBox
                ariaLabelledBy={labelId}
                ariaDescribedBy={`${hintId} ${errorId}`}
                ref={editorRef}
                valid={valid}
                id={id}
                disabled={disabled}
                value={selectedValue}
                data={data}
                onChange={onChangeHandler}
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

export const MemoizedFormComboBox = React.memo(FormComboBox);