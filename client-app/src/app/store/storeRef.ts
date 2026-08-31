/**
 * Breaks the circular import between configureStore and agent.ts.
 *
 * The cycle was: configureStore -> accountSlice -> agent -> configureStore.
 * Because agent.ts sits inside configureStore's own import graph, its
 * `import { store }` binding was never populated, so the axios request
 * interceptor read `store` as undefined and threw - which made axios reject
 * every request before sending it. No network call, no console error.
 *
 * This module imports nothing, so it cannot participate in a cycle. The store
 * registers itself here once created, and consumers pull it lazily at call
 * time rather than capturing it at module-evaluation time.
 */

// Deliberately untyped to avoid importing the store's types, which would
// reintroduce the cycle.
type AnyStore = { getState: () => any };

let registeredStore: AnyStore | undefined;

export function setStore(store: AnyStore): void {
    registeredStore = store;
}

/** The store, or undefined if called before configureStore has run. */
export function getStore(): AnyStore | undefined {
    return registeredStore;
}
