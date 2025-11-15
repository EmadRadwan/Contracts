import React from 'react';
import { Box, List, ListItem, ListItemIcon, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import RequestQuoteOutlinedIcon from '@mui/icons-material/RequestQuoteOutlined';
import {useAppDispatch} from "../../../../../app/store/configureStore";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper"; // New icon for Sales Requests

interface SalesRequestMenuProps {
    selectedMenuItem?: string;
}

const links = [
    {
        title: 'Sales Requests',
        path: '/sales-requests',
        key: "salesRequest.menu.salesRequests",
        icon: <RequestQuoteOutlinedIcon sx={{ color: "#4CAF50" }} /> // Green for sales/finance
    },
    
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function SalesRequestMenu({ selectedMenuItem }: SalesRequestMenuProps) {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const theme = useTheme();

    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');

    // REFACTOR: Extracted shared NavLink styling into a reusable function to avoid duplication and improve maintainability
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