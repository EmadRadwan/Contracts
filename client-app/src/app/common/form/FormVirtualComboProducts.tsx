import * as React from "react";
import {
    ComboBox,
    ComboBoxChangeEvent,
    ComboBoxFilterChangeEvent,
    ComboBoxPageChangeEvent,
} from "@progress/kendo-react-dropdowns";
import agent from "../../../app/api/agent";

interface Item {
    productId: string;
    productName: string;
}

const textField = "productName";
const keyField = "productId";
const emptyItem: Item = {[textField]: "loading ...", productId: "0"};
const pageSize = 10;

const loadingData: Item[] = [];
while (loadingData.length < pageSize) {
    loadingData.push({...emptyItem});
}

export const FormVirtualCombo = () => {
    const dataCaching = React.useRef<any>([]);
    const pendingRequest = React.useRef<any>();
    const requestStarted = React.useRef(false);

    const [data, setData] = React.useState<Item[]>([]);
    const [value, setValue] = React.useState(null);
    const [total, setTotal] = React.useState(0);
    const [filter, setFilter] = React.useState("");

    const skipRef = React.useRef(0);

    const resetCach = () => {
        dataCaching.current.length = 0;
    };

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
        agent.Products.getSalesProductsLov(params)
            .then((json) => {
                if (json) {
                    const total = json.productCount;
                    const items: Item[] = [];
                    json.products.forEach((element: any, index: any) => {
                        const {productId, productName} = element;
                        const item: Item = {
                            [keyField]: productId,
                            [textField]: productName,
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

    React.useEffect(() => {
        requestData(0, filter);
        return () => {
            resetCach();
        };
    }, [filter, requestData]);

    const onFilterChange = React.useCallback(
        (event: ComboBoxFilterChangeEvent) => {
            const filter = event.filter.value;

            resetCach();
            requestData(0, filter);

            setData(loadingData);
            skipRef.current = 0;
            setFilter(filter);
        },
        []
    );

    const shouldRequestData = React.useCallback((skip) => {
        for (let i = 0; i < pageSize; i++) {
            if (!dataCaching.current[skip + i]) {
                return true;
            }
        }
        return false;
    }, []);

    const getCachedData = React.useCallback((skip) => {
        const data: Array<any> = [];
        for (let i = 0; i < pageSize; i++) {
            data.push(dataCaching.current[i + skip] || {...emptyItem});
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

    const onChange = React.useCallback((event: ComboBoxChangeEvent) => {
        const value = event.target.value;
        if (value && value[textField] === emptyItem[textField]) {
            return;
        }
        setValue(value);
    }, []);

    return (
        <ComboBox
            data={data}
            value={value}
            onChange={onChange}
            dataItemKey={keyField}
            textField={textField}
            filterable={true}
            onFilterChange={onFilterChange}
            virtual={{
                pageSize: pageSize,
                skip: skipRef.current,
                total: total,
            }}
            onPageChange={pageChange}
            style={{width: "200px"}}
        />
    );
};