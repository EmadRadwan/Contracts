import { Box, List, ListItem, Toolbar, Typography, Menu, MenuItem, IconButton } from "@mui/material";
import { NavLink, NavLinkProps } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import ReceiptOutlinedIcon from '@mui/icons-material/ReceiptOutlined';
import PaymentOutlinedIcon from '@mui/icons-material/PaymentOutlined';
import LocalAtmOutlinedIcon from '@mui/icons-material/LocalAtmOutlined';
import AccountTreeOutlinedIcon from '@mui/icons-material/AccountTreeOutlined';
import AccountBalanceWalletOutlinedIcon from '@mui/icons-material/AccountBalanceWalletOutlined';
import BatteryCharging60Icon from '@mui/icons-material/BatteryCharging60';
import AddShoppingCartIcon from '@mui/icons-material/AddShoppingCart';
import StoreIcon from '@mui/icons-material/Store';
import ReceiptIcon from '@mui/icons-material/Receipt';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import PaidIcon from '@mui/icons-material/Paid';
import BalanceIcon from '@mui/icons-material/Balance';
import PriceCheckIcon from '@mui/icons-material/PriceCheck';
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown';
import withFloatingLabelFlexible from '../../../../app/components/FloatingLabel';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import React, { useState } from "react";

interface AccountingMenuProps {
    selectedMenuItem?: string;
    onMenuSelect?: (key: string) => void;
}

