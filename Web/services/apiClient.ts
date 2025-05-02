import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { refreshToken } from './authService';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Flag to prevent multiple refresh token requests
let isRefreshing = false;
// Store all requests that should be retried after token refresh
let refreshSubscribers: Array<(token: string) => void> = [];

/**
 * Execute all stored requests with the new access token
 */
const onTokenRefreshed = (newToken: string) => {
  refreshSubscribers.forEach(callback => callback(newToken));
  refreshSubscribers = [];
};

/**
 * Add a request callback to the refresh subscribers queue
 */
const addRefreshSubscriber = (callback: (token: string) => void) => {
  refreshSubscribers.push(callback);
};

// Add a request interceptor to include auth token
apiClient.interceptors.request.use(
  (config) => {
    // Check if we're in the browser
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      
      // Add preferred language header for internationalization
      const language = localStorage.getItem('preferredLanguage') || 'en';
      config.headers['Accept-Language'] = language;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for handling common errors and token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    
    // Only handle 401 errors for requests that haven't been retried yet
    if (
      error.response?.status === 401 && 
      !originalRequest._retry &&
      typeof window !== 'undefined'
    ) {
      // Check if we're already refreshing to prevent multiple parallel refresh requests
      if (isRefreshing) {
        // Wait for the token to be refreshed
        return new Promise(resolve => {
          addRefreshSubscriber((token: string) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            originalRequest._retry = true;
            resolve(axios(originalRequest));
          });
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshTokenValue = localStorage.getItem('refreshToken');
        
        if (!refreshTokenValue) {
          // No refresh token available, redirect to login
          localStorage.clear();
          window.location.href = '/auth/login?expired=true';
          return Promise.reject(error);
        }
        
        // Try to refresh the token
        const response = await refreshToken(refreshTokenValue);
        const { token } = response;
        
        // Notify all subscribers that the token has been refreshed
        onTokenRefreshed(token);
        
        // Retry the original request
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return axios(originalRequest);
      } catch (refreshError) {
        // If refresh token is invalid or expired
        localStorage.clear();
        window.location.href = '/auth/login?expired=true';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }
    
    // For 403 Forbidden errors (insufficient permissions)
    if (error.response?.status === 403) {
      // Redirect to access denied page
      window.location.href = '/access-denied';
    }
    
    return Promise.reject(error);
  }
);

export default apiClient;