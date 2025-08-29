import * as React from "react";

import {FieldRenderProps, FieldWrapper} from "@progress/kendo-react-form";
import {Label} from "@progress/kendo-react-labels";
import {ComboBox, ComboBoxFilterChangeEvent, ComboBoxPageChangeEvent} from "@progress/kendo-react-dropdowns";
import {Notification, NotificationGroup} from "@progress/kendo-react-notification";
import agent from "../../api/agent";
import {useAppDispatch} from "../../store/configureStore";


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
    const editorRef = React.useRef(null);
    const [focused, setFocused] = React.useState(false);
    const dispatch = useAppDispatch();


    const position = {
        topLeft: {
            top: 0,
            left: 0,
            alignItems: "flex-start",
        },
        topCenter: {
            top: 0,
            left: "50%",
            transform: "translateX(-50%)",
        },
        topRight: {
            top: 0,
            right: 0,
            alignItems: "flex-end",
        },
        bottomLeft: {
            bottom: 0,
            left: 0,
            alignItems: "flex-start",
        },
        bottomCenter: {
            bottom: 0,
            left: "50%",
            transform: "translateX(-50%)",
        },
        bottomRight: {
            bottom: 0,
            right: 0,
            alignItems: "flex-end",
        },
    };
    const showValidationMessage = !focused && touched && validationMessage;
    const showHint = !showValidationMessage && focused && hint;
    const hintId = showHint ? `${id}_hint` : "";
    const errorId = showValidationMessage ? `${id}_error` : "";
    const labelId = label ? `${id}_label` : '';


    const handleOnFocus = React.useCallback(
        () => {
            onFocus();
            setFocused(true);
        },
        [onFocus]
    );

    const handleOnBlur = React.useCallback(
        () => {
            onBlur();
            setFocused(false);
        },
        [onBlur]
    );

    ////////////////////////////////////////////

    interface Item {
        fromPartyId: string;
        fromPartyName: string;
    }

    const textField = "fromPartyName";
    const keyField = "fromPartyId";
    const emptyItem: Item = {[textField]: "loading ...", fromPartyId: "0"};
    const pageSize = 10;

    const loadingData: Item[] = [];
    while (loadingData.length < pageSize) {
        loadingData.push({...emptyItem});
    }

    const dataCaching = React.useRef<any>([]);
    const pendingRequest = React.useRef<any>();
    const requestStarted = React.useRef(false);

    const [data, setData] = React.useState<Item[]>([]);
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
        agent.Parties.getContractorsLov(params)
            .then((json) => {
                if (json) {
                    const total = json.partyCount;
                    const items: Item[] = [];
                    json.parties.forEach((element: any, index: any) => {
                        const {fromPartyId, fromPartyName} = element;
                        const item: Item = {
                            [keyField]: fromPartyId,
                            [textField]: fromPartyName,
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
        const ac = new AbortController();
        requestData(0, filter);
        return () => {
            resetCach();
            ac.abort();
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


    const onChangeHandler = React.useCallback(
        (event) => {
            
            onChange({value: event.value && event.value})
        },
        [onChange, value]
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
                    pageSize: pageSize,
                    skip: skipRef.current,
                    total: total,
                }}
                onPageChange={pageChange}
                //style={{width: "200px"}}
            />
            {
                showHint &&
                <NotificationGroup style={position.bottomRight}>
                    <Notification type={{style: 'info', icon: true}} closable={false}>
                        <span>{hint}</span>
                    </Notification>
                </NotificationGroup>
            }
           
        </FieldWrapper>
    );
};
