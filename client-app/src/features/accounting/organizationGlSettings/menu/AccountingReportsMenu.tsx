import {
  Box,
  List,
  ListItem,
  Toolbar,
  Typography,
  useTheme,
} from "@mui/material";
import React from "react";
import { NavLink } from "react-router-dom";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

const normalizePath = (path: string) => path.replace(/^\//, "").toLowerCase();

interface AccountingMenuProps {
  selectedMenuItem?: string;
}

const links1 = [
  { path: "/trialBalance", title: "Trial Balance" },
  { path: "/transactionTotals", title: "Transaction Totals" },
  { path: "/incomeStatement", title: "Income Statement" },
  { path: "/cashFlowStatement", title: "Cash Flow Statement" },
  { path: "/balanceSheet", title: "Balance Sheet" },
  {
    path: "/comparativeIncomeStatement",
    title: "Comparative Income Statement",
  },
  {
    path: "/comparativeCashFlowStatement",
    title: "Comparative Cash Flow Statement",
  },
];

const links2 = [
  
  { path: "/comparativeBalanceSheet", title: "Comparative Balance Sheet" },
  { path: "/glAccountTrialBalance", title: "GL Account Trial Balance" },
  { path: "/inventoryValuation", title: "Inventory Valuation" },
  { path: "/costCenters", title: "Cost Centers" },
];

const AccountingReportsMenu = ({ selectedMenuItem = "" }: AccountingMenuProps) => {
  const theme = useTheme();
  const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || "");
  const { getTranslatedLabel } = useTranslationHelper();

  const navStyles = (path: string) => {
    const normalizedPath = normalizePath(path);
    const isSelected = normalizedPath === normalizedSelectedMenuItem;

    return {
      color: isSelected ? theme.palette.primary.main : "inherit",
      "&.active": {
        color: theme.palette.primary.main,
      },
      textDecoration: "none",
      typography: "h6",
      "&:hover": {
        color: "grey.500",
      },
      fontWeight: isSelected ? "bold" : "normal",
      display: "flex",
      borderRadius: "4px",
      padding: "4px",
      border: "1px solid",
      borderColor: theme.palette.grey[300],
      alignItems: "center",
      whiteSpace: "nowrap",
      margin: "4px",
    };
  };

  return (
    <Toolbar
      sx={{
        display: "flex",
        justifyContent: "flex-start",
        alignItems: "start",
        flexDirection: "column",
        width: "100%",
        marginTop: 3,
      }}
    >
      <Box display="flex" alignItems="left">
        <List
          sx={{ display: "flex", flexWrap: "nowrap", margin: 0, padding: 0 }}
        >
          {links1.map(({ title, path }, index) => (
            <ListItem
              component={NavLink}
              to={path}
              key={index}
              sx={navStyles(path)}
            >
              <Typography variant="body1">{title}</Typography>
            </ListItem>
          ))}
        </List>
      </Box>
      <Box display="flex" alignItems="left">
        <List
          sx={{ display: "flex", flexWrap: "nowrap", margin: 0, padding: 0 }}
        >
          {links2.map(({ title, path }, index) => (
            <ListItem
              component={NavLink}
              to={path}
              key={index}
              sx={navStyles(path)}
            >
              <Typography variant="body1">{title}</Typography>
            </ListItem>
          ))}
        </List>
      </Box>
    </Toolbar>
  );
};

export default AccountingReportsMenu;
