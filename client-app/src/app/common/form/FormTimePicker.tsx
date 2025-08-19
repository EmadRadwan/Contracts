import {FieldRenderProps, FieldWrapper} from '@progress/kendo-react-form';
import {Label, Error} from '@progress/kendo-react-labels';
import {TimePicker} from "@progress/kendo-react-dateinputs";

export const FormTimePicker = (fieldRenderProps: FieldRenderProps) => {
    const { validationMessage, touched, label, id, valid, disabled, hint, wrapperStyle, ...others } = fieldRenderProps;

    const showValidationMessage: string | false | null = touched && validationMessage;
    const showHint: boolean = !showValidationMessage && hint
    const hintId: string = showHint ? `${id}_hint` : '';
    const errorId: string = showValidationMessage ? `${id}_error` : '';
    const labelId: string = label ? `${id}_label` : '';

    return (
        <FieldWrapper style={wrapperStyle}>
            <Label id={labelId} editorId={id} editorValid={valid} editorDisabled={disabled} className='k-form-label'>
                {label}
            </Label>
            <div className={'k-form-field-wrap'}>
                <TimePicker
                    ariaLabelledBy={labelId}
                    ariaDescribedBy={`${hintId} ${errorId}`}
                    valid={valid}
                    id={id}
                    disabled={disabled}
                    {...others}
                />
                {
                    showValidationMessage &&
                    <Error id={errorId}>{validationMessage}</Error>
                }
            </div>
        </FieldWrapper>
    );
};
