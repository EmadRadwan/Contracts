/**
 * Centralized Design System
 * Ensures consistency across all Sales Request components
 * Light mode only (for now)
 */

export const designSystem = {
  colors: {
    // Primary
    primary: {
      main: '#09419A',
      light: '#E3F2FD',
      dark: '#004A99',
    },

    // Secondary / Accent
    secondary: {
      main: '#2E7D32',
      light: '#E8F5E9',
      dark: '#1E7E34',
    },

    // Status colors
    status: {
      pending: '#FF9800',
      approved: '#4CAF50',
      rejected: '#F44336',
      cancelled: '#D32F2F',
      info: '#2196F3',
    },

    // Neutral grays
    gray: {
      50: '#FAFAFA',
      100: '#F5F5F5',
      200: '#EEEEEE',
      300: '#E0E0E0',
      400: '#BDBDBD',
      500: '#9E9E9E',
      600: '#757575',
      700: '#616161',
      800: '#424242',
      900: '#212121',
    },

    // Semantic
    background: {
      light: '#FFFFFF',
      page: '#F8F9FA',
      hover: '#F5F5F5',
    },

    text: {
      primary: '#212121',
      secondary: '#666666',
      disabled: '#BDBDBD',
      inverse: '#FFFFFF',
    },

    border: {
      light: '#E0E0E0',
      medium: '#BDBDBD',
      dark: '#757575',
    },

    // Feedback
    success: '#4CAF50',
    warning: '#FF9800',
    error: '#F44336',
    info: '#2196F3',
  },

  typography: {
    fontFamily: {
      base: '"Roboto", "-apple-system", "BlinkMacSystemFont", "Segoe UI", sans-serif',
      mono: '"Roboto Mono", monospace',
    },

    // Font sizes (in px, for consistency with design mockups)
    fontSize: {
      xs: '12px',
      sm: '13px',
      base: '14px',
      lg: '16px',
      xl: '18px',
      '2xl': '20px',
      '3xl': '24px',
      '4xl': '28px',
    },

    // Font weights
    fontWeight: {
      light: 300,
      normal: 400,
      medium: 500,
      semibold: 600,
      bold: 700,
    },

    // Line heights
    lineHeight: {
      tight: 1.2,
      normal: 1.5,
      relaxed: 1.75,
    },

    // Predefined text styles
    styles: {
      h1: {
        fontSize: '28px',
        fontWeight: 700,
        lineHeight: 1.2,
        letterSpacing: '-0.5px',
      },
      h2: {
        fontSize: '24px',
        fontWeight: 700,
        lineHeight: 1.2,
        letterSpacing: '-0.25px',
      },
      h3: {
        fontSize: '20px',
        fontWeight: 600,
        lineHeight: 1.3,
      },
      h4: {
        fontSize: '18px',
        fontWeight: 600,
        lineHeight: 1.4,
      },
      h5: {
        fontSize: '16px',
        fontWeight: 600,
        lineHeight: 1.4,
      },
      label: {
        fontSize: '13px',
        fontWeight: 600,
        lineHeight: 1.2,
        letterSpacing: '0.5px',
        textTransform: 'uppercase',
      },
      body: {
        fontSize: '14px',
        fontWeight: 400,
        lineHeight: 1.5,
      },
      caption: {
        fontSize: '12px',
        fontWeight: 400,
        lineHeight: 1.4,
      },
    },
  },

  spacing: {
    xs: '4px',
    sm: '8px',
    md: '12px',
    lg: '16px',
    xl: '24px',
    '2xl': '32px',
    '3xl': '48px',
  },

  // Border radius
  borderRadius: {
    xs: '2px',
    sm: '4px',
    md: '8px',
    lg: '12px',
    xl: '16px',
    full: '9999px',
  },

  // Shadows
  shadow: {
    xs: '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
    sm: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
    md: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
    lg: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
    xl: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
  },

  // Transitions
  transition: {
    fast: '150ms cubic-bezier(0.4, 0, 0.2, 1)',
    base: '250ms cubic-bezier(0.4, 0, 0.2, 1)',
    slow: '350ms cubic-bezier(0.4, 0, 0.2, 1)',
  },

  // Component-specific styles
  components: {
    button: {
      primary: {
        backgroundColor: '#0066CC',
        color: '#FFFFFF',
        border: 'none',
        padding: '8px 16px',
        borderRadius: '4px',
        fontSize: '14px',
        fontWeight: 600,
        cursor: 'pointer',
        transition: 'all 250ms cubic-bezier(0.4, 0, 0.2, 1)',
        '&:hover': {
          backgroundColor: '#004A99',
          boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
        },
        '&:active': {
          backgroundColor: '#003366',
        },
        '&:disabled': {
          backgroundColor: '#BDBDBD',
          cursor: 'not-allowed',
          opacity: 0.6,
        },
      },
      secondary: {
        backgroundColor: 'transparent',
        color: '#0066CC',
        border: '1px solid #0066CC',
        padding: '8px 16px',
        borderRadius: '4px',
        fontSize: '14px',
        fontWeight: 600,
        cursor: 'pointer',
        transition: 'all 250ms cubic-bezier(0.4, 0, 0.2, 1)',
        '&:hover': {
          backgroundColor: '#E3F2FD',
        },
        '&:disabled': {
          borderColor: '#BDBDBD',
          color: '#BDBDBD',
          cursor: 'not-allowed',
        },
      },
      success: {
        backgroundColor: '#28A745',
        color: '#FFFFFF',
        border: 'none',
        padding: '8px 16px',
        borderRadius: '4px',
        fontSize: '14px',
        fontWeight: 600,
        cursor: 'pointer',
        transition: 'all 250ms cubic-bezier(0.4, 0, 0.2, 1)',
        '&:hover': {
          backgroundColor: '#1E7E34',
          boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
        },
        '&:disabled': {
          backgroundColor: '#BDBDBD',
          cursor: 'not-allowed',
          opacity: 0.6,
        },
      },
    },

    input: {
      default: {
        backgroundColor: '#FFFFFF',
        color: '#212121',
        border: '1px solid #E0E0E0',
        borderRadius: '4px',
        padding: '8px 12px',
        fontSize: '14px',
        fontFamily: '"Roboto", "-apple-system", "BlinkMacSystemFont", "Segoe UI", sans-serif',
        transition: 'all 250ms cubic-bezier(0.4, 0, 0.2, 1)',
        '&:focus': {
          borderColor: '#0066CC',
          boxShadow: '0 0 0 3px rgba(0, 102, 204, 0.1)',
          outline: 'none',
        },
        '&:disabled': {
          backgroundColor: '#F5F5F5',
          color: '#BDBDBD',
          cursor: 'not-allowed',
        },
        '&::placeholder': {
          color: '#999999',
        },
      },
    },

    badge: {
      pending: {
        backgroundColor: '#FFF3E0',
        color: '#FF9800',
        borderColor: '#FFB74D',
        padding: '4px 8px',
        borderRadius: '4px',
        fontSize: '12px',
        fontWeight: 600,
        border: '1px solid #FFB74D',
      },
      approved: {
        backgroundColor: '#E8F5E9',
        color: '#4CAF50',
        borderColor: '#81C784',
        padding: '4px 8px',
        borderRadius: '4px',
        fontSize: '12px',
        fontWeight: 600,
        border: '1px solid #81C784',
      },
      rejected: {
        backgroundColor: '#FFEBEE',
        color: '#F44336',
        borderColor: '#EF5350',
        padding: '4px 8px',
        borderRadius: '4px',
        fontSize: '12px',
        fontWeight: 600,
        border: '1px solid #EF5350',
      },
      cancelled: {
        backgroundColor: '#FFEBEE',
        color: '#D32F2F',
        borderColor: '#E53935',
        padding: '4px 8px',
        borderRadius: '4px',
        fontSize: '12px',
        fontWeight: 600,
        border: '1px solid #E53935',
      },
    },

    card: {
      default: {
        backgroundColor: '#FFFFFF',
        borderRadius: '8px',
        border: '1px solid #E0E0E0',
        padding: '16px',
        boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
      },
      elevated: {
        backgroundColor: '#FFFFFF',
        borderRadius: '8px',
        border: 'none',
        padding: '16px',
        boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
      },
    },

    modal: {
      backdrop: {
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
      },
      container: {
        backgroundColor: '#FFFFFF',
        borderRadius: '8px',
        padding: '24px',
        boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1)',
        maxWidth: '600px',
      },
      header: {
        fontSize: '20px',
        fontWeight: 700,
        color: '#212121',
        marginBottom: '16px',
        paddingBottom: '16px',
        borderBottom: '1px solid #E0E0E0',
      },
      footer: {
        display: 'flex',
        gap: '12px',
        justifyContent: 'flex-end',
        marginTop: '24px',
        paddingTop: '16px',
        borderTop: '1px solid #E0E0E0',
      },
    },

    table: {
      header: {
        backgroundColor: '#F5F5F5',
        borderColor: '#E0E0E0',
        color: '#212121',
        fontSize: '12px',
        fontWeight: 600,
        textTransform: 'uppercase',
        padding: '12px 16px',
        letterSpacing: '0.5px',
      },
      row: {
        borderColor: '#E0E0E0',
        padding: '12px 16px',
        fontSize: '14px',
        color: '#212121',
        '&:hover': {
          backgroundColor: '#F8F9FA',
        },
      },
      cell: {
        padding: '12px 16px',
        fontSize: '14px',
        color: '#212121',
      },
    },
  },
};

export type DesignSystem = typeof designSystem;
