import React from "react";
import {NavLink} from "react-router-dom";
import {Box, List, ListItem, Toolbar} from "@mui/material";

const links = [
    {title: "Inventory", path: "/quotes"},
    {title: "Inventory Item", path: "/requests"},
    {title: "Inventory Item Details", path: "/orders"},
];

const navStyles = {
    color: "inherit",
    textDecoration: "none",
    typography: "h18",
    "&:hover": {
        color: "grey.500",
    },
    "&.active": {
        color: "text.secondary",
    },
};

export default function InventoryItemMenu() {
    return (
        <Toolbar
            sx={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "left",
            }}
        >
            <Box display="flex" alignItems="left">
                <List sx={{display: "flex"}}>
                    {links.map(({title, path}) => (
                        <ListItem component={NavLink} to={path} key={path} sx={navStyles}>
                            {title.toUpperCase()}
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Toolbar>
    );
}
