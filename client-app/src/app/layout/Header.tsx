import React from "react";
import { NavLink, useLocation } from "react-router-dom";
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined';
import GroupOutlinedIcon from '@mui/icons-material/GroupOutlined';
import ReceiptIcon from '@mui/icons-material/Receipt';
import ReceiptOutlinedIcon from '@mui/icons-material/ReceiptOutlined';
import HomeWork from '@mui/icons-material/HomeWork';
import { AppBar, Box, List, ListItem, ListItemIcon, ListItemText, Toolbar, Typography, } from "@mui/material";
import SignedInMenu from "./SignedInMenu";
import { useAppDispatch, useAppSelector } from "../store/configureStore";
import LanguageChooser from "./LanguageChooser";
import { setHeaderSelectedMenu } from "../slice/appUiSlice";
import { StoreMallDirectory } from "@mui/icons-material";
import { useTranslationHelper } from "../hooks/useTranslationHelper";
import withFloatingLabel from "../components/FloatingLabel";
import {Can} from "../../features/account/Can";
import ManageAccountsIcon from '@mui/icons-material/ManageAccounts';
import SupervisorAccountIcon from '@mui/icons-material/SupervisorAccount';

const MyNavLink = React.forwardRef<any, any>((props, ref) => (
    <NavLink
        ref={ref}
        to={props.to}
    >
        {props.children}
    </NavLink>
));

const moduleRoleMap: Record<string, string> = {
    catalog: "Catalog_View",
    facility: "Facility_View",
    party: "Party_View",
    accounting: "Accounting_View",
    projects: "Projects_View",
    sales: "Sales_View",
    crm: "CRM_View",
    hr: "HR_View",
};

const midLinks = [
    { title: "catalog", path: "/products", key: "catalog", icon: <ShoppingCartOutlinedIcon sx={{ color: "#FFA500" }} /> },
    { title: "facility", path: "/facilitiesDashboard", key: "facility", icon: <StoreMallDirectory sx={{ color: "#FF4081" }} /> },
    { title: "party", path: "/parties", key: "party", icon: <GroupOutlinedIcon sx={{ color: "#00BFFF" }} /> },
    { title: "accounting", path: "/invoicesDashboard", key: "accounting", icon: <ReceiptOutlinedIcon sx={{ color: "#9C27B0" }} /> },
    { title: "projects", path: "/projects", key: "projects", icon: <HomeWork sx={{ color: "#03A9F4" }} /> },
    {
        title: "sales",
        path: "/sales-requests",
        key: "sales",
        icon: <ReceiptIcon sx={{ color: "#4CAF50" }} />, // Green for sales
    },
    { title: "CRM", path: "/sales-opportunities", key: "crm", icon: <ManageAccountsIcon sx={{ color: "#fabd52" }} /> },
,
    { title: "hr", path: "/humanResources", key: "hr", icon: <SupervisorAccountIcon sx={{ color: "#fabd52" }} /> },

];


const rightLinks = [{ title: "login", path: "/login", key: "login" }];


export default function Header() {

    const location = useLocation();
    const { language } = useAppSelector(state => state.localization);
    const { getTranslatedLabel } = useTranslationHelper();
    const { user } = useAppSelector((state) => state.account);
    const { headerSelectedMenu } = useAppSelector((state) => state.appUi);
    const dispatch = useAppDispatch();

    const isRTL = language === 'ar';
    const navStyles = (selectedpath: string) => ({
        color: "inherit",
        textDecoration: selectedpath === headerSelectedMenu ? "underline" : "none",
        typography: "h6",
        transition: "color 0.2s ease-in-out", // Add a smooth transition
        "&:hover": {
            color: "white",
            backgroundColor: "rgba(0,0,0,0.1)", // Slightly darker background
            boxShadow: "0px 2px 4px rgba(0,0,0,0.15)",
            textDecoration: "underline",
        },
        fontWeight: (selectedpath === headerSelectedMenu ||
            location.pathname + 'Dashboard' === selectedpath) ? "bold" : "normal"
    });

    const FloatingLabelText = withFloatingLabel(({ children }: { children: string }) => (
        <ListItemText primary={children} sx={{ margin: 0 }} />
    ));


    return (
        <AppBar position="static">
            <Toolbar
                sx={{
                    display: "flex",
                    // flexDirection: isRTL ? "row-reverse" : "row",
                    justifyContent: "space-between",
                    alignItems: "center",
                }}
            >
                {/* Language Chooser - at start (left for LTR, right for RTL) */}
                <Box display="flex" alignItems="center">
                    <LanguageChooser />
                </Box>

                {/* Navigation Links - center */}
                <Box display="flex" alignItems="center" sx={{ flexGrow: 1, justifyContent: "center" }}>
                    <List sx={{ display: "flex"}}>
                        {midLinks.map(({ title, path, icon, key }) => {
                            const requiredRole = moduleRoleMap[key];

                            return requiredRole ? (
                                <Can perform={requiredRole} key={path}>
                                    <ListItem
                                        component={NavLink}
                                        to={path}
                                        sx={{ ...navStyles(path) }}
                                        onClick={() => dispatch(setHeaderSelectedMenu(path))}
                                    >
                                        <ListItemIcon sx={{ minWidth: "unset", marginX: "8px", fontSize: 28 }}>
                                            {icon}
                                        </ListItemIcon>
                                        <FloatingLabelText
                                            label={title}
                                            translationKey={`general.header.${key}`}
                                            placement="bottom"
                                        >
                                            {getTranslatedLabel(`general.header.${key}`, title).toUpperCase()}
                                        </FloatingLabelText>
                                    </ListItem>
                                </Can>
                            ) : null;
                        })}
                    </List>
                </Box>

                {/* User Menu / Login - at end (right for LTR, left for RTL) */}
                <Box display="flex" alignItems="center">
                    {user ? (
                        <SignedInMenu />
                    ) : (
                        <List sx={{ display: "flex" }}>
                            {rightLinks.map(({ title, path, key }) => (
                                <ListItem
                                    component={NavLink}
                                    to={path}
                                    key={path}
                                    sx={navStyles(path)}
                                    onClick={() => setHeaderSelectedMenu(path)}
                                >
                                    {getTranslatedLabel(`general.header.${key}`, title).toUpperCase()}
                                </ListItem>
                            ))}
                        </List>
                    )}
                </Box>
            </Toolbar>
        </AppBar>
    );
}
