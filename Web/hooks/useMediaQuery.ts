import { useState, useEffect } from 'react';

/**
 * Custom hook for responsive design with media queries
 */
export const useMediaQuery = (query: string): boolean => {
  const [matches, setMatches] = useState(false);
  
  useEffect(() => {
    // Avoid running on the server
    if (typeof window === 'undefined') {
      return;
    }
    
    const media = window.matchMedia(query);
    
    // Update the state with the current value
    setMatches(media.matches);
    
    // Create a listener function
    const listener = (event: MediaQueryListEvent) => {
      setMatches(event.matches);
    };
    
    // Add the listener to watch for changes
    media.addEventListener('change', listener);
    
    // Remove the listener when the hook is unmounted
    return () => {
      media.removeEventListener('change', listener);
    };
  }, [query]);
  
  return matches;
};

// Predefined media queries for common breakpoints
export const useIsMobile = () => useMediaQuery('(max-width: 639px)');
export const useIsTablet = () => useMediaQuery('(min-width: 640px) and (max-width: 1023px)');
export const useIsDesktop = () => useMediaQuery('(min-width: 1024px)');
export const useIsLargeDesktop = () => useMediaQuery('(min-width: 1280px)');