import { Box, Grid, ListItem, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import ListAltOutlinedIcon from '@mui/icons-material/ListAltOutlined';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {HomeWork} from "@mui/icons-material";

// REFACTOR: Created ProjectMenu with a single "Projects" entry, adapted from FacilityMenu.
// Maintains theme-based styling, translation, and navigation logic for consistency.
interface ProjectMenuProps {
    selectedMenuItem?: string;
}

const links = [
    {
        title: "Projects",
        key: "projects",
        path: "/projects",
        icon: <HomeWork sx={{ color: "#FF4081" }} />
    }
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

const ProjectMenu = ({ selectedMenuItem }: ProjectMenuProps) => {
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
                            >
                                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                    {icon}
                                    <Typography variant="body1" sx={{ marginX: '4px' }}>
                                        {getTranslatedLabel(`project.menu.${key}`, title)}
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

export default ProjectMenu;