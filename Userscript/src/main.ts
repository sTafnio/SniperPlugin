import { startInterception } from "./core/interceptor";
import { initLiveSearchObserver } from "./core/liveSearch";
import { initUI } from "./ui/button";

function bootstrap() {
  console.log("[SNIPER] Initializing Plugin/Userscript Application...");

  // Initialize UI components
  initUI();

  // Watch for Live Search status
  initLiveSearchObserver();

  // Start XHR Interception
  startInterception();
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", bootstrap);
} else {
  bootstrap();
}
