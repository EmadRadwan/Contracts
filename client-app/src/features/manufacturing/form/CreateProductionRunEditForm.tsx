import React, {useEffect, useState, useCallback, useRef} from "react";
import {Grid, Button, Box, Typography, Paper} from "@mui/material";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import {requiredValidator} from "../../../app/common/form/Validators";
import FormInput from "../../../app/common/form/FormInput";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {
     useFetchProductionRunTasksSimpleQuery, useFetchProductQuantityUomQuery,
    useFetchProductRoutingsQuery, useFetchRowMaterialFacilitiesQuery,
} from "../../../app/store/apis";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import {
    FormComboBoxVirtualFinishedManufacturingProduct
} from "../../../app/common/form/FormComboBoxVirtualFinishedManufacturingProduct";
import FormTextArea from "../../../app/common/form/FormTextArea";
import useProductionRun from "../hook/useProductionRun";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import ProductionRunMaterialsList from "../dashboard/ProductionRunMaterialsList";
import {useSelector} from "react-redux";
import {RootState, useAppDispatch, useAppSelector} from "../../../app/store/configureStore";
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";
import {
    clearJobRunUnderProcessing,
    setProductionRunStatusDescription
} from "../slice/manufacturingSharedUiSlice";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import {WorkEffort} from "../../../app/models/manufacturing/workEffort";
import ProductionRunPartiesList from "../dashboard/ProductionRunPartiesList";
import FormDateTimePicker from "../../../app/common/form/FormDateTimePicker";
import ProductionRunTasksListSimple from "../dashboard/ProductionRunTasksListSimple";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {ProductionRunRoutingTask} from "../../../app/models/manufacturing/productionRunRoutingTask";
import MermaidChart from "../dashboard/MermaidChart"; // Import the new MermaidChart component

interface Props {
    cancelEdit: () => void;
    editMode: number;
}

