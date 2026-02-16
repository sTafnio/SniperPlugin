import { defineConfig } from "vite";
import monkey from "vite-plugin-monkey";

export default defineConfig({
  plugins: [
    monkey({
      entry: "src/main.ts",
      userscript: {
        name: "PoE Sniper Auto-Teleport",
        namespace: "https://github.com/sTafnio/SniperPlugin/Userscript",
        version: "1.0",
        description:
          "Userscript used in combination with SniperPlugin ExileAPI plugin.",
        author: "sTafnio",
        match: ["https://www.pathofexile.com/trade/search/*"],
        grant: ["GM_xmlhttpRequest"],
        connect: ["localhost"],
      },
    }),
  ],
});
