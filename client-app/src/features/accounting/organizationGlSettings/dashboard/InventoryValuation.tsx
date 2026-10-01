import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { useMemo, useState } from "react";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridFilterChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import {
  CompositeFilterDescriptor,
  filterBy,
  orderBy,
  SortDescriptor,
  State,
} from "@progress/kendo-data-query";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { toast } from "react-toastify";

import AccountingMenu from "../../invoice/menu/AccountingMenu";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
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
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { formatNumber } from "../../../../app/util/utils";
import {
  InventoryValuationExcel,
  InventoryValuationRow,
} from "../report/InventoryValuationExcel";

type ReportParams = {
  facilityId?: string;
  productId?: string;
  dateThru?: string;
};

const InventoryValuation = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  // The key root is "accounting" — this file used to ask for "acconting.orgGL.reports...",
  // which matches nothing, so the three form labels silently fell back to their English
  // defaults even with the UI in Arabic. The date label is `.date`, not `.dateThru`.
  const localizationKey = "accounting.orgGL.reports.inventory-valuation";

  const { data: facilityList } = useFetchFacilitiesQuery(undefined);
  const { selectedAccountingCompanyId, selectedAccountingCompanyName } = useAppSelector(
    (state) => state.accountingSharedUi
  );

  const [params, setParams] = useState<ReportParams>({});

  const initialSort: SortDescriptor[] = [
    { field: "productName", dir: "asc" },
    { field: "accountingQuantitySum", dir: "desc" },
  ];
  const [sort, setSort] = useState<SortDescriptor[]>(initialSort);
  const [filter, setFilter] = useState<CompositeFilterDescriptor | undefined>(undefined);
  const [page, setPage] = useState<State>({ skip: 0, take: 20 });

  const {
    data: report,
    isSuccess,
    isLoading,
    isFetching,
  } = useFetchInventoryValuationReportQuery(
    {
      organizationPartyId: selectedAccountingCompanyId!,
      facilityId: params.facilityId,
      productId: params.productId,
      dateThru: params.dateThru,
    },
    {
      // The form lets you run on a facility, a product, or both — gating on facility alone
      // meant a product-only search silently did nothing.
      skip: !params.facilityId && !params.productId,
    }
  );

  const rows: InventoryValuationRow[] = useMemo(
    () =>
      (report?.items ?? []).map((i: any) => ({
        productId: i.productId ?? "",
        productName: i.productName ?? "",
        quantityUomDescription: i.quantityUomDescription ?? "",
        unitCost: i.unitCost ?? 0,
        currencyUomId: i.currencyUomId ?? "",
        accountingQuantitySum: i.accountingQuantitySum ?? 0,
        quantityOnHandSum: i.quantityOnHandSum ?? 0,
        value: i.value ?? 0,
      })),
    [report]
  );

  const view = useMemo(() => {
    const filtered = filterBy(rows, filter);
    return {
      data: orderBy(filtered, sort).slice(page.skip!, page.skip! + page.take!),
      total: filtered.length,
      // Total of the filtered set, so the figure agrees with what is on screen. The report's
      // own TotalValue still shows alongside it when a filter narrows the list.
      filteredValue: filtered.reduce((sum, r) => sum + (r.value ?? 0), 0),
    };
  }, [rows, filter, sort, page]);

  const facilityName = useMemo(
    () =>
      (facilityList ?? []).find((f: any) => f.facilityId === params.facilityId)?.facilityName,
    [facilityList, params.facilityId]
  );

  const handleSubmitForm = (values: any) => {
    const { facilityId, productId, dateThru } = values;

    if (!facilityId && !productId) {
      toast.error(
        getTranslatedLabel(
          `${localizationKey}.facility-or-product-required`,
          "Select a facility or a product to run the report."
        )
      );
      return;
    }

    // Assigned unconditionally: the old code only wrote each field when it had a value, so a
    // product or date chosen once could never be cleared again without a page reload.
    setParams({
      facilityId: facilityId || undefined,
      productId: productId?.productId || undefined,
      dateThru: dateThru ? new Date(dateThru).toISOString() : undefined,
    });
    setPage({ skip: 0, take: page.take });
    setFilter(undefined);
  };

  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
    return null;
  }

  return (
    <>
      <AccountingMenu selectedMenuItem={"/orgGl"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container padding={2} columnSpacing={1}>
          <AccountingReportBreadcrumbs />

          <Grid item xs={12} ml={3}>
            <Typography variant="h4" sx={{ mb: 2 }}>
              {getTranslatedLabel(`${localizationKey}.title`, "Inventory Valuation For: ")}
              {selectedAccountingCompanyName}
            </Typography>
          </Grid>

          <Grid item xs={11} ml={3}>
            <Form
              onSubmit={(values) => handleSubmitForm(values)}
              render={(formRenderProps) => (
                <FormElement>
                  <fieldset className={"k-form-fieldset"}>
                    <Grid container spacing={2} sx={{ marginBottom: 2 }} alignItems={"end"}>
                      <Grid item xs={3}>
                        <Field
                          id={"facilityId"}
                          name={"facilityId"}
                          label={getTranslatedLabel(`${localizationKey}.facility`, "Facility")}
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
                          label={getTranslatedLabel(`${localizationKey}.product`, "Product")}
                          component={FormComboBoxVirtualGetPhysicalInventoryProductsLovProduct}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Field
                          id={"dateThru"}
                          name={"dateThru"}
                          label={getTranslatedLabel(`${localizationKey}.date`, "Date Thru")}
                          component={FormDatePicker}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Button
                          variant="contained"
                          type="submit"
                          color="success"
                          disabled={
                            !formRenderProps.valueGetter("facilityId") &&
                            !formRenderProps.valueGetter("productId")
                          }
                        >
                          {getTranslatedLabel("general.generate", "Generate Report")}
                        </Button>
                      </Grid>
                    </Grid>
                  </fieldset>
                </FormElement>
              )}
            />
          </Grid>

          {isSuccess && report && (
            <Grid item xs={12}>
              <div className="div-container">
                <KendoGrid
                  scrollable="scrollable"
                  className="main-grid"
                  data={view.data}
                  resizable={true}
                  sortable={true}
                  sort={sort}
                  onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                  filterable={true}
                  filter={filter}
                  onFilterChange={(e: GridFilterChangeEvent) => {
                    setFilter(e.filter);
                    setPage({ ...page, skip: 0 });
                  }}
                  pageable={true}
                  skip={page.skip}
                  take={page.take}
                  total={view.total}
                  onPageChange={(e: GridPageChangeEvent) => setPage(e.page)}
                >
                  <GridToolbar>
                    <Typography variant="body1" fontWeight={"bold"}>
                      {getTranslatedLabel(`${localizationKey}.totalValue`, "Total Value")}:{" "}
                      <Box component="span" color="success.main">
                        {formatNumber(report.totalValue)}
                      </Box>
                    </Typography>
                    {filter && view.total !== rows.length && (
                      <Typography variant="body1">
                        {getTranslatedLabel(`${localizationKey}.filteredValue`, "Filtered Value")}:{" "}
                        <Box component="span" fontWeight="bold">
                          {formatNumber(view.filteredValue)}
                        </Box>
                      </Typography>
                    )}

                    <InventoryValuationExcel
                      companyName={selectedAccountingCompanyName ?? ""}
                      rows={rows}
                      totalValue={report.totalValue ?? 0}
                      facilityName={facilityName}
                      thruDate={params.dateThru}
                      getTranslatedLabel={getTranslatedLabel}
                      isFetching={isFetching}
                    />
                  </GridToolbar>

                  <Column
                    field="productName"
                    title={getTranslatedLabel(`${localizationKey}.product`, "Product")}
                    locked
                    width={240}
                  />
                  <Column
                    field="quantityUomDescription"
                    title={getTranslatedLabel(`${localizationKey}.quantityUom`, "Quantity UOM")}
                    width={180}
                  />
                  <Column
                    field="unitCost"
                    title={getTranslatedLabel(`${localizationKey}.unitCost`, "Unit Cost")}
                    format="{0:n2}"
                    filter={"numeric"}
                  />
                  <Column
                    field="currencyUomId"
                    title={getTranslatedLabel(`${localizationKey}.currency`, "Currency")}
                  />
                  <Column
                    field="accountingQuantitySum"
                    title={getTranslatedLabel(
                      `${localizationKey}.accountingQuantity`,
                      "Accounting Quantity Sum"
                    )}
                    format="{0:n2}"
                    filter={"numeric"}
                  />
                  <Column
                    field="quantityOnHandSum"
                    title={getTranslatedLabel(`${localizationKey}.quantityOnHand`, "QOH Sum")}
                    format="{0:n2}"
                    filter={"numeric"}
                  />
                  <Column
                    field="value"
                    title={getTranslatedLabel(`${localizationKey}.value`, "Value")}
                    format="{0:n2}"
                    filter={"numeric"}
                  />
                </KendoGrid>
              </div>
            </Grid>
          )}

          {(isLoading || isFetching) && (
            <LoadingComponent
              message={getTranslatedLabel("general.loading-report", "Loading Report Data...")}
            />
          )}
        </Grid>
      </Paper>
    </>
  );
};

export default InventoryValuation;
