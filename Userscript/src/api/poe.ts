export async function executeTeleport(token: string) {
  return new Promise<void>((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open("POST", "https://www.pathofexile.com/api/trade/whisper");
    xhr.setRequestHeader("Content-Type", "application/json");
    xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        console.log("[SNIPER] API call sent successfully!");
        resolve();
      } else {
        let errorMsg = xhr.statusText;
        try {
          const resp = JSON.parse(xhr.responseText);
          if (resp.error && resp.error.message) {
            errorMsg = resp.error.message;
          }
        } catch {}
        console.warn("[SNIPER] Teleport failed", xhr.status, errorMsg);
        reject(new Error(errorMsg));
      }
    };

    xhr.onerror = () => {
      console.error("[SNIPER] Network error during teleport");
      reject(new Error("Network error"));
    };

    xhr.send(JSON.stringify({ token }));
  });
}
