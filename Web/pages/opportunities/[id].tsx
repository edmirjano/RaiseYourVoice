import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import MainLayout from '../../components/MainLayout';
import { CommentSection } from '../../components/feed/CommentSection';
import { getPostById, Post } from '../../services/postService';
import { formatDistanceToNow } from 'date-fns';

const OpportunityDetailPage: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { id } = router.query;
  
  const [opportunity, setOpportunity] = useState<Post | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  useEffect(() => {
    const fetchOpportunity = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        const data = await getPostById(id as string);
        
        // Verify this is an opportunity post
        if (data.postType !== 'Opportunity') {
          router.push('/opportunities');
          return;
        }
        
        setOpportunity(data);
        setError('');
      } catch (err) {
        console.error('Failed to fetch opportunity:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchOpportunity();
  }, [id, router, t]);
  
  const formatDate = (dateString?: string) => {
    if (!dateString) return '';
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch (e) {
      return dateString;
    }
  };
  
  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        {loading ? (
          <div className="flex justify-center py-12">
            <div className="w-12 h-12 border-4 border-ios-black border-t-transparent rounded-full animate-spin"></div>
          </div>
        ) : error ? (
          <div className="ios-card p-6 text-center">
            <p className="text-red-600 mb-4">{error}</p>
            <button
              onClick={() => router.back()}
              className="ios-button-secondary"
            >
              {t('common.goBack')}
            </button>
          </div>
        ) : opportunity ? (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
          >
            {/* Opportunity Header */}
            <div className="ios-card mb-6 overflow-hidden">
              {/* Cover Image */}
              {opportunity.mediaUrls && opportunity.mediaUrls.length > 0 && (
                <div className="relative h-64 md:h-96">
                  <img 
                    src={opportunity.mediaUrls[0]} 
                    alt={opportunity.title} 
                    className="w-full h-full object-cover"
                  />
                </div>
              )}
              
              <div className="p-6">
                <div className="flex items-center mb-4">
                  <div className="h-10 w-10 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 font-semibold">
                    {opportunity.authorId?.charAt(0) || 'U'}
                  </div>
                  <div className="ml-3">
                    <p className="text-sm font-medium text-gray-900">{opportunity.authorId}</p>
                    <p className="text-xs text-gray-500">{formatDate(opportunity.createdAt)}</p>
                  </div>
                </div>
                
                <h1 className="text-3xl font-bold mb-4">{opportunity.title}</h1>
                
                {/* Tags */}
                {opportunity.tags && opportunity.tags.length > 0 && (
                  <div className="flex flex-wrap gap-2 mb-6">
                    {opportunity.tags.map((tag, index) => (
                      <span 
                        key={index} 
                        className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                      >
                        #{tag}
                      </span>
                    ))}
                  </div>
                )}
                
                {/* Key Details */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6 p-4 bg-gray-50 rounded-lg">
                  {opportunity.location && (
                    <div className="flex items-start">
                      <svg className="h-5 w-5 mr-2 text-gray-500 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                      </svg>
                      <div>
                        <div className="text-sm font-medium text-gray-900">{t('opportunities.location')}</div>
                        <div className="text-sm text-gray-600">
                          {opportunity.location.city}, {opportunity.location.country}
                        </div>
                      </div>
                    </div>
                  )}
                  
                  {opportunity.eventDate && (
                    <div className="flex items-start">
                      <svg className="h-5 w-5 mr-2 text-gray-500 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                      <div>
                        <div className="text-sm font-medium text-gray-900">{t('opportunities.date')}</div>
                        <div className="text-sm text-gray-600">
                          {new Date(opportunity.eventDate).toLocaleDateString()} {new Date(opportunity.eventDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                        </div>
                      </div>
                    </div>
                  )}
                </div>
                
                {/* Description */}
                <div className="prose max-w-none mb-6">
                  <h2 className="text-xl font-semibold mb-2">{t('opportunities.description')}</h2>
                  <p className="whitespace-pre-line">{opportunity.content}</p>
                </div>
                
                {/* Action Buttons */}
                <div className="flex flex-wrap gap-4">
                  <button className="ios-button">
                    {t('opportunities.register')}
                  </button>
                  <button className="ios-button-secondary">
                    {t('opportunities.share')}
                  </button>
                  <button className="ios-button-secondary">
                    {t('opportunities.saveToCalendar')}
                  </button>
                </div>
              </div>
            </div>
            
            {/* Comments Section */}
            <div className="ios-card p-6">
              <CommentSection postId={opportunity.id!} />
            </div>
          </motion.div>
        ) : (
          <div className="ios-card p-6 text-center">
            <p className="text-gray-600 mb-4">{t('errors.opportunityNotFound')}</p>
            <button
              onClick={() => router.back()}
              className="ios-button-secondary"
            >
              {t('common.goBack')}
            </button>
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

export default OpportunityDetailPage;