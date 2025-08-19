import React from "react";
import { Breadcrumbs, MenuItem, Select, Typography } from "@mui/material";
import { NavLink, useLocation, useNavigate } from "react-router-dom";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

const AccountingReportBreadcrumbs = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const navigate = useNavigate();
  const location = useLocation();

  const links = [
    { 
      path: "/trialBalance", 
      title: "Trial Balance", 
      key: "trial-balance" 
    },
    {
      path: "/transactionTotals",
      title: "Transaction Totals",
      key: "transaction-totals",
    },
    {
      path: "/incomeStatement",
      title: "Income Statement",
      key: "income-statement",
    },
    {
      path: "/cashFlowStatement",
      title: "Cash Flow Statement",
      key: "cash-flow-statement",
    },
    { 
      path: "/balanceSheet", 
      title: "Balance Sheet", 
      key: "balance-sheet" 
    },
    {
      path: "/comparativeIncomeStatement",
      title: "Comparative Income Statement",
      key: "comparative-income-statement",
    },
    {
      path: "/comparativeCashFlowStatement",
      title: "Comparative Cash Flow Statement",
      key: "comparative-cash-flow-statement",
    },
    {
      path: "/comparativeBalanceSheet",
      title: "Comparative Balance Sheet",
      key: "comparative-balance-sheet",
    },
    {
      path: "/glAccountTrialBalance",
      title: "GL Account Trial Balance",
      key: "gl-account-trial-balance",
    },
    {
      path: "/inventoryValuation",
      title: "Inventory Valuation",
      key: "inventory-valuation",
    },
    { 
      path: "/costCenters", 
      title: "Cost Centers", 
      key: "cost-centers" 
    },
  ];

  return (
    <Breadcrumbs aria-label="breadcrumb" sx={{ marginY: 2, marginLeft: 3 }}>
      <NavLink
        to="/orgGl"
        style={({ isActive }) => ({
          textDecoration: "none",
          color: isActive ? "blue" : "inherit",
        })}
      >
        {getTranslatedLabel("general.accounting", "Accounting")}
      </NavLink>
      <NavLink
        to="/orgAccountingSummary"
        style={({ isActive }) => ({
          textDecoration: "none",
          color: isActive ? "blue" : "inherit",
        })}
      >
        {getTranslatedLabel("accounting.orgGL.accounting.summary.menu.summary", "Summary")}
      </NavLink>
      <NavLink
        to="/accountingReports"
        style={({ isActive }) => ({
          textDecoration: "none",
          color: isActive ? "blue" : "inherit",
        })}
      >
        {getTranslatedLabel("accounting.orgGL.accounting.summary.menu.reports", "Reports")}
      </NavLink>
      <Select
        value={location.pathname}
        onChange={(e) => navigate(e.target.value)}
        displayEmpty
        renderValue={(selected) => (
          <Typography color="text.primary">
            {links.find((link) => link.path === selected) &&
              getTranslatedLabel(
                `accounting.orgGL.reports.menu.${links.find(
                  (link) => link.path === selected
                )?.key}`,
                links.find((link) => link.path === selected)?.title || ""
              )}
          </Typography>
        )}
        sx={{ minWidth: 150 }}
      >
        {links.map((link) => (
          <MenuItem key={link.path} value={link.path}>
            {getTranslatedLabel(
              `accounting.orgGL.reports.menu.${link.key}`,
              link.title
            )}
          </MenuItem>
        ))}
      </Select>
    </Breadcrumbs>
  );
};

export default AccountingReportBreadcrumbs;