const NavLinkWithReset = React.forwardRef<HTMLAnchorElement, NavLinkProps>(
    (props, ref) => (
        <NavLink
            ref={ref}
            {...props}
            state={{ ...(props.state ?? {}), resetPaymentForm: true }}
        />
    )
);
NavLinkWithReset.displayName = "NavLinkWithReset";

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function AccountingMenu({ selectedMenuItem, onMenuSelect }: AccountingMenuProps) {
    const theme = useTheme();
    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');
    const { getTranslatedLabel } = useTranslationHelper();

    const FloatingLabelText = withFloatingLabelFlexible(({ children }: { children: string }) => (
        <Typography variant="body2" sx={{ marginLeft: '4px' }}>
            {children}
        </Typography>
    ));

    // REFACTOR: Extracted common nav item styles into a reusable function for consistency and easier maintenance.
    const getNavItemStyles = (isSelected: boolean) => ({
        color: isSelected ? theme.palette.primary.main : 'inherit',
        textDecoration: "none",
        typography: "h6",
        "&:hover": { color: "grey.500" },
        fontWeight: isSelected ? "bold" : "normal",
        display: 'flex',
        alignItems: 'center',
        padding: '6px 12px',           // Slightly increased horizontal padding for comfort
        minWidth: '150px',             // ← Key change: consistent minimum width
        justifyContent: 'center',      // Centers content (icon + text + arrow) within the box
        textAlign: 'center',           // Ensures text centering even if it wraps
        whiteSpace: 'nowrap',          // Optional: prevent wrapping (default behavior)
        // overflow: 'hidden',         // Use with nowrap if you want to truncate very long text
        // textOverflow: 'ellipsis',
    });

    const handleClick = (key: string) => {
        if (onMenuSelect) {
            onMenuSelect(key);
        }
    };

    // REFACTOR: Grouped related menu items into logical categories to reduce top-level items and save horizontal space.
    //    - Orders: Sales + Purchase
    //    - Payments: Incoming + Outgoing (preserves special NavLinkWithReset behavior)
    //    - GL Settings: Global + Organization
    //    - Transactions: All transaction-related items
    // This reduces the number of top-level horizontal items significantly while keeping submenus compact.
    const menuGroups = [
        {
            groupKey: "orders",
            title: "Orders",
            icon: <AddShoppingCartIcon sx={{ color: "#FF4081" }} />,
            subItems: [
                { title: "Sales Orders", key: "salesOrders", path: "/orders/sales", icon: <AddShoppingCartIcon sx={{ color: "#FF4081" }} /> },
                { title: "Purchase Orders", key: "purchaseOrders", path: "/orders/purchase", icon: <StoreIcon sx={{ color: "#FF4081" }} /> },
            ],
        },
        {
            groupKey: "payments",
            title: "Payments",
            icon: <PaymentOutlinedIcon sx={{ color: "#4CAF50" }} />,
            subItems: [
                { title: "Incoming Payments", key: "incomingPayments", path: "/payments/incoming", icon: <PaymentOutlinedIcon sx={{ color: "#4CAF50" }} />, isPayment: true },
                { title: "Outgoing Payments", key: "outgoingPayments", path: "/payments/outgoing", icon: <PaymentOutlinedIcon sx={{ color: "#F44336" }} />, isPayment: true },
                { title: "Due Payments", key: "duePayments", path: "/duePayments", icon: <PaymentOutlinedIcon sx={{ color: "#F44336" }} /> },
            ],
        },
        {
            title: 'Invoices',
            key: 'invoices',
            path: '/invoices',
            icon: <ReceiptOutlinedIcon sx={{ color: "#FFA500" }} />,
        },
        {
            title: 'Billing Accounts',
            key: 'creditLimitFormAdvancePayments',
            path: '/billingAccounts',
            icon: <BatteryCharging60Icon sx={{ color: "#03A9F4" }} />,
        },
        {
            title: "Multi-Payment Certificates",
            key: "multiPaymentCertificates",
            path: "/multiPaymentCertificates",
            icon: <AccountBalanceWalletOutlinedIcon sx={{ color: "#3F51B5" }} />,
        },
        {
            groupKey: "glSettings",
            title: "GL Settings",
            icon: <LocalAtmOutlinedIcon sx={{ color: "#E91E63" }} />,
            subItems: [
                { title: 'Global GL Settings', key: 'globalGLSettings', path: '/globalGL', icon: <LocalAtmOutlinedIcon sx={{ color: "#E91E63" }} /> },
                { title: 'Organization GL Settings', key: 'organizationGLSettings', path: '/orgGL', icon: <AccountTreeOutlinedIcon sx={{ color: "#8BC34A" }} /> },
            ],
        },
        {
            groupKey: "transactions",
            title: "Transactions",
            icon: <ReceiptIcon sx={{ color: "#8BC34A" }} />,
            subItems: [
                { title: 'Transactions', key: 'transactions', path: '/accountingTransaction', icon: <ReceiptIcon sx={{ color: "#8BC34A" }} /> },
                { title: 'Transactions Entries', key: 'transactions-entries', path: '/accountingTransactionEntries', icon: <ReceiptLongIcon sx={{ color: "#FF4081" }} /> },
                { title: 'Create Transactions', key: 'create-transactions', path: '/glCreateAccountingTransaction', icon: <PaidIcon sx={{ color: "#4CAF50" }} /> },
                { title: 'Trial Balance', key: 'trialBalance', path: '/trialBalance', icon: <BalanceIcon sx={{ color: "#E91E63" }} /> },
            ],
        },
    ];

    return (
        <Toolbar sx={{ display: 'flex', justifyContent: 'flex-start', alignItems: 'center', paddingLeft: 0 }}>
            <Box display="flex" alignItems="center">
                <List sx={{ display: 'flex', padding: 0, gap: 3}}>
                    {menuGroups.map((item) => {
                        // Determine if this top-level item (or any sub-item) is selected
                        const itemPaths = 'subItems' in item ? item.subItems.map(sub => normalizePath(sub.path)) : [normalizePath(item.path || '')];
                        const isGroupSelected = itemPaths.some(p => p === normalizedSelectedMenuItem);

                        if ('subItems' in item) {
                            // REFACTOR: Introduced dropdown menus using MUI Menu + IconButton for grouped items.
                            // This keeps the main bar fully horizontal, saves significant space, and opens submenus vertically below without affecting main content area.
                            const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
                            const open = Boolean(anchorEl);

                            const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
                                setAnchorEl(event.currentTarget);
                            };

                            const handleMenuClose = () => {
                                setAnchorEl(null);
                            };

                            return (
                                <ListItem key={item.groupKey} disablePadding>
                                    <IconButton
                                        onClick={handleMenuOpen}
                                        sx={getNavItemStyles(isGroupSelected)}
                                    >
                                        {item.icon}
                                        <FloatingLabelText label={item.title} translationKey={`accounting.menu.${item.groupKey}`}>
                                            {getTranslatedLabel(`accounting.menu.${item.groupKey}`, item.title).toUpperCase()}
                                        </FloatingLabelText>
                                        <ArrowDropDownIcon fontSize="small" />
                                    </IconButton>
                                    <Menu
                                        anchorEl={anchorEl}
                                        open={open}
                                        onClose={handleMenuClose}
                                        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
                                        transformOrigin={{ vertical: 'top', horizontal: 'left' }}
                                    >
                                        {item.subItems.map((sub) => {
                                            const LinkComponent = sub.isPayment ? NavLinkWithReset : NavLink;
                                            const isSubSelected = normalizePath(sub.path) === normalizedSelectedMenuItem;

                                            return (
                                                <MenuItem
                                                    key={sub.key}
                                                    component={LinkComponent}
                                                    to={sub.path}
                                                    onClick={() => {
                                                        handleClick(sub.key);
                                                        handleMenuClose();
                                                    }}
                                                    selected={isSubSelected}
                                                    sx={{ display: 'flex', alignItems: 'center' }}
                                                >
                                                    {sub.icon}
                                                    <Typography variant="body2" sx={{ ml: 1 }}>
                                                        {getTranslatedLabel(`accounting.menu.${sub.key}`, sub.title)}
                                                    </Typography>
                                                </MenuItem>
                                            );
                                        })}
                                    </Menu>
                                </ListItem>
                            );
                        }

                        // Single items (no submenu)
                        const LinkComponent = item.key.startsWith('incoming') || item.key.startsWith('outgoing') ? NavLinkWithReset : NavLink;
                        const isSelected = normalizePath(item.path) === normalizedSelectedMenuItem;

                        return (
                            <ListItem key={item.key} component={LinkComponent} to={item.path} sx={getNavItemStyles(isSelected)} onClick={() => handleClick(item.key)} disablePadding>
                                {item.icon}
                                <FloatingLabelText label={item.title} translationKey={`accounting.menu.${item.key}`}>
                                    {getTranslatedLabel(`accounting.menu.${item.key}`, item.title).toUpperCase()}
                                </FloatingLabelText>
                            </ListItem>
                        );
                    })}
                </List>
            </Box>
        </Toolbar>
    );
}