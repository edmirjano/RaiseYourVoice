import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import MainLayout from '../components/MainLayout';
import { PostCard } from '../components/feed/PostCard';
import { CampaignCard } from '../components/campaigns/CampaignCard';
import { OrganizationCard } from '../components/organizations/OrganizationCard';
import { searchPosts } from '../services/postService';
import { searchCampaigns } from '../services/campaignService';
import { searchOrganizations } from '../services/organizationService';

const SearchPage: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { q: query } = router.query;
  
  const [activeTab, setActiveTab] = useState('all');
  const [posts, setPosts] = useState([]);
  const [campaigns, setCampaigns] = useState([]);
  const [organizations, setOrganizations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  useEffect(() => {
    const fetchSearchResults = async () => {
      if (!query) return;
      
      try {
        setLoading(true);
        const [postsData, campaignsData, orgsData] = await Promise.all([
          searchPosts(query as string),
          searchCampaigns(query as string),
          searchOrganizations(query as string)
        ]);
        
        setPosts(postsData);
        setCampaigns(campaignsData);
        setOrganizations(orgsData);
        setError('');
      } catch (err) {
        console.error('Failed to fetch search results:', err);
        setError(t('errors.searchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchSearchResults();
  }, [query, t]);
  
  // Get total results count
  const totalResults = posts.length + campaigns.length + organizations.length;
  
  // Filter results based on active tab
  const filteredResults = () => {
    switch (activeTab) {
      case 'posts':
        return posts;
      case 'campaigns':
        return campaigns;
      case 'organizations':
        return organizations;
      default:
        return [...posts, ...campaigns, ...organizations];
    }
  };
  
  return (
    <MainLayout>
      <div className="space-y-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <h1 className="text-2xl font-bold mb-2">
            {t('search.resultsFor', { query })}
          </h1>
          <p className="text-gray-600 mb-6">
            {t('search.foundResults', { count: totalResults })}
          </p>
          
          {/* Tabs */}
          <div className="border-b border-gray-200 mb-6">
            <nav className="-mb-px flex space-x-8">
              <button
                onClick={() => setActiveTab('all')}
                className={`pb-3 border-b-2 font-medium text-sm ${
                  activeTab === 'all'
                    ? 'border-ios-black text-ios-black'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                {t('search.tabs.all')}
                <span className="ml-1 text-xs text-gray-500">({totalResults})</span>
              </button>
              <button
                onClick={() => setActiveTab('posts')}
                className={`pb-3 border-b-2 font-medium text-sm ${
                  activeTab === 'posts'
                    ? 'border-ios-black text-ios-black'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                {t('search.tabs.posts')}
                <span className="ml-1 text-xs text-gray-500">({posts.length})</span>
              </button>
              <button
                onClick={() => setActiveTab('campaigns')}
                className={`pb-3 border-b-2 font-medium text-sm ${
                  activeTab === 'campaigns'
                    ? 'border-ios-black text-ios-black'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                {t('search.tabs.campaigns')}
                <span className="ml-1 text-xs text-gray-500">({campaigns.length})</span>
              </button>
              <button
                onClick={() => setActiveTab('organizations')}
                className={`pb-3 border-b-2 font-medium text-sm ${
                  activeTab === 'organizations'
                    ? 'border-ios-black text-ios-black'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                {t('search.tabs.organizations')}
                <span className="ml-1 text-xs text-gray-500">({organizations.length})</span>
              </button>
            </nav>
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
        ) : filteredResults().length === 0 ? (
          <div className="ios-card p-8 text-center">
            <h3 className="text-xl font-semibold mb-2">{t('search.noResults')}</h3>
            <p className="text-gray-600 mb-4">{t('search.noResultsDescription')}</p>
            <button
              onClick={() => router.push('/')}
              className="ios-button-secondary"
            >
              {t('search.backToHome')}
            </button>
          </div>
        ) : (
          <div className="space-y-6">
            {filteredResults().map((result) => {
              // Determine the type of result and render appropriate component
              if (result.postType) {
                return <PostCard key={`post-${result.id}`} post={result} />;
              } else if (result.goal) {
                return <CampaignCard key={`campaign-${result.id}`} campaign={result} />;
              } else {
                return <OrganizationCard key={`org-${result.id}`} organization={result} />;
              }
            })}
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

export default SearchPage;