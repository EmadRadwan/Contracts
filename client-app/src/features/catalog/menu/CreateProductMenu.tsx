import { NavLink } from "react-router-dom";
import { Box, Container, List, ListItem, Typography, Toolbar, Grid } from '@mui/material';
import CategoryOutlinedIcon from '@mui/icons-material/CategoryOutlined';
import AttachMoneyOutlinedIcon from '@mui/icons-material/AttachMoneyOutlined';
import StoreOutlinedIcon from '@mui/icons-material/StoreOutlined';
import LocalShippingOutlinedIcon from '@mui/icons-material/LocalShippingOutlined';
import InventoryOutlinedIcon from '@mui/icons-material/InventoryOutlined';
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined';
import WorkOutlineOutlinedIcon from '@mui/icons-material/WorkOutlineOutlined';
import MapIcon from '@mui/icons-material/Map';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

export default function CreateProductMenu() {
    const {getTranslatedLabel} = useTranslationHelper()
    const menuItems = [
        { name: "Associations", path: "/productAssociations", key: "product.products.form.menu.associations", icon: <BusinessOutlinedIcon sx={{ color: "#FFA500" }} /> },
        { name: "Prices", path: "/productPrices", key: "product.products.form.menu.prices", icon: <AttachMoneyOutlinedIcon sx={{ color: "#FF4081" }} /> },
        { name: "Facilities", path: "/productFacilities", key: "product.products.form.menu.facilities", icon: <StoreOutlinedIcon sx={{ color: "#00BFFF" }} /> },
        { name: "Locations", path: "/productLocations", key: "product.products.form.menu.locations", icon: <MapIcon sx={{ color: "#4CAF50" }} /> },
        { name: "Categories", path: "/productCategories", key: "product.products.form.menu.categories", icon: <CategoryOutlinedIcon sx={{ color: "#FFC107" }} /> },
        { name: "Suppliers", path: "/productSuppliers", key: "product.products.form.menu.suppliers", icon: <LocalShippingOutlinedIcon sx={{ color: "#9C27B0" }} /> },
        { name: "Inventory", path: "/facilityInventories", key: "product.products.form.menu.inventory", icon: <InventoryOutlinedIcon sx={{ color: "#FF5722" }} /> },
        { name: "Costs", path: "/productCosts", key: "product.products.form.menu.costs", icon: <WorkOutlineOutlinedIcon sx={{ color: "#E91E63" }} /> },
    ];

    return (
        <Container>
            <Toolbar sx={{ display: 'flex', justifyContent: 'flex-end', alignItems: 'center' }}>
                <Box display='flex' alignItems='center'>
                    <List sx={{ display: 'flex' }}>
                        <Grid container spacing={2} justifyContent="center">
                            {menuItems.map(({ name, path, icon, key }) => (
                                <Grid item xs={3} key={path}>
                                    <ListItem
                                        component={NavLink}
                                        to={path}
                                        key={path}
                                        sx={{
                                            color: 'inherit',
                                            textDecoration: 'none',
                                            typography: 'h6',
                                            "&:hover": {
                                                color: 'grey.500',
                                            },
                                            whiteSpace: 'nowrap',
                                            display: 'flex',
                                            alignItems: 'center'
                                        }}
                                    >
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: '2px' }}>
                                            {icon}
                                            <Typography variant="body1" sx={{ marginInlineStart: '4px' }}>
                                                {getTranslatedLabel(key, name)}
                                            </Typography>
                                        </Box>
                                    </ListItem>
                                </Grid>
                            ))}
                        </Grid>
                    </List>
                </Box>
            </Toolbar>
        </Container>
    );
}
