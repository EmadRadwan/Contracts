import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";

export default defineConfig(() => {
    return {
        build: {
            outDir: "../API/wwwroot",
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
        plugins: [react()],
    };
});
