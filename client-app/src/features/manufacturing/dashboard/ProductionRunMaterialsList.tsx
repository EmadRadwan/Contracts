import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { Fragment } from "react";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import {
  useFetchIssueProductionRunDeclComponentsQuery,
  useFetchProductionRunMaterialsQuery,
} from "../../../app/store/apis";
import { Grid, Typography } from "@mui/material";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

interface Props {
  onClose: () => void;
  productionRunId?: string | undefined;
  currentStatusDescription?: string | undefined;
}

export default function ProductionRunMaterialsList({
  productionRunId,
  currentStatusDescription,
}: Props) {
  const initialSort: Array<SortDescriptor> = [
    { field: "productName", dir: "asc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 10 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const { getTranslatedLabel } = useTranslationHelper();

  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };

  // Determine which query to run based on currentStatusDescription
  const shouldUseMaterialsQuery = [
    "Created",
    "Scheduled",
    "Confirmed",
  ].includes(currentStatusDescription || "");
  const shouldUseIssueComponentsQuery = ["Running", "Completed"].includes(
    currentStatusDescription || ""
  ); //currentStatusDescription === "Running";

  // Call the materials query only if `productionRunId` is defined and `shouldUseMaterialsQuery` is true
  const { data: productionRunMaterialsData } =
    useFetchProductionRunMaterialsQuery(productionRunId, {
      skip: productionRunId === undefined || !shouldUseMaterialsQuery,
    });

  // Call the issue components query only if `productionRunId` is defined and `shouldUseIssueComponentsQuery` is true
  const { data: productionRunIssueMaterialsData } =
    useFetchIssueProductionRunDeclComponentsQuery(productionRunId, {
      skip: productionRunId === undefined || !shouldUseIssueComponentsQuery,
    });

  console.log("currentStatusDescription", currentStatusDescription);
  console.log("shouldUseMaterialsQuery", shouldUseMaterialsQuery);
  console.log("shouldUseIssueComponentsQuery", shouldUseIssueComponentsQuery);
  return (
    <Fragment>
      {/* Render the first grid when the status is 'Created', 'Scheduled', or 'Confirmed' */}
      {shouldUseMaterialsQuery && (
        <div className="div-container">
          <KendoGrid
            data={orderBy(
              productionRunMaterialsData ? productionRunMaterialsData : [],
              sort
            ).slice(page.skip, page.take + page.skip)}
            sortable={true}
            sort={sort}
            onSortChange={(e: GridSortChangeEvent) => {
              setSort(e.sort);
            }}
            skip={page.skip}
            take={page.take}
            total={
              productionRunMaterialsData ? productionRunMaterialsData.length : 0
            }
            pageable={true}
            onPageChange={pageChange}
            resizable={true}
          >
            <GridToolbar>
              <Grid container justifyContent={"center"}>
                  <Grid item xs={12}>
                      <Typography
                        color="primary"
                        sx={{ fontSize: "18px", color: "blue", fontWeight: "bold", textAlign: "center" }}
                        variant="h6"
                      >
                        {getTranslatedLabel(
                          "manufacturing.jobshop.materials.title",
                          "Materials"
                        )}
                      </Typography>
                  </Grid>
              </Grid>
            </GridToolbar>
            <Column
              field="productName"
              title={getTranslatedLabel(
                "manufacturing.jobshop.materials.productName",
                "Product Name"
              )}
              width={200}
            />
            <Column
              field="productQuantityUom"
              title={getTranslatedLabel(
                "manufacturing.jobshop.materials.productQuantityUom",
                "Unit of Measure"
              )}
              width={150}
            />
            <Column
              field="estimatedQuantity"
              title={getTranslatedLabel(
                "manufacturing.jobshop.materials.estimatedQuantity",
                "Estimated Quantity"
              )}
              width={200}
            />
          </KendoGrid>
        </div>
      )}

      {/* Render the second grid when the status is 'Running' */}
      {shouldUseIssueComponentsQuery && (
        <KendoGrid
          data={orderBy(
            productionRunIssueMaterialsData
              ? productionRunIssueMaterialsData
              : [],
            sort
          ).slice(page.skip, page.take + page.skip)}
          sortable={true}
          sort={sort}
          onSortChange={(e: GridSortChangeEvent) => {
            setSort(e.sort);
          }}
          skip={page.skip}
          take={page.take}
          total={
            productionRunIssueMaterialsData
              ? productionRunIssueMaterialsData.length
              : 0
          }
          pageable={true}
          onPageChange={pageChange}
          resizable={true}
        >
          <GridToolbar>
            <Grid container justifyContent={"center"}>
              <Grid item xs={12}>
                <Typography
                  color="primary"
                  sx={{ fontSize: "18px", color: "blue", fontWeight: "bold" }}
                  variant="h6"
                >
                  {getTranslatedLabel(
                    "manufacturing.jobshop.materials.issuedComponents",
                    "Issued Components"
                  )}
                </Typography>
              </Grid>
            </Grid>
          </GridToolbar>
          <Column
            field="productName"
            title={getTranslatedLabel(
              "manufacturing.jobshop.materials.productName",
              "Product Name"
            )}
            width={200}
          />
          <Column
            field="productQuantityUom"
            title={getTranslatedLabel(
              "manufacturing.jobshop.materials.productQuantityUom",
              "Unit of Measure"
            )}
            width={100}
          />
          <Column
            field="estimatedQuantity"
            title={getTranslatedLabel(
              "manufacturing.jobshop.materials.estimatedQuantity",
              "Estimated Quantity"
            )}
            width={100}
          />
          <Column
            field="issuedQuantity"
            title={getTranslatedLabel(
              "manufacturing.jobshop.materials.issuedQuantity",
              "Issued"
            )}
            width={100}
          />
        </KendoGrid>
      )}
    </Fragment>
  );
}
