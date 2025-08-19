import type {Middleware, MiddlewareAPI} from "@reduxjs/toolkit";
import {isRejectedWithValue} from "@reduxjs/toolkit";

/**
 * Log a warning and show a toast!
 */
export const rtkQueryErrorLogger: Middleware =
    (api: MiddlewareAPI) => (next) => (action) => {
        // RTK Query uses `createAsyncThunk` from redux-toolkit under the hood, so we're able to utilize these matchers!
        if (isRejectedWithValue(action)) {
            console.warn("We got a rejected action!");
            console.log("The rejected action was:", action);
        }

        return next(action);
    };
