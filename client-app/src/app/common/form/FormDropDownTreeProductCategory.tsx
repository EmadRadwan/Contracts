import {FieldRenderProps, FieldWrapper} from "@progress/kendo-react-form";
import {DropDownTree, DropDownTreeProps} from '@progress/kendo-react-dropdowns';
import {expandedState, processTreeData} from './tree-data-operations';
import * as React from 'react';
import {Error, Label} from '@progress/kendo-react-labels';
import {Notification, NotificationGroup,} from "@progress/kendo-react-notification";
import {Item} from './custom-rendering'

export const FormDropDownTreeProductCategory = (fieldRenderProps: FieldRenderProps & DropDownTreeProps) => {
    const {
        validationMessage,
        touched,
        label,
        id,
        valid,
        disabled,
        hint,
        wrapperStyle,
        data,
        onChange,
        name,
        ...others
    } = fieldRenderProps;
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
    const {value, selectField, onFocus, onBlur, expandField, dataItemKey} = others;
    const [expanded, setExpanded] = React.useState(['1', '2', '3', '4']);
    const [filter, setFilter] = React.useState({value: ''});
    const [focused, setFocused] = React.useState(false);

    const treeData = React.useMemo(
        () =>
            processTreeData(
                data,
                {expanded, value, filter},
                {selectField, expandField, dataItemKey, subItemsField: 'items'}
            ),
        [expanded, value, selectField, expandField, dataItemKey, filter]
    );
    const onExpandChange = React.useCallback(
        (event: any) => setExpanded(expandedState(event.item, dataItemKey, expanded)),
        [expanded, dataItemKey]
    );
    const editorRef = React.useRef(null);
    const showValidationMessage = !focused && touched && validationMessage;
    const showHint = !showValidationMessage && focused && hint;
    const hintId = showHint ? `${id}_hint` : '';
    const errorId = showValidationMessage ? `${id}_error` : '';
    const labelId = label ? `${id}_label` : '';
    const [level, setLevel] = React.useState<number[]>([])

    const onFilterChange = (event: any) => setFilter(event.filter);

    const onChangeHandler = React.useCallback(
        (event: any) => {
            setLevel(event.level)
            onChange({value: event.value && event.value[dataItemKey]})
        },
        [onChange, dataItemKey]
    );

    function searchTree(element: any, matchingProductCategoryId: any): any {
        if (!matchingProductCategoryId) return false
        if (element.productCategoryId === matchingProductCategoryId) {
            return element;
        } else if (element.items != null) {
            let i;
            let result = null;
            for (i = 0; result == null && i < element.items.length; i++) {
                result = searchTree(element.items[i], matchingProductCategoryId);
            }
            return result;
        }
        return null;
    }

    let selectedValue = data!.find(item => item[dataItemKey] === value);
    console.log(value)
    if (value && level.length) {
        const element = filter.value && filter.value !== "" ?
            treeData![level.length && level[0]] :
            data![level.length && level[0]]
        if (element) {
            selectedValue = searchTree(element, value)
        }
    } else {
        let element: any;
        data!.forEach(item => {
            if (item[dataItemKey] === value) {
                element = item
            } else if (item.items.length > 0 && !element) {
                element = item.items.find((i: any) => i[dataItemKey] === value)
            }
        })
        if (element) {
            selectedValue = searchTree(element, value)
        }
    }
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
            <Label
                id={labelId}
                editorRef={editorRef}
                editorId={id}
                editorValid={valid}
                editorDisabled={disabled}
            >
                {label}
            </Label>
            <DropDownTree
                ariaLabelledBy={labelId}
                ariaDescribedBy={`${hintId} ${errorId}`}
                //placeholder="Select ..."
                ref={editorRef}
                valid={valid}
                id={id}
                disabled={disabled}
                data={treeData}
                onExpandChange={onExpandChange}
                filterable={true}
                onFilterChange={onFilterChange}
                // (Optional)
                // Set the filter value to the DropDownTree if you need to control the value of the filtering input.
                filter={filter.value}
                {...others}
                value={selectedValue ? selectedValue : null}
                onChange={onChangeHandler}
                onFocus={handleOnFocus}
                onBlur={handleOnBlur}
                item={Item}
            />
            {showHint && <NotificationGroup style={position.bottomRight}>
                <Notification type={{style: 'info', icon: true}} closable={false}>
                    <span>{hint}</span>
                </Notification>
            </NotificationGroup>}
        </FieldWrapper>
    );
};

