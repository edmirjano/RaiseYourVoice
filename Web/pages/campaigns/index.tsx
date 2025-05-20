import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import MainLayout from '../../components/MainLayout';
import { CampaignCard } from '../../components/campaigns/CampaignCard';
import { getCampaigns } from '../../services/campaignService';

const CampaignsPage: React.FC = () => {
  const { t } = useTranslation('common');
  const [campaigns, setCampaigns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState('all');
  const [sort, setSort] = useState('newest');
  
  useEffect(() => {
    const fetchCampaigns = async () => {
      try {
        setLoading(true);
        const data = await getCampaigns();
        setCampaigns(data);
        setError('');
      } catch (err) {
        console.error('Failed to fetch campaigns:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchCampaigns();
  }, [t]);
  
  // Filter campaigns
  const filteredCampaigns = campaigns.filter(campaign => {
    if (filter === 'all') return true;
    return campaign.status.toLowerCase() === filter;
  });
  
  // Sort campaigns
  const sortedCampaigns = [...filteredCampaigns].sort((a, b) => {
    switch (sort) {
      case 'newest':
        return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
      case 'endingSoon':
        return new Date(a.endDate).getTime() - new Date(b.endDate).getTime();
      case 'mostFunded':
        return b.amountRaised - a.amountRaised;
      case 'leastFunded':
        return a.amountRaised - b.amountRaised;
      default:
        return 0;
    }
  });
  
  return (
    <MainLayout>
      <div className="space-y-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <h1 className="text-3xl font-bold mb-2">{t('campaigns.title')}</h1>
          <p className="text-gray-600 mb-6">{t('campaigns.description')}</p>
          
          {/* Filters */}
          <div className="flex flex-col md:flex-row md:justify-between md:items-center space-y-4 md:space-y-0 mb-6">
            <div className="flex space-x-2">
              <button
                onClick={() => setFilter('all')}
                className={`px-4 py-2 rounded-full text-sm ${
                  filter === 'all' 
                    ? 'bg-ios-black text-white' 
                    : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                }`}
              >
                {t('campaigns.filters.all')}
              </button>
              <button
                onClick={() => setFilter('active')}
                className={`px-4 py-2 rounded-full text-sm ${
                  filter === 'active' 
                    ? 'bg-ios-black text-white' 
                    : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                }`}
              >
                {t('campaigns.filters.active')}
              </button>
              <button
                onClick={() => setFilter('completed')}
                className={`px-4 py-2 rounded-full text-sm ${
                  filter === 'completed' 
                    ? 'bg-ios-black text-white' 
                    : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                }`}
              >
                {t('campaigns.filters.completed')}
              </button>
            </div>
            
            <div className="flex items-center">
              <label htmlFor="sort" className="text-sm font-medium text-gray-700 mr-2">
                {t('campaigns.sort')}:
              </label>
              <select
                id="sort"
                value={sort}
                onChange={(e) => setSort(e.target.value)}
                className="ios-input text-sm py-1"
              >
                <option value="newest">{t('campaigns.sortOptions.newest')}</option>
                <option value="endingSoon">{t('campaigns.sortOptions.endingSoon')}</option>
                <option value="mostFunded">{t('campaigns.sortOptions.mostFunded')}</option>
                <option value="leastFunded">{t('campaigns.sortOptions.leastFunded')}</option>
              </select>
            </div>
          </div>
        </motion.div>
        
        {loading ? (
          <div className="flex justify-center py-12">
            <div className="w-12 h-12 border-4 border-ios-black border-t-transparent rounded-full animate-spin"></div>
          </div>
        ) : error ? (
          <div className="ios-card p-6 text-center">
            <p className="text-red-600 mb-4">{error}</p>
            <button
              onClick={() => window.location.reload()}
              className="ios-button-secondary"
            >
              {t('common.tryAgain')}
            </button>
          </div>
        ) : sortedCampaigns.length === 0 ? (
          <div className="ios-card p-8 text-center">
            <h3 className="text-xl font-semibold mb-2">{t('campaigns.noCampaigns')}</h3>
            <p className="text-gray-600">{t('campaigns.noCampaignsDescription')}</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {sortedCampaigns.map((campaign) => (
              <CampaignCard key={campaign.id} campaign={campaign} />
            ))}
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export const getServerSideProps: GetServerSideProps = async ({ locale }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale || 'en', ['common'])),
    },
  };
};

export default CampaignsPage;