import * as React from "react";

import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import { Label } from "@progress/kendo-react-labels";
import { ComboBox, ComboBoxFilterChangeEvent, ComboBoxPageChangeEvent } from "@progress/kendo-react-dropdowns";
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";
import agent from "../../api/agent";

export const FormComboBoxVirtualPartyExternalSalesManager = (fieldRenderProps: FieldRenderProps) => {
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
    const editorRef = React.useRef(null);
    const [focused, setFocused] = React.useState(false);

    const position = {
        bottomRight: { bottom: 0, right: 0, alignItems: "flex-end" },
    };

    const showValidationMessage = !focused && touched && validationMessage;
    const showHint = !showValidationMessage && focused && hint;
    const hintId = showHint ? `${id}_hint` : "";
    const errorId = showValidationMessage ? `${id}_error` : "";
    const labelId = label ? `${id}_label` : "";

    const handleOnFocus = React.useCallback(() => { onFocus(); setFocused(true); }, [onFocus]);
    const handleOnBlur = React.useCallback(() => { onBlur(); setFocused(false); }, [onBlur]);

    interface Item {
        fromPartyId: string;
        fromPartyName: string;
    }

    const textField = "fromPartyName";
    const keyField = "fromPartyId";
    const emptyItem: Item = { [textField]: "loading ...", fromPartyId: "0" };
    const pageSize = 10;

    const loadingData: Item[] = [];
    while (loadingData.length < pageSize) {
        loadingData.push({ ...emptyItem });
    }

    const dataCaching = React.useRef<any>([]);
    const pendingRequest = React.useRef<any>();
    const requestStarted = React.useRef(false);

    const [data, setData] = React.useState<Item[]>([]);
    const [total, setTotal] = React.useState(0);
    const [filter, setFilter] = React.useState("");

    const skipRef = React.useRef(0);

    const resetCach = () => { dataCaching.current.length = 0; };

    const requestData = React.useCallback((skip: number, filter: string) => {
        if (requestStarted.current) {
            clearTimeout(pendingRequest.current);
            pendingRequest.current = setTimeout(() => { requestData(skip, filter); }, 50);
            return;
        }
        requestStarted.current = true;
        const params = new URLSearchParams();
        params.append("skip", skip.toString());
        params.append("pageSize", pageSize.toString());
        if (filter) params.append("searchTerm", filter);
        agent.Parties.getPartiesExternalSalesManagersLov(params)
            .then((json: any) => {
                if (json) {
                    const items: Item[] = [];
                    json.parties.forEach((element: any, index: any) => {
                        const item: Item = {
                            [keyField]: element.fromPartyId,
                            [textField]: element.fromPartyName,
                        };
                        items.push(item);
                        dataCaching.current[index + skip] = item;
                    });
                    if (skip === skipRef.current) {
                        setData(items);
                        setTotal(json.partyCount);
                    }
                }
                requestStarted.current = false;
            });
    }, []);

    React.useEffect(() => {
        const ac = new AbortController();
        requestData(0, filter);
        return () => { resetCach(); ac.abort(); };
    }, [filter, requestData]);

    const onFilterChange = React.useCallback((event: ComboBoxFilterChangeEvent) => {
        const f = event.filter.value;
        resetCach();
        requestData(0, f);
        setData(loadingData);
        skipRef.current = 0;
        setFilter(f);
    }, []);

    const shouldRequestData = React.useCallback((skip: number) => {
        for (let i = 0; i < pageSize; i++) {
            if (!dataCaching.current[skip + i]) return true;
        }
        return false;
    }, []);

    const getCachedData = React.useCallback((skip: number) => {
        const d: Array<any> = [];
        for (let i = 0; i < pageSize; i++) {
            d.push(dataCaching.current[i + skip] || { ...emptyItem });
        }
        return d;
    }, []);

    const pageChange = React.useCallback((event: ComboBoxPageChangeEvent) => {
        const newSkip = event.page.skip;
        if (shouldRequestData(newSkip)) requestData(newSkip, filter);
        setData(getCachedData(newSkip));
        skipRef.current = newSkip;
    }, [getCachedData, requestData, shouldRequestData, filter]);

    const onChangeHandler = React.useCallback(
        (event: any) => { onChange({ value: event.value && event.value }); },
        [onChange]
    );

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
                virtual={React.useMemo(() => ({
                    pageSize,
                    skip: skipRef.current,
                    total,
                }), [total])}
                onPageChange={pageChange}
            />
            {showHint && (
                <NotificationGroup style={position.bottomRight}>
                    <Notification type={{ style: "info", icon: true }} closable={false}>
                        <span>{hint}</span>
                    </Notification>
                </NotificationGroup>
            )}
        </FieldWrapper>
    );
};
