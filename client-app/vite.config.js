import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
export default defineConfig(function () {
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
        plugins: [react()],
    };
});
