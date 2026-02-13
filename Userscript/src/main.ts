import { startInterception } from "./core/interceptor";
import { initUI } from "./ui/button";

/**
 * PoE Sniper Auto-Teleport
 * Entry Point
 */

function bootstrap() {
  console.log("[SNIPER] Initializing Plugin/Userscript Application...");

  // Initialize UI components
  initUI();

  // Start XHR Interception
  startInterception();
}

// Ensure the DOM is ready if needed, though userscripts usually run at document-end or document-idle
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", bootstrap);
} else {
  bootstrap();
}
