import { useState, useEffect, useCallback } from 'react';
import { isOnline } from '../services/apiClient';

type ApiState<T> = {
  data: T | null;
  loading: boolean;
  error: string | null;
  isOffline: boolean;
};

type ApiOptions = {
  onSuccess?: (data: any) => void;
  onError?: (error: any) => void;
  initialData?: any;
  skip?: boolean;
};

/**
 * Custom hook for making API calls with loading, error, and offline states
 */
export function useApi<T>(
  apiFunction: (...args: any[]) => Promise<T>,
  dependencies: any[] = [],
  options: ApiOptions = {}
): [ApiState<T>, (...args: any[]) => Promise<T | null>] {
  const { onSuccess, onError, initialData = null, skip = false } = options;
  
  const [state, setState] = useState<ApiState<T>>({
    data: initialData,
    loading: false,
    error: null,
    isOffline: !isOnline()
  });
  
  // Check online status
  useEffect(() => {
    const handleOnline = () => {
      setState(prev => ({ ...prev, isOffline: false }));
    };
    
    const handleOffline = () => {
      setState(prev => ({ ...prev, isOffline: true }));
    };
    
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
    
    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);
  
  const execute = useCallback(
    async (...args: any[]): Promise<T | null> => {
      if (state.isOffline) {
        setState(prev => ({
          ...prev,
          error: 'You are currently offline. Please check your internet connection.'
        }));
        return null;
      }
      
      setState(prev => ({ ...prev, loading: true, error: null }));
      
      try {
        const data = await apiFunction(...args);
        setState(prev => ({ ...prev, data, loading: false }));
        
        if (onSuccess) {
          onSuccess(data);
        }
        
        return data;
      } catch (error: any) {
        const errorMessage = error.message || 'An unexpected error occurred';
        setState(prev => ({ ...prev, error: errorMessage, loading: false }));
        
        if (onError) {
          onError(error);
        }
        
        return null;
      }
    },
    [apiFunction, onSuccess, onError, state.isOffline]
  );
  
  // Auto-execute the API call if dependencies change and not skipped
  useEffect(() => {
    if (!skip) {
      execute();
    }
  }, [...dependencies, skip]);
  
  return [state, execute];
}