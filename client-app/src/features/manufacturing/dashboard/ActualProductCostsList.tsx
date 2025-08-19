import React, { useCallback, useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
  GridDetailRowProps,
  GridExpandChangeEvent,
  StatusBar, GridPageChangeEvent,
} from "@progress/kendo-react-grid";
import { DataResult, orderBy, State } from "@progress/kendo-data-query";
import { Grid, Paper, Typography } from "@mui/material";
import Badge from "@mui/material/Badge";
import { LoadingButton } from "@mui/lab";
import {
  useFetchActualCostComponentsQuery,
  useLazyFetchFOHCostQuery,
  useLazyFetchLaborCostQuery,
  useLazyFetchMaterialCostQuery,
} from "../../../app/store/apis";
import { handleDatesArray } from "../../../app/util/utils";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import LaborCostCalculationsList from "../../catalog/dashboard/productCost/LaborCostCalculationsList";
import ProductMaterialCostList from "../../catalog/dashboard/productCost/ProductMaterialCostList";
import FactoryOverheadCostList from "../../catalog/dashboard/productCost/FactoryOverheadCostList";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { CostComponent } from "../../../app/models/manufacturing/costComponent";

interface Props {
  productionRunId?: string | undefined;
  productId?: string | undefined;
}

export default function ActualProductCostsList({ productionRunId, productId }: Props) {
  const initialDataState: State = { skip: 0, take: 9 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  
  const DetailComponent = (props: GridDetailRowProps) => {
    const { costComponentTypeDescription, children } = props.dataItem;

    if (children) {
      return (
          <KendoGrid data={children}>
            <GridToolbar>
              <Typography variant="body1">
                {`Child Cost Components for ${costComponentTypeDescription} (${
                    children.length || 0
                })`}
              </Typography>
            </GridToolbar>
            <Column field="workEffortId" title="Work Effort Id" />
            <Column field="workEffortName" title="Work Effort" />
            <Column field="cost" title="Cost" format="{0:n2}" />
            <Column field="costUomId" title="Cost UOM" />
          </KendoGrid>
      );
    }
    return null;
  };

  const findAndModifyChild = (
      costComponentTypeId: string,
      costComponents: CostComponent[],
      expandedValue: boolean
  ): CostComponent[] => {
    return costComponents.map((cc: CostComponent) => {
      if (
          cc.costComponentTypeId === costComponentTypeId &&
          cc.children &&
          cc.children.length > 0
      ) {
        return { ...cc, expanded: expandedValue };
      }
      if (cc.children) {
        return {
          ...cc,
          children: findAndModifyChild(
              costComponentTypeId,
              cc.children,
              expandedValue
          ),
        };
      }
      return cc;
    });
  };

  const expandChange = (event: GridExpandChangeEvent) => {
    const selectedAccountId = event.dataItem.costComponentTypeId;
    const modifiedAccounts = findAndModifyChild(
        selectedAccountId,
        costComponents.data,
        event.value
    );
    setCostComponents({ data: modifiedAccounts, total: ccData?.total || 0 });
  };

  const [costComponents, setCostComponents] = useState<DataResult>({
    data: [],
    total: 0,
  });

  const [footerData, setFooterData] = useState({
    type: "Sum",
    value: 0,
    formattedValue: "0",
  });

  const { data: ccData, isFetching, isSuccess, isError } = useFetchActualCostComponentsQuery(
      productionRunId,
      {
        skip: !productionRunId,
      }
  );
  
  console.log('ccData', ccData)

  const [triggerMaterial, { data: materialCosts, isFetching: isMaterialCostsFetching }] = useLazyFetchMaterialCostQuery();
  const [triggerLabor, { data: laborCosts, isFetching: isLaborCostsFetching }] = useLazyFetchLaborCostQuery();
  const [triggerFOH, { data: fohCosts, isFetching: isFOHCostsFetching }] = useLazyFetchFOHCostQuery();

  const [showLabourCost, setShowLabourCost] = useState(false);
  const [showMaterialCost, setShowMaterialCost] = useState(false);
  const [showFohCost, setShowFohCost] = useState(false);

  useEffect(() => {
    if (ccData?.costComponents) {
      const adjustedData = handleDatesArray(ccData.costComponents).map((cc: CostComponent) => ({
        ...cc,
        expanded: false,
        children: cc.children || []
      }));
      setCostComponents({ data: adjustedData, total: ccData.costComponents.length });
    }
  }, [ccData]);

  useEffect(() => {
    if (costComponents.data.length > 0) {
      const type = "Sum";
      const value = costComponents.data
          .reduce((a: number, b: CostComponent) => a + (b.cost || 0), 0)
          .toFixed(2);
      const uom = costComponents.data[0].costUomId || "";
      const formattedValue = `${value} ${uom}`;
      setFooterData({ type, value, formattedValue });
    }
  }, [costComponents]);

  const memoizedOnClose = useCallback(() => {
    setShowLabourCost(false);
    setShowMaterialCost(false);
    setShowFohCost(false);
  }, []);

  const handleShowLaborModal = () => {
    if (productionRunId) {
      triggerLabor(productId);
      setShowLabourCost(true);
    }
  };

  const handleShowFOHModal = () => {
    if (productionRunId) {
      triggerFOH(productId);
      setShowFohCost(true);
    }
  };

  const handleShowMaterialModal = () => {
    if (productionRunId) {
      triggerMaterial(productId);
      setShowMaterialCost(true);
    }
  };

  // Aggregator counts from ccData
  const directLaborCount = ccData?.directLaborCount ?? 0;
  const fohCount = ccData?.fohCostCount ?? 0;
  const materialCount = ccData?.materialCostCount ?? 0;

  return (
      <>
        {showLabourCost && laborCosts && (
            <ModalContainer show={showLabourCost} onClose={memoizedOnClose} width={950}>
              <LaborCostCalculationsList
                  laborCosts={laborCosts}
                  onClose={() => setShowLabourCost(false)}
              />
            </ModalContainer>
        )}
        {showMaterialCost && materialCosts && (
            <ModalContainer show={showMaterialCost} onClose={memoizedOnClose} width={950}>
              <ProductMaterialCostList
                  materialCosts={materialCosts}
                  onClose={() => setShowMaterialCost(false)}
              />
            </ModalContainer>
        )}
        {showFohCost && fohCosts && (
            <ModalContainer show={showFohCost} onClose={memoizedOnClose} width={950}>
              <FactoryOverheadCostList
                  fohCosts={fohCosts}
                  onClose={() => setShowFohCost(false)}
              />
            </ModalContainer>
        )}
        <Paper elevation={5} className="div-container-withBorderCurved">
          <Grid container columnSpacing={1}>
            <Grid container alignItems="center">
              <Grid item xs={8}>
                <Typography sx={{ px: 4, py: 1 }} variant="h4">
                  Actual Costs for Production Run {productionRunId}
                </Typography>
              </Grid>
            </Grid>
            <Grid item xs={12}>
              <div className="div-container">
                <KendoGrid
                    style={{ height: "400px" }}
                    resizable={true}
                    skip={page.skip}
                    take={page.take}
                    total={costComponents.total}
                    pageable={true}
                    detail={DetailComponent}
                    expandField="expanded"
                    onExpandChange={expandChange}
                    onPageChange={pageChange}
                    data={orderBy(costComponents.data, []).slice(page.skip, page.take + page.skip)}
                >
                  <GridToolbar>
                    <Grid container py={1} spacing={2}>
                      <Grid item>
                        <Badge badgeContent={directLaborCount} color="secondary">
                          <LoadingButton
                              color="secondary"
                              onClick={handleShowLaborModal}
                              variant="outlined"
                              loading={isLaborCostsFetching}
                          >
                            Direct Labor Costs
                          </LoadingButton>
                        </Badge>
                      </Grid>
                      <Grid item>
                        <Badge badgeContent={fohCount} color="secondary">
                          <LoadingButton
                              color="secondary"
                              onClick={handleShowFOHModal}
                              variant="outlined"
                              loading={isFOHCostsFetching}
                          >
                            Factory Overhead Costs
                          </LoadingButton>
                        </Badge>
                      </Grid>
                      <Grid item>
                        <Badge badgeContent={materialCount} color="secondary">
                          <LoadingButton
                              color="secondary"
                              onClick={handleShowMaterialModal}
                              variant="outlined"
                              loading={isMaterialCostsFetching}
                          >
                            Material Costs
                          </LoadingButton>
                        </Badge>
                      </Grid>
                    </Grid>
                  </GridToolbar>
                  <Column
                      field="costComponentTypeDescription"
                      title="Cost Component"
                      width={600}
                  />
                  <Column field="cost" title="Cost" format="{0:n2}" />
                  <Column field="costUomId" title="Cost UOM" />
                  <StatusBar
                      data={[
                        {
                          type: footerData.type,
                          value: footerData.value,
                          formattedValue: footerData.formattedValue,
                        },
                      ]}
                  />
                </KendoGrid>
                {isFetching && (
                    <LoadingComponent message="Loading Actual Production Run Costs..." />
                )}
              </div>
            </Grid>
          </Grid>
        </Paper>
      </>
  );
}