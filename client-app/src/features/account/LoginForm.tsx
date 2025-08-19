import Avatar from '@mui/material/Avatar';
import TextField from '@mui/material/TextField';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Container from '@mui/material/Container';
import {Button, Paper} from '@mui/material';
import {useLocation, useNavigate} from 'react-router-dom';
import {FieldValues, useForm} from 'react-hook-form';
import {useAppDispatch} from '../../app/store/configureStore';
import {signInUser} from './accountSlice';
import LockIcon from '@mui/icons-material/Lock';
import LoadingButton from '@mui/lab/LoadingButton';
import {useTranslationHelper} from "../../app/hooks/useTranslationHelper";
import {useState} from "react";
import VisibilityOffRoundedIcon from '@mui/icons-material/VisibilityOffRounded';
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded';

export default function LoginForm() {
    const {getTranslatedLabel} = useTranslationHelper();
    const [showPassword, setShowPassword] = useState(false);
    const location = useLocation();
    const dispatch = useAppDispatch();
    const {register, handleSubmit, formState: {isSubmitting, errors, isValid}} = useForm({
        mode: 'all'
    });
    const navigate = useNavigate();

    async function submitForm(data: FieldValues) {
        try {
            await dispatch(signInUser(data));
            const from = location.state?.from?.pathname || "/";
            navigate(from, {replace: true});
            //router.navigate(location.state?.from?.pathname || '/products/');
        } catch (error) {
            console.log(error);
        }
    }

    return (
        <Container component={Paper} maxWidth="sm"
                   sx={{display: 'flex', flexDirection: 'column', alignItems: 'center', p: 4}}>
            <Avatar sx={{m: 1, bgcolor: 'secondary.main'}}>
                <LockIcon/>
            </Avatar>
            <Typography component="h1" variant="h5">
                {getTranslatedLabel("general.signin", "Sign In")}
            </Typography>
            <Box component="form" onSubmit={handleSubmit(submitForm)} noValidate sx={{mt: 1, display: "flex", m: 1, p: 1, position: "relative", width: "100%", flexDirection: 'column', justifyContent: "center"}}>
                {/*<Box sx={{}}>*/}

                    <TextField
                        margin="normal"
                        fullWidth
                        dir={"ltr"}
                        label={getTranslatedLabel('general.email', 'email')}
                        autoFocus
                        {...register('email', {required: 'Email is required'})}
                        error={!!errors.email}
                        helperText={errors?.email?.message}
                    />
                    <TextField
                        margin="normal"
                        fullWidth
                        dir={"ltr"}
                        label={getTranslatedLabel("general.password","Password")}
                        type={showPassword ? "text" : "password"}
                        {...register('password', {required: 'Password is required'})}
                        error={!!errors.password}
                        helperText={errors?.password?.message}
                    />
                    <div style={{position: "absolute", right: 2, bottom: "40%"}}>
                        <Button
                            disableRipple
                            disableFocusRipple
                            disableTouchRipple
                            type={"button"}
                            onClick={() => setShowPassword(!showPassword)}
                        >
                            {showPassword ? <VisibilityOffRoundedIcon/> : <VisibilityRoundedIcon/>}
                        </Button>
                    </div>
                {/*</Box>*/}
                <LoadingButton
                    disabled={!isValid}
                    loading={isSubmitting}
                    type="submit"
                    fullWidth
                    variant="contained"
                    sx={{mt: 3, mb: 2}}
                >
                    {getTranslatedLabel("general.signin", "Sign In")}
                </LoadingButton>

            </Box>
        </Container>
    );
}