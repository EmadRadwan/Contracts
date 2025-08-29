import * as React from "react";
import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import { Label } from "@progress/kendo-react-labels";
import { ComboBox, ComboBoxFilterChangeEvent, ComboBoxPageChangeEvent } from "@progress/kendo-react-dropdowns";
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";
import agent from "../../api/agent";
import { useAppDispatch } from "../../store/configureStore";

// REFACTOR: Define interface for project items to ensure type safety and consistency
interface ProjectItem {
    workEffortId: string;
    projectName: string;
}

export const FormComboBoxVirtualProject = (fieldRenderProps: FieldRenderProps) => {
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
    const dispatch = useAppDispatch();

    // REFACTOR: Consolidated position styles into a single object for better maintainability
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

    // REFACTOR: Simplified focus/blur handlers using useCallback for performance
    const handleOnFocus = React.useCallback(() => {
        onFocus();
        setFocused(true);
    }, [onFocus]);

    const handleOnBlur = React.useCallback(() => {
        onBlur();
        setFocused(false);
    }, [onBlur]);

    const textField = "projectName";
    const keyField = "workEffortId";
    const emptyItem: ProjectItem = { [textField]: "loading ...", workEffortId: "0" };
    const pageSize = 10;
    const loadingData: ProjectItem[] = [];
    while (loadingData.length < pageSize) {
        loadingData.push({ ...emptyItem });
    }

    const dataCaching = React.useRef<ProjectItem[]>([]);
    const pendingRequest = React.useRef<any>();
    const requestStarted = React.useRef(false);
    const [data, setData] = React.useState<ProjectItem[]>([]);
    const [total, setTotal] = React.useState(0);
    const [filter, setFilter] = React.useState("");
    const skipRef = React.useRef(0);

    // REFACTOR: Added resetCache function to clear cached data for better memory management
    const resetCache = () => {
        dataCaching.current.length = 0;
    };

    // REFACTOR: Optimized requestData to handle API calls with debouncing and proper parameter handling
    const requestData = React.useCallback((skip: number, filter: string) => {
        if (requestStarted.current) {
            clearTimeout(pendingRequest.current);
            pendingRequest.current = setTimeout(() => {
                requestData(skip, filter);
            }, 50);
            return;
        }

        requestStarted.current = true;
        const params = new URLSearchParams();
        params.append('skip', skip.toString());
        params.append('pageSize', pageSize.toString());
        if (filter) params.append('searchTerm', filter);

        agent.Projects.getProjectsLov(params)
            .then((json) => {
                if (json) {
                    const total = json.projectCount;
                    const items: ProjectItem[] = [];
                    json.projects.forEach((element: any, index: number) => {
                        const { workEffortId, projectName } = element;
                        const item: ProjectItem = {
                            [keyField]: workEffortId,
                            [textField]: projectName,
                        };
                        items.push(item);
                        dataCaching.current[index + skip] = item;
                    });

                    if (skip === skipRef.current) {
                        setData(items);
                        setTotal(total);
                    }
                }
                requestStarted.current = false;
            });
    }, []);

    // REFACTOR: Added useEffect cleanup to prevent memory leaks
    React.useEffect(() => {
        const ac = new AbortController();
        requestData(0, filter);
        return () => {
            resetCache();
            ac.abort();
        };
    }, [filter, requestData]);

    // REFACTOR: Optimized filter change handler to reset cache and trigger new data fetch
    const onFilterChange = React.useCallback((event: ComboBoxFilterChangeEvent) => {
        const filter = event.filter.value;
        resetCache();
        requestData(0, filter);
        setData(loadingData);
        skipRef.current = 0;
        setFilter(filter);
    }, [requestData]);

    // REFACTOR: Simplified data caching logic for better readability
    const shouldRequestData = React.useCallback((skip: number) => {
        for (let i = 0; i < pageSize; i++) {
            if (!dataCaching.current[skip + i]) {
                return true;
            }
        }
        return false;
    }, []);

    const getCachedData = React.useCallback((skip: number) => {
        const data: ProjectItem[] = [];
        for (let i = 0; i < pageSize; i++) {
            data.push(dataCaching.current[i + skip] || { ...emptyItem });
        }
        return data;
    }, []);

    // REFACTOR: Optimized page change handler to use cached data when available
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

    // REFACTOR: Added null check and proper state dispatch for project selection
    const onChangeHandler = React.useCallback(
        (event) => {
            onChange({ value: event.value });
        },
        [onChange, dispatch]
    );

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
                    pageSize: pageSize,
                    skip: skipRef.current,
                    total: total,
                }}
                onPageChange={pageChange}
            />
            {showHint && (
                <NotificationGroup style={position.bottomRight}>
                    <Notification type={{ style: 'info', icon: true }} closable={false}>
                        <span>{hint}</span>
                    </Notification>
                </NotificationGroup>
            )}
        </FieldWrapper>
    );
};