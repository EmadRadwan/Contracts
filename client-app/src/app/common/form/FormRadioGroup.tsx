import * as React from "react";

import {FieldRenderProps, FieldWrapper} from "@progress/kendo-react-form";
import {Error, Hint, Label} from "@progress/kendo-react-labels";
import {RadioGroup} from "@progress/kendo-react-inputs";

export const FormRadioGroup = (fieldRenderProps: FieldRenderProps) => {
    const {
        validationMessage,
        touched,
        id,
        label,
        valid,
        disabled,
        hint,
        visited,
        modified,
        ...others
    } = fieldRenderProps;
    const editorRef = React.useRef<any>(null);

    const showValidationMessage: string | false | null =
        touched && validationMessage;
    const showHint: boolean = !showValidationMessage && hint;
    const hintId: string = showHint ? `${id}_hint` : "";
    const errorId: string = showValidationMessage ? `${id}_error` : "";
    const labelId: string = label ? `${id}_label` : "";

    return (
        <FieldWrapper>
            <Label
                id={labelId}
                editorRef={editorRef}
                editorId={id}
                editorValid={valid}
                editorDisabled={disabled}
            >
                {label}
            </Label>
            <RadioGroup
                ariaDescribedBy={`${hintId} ${errorId}`}
                ariaLabelledBy={labelId}
                valid={valid}
                disabled={disabled}
                ref={editorRef}
                {...others}
            />
            {showHint && <Hint id={hintId}>{hint}</Hint>}
            {showValidationMessage && <Error id={errorId}>{validationMessage}</Error>}
        </FieldWrapper>
    );
};