import {Button, Fade, Menu, MenuItem} from "@mui/material";
import React from "react";
import {Link} from "react-router-dom";
import {signOut} from "../../features/account/accountSlice";
import {useAppDispatch, useAppSelector} from "../store/configureStore";
import {useNavigate} from "react-router";
import {useTranslationHelper} from "../hooks/useTranslationHelper";
import {Can} from "../../features/account/Can";

export default function SignedInMenu() {
    const dispatch = useAppDispatch();
    const {user} = useAppSelector(state => state.account);
    const navigate = useNavigate();
    const {getTranslatedLabel} = useTranslationHelper();
    const [anchorEl, setAnchorEl] = React.useState(null);
    const open = Boolean(anchorEl);
    const handleClick = (event: any) => {
        setAnchorEl(event.currentTarget);
    };
    const handleClose = () => {
        setAnchorEl(null);
    };

    return (
        <>
            <Button
                color='inherit'
                onClick={handleClick}
                sx={{typography: 'h6'}}
            >
                {user?.displayName}
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                TransitionComponent={Fade}
            >
                <MenuItem onClick={() => {
                    handleClose();
                    navigate('/change-password');
                }}>
                    {getTranslatedLabel("menu.changePassword", "Change Password")}
                </MenuItem>
                <MenuItem onClick={() => {
                    dispatch(signOut());
                }}>{getTranslatedLabel("menu.logout", "Logout")}</MenuItem>
                <Can perform="Admin">
                    <MenuItem onClick={() => {
                        handleClose();
                        navigate('/users');
                    }}>{getTranslatedLabel("menu.userManagement", "User Management")}</MenuItem>
                </Can>
            </Menu>
        </>
    );
}