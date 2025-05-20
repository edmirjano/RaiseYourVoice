import axios, { AxiosError, AxiosRequestConfig, AxiosResponse, InternalAxiosRequestConfig } from 'axios';
import Cookies from 'js-cookie';
import { refreshToken } from './authService';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

// Create axios instance with default config
const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000, // 15 seconds timeout
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
  (config: InternalAxiosRequestConfig) => {
    // Check if we're in the browser
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token') || Cookies.get('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      
      // Add preferred language header for internationalization
      const language = localStorage.getItem('preferredLanguage') || Cookies.get('preferredLanguage') || 'en';
      config.headers['Accept-Language'] = language;
    }

    // Add timestamp to prevent caching for GET requests
    if (config.method?.toLowerCase() === 'get') {
      config.params = {
        ...config.params,
        _t: Date.now()
      };
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for handling common errors and token refresh
apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };
    
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
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            } else {
              originalRequest.headers = { Authorization: `Bearer ${token}` };
            }
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
          Cookies.remove('token');
          window.location.href = '/auth/login?expired=true';
          return Promise.reject(error);
        }
        
        // Try to refresh the token
        const response = await refreshToken(refreshTokenValue);
        const { token } = response;
        
        // Notify all subscribers that the token has been refreshed
        onTokenRefreshed(token);
        
        // Retry the original request
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${token}`;
        } else {
          originalRequest.headers = { Authorization: `Bearer ${token}` };
        }
        return axios(originalRequest);
      } catch (refreshError) {
        // If refresh token is invalid or expired
        localStorage.clear();
        Cookies.remove('token');
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
      return Promise.reject(error);
    }

    // For 404 Not Found errors
    if (error.response?.status === 404) {
      console.error('Resource not found:', error.config?.url);
      return Promise.reject(error);
    }

    // For 500 Internal Server errors
    if (error.response?.status && error.response.status >= 500) {
      console.error('Server error:', error.response.data);
      return Promise.reject(error);
    }

    // For network errors (offline)
    if (error.message === 'Network Error') {
      console.error('Network error - check your internet connection');
      // You could dispatch an action to show an offline banner here
      return Promise.reject(new Error('You are currently offline. Please check your internet connection.'));
    }
    
    return Promise.reject(error);
  }
);

// Export a function to create a new instance with custom config
export const createApiClient = (config?: AxiosRequestConfig) => {
  return axios.create({
    ...apiClient.defaults,
    ...config,
  });
};

// Export a function to check if the device is online
export const isOnline = (): boolean => {
  return typeof navigator !== 'undefined' && typeof navigator.onLine === 'boolean'
    ? navigator.onLine
    : true;
};

// Export a helper to handle API errors
export const handleApiError = (error: any): string => {
  if (!isOnline()) {
    return 'You are currently offline. Please check your internet connection.';
  }

  if (error.response) {
    // The request was made and the server responded with a status code
    // that falls out of the range of 2xx
    const { status, data } = error.response;
    
    if (status === 400) {
      // Bad request - usually validation errors
      if (data.errors && typeof data.errors === 'object') {
        // Handle validation errors
        const errorMessages = Object.values(data.errors).flat();
        return errorMessages.join('. ');
      }
      return data.message || 'Invalid request. Please check your data.';
    }
    
    if (status === 401) {
      return 'You are not authorized to perform this action. Please log in again.';
    }
    
    if (status === 403) {
      return 'You do not have permission to perform this action.';
    }
    
    if (status === 404) {
      return 'The requested resource was not found.';
    }
    
    if (status === 429) {
      return 'Too many requests. Please try again later.';
    }
    
    if (status >= 500) {
      return 'A server error occurred. Please try again later.';
    }
    
    return data.message || 'An error occurred while processing your request.';
  } else if (error.request) {
    // The request was made but no response was received
    return 'No response from server. Please try again later.';
  } else {
    // Something happened in setting up the request that triggered an Error
    return error.message || 'An unexpected error occurred.';
  }
};

export default apiClient;