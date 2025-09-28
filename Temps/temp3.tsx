import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react-swc';
// REFACTOR: Imported Node.js polyfills plugin to handle Buffer and other Node.js globals
// Improvement: Simplifies polyfilling by using vite-plugin-node-polyfills, ensuring Buffer is available in browser
// Context: The previous manual Buffer polyfill was not executing in the browser environment
import { nodePolyfills } from 'vite-plugin-node-polyfills';

export default defineConfig(() => {
    return {
        build: {
            outDir: '../API/wwwroot',
        },
        server: {
            port: 3000,
        },
        optimizeDeps: {
            // This prevents Vite from pre-bundling Mermaid, letting it run only in the browser after the DOM is ready
            exclude: ['mermaid', 'dagre-d3-es', 'd3'],
        },
        ssr: {
            // This ensures that if you do any sort of SSR or SSR-like features,
            // Vite won't attempt to bundle these packages for server usage
            noExternal: ['mermaid', 'dagre-d3-es', 'd3'],
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
        // REFACTOR: Added resolve.alias to ensure buffer module is correctly resolved
        // Improvement: Maps Node.js 'buffer' to the browser-compatible 'buffer' package
        // Context: Prevents Vite from attempting to load Node.js built-in buffer module
        resolve: {
            alias: {
                buffer: 'buffer',
            },
        },
    };
});