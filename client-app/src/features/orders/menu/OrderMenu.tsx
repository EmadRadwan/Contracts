import React from 'react';
import { Box, List, ListItem, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from '@mui/material/styles';
import { setWhatWasClicked } from "../slice/sharedOrderUiSlice";
import { useAppDispatch } from "../../../app/store/configureStore";
import FormatQuoteIcon from '@mui/icons-material/FormatQuote';
import LocalMallIcon from '@mui/icons-material/LocalMall';
import AssignmentReturnIcon from '@mui/icons-material/AssignmentReturn';
import withFloatingLabelFlexible from '../../../app/components/FloatingLabel';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';

interface OrderMenuProps {
    selectedMenuItem?: string;
}

const links = [
    { title: "quote", key: "quote", path: "/quotes", icon: <FormatQuoteIcon sx={{ color: "#FFA500" }} /> },
    { title: "order", key: "order", path: "/orders", icon: <LocalMallIcon sx={{ color: "#FF4081" }} /> },
    { title: "return", key: "return", path: "/returns", icon: <AssignmentReturnIcon sx={{ color: "#00BFFF" }} /> },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function OrderMenu({ selectedMenuItem }: OrderMenuProps) {
    const dispatch = useAppDispatch();
    const theme = useTheme();
    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');
    const { getTranslatedLabel } = useTranslationHelper();

    const FloatingLabelText = withFloatingLabelFlexible(({ children }: { children: string }) => (
        <Typography variant="body1" sx={{ marginLeft: '4px' }}>
            {children}
        </Typography>
      ));

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

    const handleClick = (event: React.MouseEvent<HTMLElement>, title: string) => {
        console.log('Clicked menu item:', title);
        if (title === 'order') {
            dispatch(setWhatWasClicked('orderMenu'));
        }
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
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={path}
                            sx={navStyles(path)}
                            onClick={(event) => handleClick(event, title)}
                        >
                            {icon}
                            <FloatingLabelText label={title} translationKey={`order.menu.${key}`}>
                                {getTranslatedLabel(`order.menu.${key}`, title).toUpperCase()}
                            </FloatingLabelText>
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Toolbar>
    );
}
