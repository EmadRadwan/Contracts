import { Box, List, ListItem, Toolbar, Typography } from "@mui/material";
import {NavLink, NavLinkProps} from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import ReceiptOutlinedIcon from '@mui/icons-material/ReceiptOutlined';
import PaymentOutlinedIcon from '@mui/icons-material/PaymentOutlined';
import LocalAtmOutlinedIcon from '@mui/icons-material/LocalAtmOutlined';
import AccountTreeOutlinedIcon from '@mui/icons-material/AccountTreeOutlined';
import AccountBalanceWalletOutlinedIcon from '@mui/icons-material/AccountBalanceWalletOutlined';
import GroupIcon from '@mui/icons-material/Group';
import withFloatingLabelFlexible from '../../../../app/components/FloatingLabel'; 
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import AddShoppingCartIcon from '@mui/icons-material/AddShoppingCart';
import StoreIcon from '@mui/icons-material/Store';
import ReceiptIcon from '@mui/icons-material/Receipt';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import PaidIcon from '@mui/icons-material/Paid';
import React from "react"; 

interface AccountingMenuProps {
    selectedMenuItem?: string;
}

const NavLinkWithReset = React.forwardRef<HTMLAnchorElement, NavLinkProps>(
    (props, ref) => (
        <NavLink
            ref={ref}
            {...props}
            // `state` is a normal prop – we just spread the incoming props
            // and override/add the state we need
            state={{ ...(props.state ?? {}), resetPaymentForm: true }}
        />
    )
);
NavLinkWithReset.displayName = "NavLinkWithReset";


const links = [
    { title: "Sales Orders", key: "salesOrders", path: "/orders/sales", icon: <AddShoppingCartIcon sx={{ color: "#FF4081" }} /> },
    { title: "Purchase Orders", key: "purchaseOrders", path: "/orders/purchase", icon: <StoreIcon sx={{ color: "#FF4081" }} /> },
    { title: 'Invoices', key: 'invoices', path: '/invoices', icon: <ReceiptOutlinedIcon sx={{ color: "#FFA500" }} /> },
    { title: 'Incoming Payments', key: 'incomingPayments', path: '/payments/incoming', icon: <PaymentOutlinedIcon sx={{ color: "#4CAF50" }} /> }, 
    { title: 'Outgoing Payments', key: 'outgoingPayments', path: '/payments/outgoing', icon: <PaymentOutlinedIcon sx={{ color: "#F44336" }} /> }, 
    { title: 'Payment Groups', key: 'pay-group', path: '/paymentGroups', icon: <GroupIcon sx={{ color: "#03A9F4" }} /> },
    { title: "Multi-Payment Certificates", key: "multiPaymentCertificates", path: "/multiPaymentCertificates", icon: <AccountBalanceWalletOutlinedIcon sx={{ color: "#3F51B5" }} /> },
    { title: 'Global GL Settings', key: 'globalGLSettings', path: '/globalGL', icon: <LocalAtmOutlinedIcon sx={{ color: "#E91E63" }} /> },
    { title: 'Organization GL Settings', key: 'organizationGLSettings', path: '/orgGL', icon: <AccountTreeOutlinedIcon sx={{ color: "#8BC34A" }} /> },
    { title: 'Transactions', key: 'transactions', path: '/accountingTransaction', icon: <ReceiptIcon sx={{ color: "#8BC34A" }} /> },
    { title: 'Transactions Entries', key: 'transactions-entries', path: '/accountingTransactionEntries', icon: <ReceiptLongIcon sx={{ color: "#FF4081" }} /> },
    { title: 'Create Transactions', key: 'create-transactions', path: '/glCreateAccountingTransaction', icon: <PaidIcon sx={{ color: "#4CAF50" }} /> },


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
                    {links.map(({ title, path, icon, key }) => {
                        const isPaymentLink = key === 'incomingPayments' || key === 'outgoingPayments';

                        const LinkComponent = isPaymentLink ? NavLinkWithReset : NavLink;


                        return (
                            <ListItem
                                component={LinkComponent}
                                to={path}
                                key={path}
                                sx={navStyles(path)}
                            >
                                {icon}
                                <FloatingLabelText
                                    label={title}
                                    translationKey={`accounting.orgGL.menu.${key}`}
                                >
                                    {getTranslatedLabel(`accounting.menu.${key}`, title).toUpperCase()}
                                </FloatingLabelText>
                            </ListItem>
                        );
                    })}
                </List>
            </Box>
        </Toolbar>
    );
}
