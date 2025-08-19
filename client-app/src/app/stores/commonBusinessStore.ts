import {makeAutoObservable, runInAction} from "mobx";
import agent from "../api/agent";
import {toast} from "react-toastify";
import {ProductType} from "../models/product/productType";
import {Uom} from "../models/common/uom";
import {ProductPriceType} from "../models/product/productPriceType";

export default class CommonBusinessStore {

    loadingInitial = false;
    productTypesRegistry = new Map<string, ProductType>();
    productPriceTypesRegistry = new Map<string, ProductPriceType>();
    currenciesRegistry = new Map<string, Uom>();

    constructor() {
        makeAutoObservable(this)
    }

    get productTypes() {
        return Array.from(this.productTypesRegistry.values());
    }

    get productPriceTypes() {
        return Array.from(this.productPriceTypesRegistry.values());
    }

    get currencies() {
        return Array.from(this.currenciesRegistry.values());
    }

    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    }

    loadProductTypes = async () => {
        if (this.productTypesRegistry.size > 0) {
            return
        }
        this.setLoadingInitial(true);
        try {
            const results = await agent.ProductTypes.list();
            results.forEach((productType: ProductType) => {
                runInAction(() => {
                    this.productTypesRegistry.set(productType.productTypeId, productType);
                })
            })
            this.setLoadingInitial(false);
        } catch (error) {
            this.setLoadingInitial(false);
            console.log(error);
            toast.error('error');

        }
    }

    loadProductPriceTypes = async () => {
        if (this.productPriceTypesRegistry.size > 0) {
            return
        }
        this.setLoadingInitial(true);
        try {
            const results = await agent.ProductPriceTypes.list();
            results.forEach((productPriceType: ProductPriceType) => {
                runInAction(() => {
                    this.productPriceTypesRegistry.set(productPriceType.productPriceTypeId, productPriceType);
                })
            })
            this.setLoadingInitial(false);
        } catch (error) {
            this.setLoadingInitial(false);
            console.log(error);
            toast.error('error');

        }
    }

    loadCurrencies = async () => {
        this.setLoadingInitial(true);
        try {
            const results = await agent.Uoms.listCurrency();
            results.forEach((uom: Uom) => {
                runInAction(() => {
                    this.currenciesRegistry.set(uom.uomId, uom);
                })
            })
            this.setLoadingInitial(false);
        } catch (error) {
            this.setLoadingInitial(false);
            console.log(error);
            toast.error('error');

        }
    }


}