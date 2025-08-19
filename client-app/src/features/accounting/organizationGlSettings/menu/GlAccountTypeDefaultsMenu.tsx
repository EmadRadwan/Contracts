import Grid from "@mui/material/Grid";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { useTheme } from "@mui/material/styles";

const links = [
  { title: "GL Account Type Defaults", path: "/gLAccountTypeDefaults" },
  { title: "Product GL Accounts", path: "/productGLAccounts" },
  { title: "Product Category GL Account", path: "/productCategoryGLAccount" },
  { title: "FinAccount Type GL Account", path: "/finAccountTypeGLAccount" },
  { title: "Sales Invoice GL Account", path: "/salesInvoiceGLAccount" },
  { title: "Purchase Invoice GL Account", path: "/purchaseInvoiceGLAccount" },
  {
    title: "Payment Type/GL Account Type ID",
    path: "/paymentTypeGLAccountTypeID",
  },
  {
    title: "Payment Method ID/GL Account ID",
    path: "/paymentMethodIDGLAccountID",
  },
  { title: "Variance Reason GL Accounts", path: "/varianceReasonGLAccounts" },
  { title: "Credit Card Type GL Account", path: "/creditCardTypeGLAccount" },
  { title: "Tax Authority GL Accounts", path: "/taxAuthorityGLAccounts" },
  { title: "Party GL Accounts", path: "/partyGLAccounts" },
  { title: "Fixed Asset Type GL Mappings", path: "/fixedAssetTypeGLMappings" },
];

const normalizePath = (path: string) => path.replace(/^\//, "").toLowerCase();

interface GlAccountTypeDefaultsMenuNavContainerProps {
  selectedMenuItem?: string;
}

function withRouter(Component: any) {
  function ComponentWithRouterProp(props: any) {
    const location = useLocation();
    const navigate = useNavigate();
    const params = useParams();
    return <Component {...props} router={{ location, navigate, params }} />;
  }

  return ComponentWithRouterProp;
}

const GlAccountTypeDefaultsMenuNavContainer = ({
  selectedMenuItem,
  router,
}: GlAccountTypeDefaultsMenuNavContainerProps & { router: any }) => {
  const theme = useTheme();
  const { location, navigate } = router;

  const normalizedCurrentPath = normalizePath(location.pathname);
  const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || "");

  const onSelect = (event: MenuSelectEvent) => {
    navigate(event.item.data.route);
  };

  const menuStyles = (path: string) => {
    const normalizedPath = normalizePath(path);
    const isSelected =
    // normalizedPath === normalizedSelectedMenuItem ||
    normalizedPath === normalizedCurrentPath 
    // console.log(
    //   `Path: ${path}, Normalized Path: ${normalizedPath}, Is Selected: ${isSelected}`
    // );

    return {
      color: isSelected ? theme.palette.primary.main : "inherit",
      fontWeight: isSelected ? "bold" : "normal",
    };
  };

  return (
    <Grid container marginTop={2} spacing={2}>
      <Grid item xs={12}>
        <Menu onSelect={onSelect}>
          {links.map((link: any, index: number) => (
            <MenuItem
              key={index}
              text={link.title}
              data={{ route: link.path }}
              cssClass={"div-container-withBorderCurved-wrap"}
              cssStyle={menuStyles(link.path)}
            />
          ))}
        </Menu>
      </Grid>
    </Grid>
  );
};

export default withRouter(GlAccountTypeDefaultsMenuNavContainer);