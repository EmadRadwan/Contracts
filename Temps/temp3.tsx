import React, { useEffect, memo } from "react";
import {
    TableRow,
    TableCell,
    TextField,
    IconButton,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import { MemoizedFormComboBox2 } from "../../../app/common/form/FormComboBox2";
import { FormDropDownTreeGlAccount3 } from "../../../app/common/form/FormDropDownTreeGlAccount3";
import { FormSimpleComboBoxServiceVirtual } from "../../../app/common/form/FormSimpleComboBoxServiceVirtual";
import { FormSimpleComboBoxRawMaterialVirtual } from "../../../app/common/form/FormSimpleComboBoxRawMaterialVirtual";
import { FormComboBoxVirtualAllParties } from "../../../app/common/form/FormComboBoxVirtualAllParties";

handleRowChange(index: number, field: keyof BulkAddRow, value: any, extraFields?: Partial<BulkAddRow>)