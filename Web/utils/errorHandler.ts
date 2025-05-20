import { AxiosError } from 'axios';

/**
 * Extracts a user-friendly error message from various error types
 */
export const getErrorMessage = (error: unknown): string => {
  // Handle Axios errors
  if (isAxiosError(error)) {
    // Handle API error responses
    if (error.response) {
      const { status, data } = error.response;
      
      // Handle validation errors
      if (status === 400 && data.errors && typeof data.errors === 'object') {
        const errorMessages = Object.values(data.errors).flat();
        return errorMessages.join('. ');
      }
      
      // Handle other status codes
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
      
      // Use the error message from the response if available
      if (data.message) {
        return data.message;
      }
    }
    
    // Network errors
    if (error.message === 'Network Error') {
      return 'Unable to connect to the server. Please check your internet connection.';
    }
    
    // Timeout errors
    if (error.code === 'ECONNABORTED') {
      return 'The request timed out. Please try again.';
    }
    
    // Other Axios errors
    return error.message || 'An error occurred while processing your request.';
  }
  
  // Handle standard Error objects
  if (error instanceof Error) {
    return error.message;
  }
  
  // Handle unknown errors
  return 'An unexpected error occurred.';
};

/**
 * Type guard for Axios errors
 */
export const isAxiosError = (error: any): error is AxiosError => {
  return error && error.isAxiosError === true;
};

/**
 * Logs errors to the console and potentially to an error tracking service
 */
export const logError = (error: unknown, context?: string): void => {
  if (context) {
    console.error(`Error in ${context}:`, error);
  } else {
    console.error('Error:', error);
  }
  
  // Here you would add code to log to an error tracking service like Sentry
  // Example: Sentry.captureException(error);
};