import React from 'react';
import { Modal, Fade, Box, Backdrop } from '@mui/material';

interface ModalContainer2Props {
    show: boolean;
    onClose: () => void;
    onEntered?: () => void; // callback when fade is complete
    children: React.ReactNode;
}

export default function ModalContainer2({
                                            show,
                                            onClose,
                                            onEntered,
                                            children
                                        }: ModalContainer2Props) {
    return (
        <Modal
            open={show}
            onClose={onClose}
            closeAfterTransition
            slots={{ backdrop: Backdrop }}
            slotProps={{
                backdrop: {
                    TransitionComponent: Fade,
                    // `onEntered` fires when the Fade in transition completes
                    onEntered: onEntered
                }
            }}
        >
            <Fade in={show}>
                <Box
                    sx={{
                        p: 3,
                        bgcolor: 'background.paper',
                        // typical absolute + transform to center
                        position: 'absolute',
                        top: '50%',
                        left: '50%',
                        transform: 'translate(-50%, -50%)'
                    }}
                >
                    {children}
                </Box>
            </Fade>
        </Modal>
    );
}
