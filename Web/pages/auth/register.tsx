import React, { useState, useEffect } from 'react';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';

import { useAuth } from '../../contexts/AuthContext';

const Register: NextPage = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { register: signUp, isAuthenticated } = useAuth();
  
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      router.push('/');
    }
  }, [isAuthenticated, router]);
  
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };
  
  const validateForm = () => {
    // Check if passwords match
    if (formData.password !== formData.confirmPassword) {
      setError(t('auth.passwordsDoNotMatch'));
      return false;
    }
    
    // Check password strength (min 8 chars, at least one number and one letter)
    const passwordRegex = /^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*#?&]{8,}$/;
    if (!passwordRegex.test(formData.password)) {
      setError(t('auth.weakPassword'));
      return false;
    }
    
    // Check email format
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      setError(t('auth.invalidEmail'));
      return false;
    }
    
    return true;
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    
    // Validate form
    if (!validateForm()) {
      return;
    }
    
    setIsSubmitting(true);
    
    try {
      await signUp({
        name: formData.name,
        email: formData.email,
        password: formData.password,
        preferredLanguage: router.locale || 'en',
      });
      
      // Redirect to home page after successful registration
      router.push('/');
    } catch (error: any) {
      if (error.response && error.response.status === 409) {
        setError(t('auth.emailAlreadyExists'));
      } else {
        setError(t('auth.registrationError'));
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
      className="min-h-screen flex flex-col justify-center items-center px-4 py-12 bg-white"
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
          <p className="text-gray-600">{t('auth.registerSubtitle')}</p>
        </div>
        
        {error && (
          <motion.div 
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            className="mb-4 p-3 bg-red-100 text-red-700 rounded-lg text-center"
          >
            {error}
          </motion.div>
        )}
        
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.nameLabel')}
            </label>
            <input
              id="name"
              name="name"
              type="text"
              value={formData.name}
              onChange={handleChange}
              required
              className="ios-input w-full"
              placeholder={t('auth.namePlaceholder')}
              autoComplete="name"
            />
          </div>
          
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.emailLabel')}
            </label>
            <input
              id="email"
              name="email"
              type="email"
              value={formData.email}
              onChange={handleChange}
              required
              className="ios-input w-full"
              placeholder={t('auth.emailPlaceholder')}
              autoComplete="email"
            />
          </div>
          
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.passwordLabel')}
            </label>
            <input
              id="password"
              name="password"
              type="password"
              value={formData.password}
              onChange={handleChange}
              required
              className="ios-input w-full"
              placeholder={t('auth.passwordPlaceholder')}
              autoComplete="new-password"
              minLength={8}
            />
            <p className="text-xs text-gray-500 mt-1">
              {t('auth.passwordRequirements')}
            </p>
          </div>
          
          <div>
            <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700 mb-1">
              {t('auth.confirmPasswordLabel')}
            </label>
            <input
              id="confirmPassword"
              name="confirmPassword"
              type="password"
              value={formData.confirmPassword}
              onChange={handleChange}
              required
              className="ios-input w-full"
              placeholder={t('auth.confirmPasswordPlaceholder')}
              autoComplete="new-password"
            />
          </div>
          
          <div className="mt-8">
            <button
              type="submit"
              disabled={isSubmitting}
              className="ios-button w-full flex justify-center"
            >
              {isSubmitting ? (
                <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></div>
              ) : null}
              {t('auth.registerButton')}
            </button>
          </div>
        </form>
        
        <div className="mt-8 text-center">
          <p className="text-gray-600">
            {t('auth.alreadyHaveAccount')}{' '}
            <Link href="/auth/login" className="text-ios-black font-medium hover:underline">
              {t('auth.loginLink')}
            </Link>
          </p>
        </div>
        
        <div className="mt-4 text-xs text-gray-500 text-center">
          <p>
            {t('auth.termsDisclaimer')}{' '}
            <Link href="/terms" className="text-ios-black hover:underline">
              {t('auth.termsLink')}
            </Link>{' '}
            {t('auth.andWord')}{' '}
            <Link href="/privacy" className="text-ios-black hover:underline">
              {t('auth.privacyLink')}
            </Link>
          </p>
        </div>
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

export default Register;