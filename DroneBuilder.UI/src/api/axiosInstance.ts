import axios from 'axios';
import { useAuthStore } from '../store/authStore';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '/api';

export const api = axios.create({
  baseURL: apiBaseUrl,
  headers: { 'Content-Type': 'application/json' },

  // The access token travels as an HttpOnly cookie, which is only sent when credentials are enabled.
  withCredentials: true,
});

api.interceptors.request.use((config) => {
  if (config.data instanceof FormData) {
    delete config.headers['Content-Type'];
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // A 401 while the store believes it is signed in means the session expired. During the initial
      // /users/me probe the store is still 'loading', and that 401 is just the anonymous answer.
      const { status, clear } = useAuthStore.getState();

      if (status === 'authenticated') {
        clear();
        if (typeof window !== 'undefined') {
          window.location.href = '/login';
        }
      }
    }
    return Promise.reject(error);
  }
);
