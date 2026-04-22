import {GridColumn as Column} from "@progress/kendo-react-grid/dist/npm/GridColumn";
import React from "react";

<Column key="total" field="totalAmount" title={getTranslatedLabel(`${itemFormLocalizationKey}.deserved`, "Deserved")} editable={false} width={110} cell={(p) => <td>{(p.dataItem.totalAmount || 0).toFixed(2)}</td>} />,
    <Column key="deserved" field="deserved" title="Deserved" editable={false} width={110} cell={(p) => <td>{(p.dataItem.deserved || 0).toFixed(2)}</td>} />

<Column key="achievementPercentage" field="achievementPercentage" title={getTranslatedLabel(`${itemFormLocalizationKey}.total`, "Total)} editor="numeric" width={110} />,

            