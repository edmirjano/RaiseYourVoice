import React, { useState, useEffect } from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { Input } from '../common/Form/Input';
import { Select } from '../common/Form/Select';
import { Button } from '../common/Button/Button';
import { useDebounce } from '../../hooks/useDebounce';

interface OpportunityFiltersProps {
  onFilterChange: (filters: any) => void;
  filters: {
    category: string;
    dateRange: string;
    location: string;
    searchQuery: string;
  };
  className?: string;
}

export const OpportunityFilters: React.FC<OpportunityFiltersProps> = ({
  onFilterChange,
  filters,
  className = '',
}) => {
  const { t } = useTranslation('common');
  const [searchQuery, setSearchQuery] = useState(filters.searchQuery);
  const [location, setLocation] = useState(filters.location);
  const [showAdvanced, setShowAdvanced] = useState(false);
  
  // Debounce search and location inputs to avoid too many filter updates
  const debouncedSearch = useDebounce(searchQuery, 300);
  const debouncedLocation = useDebounce(location, 300);
  
  // Update filters when debounced values change
  useEffect(() => {
    onFilterChange({ searchQuery: debouncedSearch });
  }, [debouncedSearch, onFilterChange]);
  
  useEffect(() => {
    onFilterChange({ location: debouncedLocation });
  }, [debouncedLocation, onFilterChange]);
  
  const handleCategoryChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    onFilterChange({ category: e.target.value });
  };
  
  const handleDateRangeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    onFilterChange({ dateRange: e.target.value });
  };
  
  const handleClearFilters = () => {
    setSearchQuery('');
    setLocation('');
    onFilterChange({
      category: 'all',
      dateRange: 'all',
      location: '',
      searchQuery: ''
    });
  };
  
  return (
    <div className={`ios-card p-4 ${className}`}>
      <div className="flex flex-col md:flex-row md:items-end gap-4">
        {/* Search Input */}
        <div className="flex-1">
          <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-1">
            {t('opportunities.search')}
          </label>
          <Input
            id="search"
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder={t('opportunities.searchPlaceholder')}
            icon={
              <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            }
          />
        </div>
        
        {/* Category Filter */}
        <div className="w-full md:w-48">
          <label htmlFor="category" className="block text-sm font-medium text-gray-700 mb-1">
            {t('opportunities.category')}
          </label>
          <Select
            id="category"
            value={filters.category}
            onChange={handleCategoryChange}
            options={[
              { value: 'all', label: t('opportunities.allCategories') },
              { value: 'funding', label: t('opportunities.categories.funding') },
              { value: 'events', label: t('opportunities.categories.events') },
              { value: 'volunteer', label: t('opportunities.categories.volunteer') },
              { value: 'mun', label: t('opportunities.categories.mun') },
              { value: 'mobility', label: t('opportunities.categories.mobility') }
            ]}
          />
        </div>
        
        {/* Date Range Filter */}
        <div className="w-full md:w-48">
          <label htmlFor="dateRange" className="block text-sm font-medium text-gray-700 mb-1">
            {t('opportunities.dateRange')}
          </label>
          <Select
            id="dateRange"
            value={filters.dateRange}
            onChange={handleDateRangeChange}
            options={[
              { value: 'all', label: t('opportunities.allDates') },
              { value: 'today', label: t('opportunities.today') },
              { value: 'tomorrow', label: t('opportunities.tomorrow') },
              { value: 'week', label: t('opportunities.thisWeek') },
              { value: 'month', label: t('opportunities.thisMonth') }
            ]}
          />
        </div>
        
        {/* Advanced Filters Toggle */}
        <div>
          <Button
            variant="secondary"
            size="sm"
            onClick={() => setShowAdvanced(!showAdvanced)}
          >
            {showAdvanced ? t('opportunities.hideAdvanced') : t('opportunities.showAdvanced')}
          </Button>
        </div>
      </div>
      
      {/* Advanced Filters */}
      <motion.div
        initial={{ height: 0, opacity: 0 }}
        animate={{ 
          height: showAdvanced ? 'auto' : 0,
          opacity: showAdvanced ? 1 : 0
        }}
        transition={{ duration: 0.3 }}
        className="overflow-hidden"
      >
        <div className="pt-4 space-y-4">
          {/* Location Filter */}
          <div>
            <label htmlFor="location" className="block text-sm font-medium text-gray-700 mb-1">
              {t('opportunities.location')}
            </label>
            <Input
              id="location"
              type="text"
              value={location}
              onChange={(e) => setLocation(e.target.value)}
              placeholder={t('opportunities.locationPlaceholder')}
              icon={
                <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
              }
            />
          </div>
          
          {/* Clear Filters Button */}
          <div className="flex justify-end">
            <Button
              variant="secondary"
              size="sm"
              onClick={handleClearFilters}
            >
              {t('opportunities.clearFilters')}
            </Button>
          </div>
        </div>
      </motion.div>
    </div>
  );
};