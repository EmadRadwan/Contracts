import Grid from "@mui/material/Grid";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import {useLocation, useNavigate, useParams} from "react-router-dom";
import {useTheme} from "@mui/material/styles";

const links = [
    {title: 'Chart Of Accounts', path: '/chartOfAccounts'},
    {title: 'Edit Custom Time Periods', path: '/customTimePeriods'},
    {title: 'Cost', path: '/accountingCosts'},
    {title: 'Payment Method Type', path: '/paymentMethodType'},
    {title: 'Invoice Item Type', path: '/invoiceItemType'},
    //{title: 'Rates', path: '/rates'},
    {title: 'Foreign Exchange Rates', path: '/FXRates'},
    // {title: 'GL Account Category', path: '/GLAccCategory'},
    //{title: 'Cost Centers', path: '/globalCostCenters'},
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

interface GlSettingsMenuNavContainerProps {
    selectedMenuItem?: string;
}

function withRouter(Component: any) {
    function ComponentWithRouterProp(props: any) {
        const location = useLocation();
        const navigate = useNavigate();
        const params = useParams();
        return <Component {...props} router={{location, navigate, params}}/>;
    }

    return ComponentWithRouterProp;
}

const GlSettingsMenuNavContainer = ({selectedMenuItem, router}: GlSettingsMenuNavContainerProps & { router: any }) => {
    const theme = useTheme();
    const {location, navigate} = router;

    const normalizedCurrentPath = normalizePath(location.pathname);
    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');

    const onSelect = (event: MenuSelectEvent) => {
        navigate(event.item.data.route);
    };

    const menuStyles = (path: string) => {
        const normalizedPath = normalizePath(path);
        const isSelected = normalizedPath === normalizedCurrentPath || normalizedPath === normalizedSelectedMenuItem;


        return {
            color: isSelected ? theme.palette.primary.main : 'inherit',
            fontWeight: isSelected ? 'bold' : 'normal',
        };
    };

    return (
        <Grid container spacing={2}>
            <Grid item xs={12}>
                <div className="col-md-6">
                    <Menu onSelect={onSelect}>
                        {links.map((link: any, index: number) => (
                            <MenuItem
                                key={index}
                                text={link.title}
                                data={{route: link.path}}
                                cssStyle={menuStyles(link.path)}
                            />
                        ))}
                    </Menu>
                </div>
            </Grid>
        </Grid>
    );
};

export default withRouter(GlSettingsMenuNavContainer);
