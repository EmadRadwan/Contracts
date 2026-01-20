import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import { DropDownTree, DropDownTreeProps } from '@progress/kendo-react-dropdowns';
import { expandedState, processTreeData } from './tree-data-operations2';
import * as React from 'react';
import { Label } from '@progress/kendo-react-labels';
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";
import { Item } from './custom-rendering2';

export const FormDropDownTreeGlAccount2 = (fieldRenderProps: FieldRenderProps & DropDownTreeProps) => {
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
    topLeft: { top: 0, left: 0, alignItems: "flex-start" },
    topCenter: { top: 0, left: "50%", transform: "translateX(-50%)" },
    topRight: { top: 0, right: 0, alignItems: "flex-end" },
    bottomLeft: { bottom: 0, left: 0, alignItems: "flex-start" },
    bottomCenter: { bottom: 0, left: "50%", transform: "translateX(-50%)" },
    bottomRight: { bottom: 0, right: 0, alignItems: "flex-end" },
  };
  const { value, selectField, onFocus, onBlur, expandField, dataItemKey } = others;
  const [expanded, setExpanded] = React.useState(['1', '2', '3', '4']);
  const [filter, setFilter] = React.useState({ value: '' });
  const [focused, setFocused] = React.useState(false);
  const [level, setLevel] = React.useState<number[]>([]);
  const dropDownTreeRef = React.useRef(null);

  // Reset expanded and level state when data changes
  React.useEffect(() => {
    setExpanded(['1', '2', '3', '4']); // Reset to initial expanded state
    setLevel([]); // Clear level to avoid stale references
  }, [data]);

  const treeData = React.useMemo(
      () =>
          processTreeData(
              data || [], // Fallback to empty array if data is undefined
              { expanded, value, filter },
              { selectField, expandField, dataItemKey, subItemsField: 'items' }
          ),
      [expanded, value, filter, selectField, expandField, dataItemKey, data]
  );

  const onExpandChange = React.useCallback(
      (event) => setExpanded(expandedState(event.item, dataItemKey, expanded)),
      [expanded, dataItemKey]
  );

  const editorRef = React.useRef(null);
  const showValidationMessage = !focused && touched && validationMessage;
  const showHint = !showValidationMessage && focused && hint;
  const hintId = showHint ? `${id}_hint` : '';
  const errorId = showValidationMessage ? `${id}_error` : '';
  const labelId = label ? `${id}_label` : '';

  const onFilterChange = (event: any) => setFilter(event.filter);



    const onChangeHandler = React.useCallback(
        (event) => {
            const selectedItem = event.value;
            setLevel(event.level || []);
            onChange({ value: selectedItem?.[dataItemKey] ?? null });
        },
        [onChange, dataItemKey]
    );


  function searchTree(element: any, matchingGlAccountId: any): any {
    if (!matchingGlAccountId) return null;
    if (element.glAccountId === matchingGlAccountId) {
      return element;
    } else if (element.items != null) {
      for (let i = 0; i < element.items.length; i++) {
        const result = searchTree(element.items[i], matchingGlAccountId);
        if (result) return result;
      }
    }
    return null;
  }

  const findItemByKey = (items: any[], key: string | number): any | null => {
    for (const item of items) {
      if (item[dataItemKey] === key) {
        return item;
      }
      if (item.items?.length) {
        const found = findItemByKey(item.items, key);
        if (found) return found;
      }
    }
    return null;
  };

  let selectedValue = value ? findItemByKey(data || [], value) : null;

  if (value && level.length) {
    const element = filter.value && filter.value !== ""
        ? treeData?.[level[0]]
        : data?.[level[0]];
    if (element) {
      selectedValue = searchTree(element, value);
    }
  } else if (value) {
    let element: any;
    data?.forEach(item => {
      if (item[dataItemKey] === value) {
        element = item;
      } else if (item.items?.length > 0 && !element) {
        element = item.items.find((i: any) => i[dataItemKey] === value);
      }
    });
    if (element) {
      selectedValue = searchTree(element, value);
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

  const popupSettings = {
    width: "400px",
    // Remove or comment out the fixed small height — it was limiting scrolling
    // height: "200px",

    // ─── This is the important change ────────────────────────────────
    popupClass: "scrollable-dropdown-tree-popup",
    // Optional: you can also add adaptive: true for mobile-friendly full-screen popup
    // adaptive: true,
  };

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
            ref={dropDownTreeRef}
            valid={valid}
            id={id}
            disabled={disabled}
            data={treeData}
            onExpandChange={onExpandChange}
            //onItemClick={onItemClick} // Custom handler to manage parent node clicks
            filterable={true}
            onFilterChange={onFilterChange}
            filter={filter.value}
            {...others}
            value={selectedValue || null}
            onChange={onChangeHandler}
            onFocus={handleOnFocus}
            onBlur={handleOnBlur}
            popupSettings={popupSettings}
            item={Item} // Use the custom Item component
        />
        {showHint && (
            <NotificationGroup style={position.bottomRight}>
              <Notification type={{ style: 'info', icon: true }} closable={false}>
                <span>{hint}</span>
              </Notification>
            </NotificationGroup>
        )}
        {/* {showValidationMessage && <Error id={errorId}>{validationMessage}</Error>} */}
      </FieldWrapper>
  );
};