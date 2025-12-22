import Avatar from '@mui/material/Avatar';
import TextField from '@mui/material/TextField';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Container from '@mui/material/Container';
import { Paper } from '@mui/material';
import LockIcon from '@mui/icons-material/Lock';
import LoadingButton from '@mui/lab/LoadingButton';
import { useTranslationHelper } from "../../app/hooks/useTranslationHelper";
import { FieldValues, useForm } from "react-hook-form";
import { useAppDispatch } from '../../app/store/configureStore';
import { changePassword } from './accountSlice';
import { useState } from "react";
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import InputAdornment from '@mui/material/InputAdornment';
import IconButton from '@mui/material/IconButton';

export default function ChangePasswordPage() {
    const { getTranslatedLabel } = useTranslationHelper();
    const [showCurrent, setShowCurrent] = useState(false);
    const [showNew, setShowNew] = useState(false);
    const [showConfirm, setShowConfirm] = useState(false);
    const dispatch = useAppDispatch();

    const { register, handleSubmit, formState: { isSubmitting, errors, isValid }, watch } = useForm({
        mode: 'all'
    });

    async function submitForm(data: FieldValues) {
        await dispatch(changePassword({
            currentPassword: data.currentPassword,
            newPassword: data.newPassword,
            confirmPassword: data.confirmPassword
        }));
    }

    const newPassword = watch("newPassword");

    // REFACTOR: Replaced absolute-positioned divs with MUI InputAdornment + IconButton.
    // This is the recommended Material-UI pattern for password visibility toggles.
    // Benefits: automatic alignment inside the input field, responsive, accessible,
    // no hardcoded margins or negative positioning needed.
    const passwordEndAdornment = (show: boolean, toggle: () => void) => (
        <InputAdornment position="end">
            <IconButton
                aria-label="toggle password visibility"
                onClick={toggle}
                edge="end"
                tabIndex={-1} // prevent focus stealing from input
            >
                {show ? <VisibilityOff /> : <Visibility />}
            </IconButton>
        </InputAdornment>
    );

    return (
        <Container component={Paper} maxWidth="sm"
                   sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', p: 4 }}>
            <Avatar sx={{ m: 1, bgcolor: 'secondary.main' }}>
                <LockIcon />
            </Avatar>
            <Typography component="h1" variant="h5">
                {getTranslatedLabel("general.changePassword", "Change Password")}
            </Typography>
            <Box component="form" onSubmit={handleSubmit(submitForm)} noValidate sx={{ mt: 1, width: "100%" }}>
                <TextField
                    margin="normal"
                    required
                    fullWidth
                    dir={"ltr"}
                    label={getTranslatedLabel('general.currentPassword', 'Current Password')}
                    type={showCurrent ? "text" : "password"}
                    {...register('currentPassword', { required: 'Current password is required' })}
                    error={!!errors.currentPassword}
                    helperText={errors?.currentPassword?.message as string}
                    InputProps={{
                        endAdornment: passwordEndAdornment(showCurrent, () => setShowCurrent(!showCurrent))
                    }}
                />

                <TextField
                    margin="normal"
                    required
                    fullWidth
                    dir={"ltr"}
                    label={getTranslatedLabel("general.newPassword", "New Password")}
                    type={showNew ? "text" : "password"}
                    {...register('newPassword', {
                        required: 'New password is required',
                        minLength: { value: 6, message: 'Password must be at least 6 characters' }
                    })}
                    error={!!errors.newPassword}
                    helperText={errors?.newPassword?.message as string}
                    InputProps={{
                        endAdornment: passwordEndAdornment(showNew, () => setShowNew(!showNew))
                    }}
                />

                <TextField
                    margin="normal"
                    required
                    fullWidth
                    dir={"ltr"}
                    label={getTranslatedLabel("general.confirmPassword", "Confirm New Password")}
                    type={showConfirm ? "text" : "password"}
                    {...register('confirmPassword', {
                        required: 'Confirm password is required',
                        validate: value => value === newPassword || 'Passwords do not match'
                    })}
                    error={!!errors.confirmPassword}
                    helperText={errors?.confirmPassword?.message as string}
                    InputProps={{
                        endAdornment: passwordEndAdornment(showConfirm, () => setShowConfirm(!showConfirm))
                    }}
                />

                <LoadingButton
                    disabled={!isValid}
                    loading={isSubmitting}
                    type="submit"
                    fullWidth
                    variant="contained"
                    sx={{ mt: 3, mb: 2 }}
                >
                    {getTranslatedLabel("general.changePassword", "Change Password")}
                </LoadingButton>
            </Box>
        </Container>
    );
}