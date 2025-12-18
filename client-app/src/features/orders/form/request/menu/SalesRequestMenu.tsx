import React from 'react';
import { Box, List, ListItem, ListItemIcon, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import RequestQuoteOutlinedIcon from '@mui/icons-material/RequestQuoteOutlined';
import { useAppDispatch } from "../../../../../app/store/configureStore";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";

// ---------------------------------------------------------------
// Props – now includes the callback that notifies the list page
// ---------------------------------------------------------------
interface SalesRequestMenuProps {
    selectedMenuItem?: string;
    /** Called with the menu item key when a link is clicked */
    onMenuSelect?: (key: string) => void;
}

// ---------------------------------------------------------------
// Menu data
// ---------------------------------------------------------------
const links = [
    {
        title: 'Sales Requests',
        path: '/sales-requests',
        key: "salesRequest.menu.salesRequests",
        icon: <RequestQuoteOutlinedIcon sx={{ color: "#4CAF50" }} />
    },
    {
        title: 'Reserve Requests',
        path: '/reserve-requests',
        key: "salesRequest.menu.reserveRequests",
        icon: <RequestQuoteOutlinedIcon sx={{ color: "#4CAF50" }} />
    },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function SalesRequestMenu({ selectedMenuItem, onMenuSelect }: SalesRequestMenuProps) {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const theme = useTheme();

    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');

    // REFACTOR: Extracted shared NavLink styling into a reusable function
    const navStyles = (path: string) => {
        const normalizedPath = normalizePath(path);
        const isSelected = normalizedPath === normalizedSelectedMenuItem;
        return {
            color: isSelected ? theme.palette.primary.main : 'inherit',
            '&.active': { color: theme.palette.primary.main },
            textDecoration: "none",
            typography: "h6",
            "&:hover": { color: "grey.500" },
            fontWeight: isSelected ? "bold" : "normal",
            display: 'flex',
            alignItems: 'center',
            marginRight: '16px'
        };
    };

    // ---------------------------------------------------------------
    // Click handler – forwards the menu key to the parent list page
    // ---------------------------------------------------------------
    const handleClick = (key: string) => {
        if (onMenuSelect) {
            // REFACTOR: Notify parent (SalesRequestsList) that the menu was selected
            // Purpose: Allows the list to exit edit/add mode and show only the grid
            // Context: Works even when the route does not change
            onMenuSelect(key);
        }
    };

    return (
        <Toolbar sx={{ display: "flex", justifyContent: "space-between", alignItems: "left" }}>
            <Box display="flex" alignItems="left">
                <List sx={{ display: "flex" }}>
                    {links.map(({ title, path, icon, key }) => (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={path}
                            sx={navStyles(path)}
                            // REFACTOR: Pass the key via callback – no URL change required
                            onClick={() => handleClick(key)}
                        >
                            <ListItemIcon sx={{ minWidth: "unset", marginX: "4px", fontSize: 28 }}>
                                {icon}
                            </ListItemIcon>
                            <Typography variant="body1" sx={{ margin: 0 }}>
                                {getTranslatedLabel(key, title).toUpperCase()}
                            </Typography>
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Toolbar>
    );
}