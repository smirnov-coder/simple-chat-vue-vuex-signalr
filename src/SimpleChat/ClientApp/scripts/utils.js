const STORAGE_KEY = "access_token";

export const readToken = () => window.localStorage.getItem(STORAGE_KEY);

export const storeToken = (accessToken) => window.localStorage.setItem(STORAGE_KEY, accessToken);

export const clearToken = () => window.localStorage.removeItem(STORAGE_KEY);
