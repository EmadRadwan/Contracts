import React, { useCallback, useEffect, useState } from "react";
import { Box, Grid, Paper, Typography } from "@mui/material";
import {useFetchProductQuantityUomQuery} from "../../../app/store/apis";
import useProductionRun from "../hook/useProductionRun";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import ProductionRunTasksList from "../dashboard/ProductionRunTasksList";
import { WorkEffort } from "../../../app/models/manufacturing/workEffort";
import { useSelector } from "react-redux";
import { RootState, useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import {
  clearJobRunUnderProcessing,
  setProductionRunStatusDescription,
} from "../slice/manufacturingSharedUiSlice";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import ProductionRunMaterialsList from "../dashboard/ProductionRunMaterialsList";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import ProductionRunPartiesList from "../dashboard/ProductionRunPartiesList";
import ActualProductCostsList from "../dashboard/ActualProductCostsList";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import ProductionRunProducedInventoryList from "../dashboard/ProductionRunProducedInventoryList";
import ProductionRunReturnedInventoryList from "../dashboard/ProductionRunReturnedInventoryList";
import {toast} from "react-toastify";
import LoadingComponent from "../../../app/layout/LoadingComponent";

export default function CreateProductionRunDisplayForm() {
  const formRef = React.useRef<any>();
  const [product, setProduct] = useState<string>();
  const [selectedMenuItem, setSelectedMenuItem] = React.useState("");
  const [showMaterialsList, setShowMaterialsList] = useState(false);
  const dispatch = useAppDispatch();
  const [showPartiesList, setShowPartiesList] = useState(false);
  const [showActualCostsList, setShowActualCostsList] = useState(false);
  const [showProducedInventoryList, setShowProducedInventoryList] = useState(false);
  const [showReturnInventoryList, setShowReturnInventoryList] = useState(false);
  const { getTranslatedLabel } = useTranslationHelper();
  const {language} = useAppSelector(state => state.localization)

  const [isLoading, setIsLoading] = useState(false);
  

  const { data: productQuantityUom } = useFetchProductQuantityUomQuery(
    { productId: product },
    { skip: product === undefined }
  );

  const {
    handleStatusChangeStart,
    handleStatusChangeComplete,
    handleIssueTaskQoh,
    handleReserveTaskQoh, handleQuickChangeProductionRunStatus,
    //handleCancelProductionRun,
    //handleQuickRunAllProductionRunTasks,
    //handleQuickStartAllProductionRunTasks,
    //handleLinkProductionRun,
  } = useProductionRun({
    selectedMenuItem,
    setIsLoading,
  });

  const jobRunUnderProcessing = useSelector(
    (state: RootState) => state.manufacturingSharedUi.jobRunUnderProcessing
  );

  const inventoryProduced = useSelector(
      (state: RootState) => state.manufacturingSharedUi.inventoryProduced
  );

  useEffect(() => {
    // This function will run when the component is unmounted
    return () => {
      //console.log('Unmounting CreateProductionRunDisplayForm');
      dispatch(clearJobRunUnderProcessing(undefined));
      dispatch(setProductionRunStatusDescription(undefined));
    };
  }, [dispatch]); // Empty dependency array ensures this effect runs only on unmount

  useEffect(() => {
    setProduct(jobRunUnderProcessing?.productId.productId);
  }, [jobRunUnderProcessing, setProduct]);

  // menu select event handler
  async function handleMenuSelect(e: MenuSelectEvent) {
    if (e.item.data === "create") {
      setSelectedMenuItem("Create Production Run");
      setTimeout(() => {
        formRef.current.onSubmit();
      });
    }
    
    if (e.item.text === "New Production Run") {
      handleNewProductionRun();
    }

    if (e.item.text === "Update Production Run") {
      setSelectedMenuItem("Update Production Run");
      setTimeout(() => {
        formRef.current.onSubmit();
      });
    }

    if (e.item.text === "Quick Complete") {
      setSelectedMenuItem("Quick Complete");
      if (jobRunUnderProcessing) {
        setIsLoading(true);
        await handleQuickChangeProductionRunStatus(jobRunUnderProcessing);
      } else {
        toast.error("No production run selected.");
      }
    }
    
    if (e.item.text === "Quick Close") {
      setSelectedMenuItem("Quick Close");
      if (jobRunUnderProcessing) {
        // For runs not yet completed, change status to CLOSED
       // await handleQuickChangeProductionRunStatus(jobRunUnderProcessing, "PRUN_CLOSED");
      } else {
        toast.error("No production run selected.");
      }
    }

    if (e.item.text === "Cancel Production Run") {
      setSelectedMenuItem("Cancel Production Run");
      if (jobRunUnderProcessing) {
        //await handleCancelProductionRun(jobRunUnderProcessing);
      } else {
        toast.error("No production run selected.");
      }
    }

    if (e.item.text === "Quick Run All Tasks") {
      setSelectedMenuItem("Quick Run All Tasks");
      if (jobRunUnderProcessing) {
        //await handleQuickRunAllProductionRunTasks(jobRunUnderProcessing);
      } else {
        toast.error("No production run selected.");
      }
    }

    if (e.item.text === "quickStartAllTasks") {
      setSelectedMenuItem("Quick Start All Tasks");
      if (jobRunUnderProcessing) {
        //await handleQuickStartAllProductionRunTasks(jobRunUnderProcessing);
      } else {
        toast.error("No production run selected.");
      }
    }

    
    if (e.item.text === "Start") {
      setSelectedMenuItem("Start");
    }

    if (e.item.data === "material") {
      setShowMaterialsList(true);
    }

    if (e.item.data === "parties") {
      setShowPartiesList(true);
    }

    if (e.item.data === "cost") {
      setShowActualCostsList(true);
    }
    
    if (e.item.data === "inv") {
      setShowProducedInventoryList(true);
    }
    
    if (e.item.data === "ret") {
      setShowReturnInventoryList(true);
    }
  }

  const handleNewProductionRun = () => {
    dispatch(clearJobRunUnderProcessing(undefined));
    dispatch(setProductionRunStatusDescription(undefined));
  };

  const startTask = (dataItem: WorkEffort): Promise<void> => {
    return handleStatusChangeStart(dataItem);
  };

  const completeTask = (dataItem: WorkEffort): Promise<void> => {
    return handleStatusChangeComplete(dataItem);
  };

  const declareTask = (dataItem: WorkEffort): Promise<void> => {
    return handleStatusChangeComplete(dataItem);
  };

  const issueTaskQoh = (dataItem: WorkEffort): Promise<void> => {
    return handleIssueTaskQoh(dataItem);
  };

 const reserveTaskQoh = (dataItem: WorkEffort): Promise<void> => {
    return handleReserveTaskQoh(dataItem);
  };
 


 
  const memoizedOnClose = useCallback(() => {
    setShowActualCostsList(false);
  }, []);
  const memoizedOnClose2 = useCallback(() => {
    setShowMaterialsList(false);
  }, []);

  const memoizedOnClose3 = useCallback(() => {
    setShowPartiesList(false);
  }, []);

  const memoizedOnClose4 = useCallback(() => {
    setShowProducedInventoryList(false);
  }, []); 
  
  const memoizedOnClose5 = useCallback(() => {
    setShowReturnInventoryList(false);
  }, []);

  console.log('producedInventory', product);
  console.log('jobRunUnderProcessing from display', jobRunUnderProcessing);

// At the top of your component (before the return)
  const status = jobRunUnderProcessing ? jobRunUnderProcessing.currentStatusId : null;

  const canShowCancel = !!jobRunUnderProcessing;
  const canShowQuickCloseComplete =
      jobRunUnderProcessing &&
      status !== "PRUN_CANCELLED" &&
      status !== "PRUN_COMPLETED" &&
      status !== "PRUN_CLOSED";
  const canShowQuickRunAllStart =
      jobRunUnderProcessing &&
      status !== "PRUN_CREATED" &&
      status !== "PRUN_SCHEDULED" &&
      status !== "PRUN_CANCELLED" &&
      status !== "PRUN_COMPLETED" &&
      status !== "PRUN_CLOSED";
  const canShowQuickCloseFromComplete = jobRunUnderProcessing && status === "PRUN_COMPLETED";

  console.log("Production run status:", status);
  console.log("canShowCancel:", canShowCancel);
  console.log("canShowQuickCloseComplete:", canShowQuickCloseComplete);
  console.log("canShowQuickRunAllStart:", canShowQuickRunAllStart);
  console.log("canShowQuickCloseFromComplete:", canShowQuickCloseFromComplete);

    
  return (
    <>
      <ManufacturingMenu selectedMenuItem={"jobShop"} />

      <Paper
        elevation={5}
        className="div-container-withBorderCurved"
        sx={{ pt: "30px" }}
      >
        {showMaterialsList && (
          <ModalContainer
            show={showMaterialsList}
            onClose={memoizedOnClose2}
            width={600}
          >
            <ProductionRunMaterialsList
              productionRunId={jobRunUnderProcessing?.workEffortId}
              onClose={() => setShowMaterialsList(false)}
              currentStatusDescription={
                jobRunUnderProcessing?.currentStatusDescription
              }
            />
          </ModalContainer>
        )}
        {showPartiesList && (
          <ModalContainer
            show={showPartiesList}
            onClose={memoizedOnClose3}
            width={900}
          >
            <ProductionRunPartiesList
              productionRunId={jobRunUnderProcessing?.workEffortId}
              onClose={() => setShowPartiesList(false)}
            />
          </ModalContainer>
        )}
        {showActualCostsList && (
          <ModalContainer
            show={showActualCostsList}
            onClose={memoizedOnClose}
            width={1200}
          >
            <ActualProductCostsList
              productionRunId={jobRunUnderProcessing?.workEffortId}
              productId={product}
            />
          </ModalContainer>
        )}
        {showProducedInventoryList && (
          <ModalContainer
          show={showProducedInventoryList}
          onClose={memoizedOnClose4}
          width={1200}
        >
          <ProductionRunProducedInventoryList productionRunId={jobRunUnderProcessing?.workEffortId} />
        </ModalContainer>
        )}
        
        {showReturnInventoryList && (
          <ModalContainer
          show={showReturnInventoryList}
          onClose={memoizedOnClose5}
          width={1000}
        >
          <ProductionRunReturnedInventoryList productionRunId={jobRunUnderProcessing?.workEffortId} />
        </ModalContainer>
        )}
        <Grid container spacing={2} alignItems={"start"}>
          <Grid item xs={5}>
            <Grid container spacing={2} alignItems={"center"}>
              <Box display="flex" flexDirection="column">
                <Box display="flex" sx={{ paddingLeft: 4 }}>
                  <Typography
                    sx={{
                      fontWeight: "bold",
                      fontSize: "18px",
                      color:
                        jobRunUnderProcessing === undefined ? "green" : "blue",
                      minWidth: "220px",
                    }}
                    variant="h6"
                  >
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.productionRunNo",
                      "Production Run No:"
                    )}{" "}
                    {jobRunUnderProcessing?.workEffortId}
                  </Typography>
                </Box>

                <Box display="flex" sx={{ paddingLeft: 4 }}>
                  <Typography sx={{ minWidth: "220px" }} variant="h6">
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.productName",
                      "Product Name:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                  >
                    {jobRunUnderProcessing?.productName}
                  </Typography>
                </Box>

                <Box display="flex" sx={{ paddingLeft: 4 }}>
                  <Typography sx={{ minWidth: "220px" }} variant="h6">
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.quantity",
                      "Quantity:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                  >
                    <span style={{ color: "blue" }}>
                      {jobRunUnderProcessing?.quantityToProduce}
                    </span>
                    <span>{` ${productQuantityUom}`}</span>
                  </Typography>
                </Box>

                <Box display="flex" sx={{ paddingLeft: 4 }}>
                  <Typography sx={{ minWidth: "220px" }} variant="h6">
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.estimatedStartDate",
                      "Estimated Start Date:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                    dir={language === "ar" ? "ltr" : "rtl"}
                  >
                    {jobRunUnderProcessing?.estimatedStartDate
                      ? new Date(
                          jobRunUnderProcessing.estimatedStartDate
                        ).toLocaleDateString(language === "ar" ? "ar-EG" : "en-GB", {
                          hour: "numeric",
                          minute: "numeric",
                        })
                      : ""}
                  </Typography>
                </Box>

                <Box display="flex" sx={{ paddingLeft: 4 }}>
                  <Typography sx={{ minWidth: "220px" }} variant="h6">
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.actualStartDate",
                      "Actual Start Date:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                  >
                    {jobRunUnderProcessing?.actualStartDate
                      ? new Date(
                          jobRunUnderProcessing.actualStartDate
                        ).toLocaleDateString(language === "ar" ? "ar-EG" : "en-GB", {
                          hour: "numeric",
                          minute: "numeric",
                        })
                      : ""}
                  </Typography>
                </Box>

                <Box display="flex" sx={{ paddingLeft: 4 }}>
                  <Typography sx={{ minWidth: "220px" }} variant="h6">
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.calculatedCompletionDate",
                      "Calculated Completion Date:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                  >
                    {jobRunUnderProcessing?.estimatedCompletionDate
                      ? new Date(
                          jobRunUnderProcessing.estimatedCompletionDate
                        ).toLocaleDateString(language === "ar" ? "ar-EG" : "en-GB", {
                          hour: "numeric",
                          minute: "numeric",
                        })
                      : ""}
                  </Typography>
                </Box>

                <Box display="flex" sx={{ paddingLeft: 4 }}>
                  <Typography sx={{ minWidth: "220px" }} variant="h6">
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.actualCompletionDate",
                      "Actual Completion Date:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                  >
                    {jobRunUnderProcessing?.actualCompletionDate
                      ? new Date(
                          jobRunUnderProcessing.actualCompletionDate
                        ).toLocaleDateString(language === "ar" ? "ar-EG" : "en-GB", {
                          hour: "numeric",
                          minute: "numeric",
                        })
                      : ""}
                  </Typography>
                </Box>
              </Box>
            </Grid>
          </Grid>
          <Grid item xs={6}>
            <Grid container spacing={2} alignItems="center">
              <Box display="flex" flexDirection="column">
                <Box display="flex">
                  <Typography
                    sx={{ minWidth: "200px" }}
                    color="black"
                    variant="h6"
                  >
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.status",
                      "Status:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="blue"
                    variant="h6"
                  >
                    {jobRunUnderProcessing?.currentStatusDescription}
                  </Typography>
                </Box>

                <Box display="flex">
                  <Typography
                    sx={{ minWidth: "200px" }}
                    color="black"
                    variant="h6"
                  >
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.facilityName",
                      "Facility Name:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                  >
                    {jobRunUnderProcessing?.facilityName}
                  </Typography>
                </Box>

                <Box display="flex">
                  <Typography
                    sx={{ minWidth: "200px" }}
                    color="black"
                    variant="h6"
                  >
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.productionRunName",
                      "Production Run Name:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                  >
                    {jobRunUnderProcessing?.workEffortName}
                  </Typography>
                </Box>

                {/*<Box display="flex">
                  <Typography
                    sx={{ minWidth: "200px" }}
                    color="black"
                    variant="h6"
                  >
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.produced",
                      "Produced:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                  >
                    {jobRunUnderProcessing?.quantityProduced}
                  </Typography>
                </Box>*/}

                {/*<Box display="flex">
                  <Typography
                    sx={{ minWidth: "200px" }}
                    color="black"
                    variant="h6"
                  >
                    {getTranslatedLabel(
                      "manufacturing.jobshop.display.rejected",
                      "Rejected:"
                    )}
                  </Typography>
                  <Typography
                    sx={{ fontWeight: "bold", marginLeft: 1 }}
                    color="black"
                    variant="h6"
                  >
                    {jobRunUnderProcessing?.quantityRejected}
                  </Typography>
                </Box>*/}
              </Box>
            </Grid>
          </Grid>
          <Grid item xs={1} sx={{ pt: 0 }}>
            <Menu onSelect={handleMenuSelect}>
              <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                {/* Always show the Create option */}
                <MenuItem
                    text={getTranslatedLabel("manufacturing.jobshop.list.createProductionRun", "Create Production Run")}
                    data={"create"}
                />
                {canShowCancel && (
                    <MenuItem
                        text={getTranslatedLabel("manufacturing.jobshop.list.cancel", "Cancel Production Run")}
                        data={"cancel"}
                    />
                )}
                {canShowQuickCloseComplete && (
                    <MenuItem
                        text={getTranslatedLabel("manufacturing.jobshop.list.quickClose", "Quick Close")}
                        data={"quickClose"}
                    />
                )}
                {canShowQuickCloseComplete && (
                    <MenuItem
                        text={getTranslatedLabel("manufacturing.jobshop.list.quickComplete", "Quick Complete")}
                        data={"quickComplete"}
                    />
                )}
                {canShowQuickRunAllStart && (
                    <MenuItem
                        text={getTranslatedLabel("manufacturing.jobshop.list.quickRunAllTasks", "Quick Run All Tasks")}
                        data={"quickRunAllTasks"}
                    />
                )}
                {canShowQuickRunAllStart && (
                    <MenuItem
                        text={getTranslatedLabel("manufacturing.jobshop.list.quickStartAllTasks", "Quick Start All Tasks")}
                        data={"quickStartAllTasks"}
                    />
                )}
                {canShowQuickCloseFromComplete && (
                    <MenuItem
                        text={getTranslatedLabel("manufacturing.jobshop.list.quickClose", "Quick Close")}
                        data={"quickCloseFromComplete"}
                    />
                )}
              </MenuItem>
            </Menu>
          </Grid>

          <Grid container sx={{ pt: 5, pl: 3 }}>
            <Grid item xs={10}>
              <ProductionRunTasksList
                productionRunId={jobRunUnderProcessing?.workEffortId}
                productId={product}
                startTask={startTask}
                completeTask={completeTask}
                declareTask={declareTask}
                issueTaskQoh={issueTaskQoh}
                reserveTaskQoh={reserveTaskQoh}
              />
            </Grid>
            <Grid item xs={2}>
              <Grid
                container
                item
                xs={2}
                flexDirection={"column"}
                justifyContent={"center"}
              >
                <Menu onSelect={handleMenuSelect} vertical={true}>
                  <MenuItem
                    text={getTranslatedLabel(
                      "manufacturing.jobshop.list.materials",
                      "Materials"
                    )}
                    data={"material"}
                  />
                </Menu>
                {/*<Menu onSelect={handleMenuSelect} vertical={true}>
                  <MenuItem
                    text={getTranslatedLabel(
                      "manufacturing.jobshop.list.parties",
                      "Parties"
                    )}
                    data="parties"
                  />
                </Menu>*/}
                <Menu onSelect={handleMenuSelect} vertical={true}>
                  <MenuItem
                    text={getTranslatedLabel(
                      "manufacturing.jobshop.list.actualCost",
                      "Actual Cost"
                    )}
                    data="cost"
                  />
                </Menu>
                { (jobRunUnderProcessing?.currentStatusId === "PRUN_COMPLETED" || inventoryProduced) && <Menu onSelect={handleMenuSelect} vertical={true}>
                  <MenuItem
                    text={getTranslatedLabel(
                      "manufacturing.jobshop.list.inv",
                      "Produced Inventory"
                    )}
                    data="inv"
                  />
                </Menu>}
                { (jobRunUnderProcessing?.currentStatusId === "PRUN_COMPLETED" || inventoryProduced) && <Menu onSelect={handleMenuSelect} vertical={true}>
                  <MenuItem
                    text={getTranslatedLabel(
                      "manufacturing.jobshop.list.ret",
                      "Return Unused Materials"
                    )}
                    data="ret"
                  />
                </Menu>}
              </Grid>
            </Grid>
          </Grid>
        </Grid>
        {isLoading && (
            <LoadingComponent message={getTranslatedLabel("manufacturing.jobshop.edit.processing", "Processing Production Run...")} />
        )}
      </Paper>
    </>
  );
}
