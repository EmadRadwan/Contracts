import { Button, Grid, Paper, Typography } from "@mui/material";
import React, { useState } from "react";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import {
  useFetchFacilitiesQuery,
  useFetchInventoryValuationReportQuery,
} from "../../../../app/store/apis";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { useAppSelector } from "../../../../app/store/configureStore";
import { router } from "../../../../app/router/Routes";
import { FormComboBoxVirtualGetPhysicalInventoryProductsLovProduct } from "../../../../app/common/form/FormComboBoxVirtualGetPhysicalInventoryProductsLovProduct";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";

const InventoryValuation = () => {
  const initialSort: Array<SortDescriptor> = [
    { field: "productName", dir: "asc" },
    { field: "accountingQuantitySum", dir: "desc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 9 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  const { getTranslatedLabel } = useTranslationHelper();
  const { data: facilityList } = useFetchFacilitiesQuery(undefined);
  const { selectedAccountingCompanyId } = useAppSelector(
    (state) => state.accountingSharedUi
  );
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }
  const [productId, setProductId] = useState<string | undefined>("");
  const [dateThru, setDateThru] = useState<string | undefined>("");
  const [facilityId, setFacilityId] = useState<string | undefined>("");
  const {
    data: inventoryValuationData,
    isSuccess,
    isLoading,
  } = useFetchInventoryValuationReportQuery(
    {
      organizationPartyId: selectedAccountingCompanyId!,
      facilityId,
      productId,
      dateThru,
    },
    {
      skip: !facilityId,
    }
  );

  const handleSubmitForm = (values: any) => {
    const { facilityId, productId, dateThru } = values;
    if (facilityId) setFacilityId(facilityId);
    if (productId) setProductId(productId.productId);
    if (dateThru) setDateThru(new Date(dateThru).toISOString());
  };
  return (
    <>
      <AccountingMenu selectedMenuItem={"/orgGl"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container padding={2} columnSpacing={1}>
          <AccountingReportBreadcrumbs />
          <Grid item xs={11} ml={3}>
            <Form
              onSubmit={(values) => handleSubmitForm(values)}
              render={(formRenderProps) => (
                <FormElement>
                  <fieldset className={"k-form-fieldset"}>
                    <Grid
                      container
                      spacing={2}
                      sx={{ marginBottom: 2 }}
                      alignItems={"end"}
                    >
                      <Grid item xs={3}>
                        <Field
                          id={"facilityId"}
                          name={"facilityId"}
                          label={getTranslatedLabel(
                            "acconting.orgGL.reports.inventory-valuation.facility",
                            "Facility"
                          )}
                          component={MemoizedFormDropDownList2}
                          data={facilityList ?? []}
                          dataItemKey={"facilityId"}
                          textField={"facilityName"}
                          autoComplete={"off"}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Field
                          id={"productId"}
                          name={"productId"}
                          label={getTranslatedLabel(
                            "acconting.orgGL.reports.inventory-valuation.product",
                            "Product"
                          )}
                          component={
                            FormComboBoxVirtualGetPhysicalInventoryProductsLovProduct
                          }
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Field
                          id={"dateThru"}
                          name={"dateThru"}
                          label={getTranslatedLabel(
                            "acconting.orgGL.reports.inventory-valuation.dateThru",
                            "Date Thru"
                          )}
                          component={FormDatePicker}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Button
                          variant="contained"
                          type="submit"
                          disabled={
                            !formRenderProps.valueGetter("facilityId") &&
                            !formRenderProps.valueGetter("productId")
                          }
                        >
                          {getTranslatedLabel(
                            "general.generate",
                            "Generate Report"
                          )}
                        </Button>
                      </Grid>
                    </Grid>
                  </fieldset>
                </FormElement>
              )}
            />
          </Grid>
          {isLoading && <LoadingComponent message={getTranslatedLabel("general.loading-report", "Loading Report Data...")} />}
          {isSuccess && (
            <KendoGrid
              data={orderBy(
                inventoryValuationData ? inventoryValuationData.items : [],
                sort
              ).slice(page.skip, page.take + page.skip)}
              sortable
              sort={sort}
              onSortChange={(e: GridSortChangeEvent) => {
                setSort(e.sort);
              }}
              skip={page.skip}
              take={page.take}
              total={
                inventoryValuationData ? inventoryValuationData.items.length : 0
              }
              pageable
              onPageChange={pageChange}
            >
              <GridToolbar>
                <Grid justifyContent={"center"}>
                  <Typography variant="body1" fontWeight={"bold"}>
                    Total Value: {inventoryValuationData.totalValue.toFixed(2)}
                  </Typography>
                </Grid>
              </GridToolbar>
              <Column field="productName" title="Product" locked width={200} />
              <Column
                field="quantityUomDescription"
                title="Quantity UOM"
                width={200}
              />
              <Column field="unitCost" format="{0:n2}" title="Unit Cost" />
              <Column field="currencyUomId" title="Currency" />
              <Column
                field="accountingQuantitySum"
                title="Accouting Quantity Sum"
                format="{0:n2}"
              />
              <Column
                field="quantityOnHandSum"
                format="{0:n2}"
                title="QOH Sum"
              />
              <Column field="value" format="{0:n2}" title="Value" />
            </KendoGrid>
          )}
        </Grid>
      </Paper>
    </>
  );
};

export default InventoryValuation;
