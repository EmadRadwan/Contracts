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
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown';
import withFloatingLabelFlexible from '../../../../app/components/FloatingLabel';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import React, { useState } from "react";
import {Can} from "../../../account/Can";

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
        <Typography variant="body2" sx={{ marginLeft: '1px' }}>
            {children}
        </Typography>
    ));

    // REFACTOR: Extracted common nav item styles into a reusable function for consistency and easier maintenance.
    const getNavItemStyles = (isSelected: boolean) => ({
        color: isSelected ? theme.palette.primary.main : 'inherit',
        textDecoration: "none",
        typography: "h6",
        "&:hover": { color: "grey.500", borderRadius: "3rem" },
        fontWeight: isSelected ? "bold" : "normal",
        display: 'flex',
        alignItems: 'center',
        padding: '6px 12px',
        minWidth: '130px',
        justifyContent: 'center',
        textAlign: 'center',
        whiteSpace: 'nowrap',
    });

    const handleClick = (key: string) => {
        if (onMenuSelect) {
            onMenuSelect(key);
        }
    };

    // REFACTOR: Flattened the Payments group — removed the dropdown and promoted the 3 sub-items to top-level menu items
    // Purpose: Display Incoming, Outgoing, and Due Payments directly in the main horizontal bar as requested
    // Improves: Better visibility and quicker access to frequently used payment views; reduces clicks
    // Context: The parent "Payments" dropdown was removed entirely
    const menuGroups = [
        {
            groupKey: "orders",
            title: "Orders",
            icon: <AddShoppingCartIcon sx={{ color: "#FF4081" }} />,
            requiredRole: "Sales_View",
            subItems: [
                { title: "Sales Orders", key: "salesOrders", path: "/orders/sales", icon: <AddShoppingCartIcon sx={{ color: "#FF4081" }} /> },
                { title: "Purchase Orders", key: "purchaseOrders", path: "/orders/purchase", icon: <StoreIcon sx={{ color: "#FF4081" }} /> },
            ],
        },
       {
            groupKey: "glSettings",
            title: "GL Settings",
            icon: <LocalAtmOutlinedIcon sx={{ color: "#E91E63" }} />,
            requiredRole: "Accounting_GLSettings_View",
            subItems: [
                { title: 'Global GL Settings', key: 'globalGLSettings', path: '/globalGL', icon: <LocalAtmOutlinedIcon sx={{ color: "#E91E63" }} /> },
                { title: 'Organization GL Settings', key: 'organizationGLSettings', path: '/orgGL', icon: <AccountTreeOutlinedIcon sx={{ color: "#8BC34A" }} /> },
            ],
        },
        {
            groupKey: "transactions",
            title: "Transactions",
            icon: <ReceiptIcon sx={{ color: "#8BC34A" }} />,
            requiredRole: "Accounting_Transactions_View",
            subItems: [
                { title: 'Transactions', key: 'transactions', path: '/accountingTransaction', icon: <ReceiptIcon sx={{ color: "#8BC34A" }} /> },
                { title: 'Transactions Entries', key: 'transactions-entries', path: '/accountingTransactionEntries', icon: <ReceiptLongIcon sx={{ color: "#FF4081" }} /> },
                { title: 'Create Transactions', key: 'create-transactions', path: '/glCreateAccountingTransaction', icon: <PaidIcon sx={{ color: "#4CAF50" }} /> },
                { title: 'Trial Balance', key: 'trialBalance', path: '/trialBalance', icon: <BalanceIcon sx={{ color: "#E91E63" }} /> },
            ],
        },
    ];

    const standaloneItems = [
        { title: "Incoming Payments", key: "incomingPayments", path: "/payments/incoming", icon: <PaymentOutlinedIcon sx={{ color: "#4CAF50" }} />, requiredRole: "Accounting_Payments_View", isPayment: true },
        { title: "Outgoing Payments", key: "outgoingPayments", path: "/payments/outgoing", icon: <PaymentOutlinedIcon sx={{ color: "#F44336" }} />, requiredRole: "Accounting_Payments_View", isPayment: true },
        { title: "Due Payments", key: "duePayments", path: "/duePayments", icon: <PaymentOutlinedIcon sx={{ color: "#F44336" }} />, requiredRole: "Accounting_Payments_Due_View", isPayment: true },
        { title: 'Invoices', key: 'invoices', path: '/invoices', icon: <ReceiptOutlinedIcon sx={{ color: "#FFA500" }} />, requiredRole: "Accounting_Invoices_View" },
        { title: 'Payroll Run', key: 'payrollRun', path: '/invoices/payroll-run', icon: <PaidIcon sx={{ color: "#4CAF50" }} />, requiredRole: "Accounting_Invoices_View" },
        { title: 'Billing Accounts', key: 'creditLimitFormAdvancePayments', path: '/billingAccounts', icon: <BatteryCharging60Icon sx={{ color: "#03A9F4" }} />, requiredRole: "Accounting_BillingAccounts_View" },
        { title: "Multi-Payment Certificates", key: "multiPaymentCertificates", path: "/multiPaymentCertificates", icon: <AccountBalanceWalletOutlinedIcon sx={{ color: "#3F51B5" }} />, requiredRole: "Accounting_MultiPaymentCertificates_View" },
    ];

    return (
        <Toolbar sx={{ display: 'flex', justifyContent: 'flex-start', alignItems: 'center', paddingLeft: 0 }}>
            <Box display="flex" alignItems="center">
                <List sx={{ display: 'flex', padding: 0, gap: .1 }}>
                    {menuGroups.map((group) => (
                        <Can perform={group.requiredRole} key={group.groupKey}>
                            {(() => {
                                const itemPaths = group.subItems.map(sub => normalizePath(sub.path));
                                const isGroupSelected = itemPaths.some(p => p === normalizedSelectedMenuItem);
                                const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
                                const open = Boolean(anchorEl);

                                const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
                                    setAnchorEl(event.currentTarget);
                                };

                                const handleMenuClose = () => {
                                    setAnchorEl(null);
                                };

                                return (
                                    <ListItem key={group.groupKey} disablePadding>
                                        <IconButton onClick={handleMenuOpen} sx={getNavItemStyles(isGroupSelected)}>
                                            {group.icon}
                                            <FloatingLabelText label={group.title} translationKey={`accounting.menu.${group.groupKey}`}>
                                                {getTranslatedLabel(`accounting.menu.${group.groupKey}`, group.title).toUpperCase()}
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
                                            {group.subItems.map((sub) => {
                                                const isSubSelected = normalizePath(sub.path) === normalizedSelectedMenuItem;
                                                return (
                                                    <MenuItem
                                                        key={sub.key}
                                                        component={NavLink}
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
                            })()}
                        </Can>
                    ))}

                    {/* Standalone Items – wrapped in <Can> */}
                    {standaloneItems.map((item) => (
                        <Can perform={item.requiredRole} key={item.key}>
                            {(() => {
                                const isSelected = normalizePath(item.path) === normalizedSelectedMenuItem;
                                const LinkComponent = item.isPayment ? NavLinkWithReset : NavLink;

                                return (
                                    <ListItem
                                        component={LinkComponent}
                                        to={item.path}
                                        sx={getNavItemStyles(isSelected)}
                                        onClick={() => handleClick(item.key)}
                                        disablePadding
                                    >
                                        {item.icon}
                                        <FloatingLabelText label={item.title} translationKey={`accounting.menu.${item.key}`}>
                                            {getTranslatedLabel(`accounting.menu.${item.key}`, item.title).toUpperCase()}
                                        </FloatingLabelText>
                                    </ListItem>
                                );
                            })()}
                        </Can>
                    ))}
                    
                </List>
            </Box>
        </Toolbar>
    );
}