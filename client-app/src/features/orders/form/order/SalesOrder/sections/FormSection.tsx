import React from 'react';
import {
    Accordion,
    AccordionSummary,
    AccordionDetails,
    Box,
    Typography,
    useTheme,
    useMediaQuery,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

interface FormSectionProps {
    title: string;
    description?: string;
    icon?: React.ReactNode;
    defaultExpanded?: boolean;
    children: React.ReactNode;
    sx?: Record<string, any>;
}

export const FormSection: React.FC<FormSectionProps> = ({
    title,
    description,
    icon,
    defaultExpanded = true,
    children,
    sx = {},
}) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

    return (
        <Accordion
            defaultExpanded={defaultExpanded}
            sx={{
                marginBottom: '24px',
                border: '1px solid #E0E0E0',
                borderRadius: '8px',
                '&:before': {
                    display: 'none',
                },
                '&.Mui-expanded': {
                    margin: '24px 0',
                },
                ...sx,
            }}
        >
            <AccordionSummary
                expandIcon={<ExpandMoreIcon />}
                sx={{
                    backgroundColor: '#F5F5F5',
                    borderBottom: '1px solid #E0E0E0',
                    '&.Mui-expanded': {
                        backgroundColor: '#F8F9FA',
                    },
                    padding: isMobile ? '12px 16px' : '16px 24px',
                }}
            >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: '12px', width: '100%' }}>
                    {icon && (
                        <Box
                            sx={{
                                display: 'flex',
                                alignItems: 'center',
                                color: '#09419A',
                            }}
                        >
                            {icon}
                        </Box>
                    )}
                    <Box sx={{ flex: 1 }}>
                        <Typography
                            variant="h5"
                            sx={{
                                fontSize: isMobile ? '16px' : '20px',
                                fontWeight: 600,
                                color: '#212121',
                                margin: 0,
                            }}
                        >
                            {title}
                        </Typography>
                        {description && (
                            <Typography
                                variant="caption"
                                sx={{
                                    color: '#666666',
                                    display: 'block',
                                    marginTop: '4px',
                                }}
                            >
                                {description}
                            </Typography>
                        )}
                    </Box>
                </Box>
            </AccordionSummary>
            <AccordionDetails
                sx={{
                    padding: isMobile ? '16px 12px' : '24px',
                    backgroundColor: '#FFFFFF',
                }}
            >
                {children}
            </AccordionDetails>
        </Accordion>
    );
};

export default FormSection;
