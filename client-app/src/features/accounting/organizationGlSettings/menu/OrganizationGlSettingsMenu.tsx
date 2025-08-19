import { useTheme } from "@mui/material";
import Grid from "@mui/material/Grid";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import { useLocation, useNavigate, useParams } from "react-router-dom";

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
const normalizePath = (path: string) => path.replace(/^\//, "").toLowerCase();

const OrganizationGlSettingsMenuNavContainer = ({
  selectedMenuItem,
  router,
}: OrgGlSettingsMenuProps & { router: any }) => {
  const theme = useTheme();
  const { location, navigate } = router;
  const onSelect = (event: MenuSelectEvent) => {
    navigate(event.item.data.route);
  };

  const normalizedCurrentPath = normalizePath(location.pathname);
  const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || "");

  const menuStyles = (path: string) => {
    const normalizedPath = normalizePath(path);
    const isSelected = normalizedPath === normalizedCurrentPath;

    return {
      color: isSelected ? theme.palette.primary.main : "inherit",
      fontWeight: isSelected ? "bold" : "normal",
    };
  };

  return (
    <Grid container spacing={2}>
      <Grid item xs={12}>
        <div className="col-md-6">
          <Menu onSelect={onSelect}>
            {links.map((link: any, index: number) => {
              return (
                <MenuItem
                  key={index}
                  text={link.title}
                  data={{ route: link.path }}
                  cssClass={"div-container-withBorderCurved-wrap"}
                  cssStyle={menuStyles(link.path)}
                />
              );
            })}
          </Menu>
        </div>
      </Grid>
    </Grid>
  );
};

const links = [
  { title: "Chart Of Accounts", path: "/orgChartOfAccount" },
  { title: "GL Account Defaults", path: "/gLAccountDefaults" },
  { title: "Time Period", path: "/timePeriod" },
];

export default withRouter(OrganizationGlSettingsMenuNavContainer);
