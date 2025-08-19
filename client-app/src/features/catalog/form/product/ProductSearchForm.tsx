import React, {useEffect, useState} from "react";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {Paper} from "@mui/material";
import RadioButtonGroup from "../../../../app/components/RadioButtonGroup";
import CheckboxButtons from "../../../../app/components/CheckboxButtons";
import {ProductParams} from "../../../../app/models/product/product";
import {useFetchProductCategoriesQuery, useFetchProductTypesQuery} from "../../../../app/store/configureStore";
import FormInput from "../../../../app/common/form/FormInput";
import {FormDropDownTreeProductCategory} from "../../../../app/common/form/FormDropDownTreeProductCategory";

interface Props {
    params: ProductParams;
    show: boolean;
    width?: number;
    onClose: () => void;
    onSubmit: (productParam: ProductParams) => void;
}

export default function ProductSearchForm({
                                              params,
                                              onSubmit,
                                              width,
                                              show,
                                              onClose,
                                          }: Props) {
    const [productTypeDesc, setProductTypeDesc] = useState<string[]>();

    const {data: productTypes} = useFetchProductTypesQuery(undefined);
    const {data: productCategories} = useFetchProductCategoriesQuery(undefined);

    const [orderBy, setOrderBy] = useState(params.orderBy);
    const [productTypeArray, setProductTypeArray] = useState<string[]>();
    const [productName, setProductName] = useState<string>("");
    const [selectedCategory, setSelectedCategory] = useState<string>("");
    // eslint-disable-next-line react-hooks/exhaustive-deps
    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };

    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, [closeOnEscapeKeyDown]);

    const getProductTypes = (): string[] => {
        return productTypes!.map((type) => type.description);
    };

    console.log(productCategories!);
    const handleChangedCheckBoxButtons = (items: string[]) => {
        const filteredProductTypes = productTypes!.filter((type) =>
            items.includes(type.description),
        );
        const values: any[] = filteredProductTypes.map(
            (type) => type.productTypeId,
        );

        const descArray = filteredProductTypes.map((type) => type.description);
        setProductTypeDesc(descArray.length > 0 ? descArray : [""]);
        setProductTypeArray(values);
    };

    const sortOptions = [
        {value: "productIdAsc", label: "Product ID Asc"},
        {value: "productIdDesc", label: "Product ID Desc"},
        {value: "createdStampAsc", label: "Product Date Asc"},
        {value: "createdStampDesc", label: "Product Date Desc"},
    ];

    async function handleSubmitData(data: any) {
        const productParam: ProductParams = {
            orderBy: orderBy,
            productTypes: productTypeArray,
            productName,
            productCategory: selectedCategory,
        };
        onSubmit(productParam);
        onClose();
    }

    return ReactDOM.createPortal(
        <CSSTransition in={show} unmountOnExit timeout={{enter: 0, exit: 300}}>
            <div className="modal">
                <div
                    className="modal-content"
                    style={{width: width}}
                    onClick={(e) => e.stopPropagation()}
                >
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <Form
                            initialValues={params}
                            onSubmitClick={(values) => handleSubmitData(values)}
                            render={(formRenderProps) => (
                                <FormElement>
                                    <fieldset className={"k-form-fieldset"}>
                                        <Grid container columnSpacing={1}>
                                            <Grid item xs={6}>
                                                <Paper sx={{mb: 2, p: 2}}>
                                                    <Field
                                                        id={"productName"}
                                                        name={"productName"}
                                                        label={"Product Name"}
                                                        component={FormInput}
                                                        onChange={(e) => setProductName(e.target.value)}
                                                    />
                                                </Paper>
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Paper sx={{mb: 2, p: 2}}>
                                                    <Field
                                                        id={"productCategoryId"}
                                                        name={"productCategory"}
                                                        label={"Product Category"}
                                                        component={FormDropDownTreeProductCategory}
                                                        data={productCategories ? productCategories : []}
                                                        textField={"text"}
                                                        dataItemKey={"productCategoryId"}
                                                        selectField={"selected"}
                                                        expandField={"expanded"}
                                                        onChange={(e) => setSelectedCategory(e.value)}
                                                    />
                                                </Paper>
                                            </Grid>
                                        </Grid>
                                        <Grid container columnSpacing={1}>
                                            <Grid item xs={6}>
                                                <Paper sx={{mb: 2, p: 2}}>
                                                    <RadioButtonGroup
                                                        selectedValue={orderBy}
                                                        options={sortOptions}
                                                        onChange={(e) => setOrderBy(e.target.value)}
                                                    />
                                                </Paper>
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Paper sx={{mb: 2, p: 2}}>
                                                    <CheckboxButtons
                                                        items={getProductTypes()}
                                                        checked={productTypeDesc}
                                                        onChange={handleChangedCheckBoxButtons}
                                                    />
                                                </Paper>
                                            </Grid>
                                        </Grid>
                                        <div className="k-form-buttons">
                                            <Grid container columnSpacing={1}>
                                                <Grid item xs={3}>
                                                    <Button
                                                        variant="contained"
                                                        type={"submit"}
                                                        color="success"
                                                    >
                                                        Find
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={3}>
                                                    <Button
                                                        onClick={() => onClose()}
                                                        color="error"
                                                        variant="contained"
                                                    >
                                                        Cancel
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </div>
                                    </fieldset>
                                </FormElement>
                            )}
                        />
                    </Paper>
                </div>
            </div>
        </CSSTransition>,
        document.getElementById("root")!,
    );
}
export const ProductSearchFormMemo = React.memo(ProductSearchForm);

//FIXME: add filter applied icon on the find product button
