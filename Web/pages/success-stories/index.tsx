import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import { PageLayout } from '../../components/layout';
import { SuccessStoryCard } from '../../components/success-stories/SuccessStoryCard';
import { ActivistOfTheMonthCard } from '../../components/success-stories/ActivistOfTheMonthCard';
import { OrganizationSpotlight } from '../../components/success-stories/OrganizationSpotlight';
import { getSuccessStories, Post } from '../../services/postService';
import { getVerifiedOrganizations, Organization } from '../../services/organizationService';
import { getFeaturedActivist, Activist } from '../../services/activistService';
import { useAuth } from '../../contexts/AuthContext';
import { Button } from '../../components/common/Button/Button';
import { Loader } from '../../components/common/Loader/Loader';
import { EmptyState } from '../../components/common/EmptyState/EmptyState';
import { Alert } from '../../components/common/Alert/Alert';
import { useInfiniteScroll } from '../../hooks/useInfiniteScroll';

const SuccessStoriesPage: React.FC = () => {
  const { t } = useTranslation('common');
  const { isAuthenticated } = useAuth();
  const [featuredActivist, setFeaturedActivist] = useState<Activist | null>(null);
  const [spotlightOrganizations, setSpotlightOrganizations] = useState<Organization[]>([]);
  const [loadingFeatured, setLoadingFeatured] = useState(true);
  const [featuredError, setFeaturedError] = useState('');

  // Use the infinite scroll hook to load success stories
  const fetchStories = async (page: number) => {
    const data = await getSuccessStories(page);
    return data;
  };

  const {
    items: stories,
    loading,
    error,
    hasMore,
    lastItemRef,
    loadMore
  } = useInfiniteScroll<Post>(fetchStories);

  // Fetch featured activist and spotlight organizations
  useEffect(() => {
    const fetchFeaturedContent = async () => {
      try {
        setLoadingFeatured(true);
        const [activistData, organizationsData] = await Promise.all([
          getFeaturedActivist(),
          getVerifiedOrganizations(1, 3)
        ]);
        
        setFeaturedActivist(activistData);
        setSpotlightOrganizations(organizationsData);
        setFeaturedError('');
      } catch (err) {
        console.error('Failed to fetch featured content:', err);
        setFeaturedError(t('errors.fetchFailed'));
      } finally {
        setLoadingFeatured(false);
      }
    };

    fetchFeaturedContent();
  }, [t]);

  const handleCreateStory = () => {
    window.location.href = '/success-stories/create';
  };

  return (
    <PageLayout title={`${t('successStories.title')} | RaiseYourVoice`}>
      <div className="max-w-7xl mx-auto">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
            <div>
              <h1 className="text-2xl font-bold mb-2">{t('successStories.title')}</h1>
              <p className="text-gray-600">{t('successStories.description')}</p>
            </div>
            
            {isAuthenticated && (
              <Button 
                onClick={handleCreateStory}
                className="mt-4 md:mt-0"
              >
                {t('successStories.createStory')}
              </Button>
            )}
          </div>
          
          {/* Featured Content Section */}
          {(loadingFeatured && !featuredActivist && spotlightOrganizations.length === 0) ? (
            <div className="flex justify-center py-8">
              <Loader size="lg" type="spinner" text={t('common.loading')} />
            </div>
          ) : featuredError ? (
            <Alert 
              variant="error" 
              className="mb-6"
              onClose={() => setFeaturedError('')}
            >
              {featuredError}
            </Alert>
          ) : (
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-12">
              {/* Activist of the Month */}
              {featuredActivist && (
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ duration: 0.3, delay: 0.1 }}
                  className="lg:col-span-2"
                >
                  <ActivistOfTheMonthCard activist={featuredActivist} />
                </motion.div>
              )}
              
              {/* Organization Spotlight */}
              {spotlightOrganizations.length > 0 && (
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ duration: 0.3, delay: 0.2 }}
                >
                  <OrganizationSpotlight organizations={spotlightOrganizations} />
                </motion.div>
              )}
            </div>
          )}
          
          {/* Success Stories Section */}
          <div>
            <h2 className="text-xl font-semibold mb-6">{t('successStories.latestStories')}</h2>
            
            {error && (
              <Alert 
                variant="error" 
                className="mb-6"
                onClose={() => {}}
              >
                {error}
              </Alert>
            )}
            
            {loading && stories.length === 0 ? (
              <div className="flex justify-center py-12">
                <Loader size="lg" type="spinner" text={t('common.loading')} />
              </div>
            ) : stories.length === 0 ? (
              <EmptyState
                title={t('successStories.noStories.title')}
                description={t('successStories.noStories.description')}
                icon={
                  <svg className="h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1M19 20a2 2 0 002-2V8a2 2 0 00-2-2h-5a2 2 0 00-2 2v12a2 2 0 002 2h5z" />
                  </svg>
                }
                action={
                  isAuthenticated ? (
                    <Button onClick={handleCreateStory}>
                      {t('successStories.createStory')}
                    </Button>
                  ) : (
                    <Button onClick={() => window.location.href = '/auth/login'}>
                      {t('auth.login')}
                    </Button>
                  )
                }
              />
            ) : (
              <div className="space-y-8">
                {stories.map((story, index) => {
                  // Add ref to the last item for infinite scrolling
                  const isLastItem = index === stories.length - 1;
                  
                  return (
                    <div key={story.id || index} ref={isLastItem ? lastItemRef : undefined}>
                      <SuccessStoryCard story={story} />
                    </div>
                  );
                })}
                
                {/* Loading indicator for infinite scroll */}
                {loading && stories.length > 0 && (
                  <div className="py-6 flex justify-center">
                    <Loader size="md" type="spinner" />
                  </div>
                )}
                
                {/* Load more button as fallback */}
                {!loading && hasMore && (
                  <div className="py-6 flex justify-center">
                    <Button 
                      variant="secondary" 
                      onClick={loadMore}
                    >
                      {t('feed.loadMore')}
                    </Button>
                  </div>
                )}
              </div>
            )}
          </div>
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

export default SuccessStoriesPage;