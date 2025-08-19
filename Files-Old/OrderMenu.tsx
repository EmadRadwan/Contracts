import {Menu} from "semantic-ui-react";
import {NavLink} from "react-router-dom";

export default function OrderMenu() {
    console.count("OrderMenu Rendered");
    return (
        <Menu attached='top' tabular>
            <Menu.Item name="Orders" activeClassName="active" as={NavLink} exact to="/parties"/>
            <Menu.Item name="Orders" as={NavLink} exact to="/manage/"/>
        </Menu>
    )
}