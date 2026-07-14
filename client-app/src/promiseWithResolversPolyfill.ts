// pdfjs-dist (used by @react-pdf-viewer for printing/viewing payment PDFs) relies on
// Promise.withResolvers, which only shipped natively in Chrome 119 / Firefox 121 / Safari 17.4.
// Older or unpatched browsers throw "Promise.withResolvers is not a function" when opening
// the PDF viewer. Polyfill it globally before anything else loads.
type PromiseWithResolvers = <T>() => {
    promise: Promise<T>;
    resolve: (value: T | PromiseLike<T>) => void;
    reject: (reason?: unknown) => void;
};

if (typeof (Promise as unknown as { withResolvers?: PromiseWithResolvers }).withResolvers !== "function") {
    (Promise as unknown as { withResolvers: PromiseWithResolvers }).withResolvers = function <T>() {
        let resolve!: (value: T | PromiseLike<T>) => void;
        let reject!: (reason?: unknown) => void;
        const promise = new Promise<T>((res, rej) => {
            resolve = res;
            reject = rej;
        });
        return { promise, resolve, reject };
    };
}
