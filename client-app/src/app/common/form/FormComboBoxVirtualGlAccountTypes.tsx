import * as React from "react";
import {
    FieldRenderProps,
    FieldWrapper,
} from "@progress/kendo-react-form";
import { Label } from "@progress/kendo-react-labels";
import {
    ComboBox,
    ComboBoxFilterChangeEvent,
    ComboBoxPageChangeEvent,
} from "@progress/kendo-react-dropdowns";
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";
import agent from "../../api/agent";

interface GlAccountTypeItem {
    glAccountTypeId: string;
    description: string;
}

export const FormComboBoxVirtualGlAccountTypes = (fieldRenderProps: FieldRenderProps) => {
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

    const editorRef = React.useRef<any>(13); // any because ComboBox ref type
    const [focused, setFocused] = React.useState(false);

    // Notification positions (same as original)
    const position = {
        topLeft: { top: 0, left: 0, alignItems: "flex-start" },
        topCenter: { top: 0, left: "50%", transform: "translateX(-50%)" },
        topRight: { top: 0, right: 0, alignItems: "flex-end" },
        bottomLeft: { bottom: 0, left: 0, alignItems: "flex-start" },
        bottomCenter: { bottom: 0, left: "50%", transform: "translateX(-50%)" },
        bottomRight: { bottom: 0, right: 0, alignItems: "flex-end" },
    };

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

    // REFACTOR: Extracted constants for clarity and consistency with original component
    const textField = "description";
    const keyField = "glAccountTypeId";
    const emptyItem: GlAccountTypeItem = { [textField]: "جاري التحميل ...", glAccountTypeId: "" };
    const pageSize = 20; // Increased since GlAccountTypes is small

    const loadingData: GlAccountTypeItem[] = [];
    while (loadingData.length < pageSize) {
        loadingData.push({ ...emptyItem });
    }

    // REFACTOR: Simplified caching – since data is small and static-like, we cache all at once
    const dataCaching = React.useRef<GlAccountTypeItem[]>([]);
    const pendingRequest = React.useRef<any>();
    const requestStarted = React.useRef(false);

    const [data, setData] = React.useState<GlAccountTypeItem[]>([]);
    const [total, setTotal] = React.useState(0);
    const [filter, setFilter] = React.useState("");

    const skipRef = React.useRef(0);

    const resetCache = () => {
        dataCaching.current = [];
    };

    const requestData = React.useCallback((skip: number, filter: string) => {
        if (requestStarted.current) {
            clearTimeout(pendingRequest.current);
            pendingRequest.current = setTimeout(() => requestData(skip, filter), 50);
            return;
        }

        requestStarted.current = true;

        const params = new URLSearchParams();
        params.append("skip", skip.toString());
        params.append("pageSize", pageSize.toString());
        if (filter) params.append("searchTerm", filter);

        agent.GlAccountTypes.getAll(params)
            .then((json) => {
                if (json) {
                    const items: GlAccountTypeItem[] = json.glAccountTypes.map((x: any) => ({
                        glAccountTypeId: x.glAccountTypeId,
                        description: x.description,
                    }));

                    // Cache all received items
                    items.forEach((item, idx) => {
                        dataCaching.current[skip + idx] = item;
                    });

                    if (skip === skipRef.current) {
                        setData(items);
                        setTotal(json.totalCount);
                    }
                }
                requestStarted.current = false;
            })
            .catch(() => {
                requestStarted.current = false;
            });
    }, []);

    // Initial load and filter change
    React.useEffect(() => {
        resetCache();
        requestData(0, filter);
    }, [filter, requestData]);

    const onFilterChange = React.useCallback((event: ComboBoxFilterChangeEvent) => {
        const newFilter = event.filter.value;
        skipRef.current = 0;
        setFilter(newFilter);
        setData(loadingData);
    }, []);

    const shouldRequestData = React.useCallback((skip: number) => {
        for (let i = 0; i < pageSize; i++) {
            if (!dataCaching.current[skip + i]) return true;
        }
        return false;
    }, []);

    const getCachedData = React.useCallback((skip: number) => {
        const result: GlAccountTypeItem[] = [];
        for (let i = 0; i < pageSize; i++) {
            result.push(dataCaching.current[skip + i] || { ...emptyItem });
        }
        return result;
    }, []);

    const pageChange = React.useCallback(
        (event: ComboBoxPageChangeEvent) => {
            const newSkip = event.page.skip;

            if (shouldRequestData(newSkip)) {
                requestData(newSkip, filter);
            }

            setData(getCachedData(newSkip));
            skipRef.current = newSkip;
        },
        [filter, getCachedData, requestData, shouldRequestData]
    );

    const onChangeHandler = React.useCallback(
        (event: any) => {
            onChange({ value: event.value });
        },
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
                virtual={{
                    pageSize,
                    skip: skipRef.current,
                    total,
                }}
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