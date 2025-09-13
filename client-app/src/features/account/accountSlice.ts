import { createAsyncThunk, createSlice, isAnyOf } from "@reduxjs/toolkit";
import { toast } from "react-toastify";
import agent from "../../app/api/agent";
import { User } from "../../app/models/party/user";
import { FieldValues } from "react-hook-form";
import { router } from "../../app/router/Routes";

interface AccountState {
  user: User | null;
}

const initialState: AccountState = {
  user: null,
};

export const signInUser = createAsyncThunk<User, FieldValues>(
  "account/signInUser",
  async (data: any, thunkAPI) => {
    try {
      const user = await agent.Account.login(data);
      // REFACTOR: Log user and roles after login for debugging.
      console.log('Logged in user:', { ...user, roles: user.roles || [] });
      localStorage.setItem('user', JSON.stringify(user));
      return user;
    } catch (error: any) {
      return thunkAPI.rejectWithValue({ error: error.data });
    }
  }
);

export const fetchCurrentUser = createAsyncThunk<User>(
  "account/fetchCurrentUser",
  async (_, thunkAPI) => {
    thunkAPI.dispatch(setUser(JSON.parse(localStorage.getItem("user")!)));
    try {
      const user = await agent.Account.current();
      // REFACTOR: Log user and roles after fetching current user.
      console.log('Current user:', { ...user, roles: user.roles || [] });
      localStorage.setItem('user', JSON.stringify(user));
      return user;
    } catch (error: any) {
      return thunkAPI.rejectWithValue({ error: error.data });
    }
  },
  {
    condition: () => {
      if (!localStorage.getItem("user")) return false;
    },
  }
);

export const accountSlice = createSlice({
  name: "account",
  initialState,
  reducers: {
    signOut: (state) => {
      state.user = null;
      localStorage.removeItem("user");
      router.navigate("/");
    },
    setUser: (state, action) => {
      // REFACTOR: Use UserDto roles directly, with fallback to empty array.
      state.user = { ...action.payload, roles: action.payload.roles || [] };
      console.log('Set user:', { ...state.user, roles: state.user.roles });
    },
  },
  extraReducers: (builder) => {
    builder.addCase(fetchCurrentUser.rejected, (state) => {
      state.user = null;
      localStorage.removeItem("user");
      toast.error("Session expired - please login again");
      router.navigate("/");
    });
    builder.addMatcher(
        isAnyOf(signInUser.fulfilled, fetchCurrentUser.fulfilled),
        (state, action) => {
          // REFACTOR: Use UserDto roles, log for debugging.
          state.user = { ...action.payload, roles: action.payload.roles || [] };
          console.log('Updated user state:', { ...state.user, roles: state.user.roles });
        }
    );
    builder.addMatcher(isAnyOf(signInUser.rejected), (state, action) => {
      throw action.payload;
    });
  },
});

export const { signOut, setUser } = accountSlice.actions;
