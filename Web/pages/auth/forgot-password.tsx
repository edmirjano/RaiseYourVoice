import React, { useState } from 'react';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';

import { useAuth } from '../../contexts/AuthContext';

const ForgotPassword: NextPage = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { forgotPassword } = useAuth();
  
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    
    try {
      await forgotPassword(email);
      setSuccess(true);
    } catch (error: any) {
      // Still show success even if email doesn't exist for security reasons
      // (don't want to reveal which emails are registered)
      setSuccess(true);
    } finally {
      setIsSubmitting(false);
    }
  };
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      transition={{ duration: 0.3 }}
      className="min-h-screen flex flex-col justify-center items-center px-4 bg-white"
    >
      <div className="w-full max-w-md">
        <div className="text-center mb-10">
          <Link href="/">
            <motion.h1
              whileHover={{ scale: 1.02 }}
              className="text-3xl font-bold mb-2 text-ios-black"
            >
              RaiseYourVoice
            </motion.h1>
          </Link>
          <p className="text-gray-600">{t('auth.forgotPasswordTitle')}</p>
        </div>
        
        {success ? (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-green-50 p-6 rounded-xl text-center"
          >
            <div className="mb-4 flex justify-center">
              <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                </svg>
              </div>
            </div>
            <h2 className="text-xl font-medium text-gray-800 mb-2">{t('auth.emailSent')}</h2>
            <p className="text-gray-600 mb-6">
              {t('auth.checkEmailForReset')}
            </p>
            <Link href="/auth/login" className="ios-button inline-block">
              {t('auth.backToLogin')}
            </Link>
          </motion.div>
        ) : (
          <>
            {error && (
              <motion.div 
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                className="mb-4 p-3 bg-red-100 text-red-700 rounded-lg text-center"
              >
                {error}
              </motion.div>
            )}
            
            <p className="text-gray-600 mb-6">
              {t('auth.forgotPasswordInstructions')}
            </p>
            
            <form onSubmit={handleSubmit} className="space-y-6">
              <div>
                <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
                  {t('auth.emailLabel')}
                </label>
                <input
                  id="email"
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  className="ios-input w-full"
                  placeholder={t('auth.emailPlaceholder')}
                  autoComplete="email"
                />
              </div>
              
              <button
                type="submit"
                disabled={isSubmitting}
                className="ios-button w-full flex justify-center"
              >
                {isSubmitting ? (
                  <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></div>
                ) : null}
                {t('auth.sendResetLink')}
              </button>
            </form>
            
            <div className="mt-6 text-center">
              <Link 
                href="/auth/login"
                className="text-ios-black hover:underline text-sm font-medium"
              >
                {t('auth.backToLogin')}
              </Link>
            </div>
          </>
        )}
      </div>
    </motion.div>
  );
};

export const getStaticProps = async ({ locale }: { locale: string }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale, ['common'])),
    },
  };
};

export default ForgotPassword;