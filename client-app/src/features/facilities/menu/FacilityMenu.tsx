import { Box, Grid, ListItem, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import StorageOutlinedIcon from '@mui/icons-material/StorageOutlined';
import ListAltOutlinedIcon from '@mui/icons-material/ListAltOutlined';
import DetailsOutlinedIcon from '@mui/icons-material/DetailsOutlined';
import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined';
import SyncAltOutlinedIcon from '@mui/icons-material/SyncAltOutlined';
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined';
import Output from '@mui/icons-material/Output';
import InventoryIcon from '@mui/icons-material/Inventory';
import MapIcon from '@mui/icons-material/Map';
import TransferWithinAStationIcon from '@mui/icons-material/TransferWithinAStation';
import AddShoppingCartIcon from '@mui/icons-material/AddShoppingCart';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

interface FacilityMenuProps {
    selectedMenuItem?: string;
}

const links = [
    { title: "Inventory", key: "inventory", path: "/facilityInventories", icon: <StorageOutlinedIcon sx={{ color: "#FFA500" }} /> },
    { title: "Inventory Items", key: "items", path: "/inventoryItems", icon: <ListAltOutlinedIcon sx={{ color: "#FF4081" }} /> },
    { title: "Inventory Item Details", key: "details", path: "/inventoryItemDetails", icon: <DetailsOutlinedIcon sx={{ color: "#00BFFF" }} /> },
    { title: "Receive Inventory", key: "receive", path: "/receiveInventory", icon: <InboxOutlinedIcon sx={{ color: "#4CAF50" }} /> },
    { title: "Inventory Transfer", key: "transfer", path: "/inventoryTransfer", icon: <SyncAltOutlinedIcon sx={{ color: "#FFC107" }} /> },
    { title: "Facilities", key: "facilities", path: "/facilities", icon: <BusinessOutlinedIcon sx={{ color: "#9C27B0" }} /> },
    { title: "Physical Inventory", key: "physical", path: "/physicalInventory", icon: <InventoryIcon sx={{ color: "#03A9F4" }} /> },
    { title: "Locations", key: "locations", path: "/locations", icon: <MapIcon sx={{ color: "#E91E63" }} /> },
    //{ title: "Picking", key: "picking", path: "/picking", icon: <AddShoppingCartIcon sx={{ color: "#00BFFF" }} /> },
    //{ title: "Stock Moves", key: "stockMoves", path: "/stockMoves", icon: <TransferWithinAStationIcon sx={{ color: "#FFA500" }} /> },
    { title: "Issue Inventory", key: "pack", path: "/packing", icon: <Output sx={{ color: "#9C27B0" }} /> },
    //{ title: "Manage Picklists", key: "picklists", path: "/managePicklists", icon: <ViewListIcon sx={{ color: "#9C27B0" }} /> },

    // 🆕 NEW ITEM: "Issue Raw Materials"
    {
        title: "Issue Raw Materials",
        key: "issueRawMaterials",
        path: "/issueRawMaterials",
        icon: <InventoryIcon sx={{ color: "#FF4500" }} />
    },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

const FacilityMenu = ({ selectedMenuItem }: FacilityMenuProps) => {
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
                                        {getTranslatedLabel(`facility.menu.${key}`, title)}
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

export default FacilityMenu;
