import React, { useState, useEffect } from 'react';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';

import { useAuth } from '../../contexts/AuthContext';

const ResetPassword: NextPage = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { resetPassword } = useAuth();
  
  const [token, setToken] = useState<string>('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  // Get token from URL query parameter
  useEffect(() => {
    if (router.query.token) {
      setToken(router.query.token as string);
    }
  }, [router.query]);
  
  const validatePasswords = () => {
    // Check if passwords match
    if (password !== confirmPassword) {
      setError(t('auth.passwordsDoNotMatch'));
      return false;
    }
    
    // Check password strength (min 8 chars, at least one number and one letter)
    const passwordRegex = /^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*#?&]{8,}$/;
    if (!passwordRegex.test(password)) {
      setError(t('auth.weakPassword'));
      return false;
    }
    
    return true;
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    
    if (!validatePasswords()) {
      return;
    }
    
    setIsSubmitting(true);
    
    try {
      await resetPassword(token, password);
      setSuccess(true);
    } catch (error: any) {
      if (error.response && error.response.status === 400) {
        setError(t('auth.invalidOrExpiredToken'));
      } else {
        setError(t('auth.resetPasswordError'));
      }
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
          <p className="text-gray-600">{t('auth.resetPasswordTitle')}</p>
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
            <h2 className="text-xl font-medium text-gray-800 mb-2">{t('auth.passwordResetSuccess')}</h2>
            <p className="text-gray-600 mb-6">
              {t('auth.passwordResetSuccessMessage')}
            </p>
            <Link href="/auth/login" className="ios-button inline-block">
              {t('auth.loginNow')}
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
            
            {!token && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                className="mb-4 p-3 bg-yellow-100 text-yellow-700 rounded-lg text-center"
              >
                {t('auth.noResetToken')}
              </motion.div>
            )}
            
            <form onSubmit={handleSubmit} className="space-y-6">
              <div>
                <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
                  {t('auth.newPasswordLabel')}
                </label>
                <input
                  id="password"
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  className="ios-input w-full"
                  placeholder={t('auth.newPasswordPlaceholder')}
                  autoComplete="new-password"
                  minLength={8}
                  disabled={!token || isSubmitting}
                />
                <p className="text-xs text-gray-500 mt-1">
                  {t('auth.passwordRequirements')}
                </p>
              </div>
              
              <div>
                <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700 mb-1">
                  {t('auth.confirmNewPasswordLabel')}
                </label>
                <input
                  id="confirmPassword"
                  type="password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  required
                  className="ios-input w-full"
                  placeholder={t('auth.confirmNewPasswordPlaceholder')}
                  autoComplete="new-password"
                  disabled={!token || isSubmitting}
                />
              </div>
              
              <button
                type="submit"
                disabled={!token || isSubmitting}
                className="ios-button w-full flex justify-center"
              >
                {isSubmitting ? (
                  <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></div>
                ) : null}
                {t('auth.resetPasswordButton')}
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

export const getServerSideProps = async ({ locale }: { locale: string }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale, ['common'])),
    },
  };
};

export default ResetPassword;