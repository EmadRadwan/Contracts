import {MemoizedFormCheckBox} from "../client-app/src/app/common/form/FormCheckBox";
import {setAddTax} from "../client-app/src/features/orders/slice/sharedOrderUiSlice";
import {Field} from "@progress/kendo-react-form";
import React from "react";

<Field
    id={"isChequesDelivered"}
    name={"isChequesDelivered"}
    label={getTranslatedLabel(`${localizationKey}.isChequesDelivered`, "isChequesDelivered")}
    component={MemoizedFormCheckBox}
/>