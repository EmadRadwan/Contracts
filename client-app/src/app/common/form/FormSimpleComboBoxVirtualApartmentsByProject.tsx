import * as React from "react";
import { FieldRenderProps, FieldWrapper } from "@progress/kendo-react-form";
import { Label } from "@progress/kendo-react-labels";
import {
    MultiColumnComboBox,
    ComboBoxFilterChangeEvent,
    ComboBoxPageChangeEvent,
    ComboBoxChangeEvent
} from "@progress/kendo-react-dropdowns";
import { Notification, NotificationGroup } from "@progress/kendo-react-notification";
import agent from "../../api/agent";
import { useAppSelector } from "../../store/configureStore";

interface ApartmentItem {
    apartmentId: string;
    apartmentName: string;
    apartmentType: string;
    projectName: string;
    floorNumber: string;
    apartmentSpaceM2: number;
    gardenSpaceM2: number | null;
    gardenPricePerM2: number | null;
    apartmentPricePerM2: number;
    apartmentStatusId: string;
    apartmentStatusDescription: string;
}

const statusCellStyle = `
    /* Target the exact cells rendered inside the MultiColumnComboBox popup */
    .k-popup .k-table-td.status-cell-override {
        background-color: inherit !important;
    }
    /* Extra safety – also beat the row-level background */
    .k-popup .k-table-row .status-cell-override {
        background-color: inherit !important;
    }
` as const;

