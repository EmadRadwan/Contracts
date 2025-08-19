import React, { useState } from "react";
import { useAppSelector, useFetchInvoiceItemTypesQuery } from "../../../../../app/store/configureStore";
import { State } from "@progress/kendo-data-query";
import {
  GridDataStateChangeEvent,
  Grid as KendoGrid,
  GridColumn as Column,
} from "@progress/kendo-react-grid";
import GlAccountDefaults from "./GlAccountDefaults";
import { Grid, Paper } from "@mui/material";
import { router } from "../../../../../app/router/Routes";
import PurchaseInvoiceAccountForm from "../../form/PurchaseInvoiceAccountForm";

const PurchaseInvoiceAccountList = () => {
    const selectedAccountingCompanyId = useAppSelector(
        (state) => state.accountingSharedUi.selectedAccountingCompanyId
      );

      if (!selectedAccountingCompanyId) {
        router.navigate("/orgGl");
      }

    let newDataState = {
        "filter": {
            "logic": "or",
            "filters": [
                {
                    "logic": "or",
                    "filters": [
                        {
                            "field": "parentTypeId",
                            "operator": "contains",
                            "value": "PINV"
                        },
                        {
                            "field": "parentTypeId",
                            "operator": "contains",
                            "value": "PINVOICE"
                        }
                    ]
                },
                {
                    "logic": "and",
                    "filters": [
                        {
                            "field": "description",
                            "operator": "contains",
                            "value": "(Purchase)"
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

  const { data: purchaseInvoiceTypes } = useFetchInvoiceItemTypesQuery({...dataState});
  console.log(purchaseInvoiceTypes);

  return (
    <>
      <GlAccountDefaults />
      <Grid container padding={2} columnSpacing={1} justifyContent={"center"}>
        <Grid item xs={8}>
          <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <PurchaseInvoiceAccountForm selectedAccountingCompanyId={selectedAccountingCompanyId} />
            <div className="div-container">
              <KendoGrid
                style={{ height: "65vh", flex: 1 }}
                data={
                  purchaseInvoiceTypes ? purchaseInvoiceTypes : { data: [], total: 0 }
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

export default PurchaseInvoiceAccountList;
