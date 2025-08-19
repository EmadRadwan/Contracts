import { Box, List, ListItem, Toolbar, Typography, useTheme } from "@mui/material";
import {NavLink, useLocation, useNavigate, useParams} from "react-router-dom";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { router } from "../../../../app/router/Routes";


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

const FinancialAccountsMenuNavContainer = (props: any) => {
    const theme = useTheme();
    const normalizedSelectedMenuItem = normalizePath(props.selectedMenuItem || '');
    const { getTranslatedLabel } = useTranslationHelper();

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
        <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'left' }}>

            <Box display='flex' alignItems='left'>
                <List sx={{ display: 'flex' }}>
                    {links.map(({ title, path, key }, index) => (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={index}
                            end={path === "/financialAccounts"}
                            sx={navStyles(path)}
                        >
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                <Typography variant="body1" sx={{ marginX: '4px' }}>
                                    {getTranslatedLabel(`accounting.orgGL.menu.${key}`, title)}
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
    {title: 'Account', path: '/financialAccounts', key: "acc"},
    {title: 'Transactions', path: '/financialAccounts/transactions', key: "trx"},
    //{title: 'Deposit/Withdraw', path: '/financialAccounts/depositWithdraw', key: "dep"},
    // {title: 'Deposit Slips', path: '/finAccountDepositSlips', key: "slips"},
    // {title: 'Reconciliation', path: '/finAccountReconciliation', key: "recon"}
];


export default withRouter(FinancialAccountsMenuNavContainer)
