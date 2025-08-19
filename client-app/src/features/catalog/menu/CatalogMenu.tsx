import React from 'react';
import { Box, List, ListItem, ListItemIcon, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useAppDispatch } from "../../../app/store/configureStore";
import { useTheme } from "@mui/material/styles";
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined';
import LocalOfferOutlinedIcon from '@mui/icons-material/LocalOfferOutlined';
import StorefrontOutlinedIcon from '@mui/icons-material/StorefrontOutlined';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';

interface CatalogMenuProps {
    selectedMenuItem?: string;
}

const links = [
    { title: 'Products', path: '/products', key: "product.menu.products", icon: <ShoppingCartOutlinedIcon sx={{ color: "#FFA500" }} /> },
    //{ title: 'Promos', path: '/promos', key: "product.menu.promos", icon: <LocalOfferOutlinedIcon sx={{ color: "#FF4081" }} /> },
    //{ title: 'Stores', path: '/stores', key: "product.menu.stores", icon: <StorefrontOutlinedIcon sx={{ color: "#00BFFF" }} /> }
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function CatalogMenu({ selectedMenuItem }: CatalogMenuProps) {
    const {getTranslatedLabel} = useTranslationHelper()
    const dispatch = useAppDispatch();
    const theme = useTheme();
    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');

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
            marginRight: '16px'
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
                    {links.map(({ title, path, icon, key }) => (
                        <ListItem component={NavLink} to={path} key={path} sx={navStyles(path)}>
                                <ListItemIcon sx={{ minWidth: "unset", marginX: "4px", fontSize: 28 }}>{icon}</ListItemIcon>
                                <Typography variant="body1" sx={{ margin: 0 }}>
                                    {getTranslatedLabel(key, title).toUpperCase()}
                                </Typography>
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Toolbar>
    );
}
