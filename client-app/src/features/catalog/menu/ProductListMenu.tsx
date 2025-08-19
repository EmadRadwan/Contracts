import React from 'react'
import {observer} from "mobx-react-lite";
import {NavLink} from "react-router-dom";
import '../../../app/layout/styles.css';
import {Box, List, ListItem, Toolbar} from "@mui/material";


const navStyles = {
    color: 'inherit',
    textDecoration: 'none',
    typography: 'button',
    '&:hover': {
        color: 'blue.500'
    },
    '&.active': {
        color: 'text.secondary'
    }
}

const midLinks = [
    {title: 'New Product', path: '/createProduct'},


]

export default observer(function ProductListMenu() {

    return (

        <Toolbar sx={{display: 'flex', justifyContent: '', alignItems: 'center'}}>

            <Box display='flex' alignItems='center'>
                <List sx={{display: 'block flex'}}>
                    {midLinks.map(({title, path}) => (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={path}
                            sx={navStyles}
                        >
                            {title}
                        </ListItem>
                    ))}

                </List>
            </Box>


        </Toolbar>
    )
})