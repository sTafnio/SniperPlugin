/**
 * Represents the raw item data received from the PoE Trade API fetch request.
 */
export interface PoeTradeItem {
  id: string;
  listing: {
    method: string;
    hideout_token?: string;
    fee?: number;
    price?: {
      amount: number;
      currency: string;
    };
    stash: {
      x: number;
      y: number;
    };
  };
  item: {
    name: string;
    typeLine: string;
    w: number;
    h: number;
  };
}

/**
 * Matches C# ItemData record.
 */
export interface ItemData {
  id: string;
  name: string;
  token: string;
  fee: number;
  size: {
    w: number;
    h: number;
  };
  position: {
    x: number;
    y: number;
  };
  price: {
    amount: number;
    currency: string;
  };
}

/** Matches C# DecisionAction enum */
export type DecisionAction = "ignore" | "teleport";

/** Matches C# ItemDecision record */
export interface ItemDecision {
  action: DecisionAction;
  reason: string;
}
