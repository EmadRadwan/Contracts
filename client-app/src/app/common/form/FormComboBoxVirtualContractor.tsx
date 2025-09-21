import * as React from "react";
import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import { Label } from "@progress/kendo-react-labels";
import { MultiColumnComboBox, ComboBoxFilterChangeEvent, ComboBoxPageChangeEvent, ComboBoxChangeEvent } from "@progress/kendo-react-dropdowns";
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";
import agent from "../../api/agent";
import { useAppDispatch } from "../../store/configureStore";
// REFACTOR: Update import for contractor-specific action
// Purpose: Use a contractor-specific action to set contractor ID
// Context: Replaces setSupplierId to align with contractor context

interface Item {
    fromPartyId: string;
    fromPartyName: string;
}

export const FormComboBoxVirtualContractor = (fieldRenderProps: FieldRenderProps) => {
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
    const editorRef = React.useRef<any>(null);
    const [focused, setFocused] = React.useState(false);
    const dispatch = useAppDispatch();
    const keyField = "fromPartyId";
    const textField = "fromPartyName";
    const emptyItem: Item = { fromPartyId: "0", fromPartyName: "loading ..." };
    const pageSize = 10;

    // REFACTOR: Initialize loading data
    // Purpose: Use placeholder data to show loading state
    // Context: Prevents empty dropdown on initial render
    const loadingData: Item[] = [];
    while (loadingData.length < pageSize) {
        loadingData.push({ ...emptyItem });
    }

    const columns = [
        { field: "fromPartyId", header: "Party ID", width: "100px" },
        { field: "fromPartyName", header: "Party Name", width: "200px" },
    ];

    const position = {
        bottomRight: { bottom: 0, right: 0, alignItems: "flex-end" },
    };

    // REFACTOR: Initialize data state with loadingData
    // Purpose: Display loading placeholders on initial render
    // Context: Fixes empty dropdown issue by showing "loading ..." until API responds
    const [data, setData] = React.useState<Item[]>(loadingData);
    const [total, setTotal] = React.useState(0);
    const [filter, setFilter] = React.useState("");
    const dataCaching = React.useRef<Item[]>([]);
    const requestStarted = React.useRef(false);
    const pendingRequest = React.useRef<NodeJS.Timeout | null>(null);
    const skipRef = React.useRef(0);

    const resetCache = () => {
        dataCaching.current.length = 0;
    };

    const requestData = React.useCallback(
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

            // REFACTOR: Update API call to getContractorsLov
            // Purpose: Fetch contractors instead of suppliers
            // Context: Matches backend endpoint for contractors
            agent.Parties.getContractorsLov(params)
                .then((json) => {
                    // REFACTOR: Add debug logging
                    // Purpose: Verify API response content
                    // Context: Helps diagnose if data is empty or malformed
                    console.log("API Response:", json);
                    if (json && json.parties) {
                        const total = json.partyCount || 0;
                        const items: Item[] = json.parties.map((element: any, index: number) => {
                            const item: Item = {
                                fromPartyId: element.fromPartyId,
                                fromPartyName: element.fromPartyName,
                            };
                            dataCaching.current[index + skip] = item;
                            return item;
                        });
                        if (skip === skipRef.current) {
                            setData(items.length > 0 ? items : loadingData);
                            setTotal(total);
                        }
                    } else {
                        setData(loadingData);
                        setTotal(0);
                    }
                    requestStarted.current = false;
                })
                .catch((error) => {
                    // REFACTOR: Handle API errors
                    // Purpose: Ensure loading state persists on failure
                    // Context: Prevents empty dropdown on error
                    console.error("API Error:", error);
                    setData(loadingData);
                    setTotal(0);
                    requestStarted.current = false;
                });
        },
        []
    );

    React.useEffect(() => {
        const ac = new AbortController();
        requestData(0, filter);
        return () => {
            resetCache();
            ac.abort();
        };
    }, [filter, requestData]);

    const onFilterChange = React.useCallback(
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

    const shouldRequestData = React.useCallback((skip: number) => {
        for (let i = 0; i < pageSize; i++) {
            if (!dataCaching.current[skip + i]) {
                return true;
            }
        }
        return false;
    }, []);

    const getCachedData = React.useCallback((skip: number) => {
        const data: Item[] = [];
        for (let i = 0; i < pageSize; i++) {
            data.push(dataCaching.current[i + skip] || { ...emptyItem });
        }
        return data;
    }, []);

    const pageChange = React.useCallback(
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

    const onChangeHandler = React.useCallback(
        (event: ComboBoxChangeEvent) => {
            onChange({ value: event.value || null });
        },
        [onChange, dispatch]
    );

    const showValidationMessage = !focused && touched && validationMessage;
    const showHint = !showValidationMessage && focused && hint;
    const hintId = showHint ? `${id}_hint` : "";
    const errorId = showValidationMessage ? `${id}_error` : "";
    const labelId = label ? `${id}_label` : "";

    const handleOnFocus = React.useCallback(() => {
        onFocus();
        setFocused(true);
    }, [onFocus]);

    const handleOnBlur = React.useCallback(() => {
        onBlur();
        setFocused(false);
    }, [onBlur]);

    return (
        <FieldWrapper style={wrapperStyle}>
            <Label id={labelId} editorRef={editorRef} editorId={id} editorValid={valid} editorDisabled={disabled}>
                {label}
            </Label>
            <MultiColumnComboBox
                ariaLabelledBy={labelId}
                ariaDescribedBy={`${hintId} ${errorId}`}
                ref={editorRef}
                valid={valid}
                id={id}
                disabled={disabled}
                dataItemKey={keyField}
                textField={textField}
                columns={columns}
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
                <NotificationGroup style={position.bottomRight}>
                    <Notification type={{ style: "info", icon: true }} closable={false}>
                        <span>{hint}</span>
                    </Notification>
                </NotificationGroup>
            )}
        </FieldWrapper>
    );
};