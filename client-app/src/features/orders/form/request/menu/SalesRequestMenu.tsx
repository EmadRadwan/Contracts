import React from 'react';
import { Box, List, ListItem, ListItemIcon, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import RequestQuoteOutlinedIcon from '@mui/icons-material/RequestQuoteOutlined';
import PercentIcon from '@mui/icons-material/Percent';
import { useAppDispatch } from "../../../../../app/store/configureStore";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import {Can} from "../../../../account/Can";

// ---------------------------------------------------------------
// Props – includes callback that notifies the list page
// ---------------------------------------------------------------
interface SalesRequestMenuProps {
    selectedMenuItem?: string;
    /** Called with the menu item key when a link is clicked */
    onMenuSelect?: (key: string) => void;
}

// ---------------------------------------------------------------
// Menu data – now with requiredRole for each item
// ---------------------------------------------------------------
const links = [
    {
        title: 'Sales Requests',
        path: '/sales-requests',
        key: "salesRequest.menu.salesRequests",
        translationKey: "salesRequest.menu.salesRequests",
        icon: <RequestQuoteOutlinedIcon sx={{ color: "#4CAF50" }} />,
        requiredRole: "CreateSalesRequest" as const,
    },
    {
        title: 'Reserve Requests',
        path: '/reserve-requests',
        key: "salesRequest.menu.reserveRequests",
        translationKey: "salesRequest.menu.reserveRequests",
        icon: <RequestQuoteOutlinedIcon sx={{ color: "#4CAF50" }} />,
        requiredRole: "CreateReserveRequest" as const,
    },
    {
        title: 'Commission Rates',
        path: '/project-commission-rates',
        key: "salesRequest.menu.commissionRates",
        translationKey: "salesRequest.menu.commissionRates",
        icon: <PercentIcon sx={{ color: "#2196F3" }} />,
        requiredRole: "CreateSalesRequest" as const,
    },
    {
        title: 'Sales Commissions',
        path: '/sales-commissions',
        key: "salesRequest.menu.salesCommissions",
        translationKey: "salesRequest.menu.salesCommissions",
        icon: <PercentIcon sx={{ color: "#FF9800" }} />,
        requiredRole: "CreateSalesRequest" as const,
    },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function SalesRequestMenu({ selectedMenuItem, onMenuSelect }: SalesRequestMenuProps) {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const theme = useTheme();

    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');

    // REFACTOR: Extracted shared NavLink styling into a reusable function (consistent with AccountingMenu)
    // Purpose: Centralize style logic, ensure visual consistency across menus, and simplify future changes
    const getNavItemStyles = (isSelected: boolean) => ({
        color: isSelected ? theme.palette.primary.main : 'inherit',
        textDecoration: "none",
        typography: "h6",
        "&:hover": { color: "grey.500" },
        fontWeight: isSelected ? "bold" : "normal",
        display: 'flex',
        alignItems: 'center',
        marginRight: '16px',
    });

    // ---------------------------------------------------------------
    // Click handler – forwards the menu key to the parent list page
    // ---------------------------------------------------------------
    const handleClick = (key: string) => {
        if (onMenuSelect) {
            // REFACTOR: Notify parent that the menu was selected
            // Purpose: Allows the list page to exit edit/add mode even when route doesn't change
            onMenuSelect(key);
        }
    };

    return (
        <Toolbar sx={{ display: "flex", justifyContent: "space-between", alignItems: "left" }}>
            <Box display="flex" alignItems="left">
                <List sx={{ display: "flex" }}>
                    {links.map((link) => (
                        // REFACTOR: Wrapped each item in <Can> to enforce role-based visibility
                        // Purpose: Hide menu items the user is not authorized to access
                        // Improves: Security and UI cleanliness – matches pattern used in AccountingMenu
                        <Can perform={link.requiredRole} key={link.path}>
                            {(() => {
                                const isSelected = normalizePath(link.path) === normalizedSelectedMenuItem;

                                return (
                                    <ListItem
                                        component={NavLink}
                                        to={link.path}
                                        sx={getNavItemStyles(isSelected)}
                                        onClick={() => handleClick(link.key)}
                                        disablePadding
                                    >
                                        <ListItemIcon sx={{ minWidth: "unset", marginX: "4px", fontSize: 28 }}>
                                            {link.icon}
                                        </ListItemIcon>
                                        <Typography variant="body1" sx={{ margin: 0 }}>
                                            {getTranslatedLabel(link.translationKey, link.title).toUpperCase()}
                                        </Typography>
                                    </ListItem>
                                );
                            })()}
                        </Can>
                    ))}
                </List>
            </Box>
        </Toolbar>
    );
}