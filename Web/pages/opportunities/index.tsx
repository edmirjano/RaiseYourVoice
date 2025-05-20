import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import { PageLayout } from '../../components/layout';
import { OpportunityCard } from '../../components/opportunities/OpportunityCard';
import { OpportunityFilters } from '../../components/opportunities/OpportunityFilters';
import { MapView } from '../../components/opportunities/MapView';
import { getOpportunities, Post } from '../../services/postService';
import { useAuth } from '../../contexts/AuthContext';
import { Button } from '../../components/common/Button/Button';
import { Loader } from '../../components/common/Loader/Loader';
import { EmptyState } from '../../components/common/EmptyState/EmptyState';
import { Alert } from '../../components/common/Alert/Alert';

const OpportunitiesPage: React.FC = () => {
  const { t } = useTranslation('common');
  const { isAuthenticated } = useAuth();
  const [opportunities, setOpportunities] = useState<Post[]>([]);
  const [filteredOpportunities, setFilteredOpportunities] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [viewMode, setViewMode] = useState<'grid' | 'map'>('grid');
  const [filters, setFilters] = useState({
    category: 'all',
    dateRange: 'all',
    location: '',
    searchQuery: ''
  });

  useEffect(() => {
    const fetchOpportunities = async () => {
      try {
        setLoading(true);
        const data = await getOpportunities();
        setOpportunities(data);
        setFilteredOpportunities(data);
        setError('');
      } catch (err) {
        console.error('Failed to fetch opportunities:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };

    fetchOpportunities();
  }, [t]);

  useEffect(() => {
    // Apply filters to opportunities
    let filtered = [...opportunities];

    // Filter by category
    if (filters.category !== 'all') {
      filtered = filtered.filter(opp => 
        opp.tags?.includes(filters.category)
      );
    }

    // Filter by date range
    if (filters.dateRange !== 'all') {
      const now = new Date();
      const tomorrow = new Date(now);
      tomorrow.setDate(tomorrow.getDate() + 1);
      
      const nextWeek = new Date(now);
      nextWeek.setDate(nextWeek.getDate() + 7);
      
      const nextMonth = new Date(now);
      nextMonth.setMonth(nextMonth.getMonth() + 1);

      filtered = filtered.filter(opp => {
        if (!opp.eventDate) return false;
        
        const eventDate = new Date(opp.eventDate);
        
        switch (filters.dateRange) {
          case 'today':
            return eventDate.toDateString() === now.toDateString();
          case 'tomorrow':
            return eventDate.toDateString() === tomorrow.toDateString();
          case 'week':
            return eventDate > now && eventDate <= nextWeek;
          case 'month':
            return eventDate > now && eventDate <= nextMonth;
          default:
            return true;
        }
      });
    }

    // Filter by location
    if (filters.location) {
      const location = filters.location.toLowerCase();
      filtered = filtered.filter(opp => 
        opp.location?.city?.toLowerCase().includes(location) ||
        opp.location?.country?.toLowerCase().includes(location)
      );
    }

    // Filter by search query
    if (filters.searchQuery) {
      const query = filters.searchQuery.toLowerCase();
      filtered = filtered.filter(opp => 
        opp.title.toLowerCase().includes(query) ||
        opp.content.toLowerCase().includes(query) ||
        opp.tags?.some(tag => tag.toLowerCase().includes(query))
      );
    }

    setFilteredOpportunities(filtered);
  }, [filters, opportunities]);

  const handleFilterChange = (newFilters: any) => {
    setFilters({ ...filters, ...newFilters });
  };

  const handleCreateOpportunity = () => {
    // Navigate to create opportunity page
    window.location.href = '/opportunities/create';
  };

  return (
    <PageLayout title={`${t('opportunities.title')} | RaiseYourVoice`}>
      <div className="max-w-7xl mx-auto">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
            <div>
              <h1 className="text-2xl font-bold mb-2">{t('opportunities.title')}</h1>
              <p className="text-gray-600">{t('opportunities.description')}</p>
            </div>
            
            {isAuthenticated && (
              <Button 
                onClick={handleCreateOpportunity}
                className="mt-4 md:mt-0"
              >
                {t('opportunities.createOpportunity')}
              </Button>
            )}
          </div>
          
          {/* Filters and View Mode Selector */}
          <div className="mb-6">
            <OpportunityFilters 
              onFilterChange={handleFilterChange}
              filters={filters}
            />
            
            <div className="flex justify-end mt-4">
              <div className="inline-flex rounded-md shadow-sm">
                <button
                  type="button"
                  onClick={() => setViewMode('grid')}
                  className={`px-4 py-2 text-sm font-medium rounded-l-md ${
                    viewMode === 'grid'
                      ? 'bg-ios-black text-white'
                      : 'bg-white text-gray-700 hover:bg-gray-50'
                  } border border-gray-300`}
                >
                  <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
                  </svg>
                </button>
                <button
                  type="button"
                  onClick={() => setViewMode('map')}
                  className={`px-4 py-2 text-sm font-medium rounded-r-md ${
                    viewMode === 'map'
                      ? 'bg-ios-black text-white'
                      : 'bg-white text-gray-700 hover:bg-gray-50'
                  } border border-gray-300`}
                >
                  <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
          
          {error && (
            <Alert 
              variant="error" 
              className="mb-6"
              onClose={() => setError('')}
            >
              {error}
            </Alert>
          )}
          
          {loading ? (
            <div className="flex justify-center py-12">
              <Loader size="lg" type="spinner" text={t('common.loading')} />
            </div>
          ) : filteredOpportunities.length === 0 ? (
            <EmptyState
              title={t('opportunities.noOpportunities.title')}
              description={t('opportunities.noOpportunities.description')}
              icon={
                <svg className="h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              }
              action={
                isAuthenticated ? (
                  <Button onClick={handleCreateOpportunity}>
                    {t('opportunities.createOpportunity')}
                  </Button>
                ) : (
                  <Button onClick={() => window.location.href = '/auth/login'}>
                    {t('auth.login')}
                  </Button>
                )
              }
            />
          ) : viewMode === 'grid' ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {filteredOpportunities.map((opportunity) => (
                <OpportunityCard key={opportunity.id} opportunity={opportunity} />
              ))}
            </div>
          ) : (
            <MapView opportunities={filteredOpportunities} />
          )}
        </motion.div>
      </div>
    </PageLayout>
  );
};

export const getServerSideProps: GetServerSideProps = async ({ locale }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale || 'en', ['common'])),
    },
  };
};

export default OpportunitiesPage;