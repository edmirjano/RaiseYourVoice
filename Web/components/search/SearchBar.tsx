import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { searchPosts } from '../../services/postService';
import { searchCampaigns } from '../../services/campaignService';
import { searchOrganizations } from '../../services/organizationService';

export const SearchBar: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const [query, setQuery] = useState('');
  const [isSearching, setIsSearching] = useState(false);
  const [results, setResults] = useState<any[]>([]);
  const [showResults, setShowResults] = useState(false);
  const searchRef = useRef<HTMLDivElement>(null);
  
  // Close search results when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setShowResults(false);
      }
    };
    
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);
  
  // Debounced search
  useEffect(() => {
    if (!query.trim()) {
      setResults([]);
      setShowResults(false);
      return;
    }
    
    const timer = setTimeout(async () => {
      setIsSearching(true);
      try {
        // Search across different content types
        const [posts, campaigns, organizations] = await Promise.all([
          searchPosts(query),
          searchCampaigns(query),
          searchOrganizations(query)
        ]);
        
        // Combine and format results
        const formattedResults = [
          ...posts.slice(0, 3).map(post => ({
            id: post.id,
            title: post.title,
            type: 'post',
            url: `/posts/${post.id}`
          })),
          ...campaigns.slice(0, 3).map(campaign => ({
            id: campaign.id,
            title: campaign.title,
            type: 'campaign',
            url: `/campaigns/${campaign.id}`
          })),
          ...organizations.slice(0, 3).map(org => ({
            id: org.id,
            title: org.name,
            type: 'organization',
            url: `/organizations/${org.id}`
          }))
        ];
        
        setResults(formattedResults);
        setShowResults(true);
      } catch (error) {
        console.error('Search error:', error);
      } finally {
        setIsSearching(false);
      }
    }, 300);
    
    return () => clearTimeout(timer);
  }, [query]);
  
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (!query.trim()) return;
    
    router.push(`/search?q=${encodeURIComponent(query)}`);
    setShowResults(false);
  };
  
  const handleResultClick = (url: string) => {
    router.push(url);
    setShowResults(false);
    setQuery('');
  };
  
  return (
    <div className="relative" ref={searchRef}>
      <form onSubmit={handleSearch}>
        <div className="relative">
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={t('search.placeholder')}
            className="ios-input pl-10 w-full"
            onFocus={() => query.trim() && setShowResults(true)}
          />
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          {isSearching && (
            <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
              <div className="animate-spin rounded-full h-4 w-4 border-2 border-ios-black border-t-transparent"></div>
            </div>
          )}
        </div>
      </form>
      
      <AnimatePresence>
        {showResults && results.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 10, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 10, scale: 0.95 }}
            transition={{ duration: 0.2 }}
            className="absolute z-10 mt-2 w-full bg-white rounded-lg shadow-lg overflow-hidden"
          >
            <ul className="divide-y divide-gray-100">
              {results.map((result) => (
                <li key={`${result.type}-${result.id}`}>
                  <button
                    onClick={() => handleResultClick(result.url)}
                    className="w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors"
                  >
                    <div className="flex items-center">
                      <div className="flex-shrink-0">
                        {result.type === 'post' && (
                          <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1M19 20a2 2 0 002-2V8a2 2 0 00-2-2h-5a2 2 0 00-2 2v12a2 2 0 002 2h5z" />
                          </svg>
                        )}
                        {result.type === 'campaign' && (
                          <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                          </svg>
                        )}
                        {result.type === 'organization' && (
                          <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                          </svg>
                        )}
                      </div>
                      <div className="ml-3">
                        <p className="text-sm font-medium text-gray-900">{result.title}</p>
                        <p className="text-xs text-gray-500">
                          {t(`search.types.${result.type}`)}
                        </p>
                      </div>
                    </div>
                  </button>
                </li>
              ))}
              
              {/* View all results link */}
              <li>
                <button
                  onClick={() => handleResultClick(`/search?q=${encodeURIComponent(query)}`)}
                  className="w-full text-center px-4 py-3 text-sm text-ios-black font-medium hover:bg-gray-50 transition-colors"
                >
                  {t('search.viewAllResults')}
                </button>
              </li>
            </ul>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};