import { state } from "./state";
import {
  reportItemToPlugin,
  notifyTeleportSuccess,
  notifyTeleportFailure,
} from "../api/plugin";
import { executeTeleport } from "../api/poe";
import type { PoeTradeItem, ItemData } from "../types";

export function startInterception() {
  const originalXHR = window.XMLHttpRequest.prototype.open;

  window.XMLHttpRequest.prototype.open = function (
    _method: string,
    url: string | URL,
  ) {
    const urlStr = typeof url === "string" ? url : url.toString();

    this.addEventListener("load", function () {
      if (!urlStr || !urlStr.includes("/api/trade/fetch")) return;
      if (!state.tabEnabled) return;

      try {
        const data = JSON.parse(this.responseText);
        if (!data.result || data.result.length === 0) return;

        processNewItems(data.result);
      } catch (e) {
        console.error("[SNIPER] XHR JSON Parse Error", e);
      }
    });

    return originalXHR.apply(this, arguments as any);
  };
}

async function processNewItems(items: PoeTradeItem[]) {
  for (const item of items) {
    try {
      await handleSingleItem(item);
    } catch (e) {
      console.error("[SNIPER] Item handling error", e);
    }
  }
}

async function handleSingleItem(rawItem: PoeTradeItem) {
  const itemData: ItemData = {
    id: rawItem.id,
    name: `${rawItem.item.name ? `${rawItem.item.name} ` : ""}${rawItem.item.typeLine}`.trim(),
    token: rawItem.listing?.hideout_token || "",
    fee: rawItem.listing.fee || 0,
    size: { w: rawItem.item.w, h: rawItem.item.h },
    position: { x: rawItem.listing.stash.x, y: rawItem.listing.stash.y },
    price: {
      amount: rawItem.listing.price?.amount || 0,
      currency: rawItem.listing.price?.currency || "unknown",
    },
  };

  try {
    console.log(
      `[SNIPER] Reporting ${itemData.name} (${itemData.id}) to plugin...`,
    );

    // 1. Ask permission to teleport
    const decision = await reportItemToPlugin(itemData);

    if (decision.action === "teleport") {
      console.log(
        `[SNIPER] Teleport approved for ${itemData.name} (${itemData.id}): ${decision.reason}`,
      );

      try {
        // 2. Execute the whisper/teleport
        await executeTeleport(itemData.token);

        // 3. Success -> Tell plugin to start automation
        await notifyTeleportSuccess(itemData);
      } catch (err: any) {
        // 4. Failure -> Tell plugin to release queue
        console.warn(
          `[SNIPER] Teleport failed for ${itemData.name} (${itemData.id}): ${err.message}`,
        );
        await notifyTeleportFailure(err.message);
      }
    } else {
      console.log(
        `[SNIPER] Teleport rejected for ${itemData.name} (${itemData.id}): ${decision.reason}`,
      );
    }
  } catch (e) {
    console.error(
      `[SNIPER] Error processing item ${itemData.name} (${itemData.id})`,
      e,
    );
  }
}
