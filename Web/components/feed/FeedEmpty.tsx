import React from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { EmptyState } from '../common/EmptyState/EmptyState';
import { Button } from '../common/Button/Button';
import { useAuth } from '../../contexts/AuthContext';

interface FeedEmptyProps {
  onCreatePost?: () => void;
  className?: string;
}

export const FeedEmpty: React.FC<FeedEmptyProps> = ({
  onCreatePost,
  className = '',
}) => {
  const { t } = useTranslation('common');
  const { isAuthenticated } = useAuth();
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className={className}
    >
      <EmptyState
        title={t('feed.noPosts.title')}
        description={t('feed.noPosts.description')}
        icon={
          <svg className="h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1M19 20a2 2 0 002-2V8a2 2 0 00-2-2h-5a2 2 0 00-2 2v12a2 2 0 002 2h5z" />
          </svg>
        }
        action={
          isAuthenticated ? (
            <Button onClick={onCreatePost}>
              {t('feed.createPost')}
            </Button>
          ) : (
            <Button onClick={() => window.location.href = '/auth/login'}>
              {t('auth.login')}
            </Button>
          )
        }
      />
    </motion.div>
  );
};