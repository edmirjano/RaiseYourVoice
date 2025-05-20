import React, { useState } from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { Button } from '../common/Button/Button';
import { Input } from '../common/Form/Input';
import { Select } from '../common/Form/Select';
import { Badge } from '../common/Badge/Badge';

type FilterType = 'all' | 'activism' | 'opportunity' | 'success';
type SortType = 'recent' | 'popular' | 'comments';

interface FeedFiltersProps {
  onFilterChange: (filter: FilterType) => void;
  onSortChange: (sort: SortType) => void;
  onSearch: (query: string) => void;
  className?: string;
}

export const FeedFilters: React.FC<FeedFiltersProps> = ({
  onFilterChange,
  onSortChange,
  onSearch,
  className = '',
}) => {
  const { t } = useTranslation('common');
  const [activeFilter, setActiveFilter] = useState<FilterType>('all');
  const [activeSort, setActiveSort] = useState<SortType>('recent');
  const [searchQuery, setSearchQuery] = useState('');
  const [showFilters, setShowFilters] = useState(false);
  
  const handleFilterChange = (filter: FilterType) => {
    setActiveFilter(filter);
    onFilterChange(filter);
  };
  
  const handleSortChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const sort = e.target.value as SortType;
    setActiveSort(sort);
    onSortChange(sort);
  };
  
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    onSearch(searchQuery);
  };
  
  return (
    <div className={`mb-6 ${className}`}>
      <div className="flex flex-col md:flex-row md:items-center md:justify-between space-y-4 md:space-y-0">
        {/* Filter Pills */}
        <div className="flex flex-wrap gap-2">
          <button
            onClick={() => handleFilterChange('all')}
            className={`px-3 py-1.5 rounded-full text-sm font-medium ${
              activeFilter === 'all'
                ? 'bg-ios-black text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            {t('feed.filters.all')}
          </button>
          <button
            onClick={() => handleFilterChange('activism')}
            className={`px-3 py-1.5 rounded-full text-sm font-medium ${
              activeFilter === 'activism'
                ? 'bg-ios-black text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            {t('feed.filters.activism')}
          </button>
          <button
            onClick={() => handleFilterChange('opportunity')}
            className={`px-3 py-1.5 rounded-full text-sm font-medium ${
              activeFilter === 'opportunity'
                ? 'bg-ios-black text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            {t('feed.filters.opportunities')}
          </button>
          <button
            onClick={() => handleFilterChange('success')}
            className={`px-3 py-1.5 rounded-full text-sm font-medium ${
              activeFilter === 'success'
                ? 'bg-ios-black text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            {t('feed.filters.successStories')}
          </button>
        </div>
        
        {/* Sort and Advanced Filters Toggle */}
        <div className="flex items-center space-x-2">
          <Select
            options={[
              { value: 'recent', label: t('feed.sort.recent') },
              { value: 'popular', label: t('feed.sort.popular') },
              { value: 'comments', label: t('feed.sort.comments') }
            ]}
            value={activeSort}
            onChange={handleSortChange}
            className="w-40"
          />
          
          <Button
            variant="secondary"
            size="sm"
            onClick={() => setShowFilters(!showFilters)}
          >
            {showFilters ? t('feed.hideFilters') : t('feed.showFilters')}
          </Button>
        </div>
      </div>
      
      {/* Advanced Filters */}
      <motion.div
        initial={{ height: 0, opacity: 0 }}
        animate={{ 
          height: showFilters ? 'auto' : 0,
          opacity: showFilters ? 1 : 0
        }}
        transition={{ duration: 0.3 }}
        className="overflow-hidden"
      >
        <div className="pt-4 pb-2 space-y-4">
          <form onSubmit={handleSearch} className="flex space-x-2">
            <Input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder={t('feed.searchPlaceholder')}
              className="flex-1"
            />
            <Button type="submit" disabled={!searchQuery.trim()}>
              {t('feed.search')}
            </Button>
          </form>
          
          <div>
            <p className="text-sm font-medium text-gray-700 mb-2">{t('feed.popularTags')}</p>
            <div className="flex flex-wrap gap-2">
              {['activism', 'environment', 'humanrights', 'education', 'community'].map((tag) => (
                <Badge
                  key={tag}
                  variant="secondary"
                  className="cursor-pointer hover:bg-gray-200"
                  onClick={() => {
                    setSearchQuery(`#${tag}`);
                    onSearch(`#${tag}`);
                  }}
                >
                  #{tag}
                </Badge>
              ))}
            </div>
          </div>
        </div>
      </motion.div>
    </div>
  );
};