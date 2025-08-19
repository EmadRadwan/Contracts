import {useCallback, useEffect, useState} from "react";
import {Outlet, useLocation} from "react-router-dom";
import HomePage from "../../features/home/HomePage";
import {ToastContainer} from "react-toastify";
import LoadingComponent from "./LoadingComponent";
import {IntlProvider, LocalizationProvider} from "@progress/kendo-react-intl";
import {useAppDispatch, useAppSelector} from "../store/configureStore";
import {fetchCurrentUser} from "../../features/account/accountSlice";
import {Container, CssBaseline, ThemeProvider,} from "@mui/material";
import { createTheme } from '@mui/material/styles'; 
import Header from "./Header";
import { Slide, Zoom, Flip, Bounce } from 'react-toastify';
import Rubik from "../fonts/Rubik/Rubik-Regular.ttf";
import RubikBold from "../fonts/Rubik/Rubik-Bold.ttf";

function App() {
    const location = useLocation();
    const language = useAppSelector((state) => state.localization.language);

    const dispatch = useAppDispatch();
    const [loading, setLoading] = useState(true);


    //console.log("language", language);

    const initApp = useCallback(async () => {
        try {
            await dispatch(fetchCurrentUser());
        } catch (error) {
            console.log(error);
        }
    }, [dispatch]);

    useEffect(() => {
        initApp().then(() => setLoading(false));
    }, [initApp]);

    if (loading) return <LoadingComponent message={language === "en" ? "Initialising app...": language === "ar" ? "... جاري تحميل التطبيق" : "Uygulama başlatılıyor..."}/>;

    const theme = createTheme({
        palette: {
            primary: {
                main: '#005CB2' // Your desired blue color
            }
        },
        typography: {
            fontFamily: ['Rubik', 'Arial', 'sans-serif'].join(","),
        },
        components: {
            MuiCssBaseline: {
                styleOverrides: `
                    @font-face  {
                        font-family: 'Rubik';
                        font-style: normal;
                        font-display: swap;
                        font-weight: 400;
                        src: local('Rubik'), local('Rubik-Regular'), url(${Rubik}) format('truetype');
                        unicodeRange: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF;
                    },
                    @font-face  {
                        font-family: 'Rubik';
                        font-style: normal;
                        font-display: swap;
                        font-weight: 800;
                        src: local('Rubik'), local('Rubik-Bold'), url(${RubikBold}) format('truetype');
                        unicodeRange: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF;
                    }
                `
            },
          },
    });

    return (
        <ThemeProvider theme={theme}>
            <LocalizationProvider language={language}>
                {/* <IntlProvider locale={language === "ar" ? "ar-EG" : "en-GB"}> */}
                <div dir={language === "ar" ? "rtl" : "ltr"}>
                    <ToastContainer position={language === "ar" ? "bottom-center" : "bottom-right"} transition={Flip}/>
                    <CssBaseline/>
                    <Header/>
                    {location.pathname === "/" ? (
                        <HomePage/>
                    ) : (
                        <>
                            <Container maxWidth={false}>
                                <Outlet/>
                            </Container>
                        </>
                    )}
                </div>
                {/* </IntlProvider> */}
            </LocalizationProvider>
        </ThemeProvider>
    )
}

export default App;
