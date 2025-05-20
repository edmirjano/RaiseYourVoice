import React, { useState } from 'react';
import { GetServerSideProps } from 'next';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import { PageLayout } from '../../components/layout';
import { OpportunityForm } from '../../components/opportunities/OpportunityForm';
import { createPost, Post } from '../../services/postService';
import { useAuth } from '../../contexts/AuthContext';
import { AuthGuard } from '../../components/auth/AuthGuard';
import { Alert } from '../../components/common/Alert/Alert';

const CreateOpportunityPage: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (opportunityData: Partial<Post>) => {
    try {
      setIsSubmitting(true);
      setError('');
      
      // Ensure the post type is set to Opportunity
      const postData: Post = {
        ...opportunityData as Post,
        postType: 'Opportunity'
      };
      
      const createdOpportunity = await createPost(postData);
      
      // Redirect to the newly created opportunity
      router.push(`/opportunities/${createdOpportunity.id}`);
    } catch (err) {
      console.error('Failed to create opportunity:', err);
      setError(t('errors.postFailed'));
      setIsSubmitting(false);
    }
  };

  return (
    <AuthGuard>
      <PageLayout title={`${t('opportunities.createOpportunity')} | RaiseYourVoice`}>
        <div className="max-w-3xl mx-auto">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
          >
            <h1 className="text-2xl font-bold mb-6">{t('opportunities.createOpportunity')}</h1>
            
            {error && (
              <Alert 
                variant="error" 
                className="mb-6"
                onClose={() => setError('')}
              >
                {error}
              </Alert>
            )}
            
            <OpportunityForm 
              onSubmit={handleSubmit}
              isSubmitting={isSubmitting}
            />
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

export default CreateOpportunityPage;