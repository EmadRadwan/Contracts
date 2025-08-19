import * as React from "react";

import {FieldRenderProps, FieldWrapper} from "@progress/kendo-react-form";
import {Checkbox} from "@progress/kendo-react-inputs"
import {Error, Hint,} from "@progress/kendo-react-labels";

const FormCheckbox = (fieldRenderProps: FieldRenderProps) => {
    const { validationMessage, touched, id, valid, disabled, hint, optional, label, ...others } = fieldRenderProps;

    const showValidationMessage = touched && validationMessage;
    const showHint = !showValidationMessage && hint;
    const fullLabel = <span>{label}{optional ? <span className={'k-label-optional'}>(Optional)</span> : ''}</span>;
    const hindId = showHint ? `${id}_hint` : '';
    const errorId = showValidationMessage ? `${id}_error` : '';

    return (
        <FieldWrapper>
            <Checkbox
                ariaDescribedBy={`${hindId} ${errorId}`}
                label={fullLabel}
                valid={valid}
                id={id}
                disabled={disabled}
                {...others}
            />
            {
                showHint &&
                <Hint id={hindId}>{hint}</Hint>
            }
            {
                showValidationMessage &&
                <Error id={errorId}>{validationMessage}</Error>
            }
        </FieldWrapper>
    );
};
export const MemoizedFormCheckBox = React.memo(FormCheckbox);
