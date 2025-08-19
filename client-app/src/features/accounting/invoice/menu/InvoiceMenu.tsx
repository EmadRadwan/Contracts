import React from 'react'
import {Container, Menu} from 'semantic-ui-react'
import {NavLink} from "react-router-dom";
import '../../../../app/layout/styles.css';


const InvoiceMenu = () => {

    return (
        <Menu attached='top' tabular>
            <Container>
                <Menu.Menu position='right'>
                    <Menu.Item name="Items" className="invoiceMenu" as={NavLink}
                               to='/invoiceItems'/>
                    <Menu.Item name="Applications" className="invoiceMenu" as={NavLink}
                               to='/invoicePayments'/>
                </Menu.Menu>
            </Container>

        </Menu>
    )
}

export default InvoiceMenu