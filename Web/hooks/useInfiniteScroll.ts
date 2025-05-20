import { useState, useEffect, useCallback, useRef } from 'react';

type UseInfiniteScrollOptions = {
  threshold?: number;
  initialPage?: number;
  enabled?: boolean;
};

/**
 * Custom hook for implementing infinite scrolling
 */
export function useInfiniteScroll<T>(
  fetchFunction: (page: number) => Promise<T[]>,
  options: UseInfiniteScrollOptions = {}
) {
  const { threshold = 200, initialPage = 1, enabled = true } = options;
  
  const [items, setItems] = useState<T[]>([]);
  const [page, setPage] = useState(initialPage);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(true);
  const observer = useRef<IntersectionObserver | null>(null);
  
  // Function to load more items
  const loadMore = useCallback(async () => {
    if (loading || !hasMore || !enabled) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const newItems = await fetchFunction(page);
      
      if (newItems.length === 0) {
        setHasMore(false);
      } else {
        setItems(prevItems => [...prevItems, ...newItems]);
        setPage(prevPage => prevPage + 1);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred while fetching data');
    } finally {
      setLoading(false);
    }
  }, [fetchFunction, page, loading, hasMore, enabled]);
  
  // Reset function
  const reset = useCallback(() => {
    setItems([]);
    setPage(initialPage);
    setLoading(false);
    setError(null);
    setHasMore(true);
  }, [initialPage]);
  
  // Ref callback for the last item
  const lastItemRef = useCallback(
    (node: HTMLElement | null) => {
      if (loading || !enabled) return;
      
      if (observer.current) {
        observer.current.disconnect();
      }
      
      observer.current = new IntersectionObserver(
        entries => {
          if (entries[0].isIntersecting && hasMore) {
            loadMore();
          }
        },
        { rootMargin: `0px 0px ${threshold}px 0px` }
      );
      
      if (node) {
        observer.current.observe(node);
      }
    },
    [loading, hasMore, loadMore, threshold, enabled]
  );
  
  // Initial load
  useEffect(() => {
    if (enabled) {
      loadMore();
    }
    
    return () => {
      if (observer.current) {
        observer.current.disconnect();
      }
    };
  }, [enabled]); // eslint-disable-line react-hooks/exhaustive-deps
  
  return { items, loading, error, hasMore, lastItemRef, loadMore, reset };
}