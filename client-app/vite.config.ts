import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import { nodePolyfills } from 'vite-plugin-node-polyfills';


export default defineConfig(() => {
    return {
        build: {
            outDir: "../API/wwwroot",
            // Vite will not empty an outDir outside the project root unless told to.
            // Everything here is regenerated from client-app/public + the build,
            // so clearing it each build avoids stale hashed bundles accumulating.
            emptyOutDir: true,
        },
        server: {
            port: 3000,
        },
        optimizeDeps: {
            // This prevents Vite from pre-bundling Mermaid, 
            // letting it run only in the browser after the DOM is ready
            exclude: ["mermaid", "dagre-d3-es", "d3"],
        },
        ssr: {
            // This ensures that if you do any sort of SSR or SSR-like features,
            // Vite won't attempt to bundle these packages for server usage.
            noExternal: ["mermaid", "dagre-d3-es", "d3"],
        },
        // REFACTOR: Added assetsInclude to handle .ttf files
        // Purpose: Allows Vite to process .ttf files as assets, ensuring correct import or public serving
        assetsInclude: ['**/*.ttf'],
        plugins: [
            react(),
            // REFACTOR: Added nodePolyfills plugin to provide Buffer and other Node.js globals
            // Improvement: Automatically handles Buffer polyfilling without manual window.Buffer assignment
            // Context: Ensures @react-pdf/renderer can access Buffer in the browser
            nodePolyfills({
                globals: {
                    Buffer: true, // Explicitly enable Buffer polyfill
                },
            }),
        ],
        resolve: {
            alias: {
                buffer: 'buffer',
            },
        },
    };
});
