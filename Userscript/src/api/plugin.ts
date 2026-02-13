import { GM_xmlhttpRequest } from "$";
import type { ItemData, ItemDecision } from "../types";

const SERVER_URL = "http://localhost:49152";

/**
 * Sends a single item to the C# plugin for evaluation.
 */
export async function reportItemToPlugin(
  itemData: ItemData,
): Promise<ItemDecision> {
  return sendPost("/item", itemData);
}

/**
 * Notifies the plugin that the whisper/teleport was successfully initiated.
 */
export async function notifyTeleportSuccess(itemData: ItemData): Promise<void> {
  await sendPost("/teleport-success", itemData);
}

/**
 * Notifies the plugin that the whisper failed (e.g., item sold).
 */
export async function notifyTeleportFailure(
  itemData: ItemData,
  error: string,
): Promise<void> {
  await sendPost("/teleport-failure", { item: itemData, error });
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
          resolve({ action: "ignore", reason: "Server Error" });
          return;
        }
        try {
          resolve(JSON.parse(res.responseText));
        } catch (e) {
          resolve({ action: "ignore", reason: "Parse Error" });
        }
      },
      onerror: () => resolve({ action: "ignore", reason: "Server Offline" }),
      ontimeout: () => resolve({ action: "ignore", reason: "Request Timeout" }),
    });
  });
}
