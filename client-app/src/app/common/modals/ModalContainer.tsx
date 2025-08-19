import React, {ReactNode} from 'react';
import Modal from '@mui/material/Modal';
import Backdrop from '@mui/material/Backdrop';
import Box from '@mui/material/Box';
import {animated, useSpring} from '@react-spring/web';

interface FadeProps {
    children: React.ReactElement;
    in?: boolean;
    onClick?: (event: React.MouseEvent) => void;
    onEnter?: (node: HTMLElement, isAppearing: boolean) => void;
    onExited?: (node: HTMLElement, isAppearing: boolean) => void;
    ownerState?: any;
}

const Fade = React.forwardRef<HTMLDivElement, FadeProps>(function Fade(props, ref) {
    const {children, in: open, onClick, onEnter, onExited, ownerState, ...other} = props;
    const style = useSpring({
        from: {opacity: 0},
        to: {opacity: open ? 1 : 0},
        onStart: () => {
            if (open && onEnter) {
                onEnter(null as any, true);
            }
        },
        onRest: () => {
            if (!open && onExited) {
                onExited(null as any, true);
            }
        },
    });

    return (
        <animated.div ref={ref} style={style} {...other}>
            {React.cloneElement(children, {onClick})}
        </animated.div>
    );
});

interface ModalContainerProps {
    show: boolean;
    onClose: () => void;
    children: ReactNode;
    width?: number;
}


const ModalContainer: React.FC<ModalContainerProps> = ({show, onClose, children, width}) => {

    const style = {
        position: 'absolute' as 'absolute',
        top: '50%',
        left: '50%',
        transform: 'translate(-50%, -50%)',
        width: width ? `${width}px` : 'auto',
        bgcolor: 'background.paper',
        border: '2px solid #000',
        boxShadow: 24,
        p: 4,
    };

    return (
        <Modal
            aria-labelledby="spring-modal-title"
            aria-describedby="spring-modal-description"
            open={show}
            onClose={onClose}
            closeAfterTransition
            slots={{backdrop: Backdrop}}
            slotProps={{
                backdrop: {
                    TransitionComponent: Fade,
                },
            }}
            sx={{zIndex: 99}}
        >
            <Fade in={show}>
                <Box sx={style}>{children}</Box>
            </Fade>
        </Modal>
    );
};

export default ModalContainer;
