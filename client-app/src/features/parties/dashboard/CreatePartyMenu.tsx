import React from 'react'
import {Menu} from 'semantic-ui-react'
import {observer} from "mobx-react-lite";
import {NavLink} from "react-router-dom";

export default observer(function CreatePartyMenu() {

    return (
        <Menu attached='top' tabular>
            <Menu.Item name="Create Customer" activeClassName="active" as={NavLink} exact to="/createCustomer"/>
            <Menu.Item name="Create Employee" as={NavLink} exact to="/createCustomer/"/>
        </Menu>
    )
})