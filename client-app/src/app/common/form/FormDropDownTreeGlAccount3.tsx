import React, { useState, useMemo, useCallback, useEffect } from 'react';
import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import { DropDownTree, DropDownTreeProps } from '@progress/kendo-react-dropdowns';
import { expandedState, processTreeData } from './tree-data-operations2';
import { Label } from '@progress/kendo-react-labels';
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";
import { Item } from './custom-rendering2';

export const FormDropDownTreeGlAccount3 = (fieldRenderProps: FieldRenderProps & DropDownTreeProps) => {
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
    value: propValue,
    ...others
  } = fieldRenderProps;

  const {
    selectField,
    expandField,
    dataItemKey = 'glAccountId',
    subItemsField = 'items',
    textField = 'text',           // important for built-in filtering
  } = others;

  const [expanded, setExpanded] = useState<string[]>([]);
  const [filterValue, setFilterValue] = useState<string>('');   // only the string value
  const [focused, setFocused] = useState(false);

  // ==================== Auto-expand Helpers ====================

  const getAllAncestors = useCallback((nodes: any[], targetId: any, path: any[] = []): string[] => {
    if (!nodes || !targetId) return [];

    for (const node of nodes) {
      const currentPath = [...path, String(node[dataItemKey])];

      if (String(node[dataItemKey]) === String(targetId)) {
        return currentPath;
      }

      if (Array.isArray(node[subItemsField]) && node[subItemsField].length > 0) {
        const found = getAllAncestors(node[subItemsField], targetId, currentPath);
        if (found.length > 0) return found;
      }
    }
    return [];
  }, [dataItemKey, subItemsField]);

  const getMatchingIds = useCallback((nodes: any[], filterText: string): any[] => {
    const matches: any[] = [];
    const lowerFilter = filterText.toLowerCase().trim();

    const traverse = (items: any[]) => {
      for (const item of items) {
        const text = (item[textField] || item.accountName || item.text || '').toString().toLowerCase();
        if (lowerFilter && text.includes(lowerFilter)) {
          matches.push(item[dataItemKey]);
        }
        if (Array.isArray(item[subItemsField])) {
          traverse(item[subItemsField]);
        }
      }
    };

    traverse(nodes || []);
    return matches;
  }, [dataItemKey, subItemsField, textField]);

  // ==================== Filter Change Handler ====================

  const handleFilterChange = useCallback((event: any) => {
    const newFilterValue = (event.filter?.value || '').trim();
    setFilterValue(newFilterValue);

    if (!newFilterValue || !data || data.length === 0) {
      setExpanded([]);
      return;
    }

    // Auto-expand all parents of matching items
    const matchingIds = getMatchingIds(data, newFilterValue);
    let allPaths: string[] = [];

    matchingIds.forEach(id => {
      const path = getAllAncestors(data, id);
      allPaths.push(...path);
    });

    setExpanded([...new Set(allPaths)]);
  }, [data, getMatchingIds, getAllAncestors]);

  // Reset when data changes
  useEffect(() => {
    setExpanded([]);
    setFilterValue('');
  }, [data]);

  // Process tree with expanded state only (let DropDownTree handle filtering)
  const treeData = useMemo(() => {
    return processTreeData(
        data || [],
        { expanded, value: propValue },
        {
          selectField,
          expandField,
          dataItemKey,
          subItemsField,
        }
    );
  }, [data, expanded, propValue, selectField, expandField, dataItemKey, subItemsField]);

  const onExpandChange = useCallback((event: any) => {
    setExpanded(expandedState(event.item, dataItemKey, expanded));
  }, [expanded, dataItemKey]);

  const onChangeHandler = useCallback((event: any) => {
    onChange({ value: event.value?.[dataItemKey] ?? null });
  }, [onChange, dataItemKey]);

  // Find selected item for display
  const findItemByKey = (items: any[], key: any): any | null => {
    for (const item of items) {
      if (String(item[dataItemKey]) === String(key)) return item;
      if (item[subItemsField]?.length) {
        const found = findItemByKey(item[subItemsField], key);
        if (found) return found;
      }
    }
    return null;
  };

  const selectedValue = propValue ? findItemByKey(data || [], propValue) : null;

  const popupSettings = {
    width: "500px",
    popupClass: "scrollable-dropdown-tree-popup",
  };

  return (
      <FieldWrapper style={wrapperStyle}>
        <Label id={id} editorValid={valid} editorDisabled={disabled}>
          {label}
        </Label>

        <DropDownTree
            {...others}
            data={treeData}
            value={selectedValue || null}
            onChange={onChangeHandler}
            filterable={true}
            filter={filterValue}                    // pass the string only
            onFilterChange={handleFilterChange}
            onExpandChange={onExpandChange}
            expandedField={expandField}
            textField={textField}
            onFocus={() => setFocused(true)}
            onBlur={() => setFocused(false)}
            popupSettings={popupSettings}
            item={Item}
            disabled={disabled}
        />

        {/* Hint / Validation */}
        {hint && focused && (
            <NotificationGroup>
              <Notification type={{ style: 'info', icon: true }} closable={false}>
                {hint}
              </Notification>
            </NotificationGroup>
        )}
      </FieldWrapper>
  );
};