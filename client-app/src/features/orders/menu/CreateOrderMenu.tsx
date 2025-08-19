import React from 'react'
import {Container, Menu} from 'semantic-ui-react'
import {NavLink} from "react-router-dom";
import '../../../app/layout/styles.css';


export default function CreateOrderMenu() {

    return (
        <Menu attached='top' tabular>
            <Container>
                <Menu.Menu position='right'>
                    <Menu.Item name="Orders" className="productMenu" as={NavLink} exact to='/partyContacts'/>
                </Menu.Menu>
            </Container>

        </Menu>
    )
}