export default function CreateProductionRunEditForm({cancelEdit, editMode}: Props) {
    const formRef = React.useRef<any>();
    const [formKey, setFormKey] = useState(Math.random());
    const [product, setProduct] = useState<string>();
    const [selectedMenuItem, setSelectedMenuItem] = React.useState('');
    const [showTasksList, setShowTasksList] = useState(false);
    const [showMaterialsList, setShowMaterialsList] = useState(false);
    const [showPartiesList, setShowPartiesList] = useState(false);
    const jobRunUnderProcessing = useSelector((state: RootState) => state.manufacturingSharedUi.jobRunUnderProcessing);
    const [isEditing, setIsEditing] = useState(false);
    const dispatch = useAppDispatch();
    const [productionRun, setProductionRun] = useState<WorkEffort | undefined>(undefined);
    const productionRunStatusDescription = useSelector((state: RootState) => state.manufacturingSharedUi.productionRunStatusDescription);
    const {getTranslatedLabel} = useTranslationHelper()
    const {language} = useAppSelector(state => state.localization)
    const [productionRunTasks, setProductionRunTasks] = useState<ProductionRunRoutingTask[]>([]);


    const { data: productionRunTasksData } = useFetchProductionRunTasksSimpleQuery(productionRun?.workEffortId, { skip: !productionRun?.workEffortId });

    useEffect(() => {
        if (productionRunTasksData) {
            const adjustedData = handleDatesArray(productionRunTasksData);
            setProductionRunTasks(adjustedData);
        }
    }, [productionRunTasksData]);

    
    const [formValues, setFormValues] = useState({
        productName: "",
        productId: "",
        quantityToProduce: 0,
        estimatedStartDate: new Date(),
        facilityId: "",
    });

    const [isLoading, setIsLoading] = useState(false);
    const {data: facilities} = useFetchRowMaterialFacilitiesQuery(undefined);
    const {data: productRoutings} = useFetchProductRoutingsQuery(
        {productId: product},
        {skip: product === undefined}
    );

    const {data: productQuantityUom} = useFetchProductQuantityUomQuery(
        {productId: product},
        {skip: product === undefined}
    );

    const [diagram, setDiagram] = useState('');

    useEffect(() => {
        if (productionRunTasksData?.length > 0) {
            // Start a simple flowchart with left-to-right direction
            let chartString = "flowchart LR\n";
            chartString += "  Start((Start))\n";

            productionRunTasksData.forEach((task, index) => {
                const label = `${task.sequenceNum}<br/>${task.workEffortName}`;

                if (index === 0) {
                    // First task flows from Start
                    chartString += `  Start --> T${task.sequenceNum}("${label}")\n`;
                } else {
                    const prevSequence = productionRunTasksData[index - 1].sequenceNum;
                    chartString += `  T${prevSequence} --> T${task.sequenceNum}("${label}")\n`;
                }
            });

            // Finally, link the last task to End
            const lastTask = productionRunTasksData[productionRunTasksData.length - 1];
            chartString += `  T${lastTask.sequenceNum} --> End((End))\n`;

            setDiagram(chartString);
        } else {
            // If no tasks, clear or set a placeholder
            setDiagram("");
        }
    }, [productionRunTasksData]);

    //console.log('diagram', diagram)
    //console.log('productionRunTasksData', productionRunTasksData)
    //console.log('productionRunTasks', productionRunTasks)
    console.log('productionRun', productionRun)
    console.log('jobRunUnderProcessing', jobRunUnderProcessing)


    const {
        handleCreate,
    } = useProductionRun({
        selectedMenuItem,
        setIsLoading
    });

    const handleFieldChange = useCallback((name: string, value: any) => {
        setFormValues((prevValues) => ({
            ...prevValues,
            [name]: value,
        }));
        setIsEditing(true); // Set isEditing to true when any field is changed
        if (name === "productId") {
            setProduct(value.productId);
        }
    }, []);

    useEffect(() => {
        const {productId, quantityToProduce, estimatedStartDate, facilityId} = formValues;
        if (editMode === 1) {

            if (productId && quantityToProduce && estimatedStartDate && facilityId) {
                const facilityName = facilities?.find(
                    (facility) => facility.facilityId === facilityId
                )?.facilityName;
                const description = `Production run for ${quantityToProduce} ${productQuantityUom} unit(s) of ${
                    productId.productName
                }, starting on ${new Date(
                    estimatedStartDate
                ).toDateString()} inside facility (${facilityName})`;

                formRef.current?.onChange("workEffortName", {value: description});
            }

        }
    }, [formValues, facilities, editMode]);


    useEffect(() => {
        if (jobRunUnderProcessing) {
            setProduct(jobRunUnderProcessing.productId.productId);
            setProductionRun(handleDatesObject(jobRunUnderProcessing));
            setFormKey(Math.random());
        } else {
            setProductionRun(handleDatesObject(jobRunUnderProcessing));
            //setFormKey(Math.random());
        }

    }, [jobRunUnderProcessing]);


    // menu select event handler
    async function handleMenuSelect(e: MenuSelectEvent) {
        if (e.item.data === 'create') {
            setSelectedMenuItem('Create Production Run');
            setTimeout(() => {
                formRef.current.onSubmit();
            });
        }

        if (e.item.text === 'New Production Run') {
            handleNewProductionRun()
        }

        if (e.item.data === 'edit') {
            setSelectedMenuItem('Edit Production Run');
            setTimeout(() => {
                formRef.current.onSubmit();
            });
        }

        if (e.item.data === 'schedule') {
            setSelectedMenuItem('Schedule');
            setTimeout(() => {
                formRef.current.onSubmit();
            });
        }

        if (e.item.data === 'confirm') {
            setSelectedMenuItem('Confirm');
            setTimeout(() => {
                formRef.current.onSubmit();
            });
        }


        if (e.item.data === 'tasks') {
            setShowTasksList(true);
        }

        if (e.item.data === 'materials') {
            setShowMaterialsList(true);
        }

        if (e.item.data === 'parties') {
            setShowPartiesList(true);
        }


    }


    const handleSubmit = (data: any) => {
        if (!data.isValid) {
            return false
        }
        setIsLoading(true);
        setIsEditing(false);
        handleCreate(data);
    };

    const handleCancelForm = () => {
        dispatch(clearJobRunUnderProcessing(undefined));
        dispatch(setProductionRunStatusDescription(undefined));
        setProductionRun(undefined);
        setFormKey(Math.random());
        setIsEditing(false);
        setProduct('');
        cancelEdit();
    };

    const handleNewProductionRun = () => {
        dispatch(clearJobRunUnderProcessing(undefined));
        dispatch(setProductionRunStatusDescription(undefined));
        setProductionRun(undefined);
        //setFormKey(Math.random());
    };

    const handleButtonClick = (text: string) => {
        const mockEvent: MenuSelectEvent = {
            item: {data: editMode === 1 ? text : 'edit'},
            itemId: editMode === 1 ? text : 'Edit Production Run',
            syntheticEvent: {} as React.SyntheticEvent, // Mocking the SyntheticEvent, adjust if necessary
            nativeEvent: {} as Event, // Mocking the native Event, adjust if necessary
            target: {} as any // Mocking the target, adjust if necessary
        };
        handleMenuSelect(mockEvent);
    };


    const memoizedOnClose = useCallback(
        () => {
            setShowTasksList(false)
        },
        [],
    );

    const memoizedOnClose2 = useCallback(
        () => {
            setShowMaterialsList(false)
        },
        [],
    );
    
    const memoizedOnClose3 = useCallback(
        () => {
            setShowPartiesList(false)
        },
        [],
    );
    
    console.log('productionRun from Edit form ', productionRun)
    console.log('jobRunUnderProcessing from Edit form ', jobRunUnderProcessing);
    console.log('showTasksList:', showTasksList)
    //console.log('isEditing ', isEditing);
    return (
        <>
            <ManufacturingMenu selectedMenuItem={"jobShop"}/>

            <Paper elevation={5} className="div-container-withBorderCurved" sx={{pt: "30px"}}>
                {showTasksList && (<ModalContainer show={showTasksList} onClose={memoizedOnClose} width={950}>
                    <ProductionRunTasksListSimple productionRunTasksData={productionRunTasks}/>
                </ModalContainer>)}

                
                
                
                
                {showMaterialsList && (<ModalContainer show={showMaterialsList} onClose={memoizedOnClose2} width={600}>
                    <ProductionRunMaterialsList productionRunId={productionRun?.workEffortId}
                                                onClose={() => setShowMaterialsList(false)} currentStatusDescription={productionRun.currentStatusDescription}/>
                </ModalContainer>)}
                {showPartiesList && (<ModalContainer show={showPartiesList} onClose={memoizedOnClose3} width={900}>
                    <ProductionRunPartiesList productionRunId={productionRun?.workEffortId}
                                                onClose={() => setShowPartiesList(false)}/>
                </ModalContainer>)}
                <Grid container spacing={2} alignItems={"center"}>
    <Grid item xs={3}>
        <Box display="flex" justifyContent="space-between">
            <Typography
                sx={{
                    fontWeight: "bold",
                    paddingLeft: 0,
                    fontSize: '18px',
                    color: jobRunUnderProcessing === undefined ? "green" : "black"
                }}
                variant="h6"
            >
                {productionRun && productionRun?.workEffortId
                    ? getTranslatedLabel("manufacturing.jobshop.edit.productionRunNo", "Production Run No:") + ` ${productionRun?.workEffortId}`
                    : getTranslatedLabel("manufacturing.jobshop.edit.newProductionRun", "New Production Run")
                }
            </Typography>
        </Box>
    </Grid>
    <Grid item xs={3}>
        {productionRun && productionRun.currentStatusDescription && (
            <Typography color="primary"
                        sx={{fontSize: '18px', color: 'blue', fontWeight: 'bold'}}
                        variant="h6">
                {getTranslatedLabel("manufacturing.jobshop.edit.status", "Status:")} {productionRun.currentStatusDescription}
            </Typography>
        )}
    </Grid>

    {productionRun && productionRun.currentStatusDescription ?
        <Grid item xs={4}>
            <Box display="flex" justifyContent="space-between" style={{ gap: '8px' }} className="custom-menu">
                <Menu onSelect={handleMenuSelect}>
                    <MenuItem text={getTranslatedLabel("manufacturing.jobshop.edit.tasks", "Tasks")} data={"tasks"}  />
                </Menu>
                <Menu onSelect={handleMenuSelect}>
                    <MenuItem text={getTranslatedLabel("manufacturing.jobshop.edit.materials", "Materials")} data="materials" />
                </Menu>
               {/* <Menu onSelect={handleMenuSelect}>
                    <MenuItem text={getTranslatedLabel("manufacturing.jobshop.edit.parties", "Parties")} data="parties" />
                </Menu>*/}
                
            </Box>
        </Grid>
        : <Grid item xs={4}></Grid>}

    <Grid item xs={2}>
        <Box display="flex" justifyContent="flex-end">
            <Menu onSelect={handleMenuSelect}>
                <MenuItem text={getTranslatedLabel("manufacturing.jobshop.edit.actions", "Actions")}>
                    {!productionRun?.workEffortId && (
                        <MenuItem text={getTranslatedLabel("manufacturing.jobshop.edit.createProductionRun", "Create Production Run")} data="create" />
                    )}
                    {/*{productionRunStatusDescription === "Created" && !isEditing &&
                        <MenuItem text={getTranslatedLabel("manufacturing.jobshop.edit.schedule", "Schedule")} data='schedule' />}*/}
                    {(productionRunStatusDescription === "Created" || productionRunStatusDescription === "Scheduled") && !isEditing &&
                        <MenuItem text={getTranslatedLabel("manufacturing.jobshop.edit.confirm", "Confirm")} data='confirm' />}
                    {productionRunStatusDescription === "Running" && !isEditing &&
                        <MenuItem text={getTranslatedLabel("manufacturing.jobshop.edit.quickClose", "Quick Close")} data={'close'} />}
                    {productionRunStatusDescription === "Running" && !isEditing &&
                        <MenuItem text={getTranslatedLabel("manufacturing.jobshop.edit.quickComplete", "Quick Complete")} data='complete' />}
                </MenuItem>
            </Menu>
        </Box>
    </Grid>
</Grid>

<Form
    ref={formRef}
    key={formKey}
    initialValues={editMode === 1 ? formValues : productionRun}
    onSubmitClick={(values) => handleSubmit(values)}
    render={(formRenderProps) => (
        <FormElement>
            <fieldset className={"k-form-fieldset"}>
                <Grid container spacing={2}>
                    <Grid item xs={9}>
                        <Grid item container xs={11} spacing={2} alignItems={"flex-end"}>
                            <Grid item xs={3}>
                                <Field
                                    id={"productId"}
                                    name={"productId"}
                                    label={getTranslatedLabel("manufacturing.jobshop.edit.product", "Product *")}
                                    component={FormComboBoxVirtualFinishedManufacturingProduct}
                                    autoComplete={"off"}
                                    validator={requiredValidator}
                                    disabled={editMode > 1}
                                    onChange={(e) =>
                                        handleFieldChange("productId", e.value)
                                    }
                                />
                            </Grid>
                            <Grid item xs={3}>
                                <Field
                                    id={"estimatedStartDate"}
                                    name={"estimatedStartDate"}
                                    label={getTranslatedLabel("manufacturing.jobshop.edit.startDate", "Start Date *")}
                                    component={FormDateTimePicker}
                                    validator={requiredValidator}
                                    onChange={(e) => handleFieldChange("estimatedStartDate", e.value)}
                                />
                            </Grid>
                            <Grid item xs={5}>
                                {productionRun && productionRun.estimatedCompletionDate ?
                                <Box display="flex" sx={{flexDirection: 'column'}}>
                                    <Typography variant="h6">
                                        {getTranslatedLabel("manufacturing.jobshop.edit.calculatedCompletionDate", "Calculated Completion Date:")}
                                    </Typography>
                                    <Typography sx={{fontWeight: "bold"}} color="black" variant="h6" >
                                        {productionRun?.estimatedCompletionDate ? new Date(productionRun.estimatedCompletionDate).toLocaleDateString(language === "ar" ? "ar-EG" : 'en-GB', {
                                            hour: 'numeric',
                                            minute: 'numeric'
                                        }) : ''}
                                    </Typography>
                                </Box> : null}
                            </Grid>
                        </Grid>
                        <Grid item container xs={10} spacing={2}>
                            <Grid item xs={3}>
                                <Field
                                    id={"quantityToProduce"}
                                    format="n2"
                                    min={0}
                                    name={"quantityToProduce"}
                                    label={getTranslatedLabel("manufacturing.jobshop.edit.quantityToProduce", "Quantity To Produce *")}
                                    component={FormNumericTextBox}
                                    validator={requiredValidator}
                                    onChange={(e) =>
                                        handleFieldChange("quantityToProduce", e.value)
                                    }
                                />
                            </Grid>
                            <Grid item xs={4}>
                                {productQuantityUom && (
                                    <Typography color="primary"
                                                sx={{
                                                    fontSize: '16px',
                                                    color: 'blue',
                                                    fontWeight: 'bold',
                                                    pt: 6
                                                }}
                                                variant="h6">
                                        {productQuantityUom}
                                    </Typography>
                                )}
                            </Grid>
                        </Grid>
                        <Grid item container xs={10} spacing={2}>
                            {productionRun && productionRun.currentStatusDescription ? null :
                                <Grid item xs={5}>
                                    <Field
                                        id={"routingId"}
                                        name={"routingId"}
                                        label={getTranslatedLabel("manufacturing.jobshop.edit.routing", "Routing *")}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey={"routingId"}
                                        textField={"routingName"}
                                        data={productRoutings ? productRoutings : []}
                                        validator={requiredValidator}
                                        onChange={(e) =>
                                            handleFieldChange("routingId", e.value)
                                        }
                                    />
                                </Grid>}
                            <Grid item xs={5}>
                                <Field
                                    id={"facilityId"}
                                    name={"facilityId"}
                                    label={getTranslatedLabel("manufacturing.jobshop.edit.facility", "Row Material Facility *")}
                                    component={MemoizedFormDropDownList}
                                    dataItemKey={"facilityId"}
                                    textField={"facilityName"}
                                    data={facilities ? facilities : []}
                                    validator={requiredValidator}
                                    onChange={(e) =>
                                        handleFieldChange("facilityId", e.value)
                                    }
                                />
                            </Grid>
                        </Grid>
                        <Grid item container xs={10} spacing={2} alignItems={"flex-end"}>
                            <Grid item xs={5}>
                                <Field
                                    id={"workEffortName"}
                                    name={"workEffortName"}
                                    label={getTranslatedLabel("manufacturing.jobshop.edit.productionRunName", "Production Run Name *")}
                                    component={FormInput}
                                    autoComplete={"off"}
                                    validator={requiredValidator}
                                />
                            </Grid>
                            <Grid item xs={5}>
                                <Field
                                    id={"description"}
                                    name={"description"}
                                    label={getTranslatedLabel("manufacturing.jobshop.edit.description", "Description")}
                                    component={FormTextArea}
                                    autoComplete={"off"}
                                />
                            </Grid>
                        </Grid>
                    </Grid>
                    <Grid container item xs={3} flexDirection={"column"} justifyContent={"center"}>
                        <img
                            src="/images/medication-factory.jpg"
                            alt={getTranslatedLabel("manufacturing.jobshop.edit.factoryImage", "Factory")}
                            style={{ width: '100%' }}
                        />
                    </Grid>
                   {/* <Grid item xs={12}>
                        <Paper sx={{ overflowX: 'auto', maxWidth: '100%' }}>
                            <MermaidChart chart={diagram} />
                        </Paper>
                    </Grid>*/}

                </Grid>
                <div className="k-form-buttons">
                    <Grid container rowSpacing={2}>
                        <Grid item xs={2}>
                            <Button
                                onClick={() => handleButtonClick('create')}
                                color="success"
                                variant="contained"
                                sx={{pr: "10px", pl: "10px"}}
                                disabled={!formRenderProps.allowSubmit}
                            >
                                {editMode === 1 ? getTranslatedLabel("manufacturing.jobshop.edit.createProductionRun", "Create Production Run") : getTranslatedLabel("manufacturing.jobshop.edit.editProductionRun", "Edit Production Run")}
                            </Button>
                        </Grid>
                        <Grid item xs={1}>
                            <Button
                                onClick={handleCancelForm}
                                color="error"
                                variant="contained"
                            >
                                {getTranslatedLabel("manufacturing.jobshop.edit.cancel", "Cancel")}
                            </Button>
                        </Grid>
                        
                    </Grid>
                </div>
                {isLoading && (
                    <LoadingComponent message={getTranslatedLabel("manufacturing.jobshop.edit.processing", "Processing Production Run...")} />
                )}
            </fieldset>
        </FormElement>
    )}
/>

            </Paper>
        </>
    );
}
