import React, { useState } from "react";
import { useAppSelector, useFetchInvoiceItemTypesQuery } from "../../../../../app/store/configureStore";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import {
  GridDataStateChangeEvent,
  GridSortChangeEvent,
  Grid as KendoGrid,
  GridColumn as Column,
  GridPageChangeEvent,
} from "@progress/kendo-react-grid";
import GlAccountDefaults from "./GlAccountDefaults";
import { Grid, Paper } from "@mui/material";
import SalesInvoiceAccountForm from "../../form/SalesInvoiceAccountForm";
import { router } from "../../../../../app/router/Routes";

const SalesInvoiceAccountList = () => {
    const selectedAccountingCompanyId = useAppSelector(
        (state) => state.accountingSharedUi.selectedAccountingCompanyId
      );

      if (!selectedAccountingCompanyId) {
        router.navigate("/orgGl");
      }

    let newDataState = {
        "filter": {
            "logic": "and",
            "filters": [
                {
                    "logic": "or",
                    "filters": [
                        {
                            "field": "parentTypeId",
                            "operator": "contains",
                            "value": "SINV"
                        },
                        {
                            "field": "parentTypeId",
                            "operator": "contains",
                            "value": "SINVOICE"
                        }
                    ]
                },
                {
                    "logic": "and",
                    "filters": [
                        {
                            "field": "description",
                            "operator": "contains",
                            "value": "(Sales)"
                        },
                        
                    ]
                }
            ]
        },
        "skip": 0,
        "take": 8
    };
    const [dataState, setDataState] = useState<State>(newDataState);
  const dataStateChange = (e: GridDataStateChangeEvent) => {
    console.log("dataStateChange", e.dataState);
    setDataState(e.dataState);
  };

  const { data: salesInvoiceTypes } = useFetchInvoiceItemTypesQuery({...dataState});
  console.log(salesInvoiceTypes);

  return (
    <>
      <GlAccountDefaults />
      <Grid container padding={2} columnSpacing={1} justifyContent={"center"}>
        <Grid item xs={8}>
          <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <SalesInvoiceAccountForm selectedAccountingCompanyId={selectedAccountingCompanyId} />
            <div className="div-container">
              <KendoGrid
                style={{ height: "65vh", flex: 1 }}
                data={
                  salesInvoiceTypes ? salesInvoiceTypes : { data: [], total: 0 }
                }
                sortable={true}
                pageable={true}
                filterable={true}
                resizable={true}
                {...dataState}
                onDataStateChange={dataStateChange}
              >
                <Column
                  field="description"
                  title="Description"
                  width={350}
                />
                <Column
                  field="defaultGlAccountId"
                  title="Gl Account"
                />
                {/* <Column cell={CommandCell} width="auto" /> */}
              </KendoGrid>
            </div>
          </Paper>
        </Grid>
      </Grid>
    </>
  );
};

export default SalesInvoiceAccountList;
