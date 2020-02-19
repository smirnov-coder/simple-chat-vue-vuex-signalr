// Ключ, по которому хранится маркер доступа к API в localStorage браузера.
export const ACCESS_TOKEN_STORAGE_KEY = "access_token";

// Извлекает маркер доступа к API из localStorage браузера.
export const readToken = () => window.localStorage.getItem(ACCESS_TOKEN_STORAGE_KEY);

// Сохраняет маркер доступа к API в localStorage браузера.
export const storeToken = (accessToken) => window.localStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, accessToken);

// Удаляет маркер доступа к API из localStorage браузера.
export const clearToken = () => window.localStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY);

// Задаёт полю объекта значение, равное значению имени поля.
export function mapKeyToValue(obj, toLowerCase = false) {
    let result = {};
    Object.keys(obj).map(key => {
        result[key] = toLowerCase ? key.toLowerCase() : key;
    });
    return result;
}
