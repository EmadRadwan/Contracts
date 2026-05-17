import React, {useState} from 'react';
import {Button, Menu, MenuItem} from '@mui/material';
import {useNavigate} from 'react-router-dom';
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";

interface Props {
    partyId?: string;
    partyName?: string;
}

const CreateCustomerMenu: React.FC<Props> = ({partyId, partyName}) => {
    const navigate = useNavigate();
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const {getTranslatedLabel} = useTranslationHelper();

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const handleFinancialHistory = () => {
        if (partyId) {
            navigate(`/party/${partyId}/financial-history`, {
                state: { partyName } // ← Pass partyName here
            });
        }
        handleClose();
    };

    console.log('partyName:', partyName);
    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleClick}
                sx={{mt: 2, mr: 2}}
                disabled={!partyId}
            >
                {getTranslatedLabel("general.actions", "Actions")}
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                anchorOrigin={{vertical: 'bottom', horizontal: 'right'}}
                transformOrigin={{vertical: 'top', horizontal: 'right'}}
            >
                <MenuItem onClick={handleFinancialHistory} disabled={!partyId}>
                    {getTranslatedLabel("party.financial.history.menuTitle", "Financial History")}
                </MenuItem>
            </Menu>
        </>
    );
};

export default CreateCustomerMenu;