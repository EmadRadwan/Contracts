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

interface ProductItem {
    ProductId: string;
    ProductName: string;
    ProductType: string;
    PrimaryProductCategoryDescription: string;
}

// Purpose: Variant of FormSimpleComboBoxVirtualProduct used only in CertificateItemKendoBulkAddV2 —
// adds a "Product Category" column (primaryProductCategoryDescription, same field ProductsList.tsx displays)
// so bulk-add rows can disambiguate products that share a name across categories.
export const FormSimpleComboBoxVirtualProductWithCategory = (fieldRenderProps: FieldRenderProps) => {
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
    const { currentCertificateType } = useAppSelector((state) => state.certificateUi);
    const keyField = "ProductId";
    const textField = "ProductName";
    const emptyItem: ProductItem = { ProductId: "0", ProductName: "loading ...", ProductType: "", PrimaryProductCategoryDescription: "" };
    const pageSize = 10;

    const columns = [
        { field: "ProductId", header: "رقم المنتج", width: "200px" },
        { field: "ProductName", header: "اسم المنتج", width: "250px" },
        { field: "PrimaryProductCategoryDescription", header: "فئة المنتج", width: "200px" },
    ];

    const loadingData: ProductItem[] = [];
    while (loadingData.length < pageSize) {
        loadingData.push({ ...emptyItem });
    }

    const dataCaching = React.useRef<ProductItem[]>([]);
    const requestStarted = React.useRef(false);
    const pendingRequest = React.useRef<NodeJS.Timeout | null>(null);
    const [data, setData] = React.useState<ProductItem[]>([]);
    const [total, setTotal] = React.useState(0);
    const [filter, setFilter] = React.useState("");
    const skipRef = React.useRef(0);

    const position = {
        bottomRight: { bottom: 0, right: 0, alignItems: "flex-end" },
    };

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
            if (currentCertificateType) params.append("certificateType", currentCertificateType);

            agent.Products.getSimpleProductsLov(params)
                .then((json) => {
                    if (json) {
                        const total = json.productCount;
                        const items: ProductItem[] = [];
                        json.products.forEach((element: any, index: number) => {
                            const item: ProductItem = {
                                ProductId: element.productId,
                                ProductName: element.productName,
                                ProductType: element.productType,
                                PrimaryProductCategoryDescription: element.primaryProductCategoryDescription || "",
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
                .catch(() => {
                    requestStarted.current = false;
                });
        },
        [currentCertificateType]
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
            if (!dataCaching.current[skip + i]) {
                return true;
            }
        }
        return false;
    }, []);

    const getCachedData = React.useCallback((skip: number) => {
        const data: ProductItem[] = [];
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
        (event: ComboBoxChangeEvent) => onChange({ value: event.value || null }),
        [onChange]
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
