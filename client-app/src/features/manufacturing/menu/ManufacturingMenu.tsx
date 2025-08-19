import { NavLink, useNavigate } from "react-router-dom";
import { Box, List, ListItem, Toolbar, Typography } from "@mui/material";
import { useTheme } from "@mui/material/styles";
import WorkOutlineOutlinedIcon from '@mui/icons-material/WorkOutlineOutlined';
import TimelineOutlinedIcon from '@mui/icons-material/TimelineOutlined';
import AssignmentTurnedInOutlinedIcon from '@mui/icons-material/AssignmentTurnedInOutlined';
import MonetizationOnOutlinedIcon from '@mui/icons-material/MonetizationOnOutlined';
import ListAltOutlinedIcon from '@mui/icons-material/ListAltOutlined';
import {setJobRunUnderProcessing, setProductionRunStatusDescription} from "../slice/manufacturingSharedUiSlice";
import {useAppDispatch} from "../../../app/store/configureStore";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

interface ManufacturingMenuProps {
    selectedMenuItem?: string;
}

const links = [
    { title: "Work Orders", key: "jobshop", path: "/jobShop", icon: <WorkOutlineOutlinedIcon sx={{ color: "#FFA500" }} /> },
    { title: "Routings", key: "routing", path: "/routings", icon: <TimelineOutlinedIcon sx={{ color: "#FF4081" }} /> },
    { title: "Routing Tasks", key: "tasks", path: "/routingTasks", icon: <AssignmentTurnedInOutlinedIcon sx={{ color: "#00BFFF" }} /> },
    { title: "Costs", key: "costs", path: "/costs", icon: <MonetizationOnOutlinedIcon sx={{ color: "#4CAF50" }} /> },
    { title: "Bill Of Materials", key: "bom", path: "/billOfMaterials", icon: <ListAltOutlinedIcon sx={{ color: "#FFC107" }} /> },
    //{ title: "MRP", path: "/mrp", icon: <IconComponent /> },
];

const normalizePath = (path: string) => path.replace(/^\//, '').toLowerCase();

export default function ManufacturingMenu({ selectedMenuItem }: ManufacturingMenuProps) {
    const theme = useTheme();
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const {getTranslatedLabel} = useTranslationHelper()

    const normalizedSelectedMenuItem = normalizePath(selectedMenuItem || '');
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
            marginRight: '4px'
        };
    };

    const handleJobShopClick = (event: React.MouseEvent<HTMLLIElement, MouseEvent>) => {
        event.preventDefault();
        
        navigate("/jobShop", { state: { editMode: 0 } });
    };

    return (
        <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'left' }}>
            <Box display='flex' alignItems='left'>
                <List sx={{ display: 'flex' }}>
                    {links.map(({ title, path, icon, key }) => (
                        <ListItem
                            component={title === "Work Orders" ? 'div' : NavLink}
                            to={path}
                            key={path}
                            sx={navStyles(path)}
                            onClick={title === "Work Orders" ? handleJobShopClick : undefined}
                        >
                            <Box sx={{ display: 'flex', alignItems: 'center', marginRight: '8px', gap: 1 }}>
                                {icon}
                                <Typography variant="body1">
                                    {getTranslatedLabel(`manufacturing.menu.${key}`, title)}
                                </Typography>
                            </Box>
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Toolbar>
    );
}
