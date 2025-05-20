import React, { useState } from 'react';
import { GetServerSideProps } from 'next';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import { PageLayout } from '../../components/layout';
import { SuccessStoryForm } from '../../components/success-stories/SuccessStoryForm';
import { createPost, Post } from '../../services/postService';
import { useAuth } from '../../contexts/AuthContext';
import { AuthGuard } from '../../components/auth/AuthGuard';
import { Alert } from '../../components/common/Alert/Alert';

const CreateSuccessStoryPage: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (storyData: Partial<Post>) => {
    try {
      setIsSubmitting(true);
      setError('');
      
      // Ensure the post type is set to SuccessStory
      const postData: Post = {
        ...storyData as Post,
        postType: 'SuccessStory'
      };
      
      const createdStory = await createPost(postData);
      
      // Redirect to the newly created story
      router.push(`/success-stories/${createdStory.id}`);
    } catch (err) {
      console.error('Failed to create success story:', err);
      setError(t('errors.postFailed'));
      setIsSubmitting(false);
    }
  };

  return (
    <AuthGuard>
      <PageLayout title={`${t('successStories.createStory')} | RaiseYourVoice`}>
        <div className="max-w-3xl mx-auto">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
          >
            <h1 className="text-2xl font-bold mb-6">{t('successStories.createStory')}</h1>
            
            {error && (
              <Alert 
                variant="error" 
                className="mb-6"
                onClose={() => setError('')}
              >
                {error}
              </Alert>
            )}
            
            <SuccessStoryForm 
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

export default CreateSuccessStoryPage;