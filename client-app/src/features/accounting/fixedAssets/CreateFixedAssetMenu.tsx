import React from 'react'
import {Container, Menu} from 'semantic-ui-react'
import {NavLink} from "react-router-dom";
import '../../../app/layout/styles.css';


export default function CreateFixedAssetMenu() {

    return (
        <Menu attached='top' tabular>
            <Container>
                <Menu.Menu position='right'>
                    {/* <Menu.Item name="Fixed Asset" className="fixedAssetMenu" as={NavLink}
                               to='/fixedAsset'/> */}
                    <Menu.Item name="Standard Costs" className="fixedAssetMenu" as={NavLink}
                               to='/standardCosts'/>
                    {/* <Menu.Item name="Calendar" className="fixedAssetMenu" as={NavLink}
                               to='/calendar'/>
                    <Menu.Item name="Maintenances" className="fixedAssetMenu" as={NavLink}
                               to='/maintenances'/> */}
                    <Menu.Item name="Depreciation" className="fixedAssetMenu" as={NavLink}
                               to='/depreciation'/>
                </Menu.Menu>
            </Container>

        </Menu>
    )
}