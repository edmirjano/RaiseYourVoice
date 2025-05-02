import React, { useEffect, useState } from 'react';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { verifyEmail } from '../../services/authService';

const VerifyEmail: NextPage = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const [status, setStatus] = useState<'verifying' | 'success' | 'error'>('verifying');
  const [error, setError] = useState<string | null>(null);
  
  useEffect(() => {
    const verifyEmailToken = async () => {
      // Get token from URL query parameters
      const { token } = router.query;
      
      if (!token) {
        setStatus('error');
        setError(t('auth.missingToken'));
        return;
      }
      
      try {
        await verifyEmail(token as string);
        setStatus('success');
      } catch (error: any) {
        console.error('Email verification error:', error);
        setStatus('error');
        
        if (error.response && error.response.status === 400) {
          setError(t('auth.invalidVerificationToken'));
        } else {
          setError(t('auth.verificationError'));
        }
      }
    };
    
    if (router.isReady && router.query.token) {
      verifyEmailToken();
    }
  }, [router.isReady, router.query, t]);
  
  return (
    <div className="min-h-screen flex flex-col items-center justify-center px-4 bg-white">
      <div className="w-full max-w-md text-center">
        <div className="mb-10">
          <Link href="/">
            <motion.h1
              whileHover={{ scale: 1.02 }}
              className="text-3xl font-bold mb-2 text-ios-black"
            >
              RaiseYourVoice
            </motion.h1>
          </Link>
        </div>
        
        {status === 'verifying' && (
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            className="bg-white p-8 rounded-xl shadow-sm"
          >
            <div className="w-16 h-16 mx-auto mb-4">
              <div className="w-full h-full border-4 border-ios-black border-t-transparent rounded-full animate-spin"></div>
            </div>
            <h2 className="text-xl font-semibold mb-2">{t('auth.verifyingEmail')}</h2>
            <p className="text-gray-600">{t('auth.pleaseWait')}</p>
          </motion.div>
        )}
        
        {status === 'success' && (
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            className="bg-white p-8 rounded-xl shadow-sm"
          >
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <h2 className="text-xl font-semibold mb-2">{t('auth.emailVerified')}</h2>
            <p className="text-gray-600 mb-6">{t('auth.emailVerifiedDescription')}</p>
            <Link href="/auth/login" className="ios-button inline-block">
              {t('auth.loginNow')}
            </Link>
          </motion.div>
        )}
        
        {status === 'error' && (
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            className="bg-white p-8 rounded-xl shadow-sm"
          >
            <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </div>
            <h2 className="text-xl font-semibold mb-2">{t('auth.verificationFailed')}</h2>
            <p className="text-gray-600 mb-6">{error || t('auth.verificationError')}</p>
            <div className="flex flex-col space-y-3 sm:flex-row sm:space-y-0 sm:space-x-3 justify-center">
              <Link href="/auth/login" className="ios-button inline-block">
                {t('auth.backToLogin')}
              </Link>
              <button 
                onClick={() => router.reload()}
                className="ios-button-secondary"
              >
                {t('auth.tryAgain')}
              </button>
            </div>
          </motion.div>
        )}
      </div>
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

export default VerifyEmail;