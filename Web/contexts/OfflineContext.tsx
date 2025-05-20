import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';

interface OfflineContextType {
  isOffline: boolean;
}

const OfflineContext = createContext<OfflineContextType>({
  isOffline: false,
});

export const useOffline = () => useContext(OfflineContext);

interface OfflineProviderProps {
  children: ReactNode;
}

export const OfflineProvider: React.FC<OfflineProviderProps> = ({ children }) => {
  const [isOffline, setIsOffline] = useState<boolean>(
    typeof navigator !== 'undefined' && typeof navigator.onLine === 'boolean'
      ? !navigator.onLine
      : false
  );
  
  useEffect(() => {
    const handleOnline = () => setIsOffline(false);
    const handleOffline = () => setIsOffline(true);
    
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
    
    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);
  
  return (
    <OfflineContext.Provider value={{ isOffline }}>
      {children}
      {isOffline && (
        <div className="fixed bottom-0 inset-x-0 bg-red-500 text-white py-2 px-4 text-center z-50">
          You are currently offline. Some features may be unavailable.
        </div>
      )}
    </OfflineContext.Provider>
  );
};