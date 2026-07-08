import {Backdrop, CircularProgress, Typography} from "@mui/material";
import {Box} from "@mui/system";

interface Props {
    message?: string;
    // When true, the backdrop is confined to the nearest positioned (relative) ancestor
    // instead of covering the full viewport — use this when the loading state must not
    // block clicks on controls rendered outside the loading region (e.g. a header action menu).
    scoped?: boolean;
}

export default function LoadingComponent({message = 'Loading...', scoped = false}: Props) {
    return (
        <Backdrop
            open
            invisible
            sx={scoped ? {position: 'absolute', zIndex: (theme) => theme.zIndex.drawer + 1} : undefined}
        >
            <Box display='flex' justifyContent='center' alignItems='center' height={scoped ? '100%' : '100vh'}>
                <CircularProgress size={100} color='secondary'/>
                <Typography variant='h4' sx={{justifyContent: 'center', position: scoped ? 'absolute' : 'fixed', top: '60%'}}>
                    {message}
                </Typography>
            </Box>
        </Backdrop>
    )
}