import {createContext, useContext} from "react";
import UserStore from "./userStore";
import CommonAppStore from "./commonAppStore";
import ModalStore from "./modalStore";
import GeoStore from "./geoStore";
import CommonBusinessStore from "./commonBusinessStore";


interface Store {
    userStore: UserStore;
    commonAppStore: CommonAppStore;
    commonBusinessStore: CommonBusinessStore;
    modalStore: ModalStore;
    geoStore: GeoStore;
}

export const store: Store = {
    userStore: new UserStore(),
    commonAppStore: new CommonAppStore(),
    commonBusinessStore: new CommonBusinessStore(),
    modalStore: new ModalStore(),
    geoStore: new GeoStore(),
}

export const StoreContext = createContext(store);

export function useStore() {
    return useContext(StoreContext);
}