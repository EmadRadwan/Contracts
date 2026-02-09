import React from 'react';
import { Box, List, ListItem, ListItemIcon, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import PaidOutlinedIcon from '@mui/icons-material/PaidOutlined';
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {useAppDispatch} from "../../../app/store/configureStore";
import {Can} from "../../account/Can";           // good icon for advances/payments

// ---------------------------------------------------------------
// Props – same pattern as SalesRequestMenu
// ---------------------------------------------------------------
interface EmployeeAdvanceMenuProps {
    selectedMenuItem?: string;
    /** Called with the menu item key when a link is clicked */
    onMenuSelect?: (key: string) => void;
}

// ---------------------------------------------------------------
// Menu items – add more sub-items later if needed
// ---------------------------------------------------------------
const links = [
    {
        title: 'Employee Advances',
        path: '/employee-advances',
        key: "employeeAdvance.menu.advances",
        translationKey: "employeeAdvance.menu.advances",
        icon: <PaidOutlinedIcon sx={{ color: "#2196F3" }} />, // blue-ish for money/advances
        requiredRole: "ViewEmployeeAdvances" as const,        // adjust to your actual permission
    },
    // You can easily add more later, e.g.:
    // {
    //   title: 'Advance Requests',
    //   path: '/advance-requests',
    //   key: "employeeAdvance.menu.requests",
    //   translationKey: "employeeAdvance.menu.requests",
    //   icon: <RequestPageOutlinedIcon sx={{ color: "#FF9800" }} />,
    //   requiredRole: "CreateEmployeeAdvance" as const,
    // },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function EmployeeAdvanceMenu({
                                                selectedMenuItem,
                                                onMenuSelect,
                                            }: EmployeeAdvanceMenuProps) {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch(); // kept for consistency, even if not used now
    const theme = useTheme();

    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');

    // Reusable style function – same as in SalesRequestMenu
    const getNavItemStyles = (isSelected: boolean) => ({
        color: isSelected ? theme.palette.primary.main : 'inherit',
        textDecoration: "none",
        typography: "h6",
        "&:hover": {
            color: "grey.500",
        },
        fontWeight: isSelected ? "bold" : "normal",
        display: 'flex',
        alignItems: 'center',
        marginRight: '16px',
    });

    // Click handler – notifies parent (list page) to exit form mode if needed
    const handleClick = (key: string) => {
        if (onMenuSelect) {
            onMenuSelect(key);
        }
    };

    return (
        <Toolbar sx={{ display: "flex", justifyContent: "space-between", alignItems: "left" }}>
            <Box display="flex" alignItems="left">
                <List sx={{ display: "flex" }}>
                    {links.map((link) => (
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