import React from 'react'
import FormatListNumberedIcon from '@mui/icons-material/FormatListNumbered';
import HandshakeRoundedIcon from '@mui/icons-material/HandshakeRounded';
import { NavLink } from "react-router-dom";
import { Box, Container, List, ListItem, Typography, Toolbar } from '@mui/material';

const AgreementsMenu = () => {
    const menuItems = [
        { title: "Agreement Items", path: "/agreementItems", icon: <FormatListNumberedIcon sx={{ color: "#FFA500" }} /> },
        { title: "Agreement Terms", path: "/agreementTerms", icon: <HandshakeRoundedIcon sx={{ color: "#FF4081" }} /> },        
    ]
  return (
    <Container>
            <Toolbar sx={{ display: 'flex', justifyContent: 'flex-end', alignItems: 'center' }}>
                <Box display='flex' alignItems='center'>
                    <List sx={{ display: 'flex' }}>
                        {menuItems.map(({ title, path, icon }) => (
                            <ListItem
                                component={NavLink}
                                to={path}
                                key={path}
                                sx={{
                                    color: 'inherit',
                                    textDecoration: 'none',
                                    typography: 'h6',
                                    "&:hover": {
                                        color: 'grey.500',
                                    },
                                    whiteSpace: 'nowrap',
                                    display: 'flex',
                                    alignItems: 'center'
                                }}
                            >
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: '2px' }}>
                                    {icon}
                                    <Typography variant="body1" sx={{ marginInlineStart: '4px' }}>
                                        {title}
                                    </Typography>
                                </Box>
                            </ListItem>
                        ))}
                    </List>
                </Box>
            </Toolbar>
        </Container>
  )
}

export default AgreementsMenu