import { Box, List, ListItem, Toolbar, Typography, useTheme } from "@mui/material";
import { NavLink, useLocation, useNavigate, useParams } from "react-router-dom";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

const normalizePath = (path: string) => path.replace(/^\//, "").toLowerCase();

function withRouter(Component: any) {
  function ComponentWithRouterProp(props: any) {
    const location = useLocation();
    const navigate = useNavigate();
    const params = useParams();
    return <Component {...props} router={{ location, navigate, params }} />;
  }

  return ComponentWithRouterProp;
}

interface OrgGlSettingsMenuProps {
  selectedMenuItem?: string;
}

const OrganizationGlSettingsMenuNavContainer = ({
  selectedMenuItem,
  router,
}: OrgGlSettingsMenuProps & { router: any }) => {
  const theme = useTheme();
  const { location } = router;
  const { getTranslatedLabel } = useTranslationHelper();

  const normalizedCurrentPath = normalizePath(location.pathname);

  const navStyles = (path: string) => {
    const normalizedPath = normalizePath(path);
    const isSelected = normalizedPath === normalizedCurrentPath;

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
      marginRight: "4px",
    };
  };

  return (
    <Toolbar sx={{ display: "flex", justifyContent: "space-between", alignItems: "left" }}>
      <Box display="flex" alignItems="left">
        <List sx={{ display: "flex" }}>
          {links.map(({ title, path, key }, index) => (
            <ListItem
              component={NavLink}
              to={path}
              key={index}
              sx={navStyles(path)}
            >
              <Box sx={{ display: "flex", alignItems: "center" }}>
                <Typography variant="body1" sx={{ marginX: "4px" }}>
                  {getTranslatedLabel(`accounting.orgGlSettingsMenu.${key}`, title)}
                </Typography>
              </Box>
            </ListItem>
          ))}
        </List>
      </Box>
    </Toolbar>
  );
};

const links = [
  { title: "Chart Of Accounts", path: "/orgChartOfAccount", key: "chartOfAccounts" },
  { title: "GL Account Defaults", path: "/gLAccountDefaults", key: "glAccountDefaults" },
  { title: "Time Period", path: "/timePeriod", key: "timePeriod" },
];

export default withRouter(OrganizationGlSettingsMenuNavContainer);
