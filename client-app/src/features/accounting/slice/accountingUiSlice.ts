import {createEntityAdapter, createSelector, createSlice, EntityState, PayloadAction,} from "@reduxjs/toolkit";
import {
    AccountClass,
    AccountType,
    GlAccount,
    ParentAccount,
    ResourceType
} from "../../../app/models/accounting/globalGlSettings";
import {RootState} from "../../../app/store/configureStore";

interface AccountingUiState {
    accountClasses: AccountClass[] | undefined
    accountTypes: AccountType[] | undefined
    resourceTypes: ResourceType[] | undefined
    parentAccounts: ParentAccount[] | undefined
    accounts: EntityState<GlAccount>
}

const accountsAdapter = createEntityAdapter<GlAccount>({
    selectId: (account: GlAccount) => account.glAccountId
})

const initialState: AccountingUiState = {
    accountClasses: undefined,
    accountTypes: undefined,
    resourceTypes: undefined,
    parentAccounts: undefined,
    accounts: accountsAdapter.getInitialState()
}

export const accountingUiSlice = createSlice({
    name: "accountingUi",
    initialState,
    reducers: {
        setUiGlAccounts: (state, action: PayloadAction<any>) => {

            if (action.payload.length === 0) {
                state.accounts = accountsAdapter.getInitialState()
            } else {
                state.accounts = accountsAdapter.upsertMany(
                    state.accounts, action.payload
                )
            }

            const classesSet = [...new Set(action.payload.map((res: any) => res.glAccountClassId))]
            const typesSet = [...new Set(action.payload.map((res: any) => res.glAccountTypeId))].filter(t => t)
            const resourcesSet = [...new Set(action.payload.map((res: any) => res.glResourceTypeId))]
            const parentAccountsSet = [...new Set(action.payload.map((res: any) => res.parentGlAccountId))].filter(p => p)
            state.accountClasses = classesSet.map((c: any) => {
                return {
                    glAccountClassId: c,
                    glAccountClassDescription: c.split("_").map((g: string) => g[0].concat(g.substring(1).toLowerCase())).join(" ")
                }
            })

            state.accountTypes = typesSet.map((t: any) => {
                return {
                    glAccountTypeId: t,
                    glAccountTypeDescription: t.split("_").map((g: string) => g[0].concat(g.substring(1).toLowerCase())).join(" ")
                }
            })

            state.resourceTypes = resourcesSet.map((r: any) => {
                return {
                    glResourceTypeId: r,
                    glResourceTypeDescription: r.split("_").map((g: string) => g[0].concat(g.substring(1).toLowerCase())).join(" ")
                }
            })

            state.parentAccounts = parentAccountsSet.map((p: any) => {
                return {
                    parentGlAccountId: p,
                    parentAccountName: action.payload.find((a: any) => a.accountCode === p).accountName
                }
            })
        }
    }
})

export const {
    setUiGlAccounts
} = accountingUiSlice.actions

const accountsUiSelectors = accountsAdapter.getSelectors(
    (state: RootState) => state.accountingUi.accounts
)
const accountingUiSelector = (state: RootState) => state.accountingUi
export const accountClasses = createSelector(
    (accountingUiSelector),
    (accountingUi) => accountingUi.accountClasses
)

export const accountTypes = createSelector(
    (accountingUiSelector),
    (accountingUi) => accountingUi.accountTypes
)

export const resourceTypes = createSelector(
    (accountingUiSelector),
    (accountingUi) => accountingUi.resourceTypes
)

export const parentAccounts = createSelector(
    (accountingUiSelector),
    (accountingUi) => accountingUi.parentAccounts
)

export const defaultAccountsSelector = createSelector(
    accountingUiSelector,
    (accountingUi) => Object.values(accountingUi.accounts).map(a => {
        return {
            glAccountId: a.glAccountId,
            accountName: a.glAccountName
        }
    })
)

export const {selectAll: accountUiEntities} = accountsUiSelectors