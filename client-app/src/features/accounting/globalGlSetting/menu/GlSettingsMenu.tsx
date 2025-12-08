import Grid from "@mui/material/Grid";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import {useLocation, useNavigate, useParams} from "react-router-dom";
import {useTheme} from "@mui/material/styles";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

const linkKeys = [
    {titleKey: 'accounting.glSettingsMenu.chartOfAccounts', defaultTitle: 'Chart of Accounts', path: '/chartOfAccounts'},
    {titleKey: 'accounting.glSettingsMenu.customTimePeriods', defaultTitle: 'Edit Custom Time Periods', path: '/customTimePeriods'},
    //{titleKey: 'accounting.glSettingsMenu.cost', defaultTitle: 'Cost', path: '/accountingCosts'},
    {titleKey: 'accounting.glSettingsMenu.paymentMethodType', defaultTitle: 'Payment Method Type', path: '/paymentMethodType'},
    {titleKey: 'accounting.glSettingsMenu.invoiceItemType', defaultTitle: 'Invoice Item Type', path: '/invoiceItemType'},
    //{titleKey: 'accounting.glSettingsMenu.rates', defaultTitle: 'Rates', path: '/rates'},
    {titleKey: 'accounting.glSettingsMenu.fxRates', defaultTitle: 'Foreign Exchange Rates', path: '/FXRates'},
    // {titleKey: 'accounting.glSettingsMenu.glAccCategory', defaultTitle: 'GL Account Category', path: '/GLAccCategory'},
    //{titleKey: 'accounting.glSettingsMenu.costCenters', defaultTitle: 'Cost Centers', path: '/globalCostCenters'},
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
    const { getTranslatedLabel } = useTranslationHelper();

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
                        {linkKeys.map((link: any, index: number) => (
                            <MenuItem
                                key={index}
                                text={getTranslatedLabel(link.titleKey, link.defaultTitle)}
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
