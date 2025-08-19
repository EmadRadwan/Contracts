import { Box, List, ListItem, Toolbar, Typography } from "@mui/material";
import { NavLink } from 'react-router-dom';
import { useTheme } from '@mui/material/styles';
import DirectionsCarIcon from '@mui/icons-material/DirectionsCar';
import LocalOfferIcon from '@mui/icons-material/LocalOffer';
import CommuteIcon from '@mui/icons-material/Commute';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import BuildIcon from '@mui/icons-material/Build';
import FormatQuoteIcon from '@mui/icons-material/FormatQuote';
import AssignmentIcon from '@mui/icons-material/Assignment';

const links = [
    { title: 'Vehicle', path: '/vehicles', icon: <DirectionsCarIcon sx={{ color: "#FFA500" }} /> },
    { title: 'Makes', path: '/makes', icon: <LocalOfferIcon sx={{ color: "#FF4081" }} /> },
    { title: 'Models', path: '/models', icon: <CommuteIcon sx={{ color: "#00BFFF" }} /> },
    { title: 'Service Rate', path: '/serviceRates', icon: <AttachMoneyIcon sx={{ color: "#4CAF50" }} /> },
    { title: 'Service Specification', path: '/serviceSpecifications', icon: <BuildIcon sx={{ color: "#9C27B0" }} /> },
    { title: 'Job Quote', path: '/jobQuotes', icon: <FormatQuoteIcon sx={{ color: "#FBC02D" }} /> },
    { title: 'Job Order', path: '/jobOrders', icon: <AssignmentIcon sx={{ color: "#2196F3" }} /> },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function VehicleMenu() {
    const theme = useTheme();
    const normalizedSelectedMenuItem = normalizePath(location.pathname || '');

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
            marginRight: '8px', // Adjust the space between icon and text
        };
    };

    return (
        <Toolbar
            sx={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "left",
            }}
        >
            <Box display="flex" alignItems="left">
                <List sx={{ display: "flex" }}>
                    {links.map(({ title, path, icon }) => (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={path}
                            sx={navStyles(path)}
                        >
                            {icon}
                            <Typography variant="body1" sx={{ marginLeft: '4px' }}>
                                {title}
                            </Typography>
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Toolbar>
    );
}
