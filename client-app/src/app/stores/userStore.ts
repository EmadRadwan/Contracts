import {makeAutoObservable, runInAction} from "mobx";
import agent from "../api/agent";
import {User} from "../models/party/user";

export default class UserStore {
    user: User | null = null;

    constructor() {
        makeAutoObservable(this)
    }

    get isLoggedIn() {
        return !!this.user;
    }

    /*login = async (creds: UserFormValues) => {
      try {
        const user = await agent.Account.login(creds);
        store.commonAppStore.setToken(user.token);
        runInAction(() => this.user = user);
        history.push('/products');
        store.modalStore.closeModal();
      } catch (error) {
        throw error;
      }
    }
  
    logout = () => {
      store.commonAppStore.setToken(null);
      window.localStorage.removeItem('jwt');
      this.user = null;
      history.push('/');
    }*/

    getUser = async () => {
        try {
            const user = await agent.Account.current();
            runInAction(() => this.user = user);
        } catch (error) {
            console.log(error);
        }
    }

    /*register = async (creds: UserFormValues) => {
      try {
        const user = await agent.Account.register(creds);
        store.commonAppStore.setToken(user.token);
        runInAction(() => this.user = user);
        history.push('/parties');
        store.modalStore.closeModal();
      } catch (error) {
        throw error;
      }
    }*/

    setImage = (image: string) => {
        if (this.user) this.user.image = image;
    }

    setDisplayName = (name: string) => {
        if (this.user) this.user.displayName = name;
    }
}