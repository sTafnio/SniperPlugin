export interface AppState {
  tabEnabled: boolean;
}

export const state: AppState = {
  tabEnabled: false,
};

export const toggleTab = () => {
  state.tabEnabled = !state.tabEnabled;
};
