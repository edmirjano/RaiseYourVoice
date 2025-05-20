/**
 * Check if the device is online
 */
export const isOnline = (): boolean => {
  return typeof navigator !== 'undefined' && typeof navigator.onLine === 'boolean'
    ? navigator.onLine
    : true;
};

/**
 * Check if the connection is slow based on the Network Information API
 */
export const isSlowConnection = (): boolean => {
  if (typeof navigator === 'undefined' || !('connection' in navigator)) {
    return false;
  }
  
  const connection = (navigator as any).connection;
  
  if (!connection) {
    return false;
  }
  
  // Check if the connection is slow based on effectiveType
  if (connection.effectiveType) {
    return ['slow-2g', '2g', '3g'].includes(connection.effectiveType);
  }
  
  // Fallback to checking downlink speed if effectiveType is not available
  if (typeof connection.downlink === 'number') {
    return connection.downlink < 1.5; // Less than 1.5 Mbps is considered slow
  }
  
  return false;
};

/**
 * Get the connection type if available
 */
export const getConnectionType = (): string | null => {
  if (typeof navigator === 'undefined' || !('connection' in navigator)) {
    return null;
  }
  
  const connection = (navigator as any).connection;
  
  if (!connection) {
    return null;
  }
  
  return connection.effectiveType || connection.type || null;
};

/**
 * Check if the device is on a metered connection
 */
export const isMeteredConnection = (): boolean => {
  if (typeof navigator === 'undefined' || !('connection' in navigator)) {
    return false;
  }
  
  const connection = (navigator as any).connection;
  
  if (!connection) {
    return false;
  }
  
  return connection.saveData === true;
};

/**
 * Add event listeners for connection changes
 */
export const addConnectionChangeListeners = (
  onOnline: () => void,
  onOffline: () => void,
  onConnectionChange?: (type: string) => void
): () => void => {
  window.addEventListener('online', onOnline);
  window.addEventListener('offline', onOffline);
  
  let connectionChangeListener: (() => void) | undefined;
  
  if (typeof navigator !== 'undefined' && 'connection' in navigator && onConnectionChange) {
    const connection = (navigator as any).connection;
    
    if (connection) {
      connectionChangeListener = () => {
        const type = connection.effectiveType || connection.type || 'unknown';
        onConnectionChange(type);
      };
      
      connection.addEventListener('change', connectionChangeListener);
    }
  }
  
  // Return a cleanup function
  return () => {
    window.removeEventListener('online', onOnline);
    window.removeEventListener('offline', onOffline);
    
    if (connectionChangeListener && typeof navigator !== 'undefined' && 'connection' in navigator) {
      const connection = (navigator as any).connection;
      if (connection) {
        connection.removeEventListener('change', connectionChangeListener);
      }
    }
  };
};