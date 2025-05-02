import React, { useEffect, useState } from 'react';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';

import { handleOAuthCallback } from '../../services/authService';

const OAuthCallback: NextPage = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  
  useEffect(() => {
    const processCallback = async () => {
      // Get the token from the URL query parameters
      const { token } = router.query;
      
      if (!token) {
        setError(t('auth.oauthMissingToken'));
        return;
      }
      
      try {
        // Process the OAuth callback
        await handleOAuthCallback(token as string);
        
        // Redirect to home page or the page user was trying to access
        const returnUrl = localStorage.getItem('returnUrl') || '/';
        localStorage.removeItem('returnUrl');
        router.push(returnUrl);
      } catch (error) {
        console.error('OAuth callback error:', error);
        setError(t('auth.oauthError'));
      }
    };
    
    // Only process after the router is ready and we have query params
    if (router.isReady && router.query.token) {
      processCallback();
    }
  }, [router, t]);
  
  return (
    <div className="min-h-screen flex items-center justify-center px-4 bg-white">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.3 }}
        className="text-center max-w-md w-full p-8"
      >
        {error ? (
          <div className="space-y-6">
            <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </div>
            <h2 className="text-xl font-semibold text-gray-800">{t('auth.oauthErrorTitle')}</h2>
            <p className="text-gray-600">{error}</p>
            <button
              onClick={() => router.push('/auth/login')}
              className="ios-button w-full"
            >
              {t('auth.backToLogin')}
            </button>
          </div>
        ) : (
          <div className="space-y-6">
            <div className="w-12 h-12 border-4 border-ios-black border-t-transparent rounded-full animate-spin mx-auto"></div>
            <h2 className="text-xl font-semibold text-gray-800">{t('auth.processingLogin')}</h2>
            <p className="text-gray-600">{t('auth.pleaseWait')}</p>
          </div>
        )}
      </motion.div>
    </div>
  );
};

export const getServerSideProps = async ({ locale }: { locale: string }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale, ['common'])),
    },
  };
};

export default OAuthCallback;