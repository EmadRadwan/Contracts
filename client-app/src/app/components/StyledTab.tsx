
import { styled, Tab } from "@mui/material";

export const StyledTab = styled((props) => <Tab disableRipple {...props} />)(
    ({ theme }) => ({
      textTransform: 'none',
      fontWeight: theme.typography.fontWeightBold,
      fontSize: theme.typography.pxToRem(15),
      marginRight: theme.spacing(1),
      border: "1.5px solid black",
      borderRadius: "10px",
      padding: "1em 2em",
      color: '#050505',
      '&.Mui-selected': {
        color: '#005CB2',
      },
      '&.Mui-focusVisible': {
        backgroundColor: 'rgba(100, 95, 228, 0.32)',
      },
    }),
  );