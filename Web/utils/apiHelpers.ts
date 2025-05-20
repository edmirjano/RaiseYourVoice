import { AxiosRequestConfig } from 'axios';
import apiClient, { createApiClient } from '../services/apiClient';

/**
 * Creates a paginated request config
 */
export const createPaginatedRequestConfig = (
  page: number = 1,
  pageSize: number = 10,
  additionalParams: Record<string, any> = {}
): AxiosRequestConfig => {
  return {
    params: {
      page,
      pageSize,
      ...additionalParams
    }
  };
};

/**
 * Creates a request config with a timeout
 */
export const createTimeoutRequestConfig = (
  timeoutMs: number = 30000,
  additionalConfig: AxiosRequestConfig = {}
): AxiosRequestConfig => {
  return {
    ...additionalConfig,
    timeout: timeoutMs
  };
};

/**
 * Creates a request config with a specific content type
 */
export const createContentTypeRequestConfig = (
  contentType: string,
  additionalConfig: AxiosRequestConfig = {}
): AxiosRequestConfig => {
  return {
    ...additionalConfig,
    headers: {
      ...additionalConfig.headers,
      'Content-Type': contentType
    }
  };
};

/**
 * Creates a request config for file uploads
 */
export const createFileUploadRequestConfig = (
  additionalConfig: AxiosRequestConfig = {}
): AxiosRequestConfig => {
  return createContentTypeRequestConfig('multipart/form-data', {
    ...additionalConfig,
    timeout: 60000 // Longer timeout for file uploads
  });
};

/**
 * Creates a request config with a specific language
 */
export const createLanguageRequestConfig = (
  language: string,
  additionalConfig: AxiosRequestConfig = {}
): AxiosRequestConfig => {
  return {
    ...additionalConfig,
    headers: {
      ...additionalConfig.headers,
      'Accept-Language': language
    }
  };
};

/**
 * Creates a custom API client with specific base URL
 */
export const createCustomApiClient = (baseURL: string) => {
  return createApiClient({
    baseURL
  });
};

/**
 * Cancels a request using an abort controller
 */
export const createCancellableRequest = () => {
  const controller = new AbortController();
  
  return {
    config: {
      signal: controller.signal
    } as AxiosRequestConfig,
    cancel: () => controller.abort()
  };
};