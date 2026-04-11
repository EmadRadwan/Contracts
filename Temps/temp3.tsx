interface ModalContainerProps {
    show: boolean;
    onClose: () => void;
    children: ReactNode;
    width?: number | string;     // Allow string like '95vw'
    maxWidth?: number;           // New: limit on very wide screens
    dir?: 'rtl' | 'ltr';
    lang?: string;
}

const ModalContainer: React.FC<ModalContainerProps> = ({
                                                           show,
                                                           onClose,
                                                           children,
                                                           width = '95vw',              // Default to 95% of viewport
                                                           maxWidth = 1680,             // Good upper limit (prevents it from being too stretched)
                                                           dir = 'rtl',
                                                           lang,
                                                       }) => {

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
                <Box sx={style} dir={dir} lang={effectiveLang}>
                    {children}
                </Box>
            </Fade>
        </Modal>
    );
};

export default ModalContainer;