import React, { useCallback, useEffect, useRef, useState } from "react";
import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import { Label } from "@progress/kendo-react-labels";
import {
    ComboBox,
    ComboBoxChangeEvent,
    ComboBoxFilterChangeEvent,
    ComboBoxPageChangeEvent
} from "@progress/kendo-react-dropdowns";
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";
import agent from "../../api/agent";

// REFACTOR: Define interface for UOM items
// Purpose: Type safety for UOM data
// Context: Matches UomDto
interface UOMItem {
  UomId: string;
  Description: string;
}

// REFACTOR: Define component
// Purpose: Virtualized ComboBox for UOM selection
// Context: New component for UOMId dropdown
export const FormComboBoxVirtualUOM = (fieldRenderProps: FieldRenderProps) => {
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
    onChange,
  } = fieldRenderProps;
  const editorRef = useRef<any>(null);
  const [focused, setFocused] = useState(false);

  const textField = "Description";
  const keyField = "UomId";
  const emptyItem: UOMItem = { [keyField]: "0", [textField]: "loading ..." };
  const pageSize = 10;
  const loadingData: UOMItem[] = Array(pageSize).fill({ ...emptyItem });
  const dataCaching = useRef<UOMItem[]>([]);
  const requestStarted = useRef(false);
  const pendingRequest = useRef<NodeJS.Timeout | null>(null);
  const [data, setData] = useState<UOMItem[]>([]);
  const [total, setTotal] = useState(0);
  const [filter, setFilter] = useState("");
  const skipRef = useRef(0);

  // REFACTOR: Reset cache
  // Purpose: Clear cached data when filter changes
  // Context: Standard virtualization pattern
  const resetCache = () => {
    dataCaching.current.length = 0;
  };

  // REFACTOR: Fetch UOM data
  // Purpose: Load UOMs with pagination and filtering
  // Context: Calls new getUOMsLov endpoint
  const requestData = useCallback(
    (skip: number, filter: string) => {
      if (requestStarted.current) {
        if (pendingRequest.current) clearTimeout(pendingRequest.current);
        pendingRequest.current = setTimeout(() => requestData(skip, filter), 50);
        return;
      }
      requestStarted.current = true;
      const params = new URLSearchParams();
      params.append("skip", skip.toString());
      params.append("pageSize", pageSize.toString());
      if (filter) params.append("searchTerm", filter);

      agent.Uoms.getCertificateUOMsLov(params)
        .then((json) => {
          if (json) {
            const total = json.uomCount;
            const items: UOMItem[] = json.uoms.map((element: any) => ({
              UomId: element.uomId,
              Description: element.description,
            }));
            items.forEach((item, index) => {
              dataCaching.current[index + skip] = item;
            });
            if (skip === skipRef.current) {
              setData(items);
              setTotal(total);
            }
          }
          requestStarted.current = false;
        })
        .catch(() => {
          requestStarted.current = false;
        });
    },
    []
  );

  // REFACTOR: Initialize data
  // Purpose: Fetch initial data on mount or filter change
  // Context: Standard virtualization pattern
  useEffect(() => {
    const ac = new AbortController();
    requestData(0, filter);
    return () => {
      resetCache();
      ac.abort();
    };
  }, [filter, requestData]);

  // REFACTOR: Handle filter change
  // Purpose: Update filter and reset pagination
  // Context: Standard virtualization pattern
  const onFilterChange = useCallback(
    (event: ComboBoxFilterChangeEvent) => {
      const newFilter = event.filter.value;
      resetCache();
      requestData(0, newFilter);
      setData(loadingData);
      skipRef.current = 0;
      setFilter(newFilter);
    },
    [requestData]
  );

  // REFACTOR: Check if data needs fetching
  // Purpose: Determine if new data should be requested
  // Context: Standard virtualization pattern
  const shouldRequestData = useCallback((skip: number) => {
    for (let i = 0; i < pageSize; i++) {
      if (!dataCaching.current[skip + i]) {
        return true;
      }
    }
    return false;
  }, []);

  // REFACTOR: Get cached data
  // Purpose: Retrieve cached data for current page
  // Context: Standard virtualization pattern
  const getCachedData = useCallback((skip: number) => {
    const data: UOMItem[] = [];
    for (let i = 0; i < pageSize; i++) {
      data.push(dataCaching.current[i + skip] || { ...emptyItem });
    }
    return data;
  }, []);

  // REFACTOR: Handle page change
  // Purpose: Load new page of data
  // Context: Standard virtualization pattern
  const pageChange = useCallback(
    (event: ComboBoxPageChangeEvent) => {
      const newSkip = event.page.skip;
      if (shouldRequestData(newSkip)) {
        requestData(newSkip, filter);
      }
      const data = getCachedData(newSkip);
      setData(data);
      skipRef.current = newSkip;
    },
    [getCachedData, requestData, shouldRequestData, filter]
  );

  // REFACTOR: Handle change
  // Purpose: Update form value with selected UOM
  // Context: Standard form field handler
  const onChangeHandler = useCallback(
    (event: ComboBoxChangeEvent) => onChange({ value: event.value || null }),
    [onChange]
  );

  const showValidationMessage = !focused && touched && validationMessage;
  const showHint = !showValidationMessage && focused && hint;
  const hintId = showHint ? `${id}_hint` : "";
  const errorId = showValidationMessage ? `${id}_error` : "";
  const labelId = label ? `${id}_label` : "";

  const handleOnFocus = useCallback(() => {
    onFocus();
    setFocused(true);
  }, [onFocus]);

  const handleOnBlur = useCallback(() => {
    onBlur();
    setFocused(false);
  }, [onBlur]);

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
        dataItemKey={keyField}
        textField={textField}
        value={value}
        data={data}
        onChange={onChangeHandler}
        onFocus={handleOnFocus}
        onBlur={handleOnBlur}
        filterable={true}
        onFilterChange={onFilterChange}
        virtual={{ pageSize, skip: skipRef.current, total }}
        onPageChange={pageChange}
      />
      {showHint && (
        <NotificationGroup style={{ bottom: 0, right: 0, alignItems: "flex-end" }}>
          <Notification type={{ style: "info", icon: true }} closable={false}>
            <span>{hint}</span>
          </Notification>
        </NotificationGroup>
      )}
    </FieldWrapper>
  );
};