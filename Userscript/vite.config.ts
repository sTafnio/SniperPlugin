import { defineConfig } from "vite";
import monkey from "vite-plugin-monkey";

export default defineConfig({
  plugins: [
    monkey({
      entry: "src/main.ts",
      userscript: {
        name: "PoE Sniper Auto-Teleport",
        namespace: "https://github.com/sTafnio/SniperPlugin",
        version: "1.3",
        description:
          "Intercepts search results (XHR) and sends a teleport request if SniperPlugin allows it.",
        author: "sTafnio",
        match: ["https://www.pathofexile.com/trade/search/*"],
        grant: ["GM_xmlhttpRequest"],
        connect: ["localhost"],
      },
    }),
  ],
});
