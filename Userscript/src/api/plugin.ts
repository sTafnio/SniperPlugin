import { GM_xmlhttpRequest } from "$";
import type { ItemData, ItemDecision } from "../types";

const SERVER_URL = "http://localhost:49152";

export async function reportItemToPlugin(
  itemData: ItemData,
): Promise<ItemDecision> {
  return sendPost("/item", itemData);
}

export async function notifyTeleportSuccess(itemData: ItemData): Promise<void> {
  await sendPost("/teleport-success", itemData);
}

export async function notifyTeleportFailure(error: string): Promise<void> {
  await sendPost("/teleport-failure", { error });
}

async function sendPost(path: string, data: any): Promise<any> {
  return new Promise((resolve) => {
    GM_xmlhttpRequest({
      method: "POST",
      url: `${SERVER_URL}${path}`,
      data: JSON.stringify(data),
      timeout: 35000,
      headers: {
        "Content-Type": "application/json",
      },
      onload: (res: { responseText: string; status: number }) => {
        if (res.status >= 400) {
          console.error(
            `[SNIPER] Server error on ${path}: ${res.status}`,
            res.responseText,
          );
          resolve({ action: "ignore", reason: "Server Error" });
          return;
        }
        try {
          resolve(JSON.parse(res.responseText));
        } catch (e) {
          console.error(`[SNIPER] Failed to parse response from ${path}:`, e);
          resolve({ action: "ignore", reason: "Parse Error" });
        }
      },
      onerror: () => {
        console.error(`[SNIPER] Server offline - failed to reach ${path}`);
        resolve({ action: "ignore", reason: "Server Offline" });
      },
      ontimeout: () => {
        console.warn(`[SNIPER] Request to ${path} timed out`);
        resolve({ action: "ignore", reason: "Request Timeout" });
      },
    });
  });
}
