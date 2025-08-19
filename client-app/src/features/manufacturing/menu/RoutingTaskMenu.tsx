// src/features/manufacturing/menu/RoutingTaskMenu.tsx
import React from 'react';
import { Box, List, ListItem, ListItemIcon, Toolbar, Typography } from '@mui/material';
import { NavLink } from 'react-router-dom';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import { useTheme } from '@mui/material/styles';
import MonetizationOnIcon from '@mui/icons-material/MonetizationOn';

// REFACTOR: Define interface for props to ensure type safety
interface RoutingTaskMenuProps {
    workEffortId: string;
    selectedMenuItem?: string;
}

const RoutingTaskMenu = ({ workEffortId, selectedMenuItem }: RoutingTaskMenuProps) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const theme = useTheme();

    // REFACTOR: Normalize path to handle /routings/:workEffortId structure
    // Purpose: Ensures consistent path comparison for active link styling
    const normalizePath = (path: string) => path.split('/').slice(0, -2).join('/').replace(/^\//, '').toLowerCase();
    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');

    // REFACTOR: Define menu links for routing task-related navigation
    // Purpose: Centralizes link configuration for easier maintenance
    const links = [
        {
            title: 'Routing Task Costs',
            path: `/routings/${workEffortId}/task-costs`,
            key: 'manufacturing.routingTask.menu.routingTaskCosts',
            icon: <MonetizationOnIcon sx={{ color: '#FFD700' }} />,
        },
    ];

    // REFACTOR: Define navigation styles for consistent look and feel
    // Purpose: Matches RoutingMenu styling for cohesive UI
    const navStyles = (path: string) => {
        const normalizedPath = normalizePath(path);
        const isSelected = normalizedPath === normalizedSelectedMenuItem;
        return {
            color: isSelected ? theme.palette.primary.main : 'inherit',
            '&.active': { color: theme.palette.primary.main },
            textDecoration: 'none',
            typography: 'h6',
            '&:hover': { color: 'grey.500' },
            fontWeight: isSelected ? 'bold' : 'normal',
            display: 'flex',
            alignItems: 'center',
            marginRight: '16px',
        };
    };

    return (
        <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'left', mb: 2 }}>
            <Box display="flex" alignItems="left">
                <List sx={{ display: 'flex' }}>
                    {links.map(({ title, path, icon, key }) => (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={path}
                            sx={navStyles(path)}
                            aria-label={getTranslatedLabel(key, title)}
                        >
                            <ListItemIcon sx={{ minWidth: 'unset', marginX: '4px', fontSize: 28 }}>{icon}</ListItemIcon>
                            <Typography variant="body1" sx={{ margin: 0 }}>
                                {getTranslatedLabel(key, title).toUpperCase()}
                            </Typography>
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Toolbar>
    );
};

export default RoutingTaskMenu;