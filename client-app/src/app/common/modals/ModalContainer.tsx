import React, { ReactNode } from 'react';
import Modal from '@mui/material/Modal';
import Backdrop from '@mui/material/Backdrop';
import Box from '@mui/material/Box';
import { animated, useSpring } from '@react-spring/web';
import { useAppSelector } from '../../store/configureStore';

interface FadeProps {
    children: React.ReactElement;
    in?: boolean;
    onClick?: (event: React.MouseEvent) => void;
    onEnter?: (node: HTMLElement, isAppearing: boolean) => void;
    onExited?: (node: HTMLElement, isAppearing: boolean) => void;
    ownerState?: any;
}

const Fade = React.forwardRef<HTMLDivElement, FadeProps>(function Fade(props, ref) {
    const { children, in: open, onClick, onEnter, onExited, ownerState, ...other } = props;
    const style = useSpring({
        from: { opacity: 0 },
        to: { opacity: open ? 1 : 0 },
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
            {React.cloneElement(children, { onClick })}
        </animated.div>
    );
});

interface ModalContainerProps {
    show: boolean;
    onClose: () => void;
    children: ReactNode;
    width?: number | string;     // Allow string like '95vw'
    maxWidth?: number;
    dir?: 'rtl' | 'ltr';     // now optional
    lang?: string;           // now optional
}


const ModalContainer: React.FC<ModalContainerProps> = ({
    show,
    onClose,
    children,
    width = '95vw',              // Default to 95% of viewport
    maxWidth = 1680,             // Good upper limit (prevents it from being too stretched)
    // dir = 'rtl',
    lang,
}) => {

    const language = useAppSelector((state) => state.localization.language);
    const dir = language === 'ar' ? 'rtl' : 'ltr'; // Determine direction based on language

    const effectiveLang = lang || (dir === 'rtl' ? 'ar' : 'en');
    const style = {
        position: 'absolute' as 'absolute',
        top: '50%',
        left: '50%',
        transform: 'translate(-50%, -50%)',

        // Dynamic width logic
        width: typeof width === 'number' ? `${width}px` : width,
        maxWidth: maxWidth ? `${maxWidth}px` : undefined,

        // Minimal margins + nice padding on very large screens
        maxHeight: '92vh',           // Prevent it from being too tall
        overflow: 'auto',

        bgcolor: 'background.paper',
        border: '2px solid #000',
        boxShadow: 24,
        p: 3,                        // Slightly reduced padding
        direction: dir,
        textAlign: dir === 'rtl' ? 'right' : 'left',
        borderRadius: 2,             // Optional: nicer look
    };


    return (
        <Modal
            aria-labelledby="spring-modal-title"
            aria-describedby="spring-modal-description"
            open={show}
            onClose={onClose}
            closeAfterTransition
            slots={{ backdrop: Backdrop }}
            slotProps={{
                backdrop: {
                    TransitionComponent: Fade,
                },
            }}
            sx={{ zIndex: 99 }}
        >
            <Fade in={show}>
                <Box
                    sx={style}
                    dir={dir}
                    lang={effectiveLang}
                >
                    {children}
                </Box>
            </Fade>
        </Modal>
    );
};

export default ModalContainer;
