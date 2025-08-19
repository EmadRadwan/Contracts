import { Box, List, ListItem, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import ReceiptOutlinedIcon from '@mui/icons-material/ReceiptOutlined';
import PaymentOutlinedIcon from '@mui/icons-material/PaymentOutlined';
import AccountBalanceWalletOutlinedIcon from '@mui/icons-material/AccountBalanceWalletOutlined';
import AssignmentOutlinedIcon from '@mui/icons-material/AssignmentOutlined';
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import LocalAtmOutlinedIcon from '@mui/icons-material/LocalAtmOutlined';
import AccountTreeOutlinedIcon from '@mui/icons-material/AccountTreeOutlined';
import GroupIcon from '@mui/icons-material/Group';
import { PrecisionManufacturing } from "@mui/icons-material";
import withFloatingLabelFlexible from '../../../../app/components/FloatingLabel'; 
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper'; 

interface AccountingMenuProps {
    selectedMenuItem?: string;
}

const links = [
    { title: 'Invoices', key: 'invoices', path: '/invoices', icon: <ReceiptOutlinedIcon sx={{ color: "#FFA500" }} /> },
    { title: 'Payments', key: 'payments', path: '/payments', icon: <PaymentOutlinedIcon sx={{ color: "#FF4081" }} /> },
    { title: 'Payment Groups', key: 'pay-group', path: '/paymentGroups', icon: <GroupIcon sx={{ color: "#03A9F4" }} /> },
    { title: 'Fixed Assets', key: 'fixedAssets', path: '/fixedAssets', icon: <PrecisionManufacturing sx={{ color: "#00BFFF" }} /> },
    { title: 'Tax Authorities', key: 'taxAuthorities', path: '/taxAuth', icon: <AccountBalanceWalletOutlinedIcon sx={{ color: "#4CAF50" }} /> },
    { title: 'Agreements', key: 'agreements', path: '/agreements', icon: <AssignmentOutlinedIcon sx={{ color: "#FFC107" }} /> },
    { title: 'Financial Account', key: 'financialAccount', path: '/financialAccounts', icon: <BusinessOutlinedIcon sx={{ color: "#9C27B0" }} /> },
    { title: 'Billing Accounts', key: 'billingAccounts', path: '/billingAccounts', icon: <SettingsOutlinedIcon sx={{ color: "#03A9F4" }} /> },
    { title: 'Global GL Settings', key: 'globalGLSettings', path: '/globalGL', icon: <LocalAtmOutlinedIcon sx={{ color: "#E91E63" }} /> },
    { title: 'Organization GL Settings', key: 'organizationGLSettings', path: '/orgGL', icon: <AccountTreeOutlinedIcon sx={{ color: "#8BC34A" }} /> },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function AccountingMenu({ selectedMenuItem }: AccountingMenuProps) {
    const theme = useTheme();
    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');
    const { getTranslatedLabel } = useTranslationHelper();

    const FloatingLabelText = withFloatingLabelFlexible(({ children }: { children: string }) => (
        <Typography variant="body2" sx={{ marginLeft: '4px' }}>
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
            marginRight: '4px', // Adjust the space between icon and text
        };
    };

    return (
        <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'left' }}>
            <Box display="flex" alignItems="left">
                <List sx={{ display: 'flex' }}>
                    {links.map(({ title, path, icon, key }) => (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={path}
                            sx={navStyles(path)}
                        >
                            {icon}
                            <FloatingLabelText label={title} translationKey={`accounting.orgGL.menu.${key}`}>
                                {getTranslatedLabel(`accounting.menu.${key}`, title).toUpperCase()}
                            </FloatingLabelText>
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Toolbar>
    );
}
