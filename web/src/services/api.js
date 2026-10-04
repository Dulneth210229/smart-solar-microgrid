import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add the JWT automatically to authenticated API requests.
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("smartSolarToken");

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error),
);

// Log the user out automatically if the token is no longer valid.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("smartSolarToken");
      localStorage.removeItem("smartSolarUser");
    }

    return Promise.reject(error);
  },
);

export default api;
