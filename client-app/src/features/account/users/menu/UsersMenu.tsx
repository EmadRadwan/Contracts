import { Box, Grid, ListItem, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import PeopleOutlineIcon from '@mui/icons-material/PeopleOutline';
import AdminPanelSettingsOutlinedIcon from '@mui/icons-material/AdminPanelSettingsOutlined';
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

interface UsersMenuProps {
    selectedMenuItem?: string;
    onMenuSelect?: (data: string) => void;
}

const links = [
    {
        title: "Users",
        key: "users",
        path: "/users",
        icon: <PeopleOutlineIcon sx={{ color: "#1976d2" }} />,
    },
    {
        title: "Roles",
        key: "roles",
        path: "/roles",
        icon: <AdminPanelSettingsOutlinedIcon sx={{ color: "#9c27b0" }} />,
    },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

const UsersMenu = ({ selectedMenuItem, onMenuSelect }: UsersMenuProps) => {
    const theme = useTheme();
    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');
    const { getTranslatedLabel } = useTranslationHelper();

    const navStyles = (path: string) => {
        const normalizedPath = normalizePath(path);
        const isSelected = normalizedPath === normalizedSelectedMenuItem;
        return {
            color: isSelected ? theme.palette.primary.main : 'inherit',
            '&.active': {
                color: theme.palette.primary.main,
            },
            textDecoration: "none",
            typography: "h6",
            "&:hover": {
                color: "grey.500",
            },
            fontWeight: isSelected ? "bold" : "normal",
            display: 'flex',
            alignItems: 'center',
            whiteSpace: 'nowrap',
            marginRight: '4px',
        };
    };

    const handleClick = (key: string) => {
        if (onMenuSelect) {
            onMenuSelect(key);
        }
    };

    return (
        <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'left' }}>
            <Box display='flex' alignItems='left'>
                <Grid container>
                    {links.map(({ title, path, icon, key }, index) => (
                        <Grid key={index}>
                            <ListItem
                                component={NavLink}
                                to={path}
                                key={key}
                                sx={navStyles(path)}
                                onClick={() => handleClick(key)}
                            >
                                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                    {icon}
                                    <Typography variant="body1" sx={{ marginX: '4px' }}>
                                        {getTranslatedLabel(`users.menu.${key}`, title)}
                                    </Typography>
                                </Box>
                            </ListItem>
                        </Grid>
                    ))}
                </Grid>
            </Box>
        </Toolbar>
    );
};

export default UsersMenu;
