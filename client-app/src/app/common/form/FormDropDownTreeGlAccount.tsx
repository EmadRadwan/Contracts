import * as React from "react";
import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import {
  DropDownTree,
  DropDownTreeProps,
} from "@progress/kendo-react-dropdowns";
import { Label } from "@progress/kendo-react-labels";
import {
  Notification,
  NotificationGroup,
} from "@progress/kendo-react-notification";
import { filterBy } from "@progress/kendo-react-data-tools";
import { extendDataItem, mapTree } from "@progress/kendo-react-common";

/** Helper Functions **/

// Function to process tree data for the DropDownTree component
const processTreeData = (data, state, fields) => {
  const { selectField, expandField, dataItemKey, subItemsField } = fields;
  const { expanded, value, filter } = state;
  const filtering = Boolean(filter && filter.value);

  return mapTree(
      filtering ? filterBy(data, [filter], subItemsField) : data,
      subItemsField,
      (item) => {
        const props = {
          [expandField]: expanded.includes(item[dataItemKey]),
          [selectField]: value && item[dataItemKey] === value[dataItemKey],
        };
        return filtering
            ? extendDataItem(item, subItemsField, props)
            : { ...item, ...props };
      }
  );
};

// Function to manage the expanded state of the tree nodes
const expandedState = (item, dataItemKey, expanded) => {
  const nextExpanded = expanded.slice();
  const itemKey = item[dataItemKey];
  const index = expanded.indexOf(itemKey);
  index === -1 ? nextExpanded.push(itemKey) : nextExpanded.splice(index, 1);
  return nextExpanded;
};

/** Main Component **/

export const FormDropDownTreeGlAccount = (
    fieldRenderProps: FieldRenderProps & DropDownTreeProps
) => {
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
    bottomRight: {
      bottom: 0,
      right: 0,
      alignItems: "flex-end",
    },
  };

  const { value, selectField, onFocus, onBlur, expandField, dataItemKey } =
      others;

  const [expanded, setExpanded] = React.useState<string[]>([]);
  const [filter, setFilter] = React.useState({ value: "" });
  const [focused, setFocused] = React.useState(false);
  const [level, setLevel] = React.useState<number[]>([]);
  const [opened, setOpened] = React.useState<boolean>(false); // Manage the open state manually

  const editorRef = React.useRef(null);
  const showValidationMessage = !focused && touched && validationMessage;
  const showHint = !showValidationMessage && focused && hint;
  const hintId = showHint ? `${id}_hint` : "";
  const errorId = showValidationMessage ? `${id}_error` : "";
  const labelId = label ? `${id}_label` : "";

  // Memoized tree data for performance optimization
  const treeData = React.useMemo(
      () =>
          processTreeData(
              data,
              { expanded, value, filter },
              { selectField, expandField, dataItemKey, subItemsField: "items" }
          ),
      [expanded, value, selectField, expandField, dataItemKey, filter, data]
  );

  // Handle node expand/collapse
  const onExpandChange = React.useCallback(
      (event) => {
        setExpanded((prev) => {
          return expandedState(event.item, dataItemKey, prev);
        });
      },
      [dataItemKey]
  );

  // Handle filter changes
  const onFilterChange = (event) => setFilter(event.filter);

  // Function to find the selected item in the tree data
  const findItemById = (data, id) => {
    let result = null;
    for (let item of data) {
      if (item[dataItemKey] === id) {
        result = item;
        break;
      } else if (item.items) {
        result = findItemById(item.items, id);
        if (result) break;
      }
    }
    return result;
  };

  // Compute the selected value
  const selectedValue = findItemById(data, value);

  // Handle value changes
  const onChangeHandler = React.useCallback(
      (event) => {
        const selectedItem = event.value;
        const isLeaf = selectedItem?.isLeaf;

        if (isLeaf) {
          // Leaf node selected: update value and close dropdown
          setLevel(event.level);
          onChange({
            target: {
              name: name,
              value: selectedItem && selectedItem[dataItemKey],
            },
          });
          setOpened(false); // Close the dropdown when a leaf node is selected
        } else {
          // Non-leaf node: expand/collapse and keep dropdown open
          onExpandChange({
            item: selectedItem,
          });
          event.syntheticEvent.preventDefault(); // Prevent selection
          setOpened(true); // Keep the dropdown open
        }
      },
      [onChange, dataItemKey, onExpandChange, name]
  );

  // Handle focus event
  const handleOnFocus = React.useCallback(() => {
    if (onFocus) {
      onFocus();
    }
    setFocused(true);
    setOpened(true); // Open the dropdown on focus
  }, [onFocus]);

  // Handle blur event
  const handleOnBlur = React.useCallback(() => {
    if (onBlur) {
      onBlur();
    }
    setFocused(false);
    setOpened(false); // Close the dropdown on blur
  }, [onBlur]);

  // Handle click event on the input field to toggle dropdown
  const handleInputClick = React.useCallback(() => {
    setOpened((prevOpened) => !prevOpened); // Toggle the dropdown open/close state
  }, []);

  // Optional: Item render function to style parent nodes
  const itemRender = (li, itemProps) => {
    const { item } = itemProps;
    const isLeaf = item?.isLeaf;
    const className = `k-treeview-item${isLeaf ? "" : " k-parent-node"}`;
    return React.cloneElement(li, { className });
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
        <div onClick={handleInputClick}>
          <DropDownTree
              ariaLabelledBy={labelId}
              ariaDescribedBy={`${hintId} ${errorId}`}
              ref={editorRef}
              valid={valid}
              id={id}
              disabled={disabled}
              data={treeData}
              onExpandChange={onExpandChange}
              filterable={true}
              onFilterChange={onFilterChange}
              filter={filter.value}
              {...others}
              value={selectedValue ? selectedValue : null}
              onChange={onChangeHandler}
              onFocus={handleOnFocus}
              onBlur={handleOnBlur}
              itemRender={itemRender} // Use itemRender to style items
              opened={opened} // Control the open state of the dropdown
          />
        </div>
        {showHint && (
            <NotificationGroup style={position.bottomRight}>
              <Notification type={{ style: "info", icon: true }} closable={false}>
                <span>{hint}</span>
              </Notification>
            </NotificationGroup>
        )}
        {/* {showValidationMessage && <Error id={errorId}>{validationMessage}</Error>} */}
      </FieldWrapper>
  );
};
