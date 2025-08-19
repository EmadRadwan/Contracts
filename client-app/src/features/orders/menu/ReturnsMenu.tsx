import { NavLink } from 'react-router-dom';
import { Box, List, ListItem, Typography, Toolbar, useTheme } from '@mui/material';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import ListAltOutlinedIcon from '@mui/icons-material/ListAltOutlined';

// REFACTOR: Add returnId to props for dynamic route
// Purpose: Allow navigation to /returns/:returnId/items
// Why: Aligns with separate OrderReturnItems route
interface Props {
  selectedMenuItem?: string;
  returnId?: string; // Add returnId prop
}

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function ReturnsMenu({ selectedMenuItem, returnId }: Props) {
  const { getTranslatedLabel } = useTranslationHelper();
  const theme = useTheme();
  const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');

  // REFACTOR: Update menuItems to use dynamic returnId
  // Purpose: Construct correct path for Return Items route
  // Why: Ensures navigation to the specific return's items screen
  const menuItems = [
    {
      name: 'Return Items',
      path: returnId ? `/returns/${returnId}/items` : '/returns', // Fallback to /returns if returnId is undefined
key: 'order.returns.menu.items',
    icon: <ListAltOutlinedIcon sx={{ color: '#FFA500' }} />,
},
];

return (
    <Toolbar sx={{ display: 'flex', justifyContent: 'flex-start', alignItems: 'center' }}>
        <Box display="flex" alignItems="center">
            <List sx={{ display: 'flex' }}>
                {menuItems.map(({ name, path, icon, key }) => {
                    const normalizedPath = normalizePath(path);
                    const isSelected = normalizedSelectedMenuItem === normalizedPath;
                    return (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={path}
                            sx={{
                                color: isSelected ? theme.palette.primary.main : 'inherit',
                                textDecoration: 'none',
                                fontWeight: isSelected ? 'bold' : 'normal',
                                typography: 'h6',
                                '&:hover': {
                                    color: 'grey.500',
                                },
                                whiteSpace: 'nowrap',
                                display: 'flex',
                                alignItems: 'center',
                            }}
                            // REFACTOR: Disable navigation if returnId is undefined
                            // Purpose: Prevent navigation to invalid route
                            // Why: Ensures user can't navigate to items without a valid return
                            onClick={(e) => !returnId && e.preventDefault()}
                        >
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: '2px' }}>
                                {icon}
                                <Typography variant="body1" sx={{ marginInlineEnd: '4px' }}>
                                    {getTranslatedLabel(key, name)}
                                </Typography>
                            </Box>
                        </ListItem>
                    );
                })}
            </List>
        </Box>
    </Toolbar>
);
}