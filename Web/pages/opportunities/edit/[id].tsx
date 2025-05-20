import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import { PageLayout } from '../../../components/layout';
import { OpportunityForm } from '../../../components/opportunities/OpportunityForm';
import { getPostById, updatePost, Post } from '../../../services/postService';
import { useAuth } from '../../../contexts/AuthContext';
import { AuthGuard } from '../../../components/auth/AuthGuard';
import { Alert } from '../../../components/common/Alert/Alert';
import { Loader } from '../../../components/common/Loader/Loader';
import { Button } from '../../../components/common/Button/Button';

const EditOpportunityPage: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { id } = router.query;
  const { user } = useAuth();
  
  const [opportunity, setOpportunity] = useState<Post | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  
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
        
        // Verify user has permission to edit
        if (user?.id !== data.authorId && user?.role !== 'Admin' && user?.role !== 'Moderator') {
          router.push('/access-denied');
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
  }, [id, router, t, user]);
  
  const handleSubmit = async (opportunityData: Partial<Post>) => {
    if (!opportunity?.id) return;
    
    try {
      setIsSubmitting(true);
      setError('');
      
      // Ensure the post type is set to Opportunity
      const postData: Post = {
        ...opportunity,
        ...opportunityData as Post,
        postType: 'Opportunity',
        id: opportunity.id
      };
      
      await updatePost(opportunity.id, postData);
      
      // Redirect to the updated opportunity
      router.push(`/opportunities/${opportunity.id}`);
    } catch (err) {
      console.error('Failed to update opportunity:', err);
      setError(t('errors.updateFailed'));
      setIsSubmitting(false);
    }
  };
  
  return (
    <AuthGuard>
      <PageLayout title={`${t('opportunities.editOpportunity')} | RaiseYourVoice`}>
        <div className="max-w-3xl mx-auto">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
          >
            <div className="flex justify-between items-center mb-6">
              <h1 className="text-2xl font-bold">{t('opportunities.editOpportunity')}</h1>
              <Button
                variant="secondary"
                onClick={() => router.back()}
              >
                {t('common.cancel')}
              </Button>
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
            ) : opportunity ? (
              <OpportunityForm 
                initialData={opportunity}
                onSubmit={handleSubmit}
                isSubmitting={isSubmitting}
              />
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
          </motion.div>
        </div>
      </PageLayout>
    </AuthGuard>
  );
};

export const getServerSideProps: GetServerSideProps = async ({ locale }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale || 'en', ['common'])),
    },
  };
};

export default EditOpportunityPage;