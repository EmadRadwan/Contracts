import * as React from "react";
import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import { Label } from "@progress/kendo-react-labels";
import { ComboBox, ComboBoxFilterChangeEvent, ComboBoxPageChangeEvent } from "@progress/kendo-react-dropdowns";
import agent from "../../api/agent";

interface SalesRequestItem {
    salesRequestId: string;
    customerName: string;
    apartmentName: string;
    projectName: string;
    label: string; // used as textField in the input box after selection
}

const textField = "label";
const keyField = "salesRequestId";
const pageSize = 20;
const emptyItem: SalesRequestItem = {
    salesRequestId: "0",
    customerName: "...",
    apartmentName: "...",
    projectName: "...",
    label: "loading ...",
};

const colStyle: React.CSSProperties = {
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
    paddingRight: "8px",
};

const headerStyle: React.CSSProperties = {
    display: "grid",
    gridTemplateColumns: "120px 1fr 1fr 1fr",
    padding: "4px 8px",
    borderBottom: "1px solid #ddd",
    fontWeight: "bold",
    fontSize: "12px",
    background: "#f5f5f5",
    color: "#555",
};

const rowStyle: React.CSSProperties = {
    display: "grid",
    gridTemplateColumns: "120px 1fr 1fr 1fr",
    padding: "4px 8px",
    width: "100%",
    fontSize: "13px",
};

const ColumnHeader = () => (
    <div style={headerStyle}>
        <span style={colStyle}>رقم الطلب</span>
        <span style={colStyle}>العميل</span>
        <span style={colStyle}>الشقة</span>
        <span style={colStyle}>المشروع</span>
    </div>
);

const itemRender = (li: React.ReactElement, itemProps: any) => {
    const item: SalesRequestItem = itemProps.dataItem;
    const isLoading = item.salesRequestId === "0";

    const content = isLoading ? (
        <span style={{ padding: "4px 8px", color: "#999" }}>{item.label}</span>
    ) : (
        <div style={rowStyle}>
            <span style={colStyle}>{item.salesRequestId}</span>
            <span style={colStyle}>{item.customerName}</span>
            <span style={colStyle}>{item.apartmentName}</span>
            <span style={colStyle}>{item.projectName}</span>
        </div>
    );

    return React.cloneElement(li, li.props, content);
};

export const FormComboBoxVirtualSalesRequest = (fieldRenderProps: FieldRenderProps) => {
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
        onSalesRequestIdChange,
    } = fieldRenderProps;

    const editorRef = React.useRef(null);
    const [focused, setFocused] = React.useState(false);
    const [data, setData] = React.useState<SalesRequestItem[]>([]);
    const [total, setTotal] = React.useState(0);
    const [filter, setFilter] = React.useState("");

    const skipRef = React.useRef(0);
    const dataCaching = React.useRef<SalesRequestItem[]>([]);
    const pendingRequest = React.useRef<ReturnType<typeof setTimeout>>();
    const requestStarted = React.useRef(false);

    const showValidationMessage = !focused && touched && validationMessage;
    const showHint = !showValidationMessage && focused && hint;
    const hintId = showHint ? `${id}_hint` : "";
    const errorId = showValidationMessage ? `${id}_error` : "";
    const labelId = label ? `${id}_label` : "";

    const handleOnFocus = React.useCallback(() => { onFocus(); setFocused(true); }, [onFocus]);
    const handleOnBlur = React.useCallback(() => { onBlur(); setFocused(false); }, [onBlur]);

    const resetCache = () => { dataCaching.current.length = 0; };

    const requestData = React.useCallback((skip: number, currentFilter: string) => {
        if (requestStarted.current) {
            clearTimeout(pendingRequest.current);
            pendingRequest.current = setTimeout(() => requestData(skip, currentFilter), 50);
            return;
        }
        requestStarted.current = true;

        const params = new URLSearchParams();
        params.append("skip", skip.toString());
        params.append("pageSize", pageSize.toString());
        if (currentFilter) params.append("searchTerm", currentFilter);

        agent.SalesRequests.getApprovedLov(params)
            .then((envelope: any) => {
                const totalCount: number = envelope?.totalCount ?? 0;
                const items: SalesRequestItem[] = (envelope?.items ?? []).map((x: any) => ({
                    salesRequestId: x.salesRequestId,
                    customerName: x.customerName ?? "",
                    apartmentName: x.apartmentName ?? "",
                    projectName: x.projectName ?? "",
                    label: [x.customerName, x.apartmentName].filter(Boolean).join(" - "),
                }));

                items.forEach((item, i) => { dataCaching.current[i + skip] = item; });

                if (skip === skipRef.current) {
                    setData(items);
                    setTotal(totalCount);
                }
                requestStarted.current = false;
            })
            .catch(() => { requestStarted.current = false; });
    }, []);

    React.useEffect(() => {
        requestData(0, filter);
        return () => { resetCache(); };
    }, [filter, requestData]);

    const onFilterChange = React.useCallback((event: ComboBoxFilterChangeEvent) => {
        const f = event.filter.value;
        resetCache();
        requestData(0, f);
        setData(Array(pageSize).fill({ ...emptyItem }));
        skipRef.current = 0;
        setFilter(f);
    }, [requestData]);

    const shouldRequestData = React.useCallback((skip: number) => {
        for (let i = 0; i < pageSize; i++) {
            if (!dataCaching.current[skip + i]) return true;
        }
        return false;
    }, []);

    const getCachedData = React.useCallback((skip: number) => {
        return Array.from({ length: pageSize }, (_, i) => dataCaching.current[i + skip] ?? { ...emptyItem });
    }, []);

    const pageChange = React.useCallback((event: ComboBoxPageChangeEvent) => {
        const newSkip = event.page.skip;
        if (shouldRequestData(newSkip)) requestData(newSkip, filter);
        setData(getCachedData(newSkip));
        skipRef.current = newSkip;
    }, [getCachedData, requestData, shouldRequestData, filter]);

    const onChangeHandler = React.useCallback((event: any) => {
        onChange({ value: event.value ?? null });
        onSalesRequestIdChange?.(event.value?.salesRequestId ?? null);
    }, [onChange, onSalesRequestIdChange]);

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
                itemRender={itemRender}
                header={<ColumnHeader />}
                style={{ width: "100%" }}
                virtual={React.useMemo(() => ({
                    pageSize,
                    skip: skipRef.current,
                    total,
                }), [total])}
                onPageChange={pageChange}
            />
        </FieldWrapper>
    );
};
