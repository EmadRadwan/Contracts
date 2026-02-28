import * as React from "react";
import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import { Label } from "@progress/kendo-react-labels";
import { ComboBox, ComboBoxFilterChangeEvent, ComboBoxPageChangeEvent } from "@progress/kendo-react-dropdowns";
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";
import agent from "../../api/agent";
import { useAppDispatch } from "../../store/configureStore";

// REFACTOR: Added facilityId to ProjectItem interface to support facility data
interface ProjectItem {
    projectId: string;
    projectName: string;
    facilityId: string; // New field to store facility identifier
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

    // REFACTOR: Consolidated position styles (unchanged)
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

    // REFACTOR: Simplified focus/blur handlers (unchanged)
    const handleOnFocus = React.useCallback(() => {
        onFocus();
        setFocused(true);
    }, [onFocus]);

    const handleOnBlur = React.useCallback(() => {
        onBlur();
        setFocused(false);
    }, [onBlur]);

    const textField = "projectName";
    const keyField = "projectId";
    // REFACTOR: Updated emptyItem to include facilityId
    const emptyItem: ProjectItem = {
        [textField]: "loading ...",
        projectId: "0",
        facilityId: "0", // Default value for facilityId
    };

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

    // REFACTOR: Reset cache function (unchanged)
    const resetCache = () => {
        dataCaching.current.length = 0;
    };

    // REFACTOR: Updated requestData to include facilityId in the mapping
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
        params.append("skip", skip.toString());
        params.append("pageSize", pageSize.toString());
        if (filter) params.append("searchTerm", filter);

        agent.Projects.getProjectsLov(params).then((json) => {
            if (json) {
                const total = json.projectCount;
                const items: ProjectItem[] = [];
                json.projects.forEach((element: any, index: number) => {
                    const { workEffortId, projectName, facilityId } = element;
                    // REFACTOR: Added facilityId to the item mapping
                    const item: ProjectItem = {
                        [keyField]: workEffortId,
                        [textField]: projectName,
                        facilityId: facilityId || "0", // Fallback to "0" if facilityId is missing
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

    // REFACTOR: useEffect cleanup (unchanged)
    React.useEffect(() => {
        const ac = new AbortController();
        requestData(0, filter);
        return () => {
            resetCache();
            ac.abort();
        };
    }, [filter, requestData]);

    // REFACTOR: Filter change handler (unchanged)
    const onFilterChange = React.useCallback(
        (event: ComboBoxFilterChangeEvent) => {
            const filter = event.filter.value;
            resetCache();
            requestData(0, filter);
            setData(loadingData);
            skipRef.current = 0;
            setFilter(filter);
        },
        [requestData]
    );

    // REFACTOR: Cached data logic (unchanged)
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

    // REFACTOR: Page change handler (unchanged)
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

    // REFACTOR: Updated onChangeHandler to include facilityId in the value
    const onChangeHandler = React.useCallback(
        (event: any) => {
            const selectedValue = event.value;

            if (!selectedValue) {
                // User cleared the selection → pass null to form
                onChange({ value: null });
                return;
            }

            // Valid selection → map to expected shape
            onChange({
                value: {
                    projectId: selectedValue.projectId,
                    projectName: selectedValue.projectName,
                    facilityId: selectedValue.facilityId || "0",
                },
            });
        },
        [onChange]
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
                popupSettings={{ appendTo: document.body }}
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