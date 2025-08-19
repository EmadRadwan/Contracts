import AccountingMenu from "../../../invoice/menu/AccountingMenu"
import {Paper} from "@mui/material"
import GlAccountTypeDefaultsMenuNavContainer from "../../menu/GlAccountTypeDefaultsMenu";

const GlAccountDefaults = () => {


    return (
        <>
            <AccountingMenu selectedMenuItem={'orggl'}/>
            <Paper elevation={5}>
                <GlAccountTypeDefaultsMenuNavContainer selectedMenuItem={'glaccounttypedefaults'}/>
            </Paper>
        </>

    );
};

export default GlAccountDefaults