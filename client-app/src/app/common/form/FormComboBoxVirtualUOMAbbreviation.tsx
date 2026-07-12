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

// Same shape as FormComboBoxVirtualUOM's UOMItem, plus Abbreviation.
// Description is still carried on the item (used by callers that read
// value.Description, e.g. CertificateItemKendoBulkAddV2's serializeRow) —
// only the visible text (textField) differs from FormComboBoxVirtualUOM.
interface UOMItem {
  UomId: string;
  Description: string;
  Abbreviation: string;
}

// Purpose: Variant of FormComboBoxVirtualUOM that shows the UOM's Abbreviation
// column instead of its (Arabic) Description in the list/input — used only in
// CertificateItemKendoBulkAddV2's compact grid cell where the full description
// doesn't fit well. Hits the same /uoms/getUOMsLov endpoint, so allow-list,
// SortOrder, and search behavior stay identical to every other UOM dropdown.
export const FormComboBoxVirtualUOMAbbreviation = (fieldRenderProps: FieldRenderProps) => {
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

  const textField = "Abbreviation";
  const keyField = "UomId";
  const emptyItem: UOMItem = { [keyField]: "0", [textField]: "loading ...", Description: "" } as UOMItem;
  const pageSize = 10;
  const loadingData: UOMItem[] = Array(pageSize).fill({ ...emptyItem });
  const dataCaching = useRef<UOMItem[]>([]);
  const requestStarted = useRef(false);
  const pendingRequest = useRef<NodeJS.Timeout | null>(null);
  const [data, setData] = useState<UOMItem[]>([]);
  const [total, setTotal] = useState(0);
  const [filter, setFilter] = useState("");
  const skipRef = useRef(0);

  const resetCache = () => {
    dataCaching.current.length = 0;
  };

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
              Abbreviation: element.abbreviation,
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

  useEffect(() => {
    const ac = new AbortController();
    requestData(0, filter);
    return () => {
      resetCache();
      ac.abort();
    };
  }, [filter, requestData]);

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

  const shouldRequestData = useCallback((skip: number) => {
    for (let i = 0; i < pageSize; i++) {
      if (!dataCaching.current[skip + i]) {
        return true;
      }
    }
    return false;
  }, []);

  const getCachedData = useCallback((skip: number) => {
    const data: UOMItem[] = [];
    for (let i = 0; i < pageSize; i++) {
      data.push(dataCaching.current[i + skip] || { ...emptyItem });
    }
    return data;
  }, []);

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
