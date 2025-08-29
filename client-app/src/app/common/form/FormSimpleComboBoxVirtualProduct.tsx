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
import { useAppSelector } from "../../store/configureStore";

// REFACTOR: Define interface for dropdown items
// Purpose: Type safety for product data
// Context: Unchanged, matches ProductLovDto
interface ProductItem {
  ProductId: string;
  ProductName: string;
}

// REFACTOR: Define component
// Purpose: Single-column virtualized ComboBox for product selection with certificate type filtering
// Context: Added currentCertificateType to API call
export const FormSimpleComboBoxVirtualProduct = (fieldRenderProps: FieldRenderProps) => {
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
  const { currentCertificateType } = useAppSelector((state) => state.certificateUi);
  
  const textField = "ProductName";
  const keyField = "ProductId";
  const emptyItem: ProductItem = { [keyField]: "0", [textField]: "loading ..." };
  const pageSize = 10;
  const loadingData: ProductItem[] = Array(pageSize).fill({ ...emptyItem });
  const dataCaching = useRef<ProductItem[]>([]);
  const requestStarted = useRef(false);
  const pendingRequest = useRef<NodeJS.Timeout | null>(null);
  const [data, setData] = useState<ProductItem[]>([]);
  const [total, setTotal] = useState(0);
  const [filter, setFilter] = useState("");
  const skipRef = useRef(0);

  // REFACTOR: Reset cache
  // Purpose: Clear cached data when filter changes
  // Context: Unchanged
  const resetCache = () => {
    dataCaching.current.length = 0;
  };

  // REFACTOR: Fetch product data
  // Purpose: Load products with pagination, filtering, and certificate type
  // Context: Added certificateType parameter to API call
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
      if (currentCertificateType) params.append("certificateType", currentCertificateType);

      agent.Products.getSimpleProductsLov(params)
        .then((json) => {
          if (json) {
            const total = json.productCount;
            const items: ProductItem[] = json.products.map((element: any) => ({
              ProductId: element.productId,
              ProductName: element.productName,
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
    [currentCertificateType]
  );

  // REFACTOR: Initialize data
  // Purpose: Fetch initial data on mount or filter/certificate type change
  // Context: Added currentCertificateType dependency
  useEffect(() => {
    const ac = new AbortController();
    requestData(0, filter);
    return () => {
      resetCache();
      ac.abort();
    };
  }, [filter, requestData, currentCertificateType]);

  // REFACTOR: Handle filter change
  // Purpose: Update filter and reset pagination
  // Context: Unchanged
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
  // Context: Unchanged
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
  // Context: Unchanged
  const getCachedData = useCallback((skip: number) => {
    const data: ProductItem[] = [];
    for (let i = 0; i < pageSize; i++) {
      data.push(dataCaching.current[i + skip] || { ...emptyItem });
    }
    return data;
  }, []);

  // REFACTOR: Handle page change
  // Purpose: Load new page of data
  // Context: Unchanged
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
  // Purpose: Update form value with selected product
  // Context: Unchanged
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
}