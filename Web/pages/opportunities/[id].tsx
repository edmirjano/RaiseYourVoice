import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import { PageLayout } from '../../components/layout';
import { OpportunityDetails } from '../../components/opportunities/OpportunityDetails';
import { CommentSection } from '../../components/feed/CommentSection';
import { getPostById, Post } from '../../services/postService';
import { Loader } from '../../components/common/Loader/Loader';
import { Alert } from '../../components/common/Alert/Alert';
import { Button } from '../../components/common/Button/Button';

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
  
  return (
    <PageLayout title={opportunity ? `${opportunity.title} | ${t('opportunities.title')}` : t('opportunities.title')}>
      <div className="max-w-4xl mx-auto">
        {loading ? (
          <div className="flex justify-center py-12">
            <Loader size="lg" type="spinner" text={t('common.loading')} />
          </div>
        ) : error ? (
          <Alert 
            variant="error" 
            className="mb-6"
            onClose={() => setError('')}
          >
            <div className="flex flex-col">
              <p>{error}</p>
              <Button
                variant="secondary"
                size="sm"
                className="self-end mt-2"
                onClick={() => router.back()}
              >
                {t('common.goBack')}
              </Button>
            </div>
          </Alert>
        ) : opportunity ? (
          <>
            <OpportunityDetails opportunity={opportunity} />
            
            {/* Comments Section */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.3, delay: 0.2 }}
              className="ios-card p-6 mt-6"
            >
              <CommentSection postId={opportunity.id!} />
            </motion.div>
          </>
        ) : (
          <div className="ios-card p-6 text-center">
            <p className="text-gray-600 mb-4">{t('errors.opportunityNotFound')}</p>
            <Button
              variant="secondary"
              onClick={() => router.push('/opportunities')}
            >
              {t('common.goBack')}
            </Button>
          </div>
        )}
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

export default OpportunityDetailPage;