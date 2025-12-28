import { Box, List, ListItem, Toolbar, Typography } from "@mui/material";
import { NavLink } from "react-router-dom";
import { useTheme } from "@mui/material/styles";
import PersonIcon from '@mui/icons-material/Person';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LinkIcon from '@mui/icons-material/Link';
import ManageAccountsIcon from '@mui/icons-material/ManageAccounts';

interface CRMMenuProps {
    selectedMenuItem?: string;
}

const links = [
    { title: "Leads", key: "leads", path: "/leads", icon: <ManageAccountsIcon sx={{ color: "#fabd52" }} /> },
    { title: "Contacts", key: "contacts", path: "/contacts", icon: <PersonIcon sx={{ color: "#FFA500" }} /> },
    { title: "Accounts", key: "accounts", path: "/accounts", icon: <LinkIcon sx={{ color: "#FF4081" }} /> },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

const CRMMenu = ({selectedMenuItem}: CRMMenuProps) => {
  const theme = useTheme();
    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');
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
            alignItems: 'center',
            whiteSpace: 'nowrap',
            marginRight: '4px', // Adjust the space between icon and text
        };
    };

    return (
        <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'left' }}>

            <Box display='flex' alignItems='left'>
                <List sx={{ display: 'flex' }}>
                    {links.map(({ title, path, icon, key }, index) => (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={index}
                            sx={navStyles(path)}
                        >
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                {icon}
                                <Typography variant="body1">
                                    {getTranslatedLabel(`crm.menu.${key}`, title)}
                                </Typography>
                            </Box>
                        </ListItem>
                    ))}

                </List>
            </Box>

        </Toolbar>
    );
}

export default CRMMenu