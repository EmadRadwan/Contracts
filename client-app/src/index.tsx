import {createRoot} from 'react-dom/client';
import "semantic-ui-css/semantic.min.css";
import "react-toastify/dist/ReactToastify.min.css";
import "./app/layout/styles.css";
import "./app/layout/Modal.css";
import {store, StoreContext} from "./app/stores/store";
import {Provider} from "react-redux";
import {store as storeRedux} from "./app/store/configureStore";
import "slick-carousel/slick/slick.css";
import "slick-carousel/slick/slick-theme.css";
import "@progress/kendo-theme-bootstrap/dist/all.css";
import {RouterProvider} from "react-router-dom";
import {router} from "./app/router/Routes";
import React from 'react';


const root = document.getElementById("root");

if (root) {
    const reactRoot = createRoot(root);

    // Import why-did-you-render only in development
    // if (process.env.NODE_ENV === 'development') {
    //     const { default: whyDidYouRender } = await import('@welldone-software/why-did-you-render');
    //     whyDidYouRender(React, {
    //         trackAllPureComponents: true, // Tracks all pure components (e.g., those wrapped with React.memo)
    //         trackHooks: true, // Tracks hook changes (React 16.8+)
    //         logOnDifferentValues: true, // Logs only when props/state/hooks differ
    //         logOwnerReasons: true, // Logs the component that caused the re-render
    //     });
    // }
    
    reactRoot.render(
        <StoreContext.Provider value={store}>
            <Provider store={storeRedux}>
                <RouterProvider router={router}/>
            </Provider>
        </StoreContext.Provider>
    );
}