export const FormSimpleComboBoxVirtualApartmentsByProject = (fieldRenderProps: FieldRenderProps) => {
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
        projectId,
        popupSettings,
    } = fieldRenderProps;

    const editorRef = React.useRef<any>(null);
    const [focused, setFocused] = React.useState(false);
    const { currentCertificateType } = useAppSelector((state) => state.certificateUi);

    const keyField = "apartmentId";
    const textField = "apartmentName";

    const emptyItem: ApartmentItem = {
        apartmentId: "0",
        apartmentName: "loading ...",
        apartmentType: "",
        projectName: "",
        floorNumber: "",
        apartmentSpaceM2: 0,
        gardenSpaceM2: null,
        gardenPricePerM2: null,
        apartmentPricePerM2: 0,
        apartmentStatusId: "",
        apartmentStatusDescription: ""
    };

    const pageSize = 10;

    const columns = [
        { field: "apartmentId", header: "ID", width: "100px" },
        // { field: "projectName", header: "Project", width: "180px" },
        { field: "apartmentName", header: "Apartment", width: "200px" },
        { field: "floorNumber", header: "Floor", width: "110px" },
        {
            field: "apartmentStatusDescription",
            header: "Status",
            width: "130px",
            cell: (props: any) => {
                const statusId = props.dataItem.apartmentStatusId || "";
                const statusText = props.dataItem.apartmentStatusDescription || "";

                const isSold = statusId === "APARTMENT_SOLD";
                const isReserved = statusId === "APARTMENT_RESERVED";

                const bg = isSold ? "#ffebee" : isReserved ? "#fff3e0" : "#e8f5e8";
                const color = isSold ? "#c62828" : isReserved ? "#ef6c00" : "#2e7d32";

                return (
                    <td
                        className="status-cell-override"
                        style={{
                            textAlign: "center",
                            fontWeight: 600,
                            borderRadius: "4px",
                            padding: "6px 8px",
                            backgroundColor: bg,
                            color: color,
                        }}
                    >
                        {statusText || "—"}
                    </td>
                );
            },
        },
    ];

    // REFACTOR: Loading placeholder – reuse the same pattern as the product version
    const loadingData: ApartmentItem[] = [];
    while (loadingData.length < pageSize) {
        loadingData.push({ ...emptyItem });
    }

    const dataCaching = React.useRef<ApartmentItem[]>([]);
    const requestStarted = React.useRef(false);
    const pendingRequest = React.useRef<NodeJS.Timeout | null>(null);
    const [data, setData] = React.useState<ApartmentItem[]>([]);
    const [total, setTotal] = React.useState(0);
    const [filter, setFilter] = React.useState("");
    const skipRef = React.useRef(0);


    const position = { bottomRight: { bottom: 0, right: 0, alignItems: "flex-end" } };

    const resetCache = () => { dataCaching.current.length = 0; };

    // REFACTOR: API call renamed to `getSimpleApartmentsLov`
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
            if (currentCertificateType) params.append("certificateType", currentCertificateType);
            console.log(projectId)
            if (!projectId) {
                setData([]);
                setTotal(0);
                requestStarted.current = false;
                return;
            }

            agent.Products.getSimpleApartmentsByProjectLov(projectId, params)
                .then((json) => {
                    if (json) {
                        const total = json.apartmentCount;
                        const items: ApartmentItem[] = [];
                        json.apartments.forEach((element: any, index: number) => {
                            const item: ApartmentItem = {
                                apartmentId: element.apartmentId,
                                apartmentName: element.apartmentName,
                                apartmentType: element.apartmentType,
                                projectName: element.projectName ?? "",
                                floorNumber: element.floorNumber ?? "",
                                apartmentSpaceM2: element.apartmentSpaceM2,
                                gardenSpaceM2: element.gardenSpaceM2,
                                gardenPricePerM2: element.gardenPricePerM2,
                                apartmentPricePerM2: element.apartmentPricePerM2,
                                apartmentStatusId: element.apartmentStatusId,
                                apartmentStatusDescription: element.apartmentStatusDescription,
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
                })
                .catch(() => { requestStarted.current = false; });
        },
        [currentCertificateType, projectId]
    );

    React.useEffect(() => {
        const ac = new AbortController();
        requestData(0, filter);
        return () => {
            resetCache();
            ac.abort();
        };
    }, [filter, requestData, currentCertificateType]);

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
            if (!dataCaching.current[skip + i]) return true;
        }
        return false;
    }, []);

    const getCachedData = React.useCallback((skip: number) => {
        const data: ApartmentItem[] = [];
        for (let i = 0; i < pageSize; i++) {
            data.push(dataCaching.current[i + skip] || { ...emptyItem });
        }
        return data;
    }, []);

    const pageChange = React.useCallback(
        (event: ComboBoxPageChangeEvent) => {
            const newSkip = event.page.skip;
            if (shouldRequestData(newSkip)) requestData(newSkip, filter);
            const data = getCachedData(newSkip);
            setData(data);
            skipRef.current = newSkip;
        },
        [getCachedData, requestData, shouldRequestData, filter]
    );

    const onChangeHandler = React.useCallback(
        (event: ComboBoxChangeEvent) => onChange({ value: event.value || null }),
        [onChange]
    );

    const showValidationMessage = !focused && touched && validationMessage;
    const showHint = !showValidationMessage && focused && hint;
    const hintId = showHint ? `${id}_hint` : "";
    const errorId = showValidationMessage ? `${id}_error` : "";
    const labelId = label ? `${id}_label` : "";

    // Optional-called - see FormComboBoxVirtualProject: used directly in
    // AddActionsModal, where onFocus/onBlur are undefined.
    const handleOnFocus = React.useCallback(() => { onFocus?.(); setFocused(true); }, [onFocus]);
    const handleOnBlur  = React.useCallback(() => { onBlur?.();  setFocused(false); }, [onBlur]);

    return (
        <>
            <style dangerouslySetInnerHTML={{ __html: statusCellStyle }} />

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
                    popupSettings={popupSettings ? popupSettings : { appendTo: document.body }}
                />

                {showHint && (
                    <NotificationGroup style={position.bottomRight}>
                        <Notification type={{ style: "info", icon: true }} closable={false}>
                            <span>{hint}</span>
                        </Notification>
                    </NotificationGroup>
                )}
            </FieldWrapper>
        </>
    );
       
};