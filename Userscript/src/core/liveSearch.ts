import { state } from "./state";
import { updateUI } from "../ui/button";

export function isLiveSearchActive(): boolean {
  const btn = document.querySelector(".livesearch-btn");
  if (!btn) return false;
  return btn.textContent?.includes("Deactivate") ?? false;
}

export function initLiveSearchObserver() {
  document.addEventListener(
    "click",
    (e) => {
      const target = e.target as HTMLElement;
      const btn = target.closest(".livesearch-btn");

      if (btn) {
        if (btn.textContent?.includes("Deactivate")) {
          if (state.tabEnabled) {
            console.log(
              "[SNIPER] Live search deactivating, turning off sniper.",
            );
            state.tabEnabled = false;
            updateUI();
          }
        }
      }
    },
    true,
  );
}
