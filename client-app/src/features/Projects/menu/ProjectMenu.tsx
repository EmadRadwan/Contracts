import { Box, Grid, ListItem, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import ListAltOutlinedIcon from '@mui/icons-material/ListAltOutlined';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {HomeWork} from "@mui/icons-material";

interface ProjectMenuProps {
    selectedMenuItem?: string;
    onMenuSelect?: (data: string) => void;
}

const links = [
    {
        title: "Projects",
        key: "projects",
        path: "/projects",
        icon: <HomeWork sx={{ color: "#FF4081" }} />,
    },
    {
        title: "Project Certificates",
        key: "projectCertificates",
        path: "/projectCertificates",
        icon: <ListAltOutlinedIcon sx={{ color: "#FF4081" }} />,
    },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

const ProjectMenu = ({ selectedMenuItem, onMenuSelect }: ProjectMenuProps) => {
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
            // REFACTOR: Call onMenuSelect with the menu item's key
            // Purpose: Notifies parent component of menu selection
            // Context: Allows ProjectCertificatesList to handle "Project Certificates" click
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
                                        {getTranslatedLabel(`projects.menu.${key}`, title)}
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