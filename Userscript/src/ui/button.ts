import { state, toggleTab } from "../core/state";
import { isLiveSearchActive } from "../core/liveSearch";
import { buttonStyles, colors } from "./styles";

let btn: HTMLButtonElement;

export function initUI() {
  btn = document.createElement("button");
  btn.id = "sniper-toggle-btn";
  btn.style.cssText = buttonStyles;
  document.body.appendChild(btn);

  btn.onclick = (e) => {
    e.preventDefault();

    if (!state.tabEnabled) {
      if (!isLiveSearchActive()) {
        console.warn("[SNIPER] Cannot enable: Live Search is not active!");
        return;
      }
    }

    toggleTab();
    updateUI();
  };

  updateUI();
}

export function updateUI() {
  if (!btn) return;

  if (state.tabEnabled) {
    btn.innerText = "Sniper: ON";
    btn.style.backgroundColor = colors.on;
  } else {
    btn.innerText = "Sniper: OFF";
    btn.style.backgroundColor = colors.off;
  }
}
