import {Button, Fade, Menu, MenuItem} from "@mui/material";
import React from "react";
import {Link} from "react-router-dom";
import {signOut} from "../../features/account/accountSlice";
import {useAppDispatch, useAppSelector} from "../store/configureStore";
import {useNavigate} from "react-router";

export default function SignedInMenu() {
    const dispatch = useAppDispatch();
    const {user} = useAppSelector(state => state.account);
    const navigate = useNavigate();
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
                    Change Password
                </MenuItem>
                <MenuItem onClick={() => {
                    dispatch(signOut());
                }}>Logout</MenuItem>
            </Menu>
        </>
    );
}