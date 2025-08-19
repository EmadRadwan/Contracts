import React from 'react';
import {Box, List, ListItem, ListItemIcon, Toolbar, Typography} from '@mui/material';
import {NavLink} from 'react-router-dom';
import {useTranslationHelper} from '../../../app/hooks/useTranslationHelper';
import EditIcon from '@mui/icons-material/Edit';
import TaskIcon from '@mui/icons-material/Task';
import LinkIcon from '@mui/icons-material/Link';
import {useTheme} from '@mui/material/styles';


const RoutingMenu = ({ workEffortId, selectedMenuItem }: { workEffortId: string; selectedMenuItem?: string }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const theme = useTheme();
    const normalizePath = (path: string) => path.split('/').slice(0, -2).join('/').replace(/^\//, '').toLowerCase();

    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');

    // REFACTOR: Update menu links to use /routings/:workEffortId paths, replacing /routingTasks
    const links = [
        {
            title: 'Edit Routing Task Assoc',
            path: `/routings/${workEffortId}/task-assoc`,
            key: 'manufacturing.routing.menu.editRoutingTaskAssoc',
            icon: <TaskIcon sx={{ color: '#00BFFF' }} />,
        },
        {
            title: 'Edit Routing Product Link',
            path: `/routings/${workEffortId}/product-link`,
            key: 'manufacturing.routing.menu.editRoutingProductLink',
            icon: <LinkIcon sx={{ color: '#4CAF50' }} />,
        },
    ];

    // REFACTOR: Adjust normalizePath to handle /routings/:workEffortId structure

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
export default RoutingMenu;