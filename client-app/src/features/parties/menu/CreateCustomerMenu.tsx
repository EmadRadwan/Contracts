import React, { useState } from 'react';
import { Button, Menu, MenuItem } from '@mui/material';
import { useNavigate } from 'react-router-dom';

interface Props {
  partyId?: string;
}

const CreateCustomerMenu: React.FC<Props> = ({ partyId }) => {
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleFinancialHistory = () => {
    if (partyId) {
      navigate(`/party/${partyId}/financial-history`);
}
handleClose();
};

return (
    <>
        <Button
            variant="contained"
            color="primary"
            onClick={handleClick}
            sx={{ mt: 2, mr: 2 }}
            disabled={!partyId}
        >
            Actions
        </Button>
        <Menu
            anchorEl={anchorEl}
            open={open}
            onClose={handleClose}
            anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            transformOrigin={{ vertical: 'top', horizontal: 'right' }}
        >
            <MenuItem onClick={handleFinancialHistory} disabled={!partyId}>
                Financial History
            </MenuItem>
            {/* Add other menu items here if needed */}
        </Menu>
    </>
);
};

export default CreateCustomerMenu;