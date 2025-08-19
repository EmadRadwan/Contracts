import { useTheme } from "@mui/material";
import { Toolbar, Typography, Box } from "@mui/material";
import {NavLink, useLocation, useNavigate, useParams} from "react-router-dom";
import { List, ListItem } from "@mui/material";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";


function withRouter(Component: any) {
    function ComponentWithRouterProp(props: any) {
        const location = useLocation();
        const navigate = useNavigate();
        const params = useParams();
        return <Component {...props} router={{location, navigate, params}}/>;
    }

    return ComponentWithRouterProp;
}

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

const AccountingSummaryMenuNavContainer = (props: any) => {
    const theme = useTheme();
    const normalizedSelectedMenuItem = normalizePath(props.selectedMenuItem || '');
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.orgGL.accounting.summary.menu"

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
            borderRadius: '4px',
            padding: '4px',
            border: '1px solid',
            borderColor: theme.palette.grey[300],
            alignItems: 'center',
            whiteSpace: 'nowrap',
            marginRight: '4px', // Adjust the space between icon and text
        };
    };

    return (
        <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'left', width: '100%' }}>

            <Box display='flex' alignItems='left'>
                <List sx={{ display: 'flex' }}>
                    {links.map(({ title, path, key }, index) => (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={index}
                            sx={navStyles(path)}
                        >
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                <Typography variant="body1" sx={{ marginX: '4px' }}>
                                    {getTranslatedLabel(`${localizationKey}.${key}`, title)}
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
    {title: 'Summary', path: '/orgAccountingSummary', key: "summary"},
    {title: 'Accounting Transaction', path: '/accountingTransaction', key: "txns"},
    {title: 'Accounting Transaction Entries', path: '/accountingTransactionEntries', key: "txnEntries"},
    // {title: 'Account Reconciliation', path: '/accountReconciliation', key: "recon"},
    {title: 'Reports', path: '/accountingReports', key: "reports"},
    // {title: 'Checks', path: '/accountingChecks', key: "checks"},
];


export default withRouter(AccountingSummaryMenuNavContainer)
