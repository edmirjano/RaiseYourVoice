import React, { useState } from 'react';
import { useRouter } from 'next/router';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { useAuth } from '../../contexts/AuthContext';

export const LoginForm: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { login, loginWithGoogle, loginWithApple } = useAuth();
  
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    
    try {
      await login({ email, password });
      const returnUrl = router.query.returnUrl as string || '/feed';
      router.push(returnUrl);
    } catch (error: any) {
      if (error.response && error.response.status === 401) {
        setError(t('auth.invalidCredentials'));
      } else {
        setError(t('auth.loginError'));
      }
    } finally {
      setIsSubmitting(false);
    }
  };
  
  const handleGoogleLogin = async () => {
    try {
      await loginWithGoogle();
    } catch (error) {
      setError(t('auth.socialLoginError'));
    }
  };
  
  const handleAppleLogin = async () => {
    try {
      await loginWithApple();
    } catch (error) {
      setError(t('auth.socialLoginError'));
    }
  };
  
  return (
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
        <p className="text-gray-600">{t('auth.login.subtitle')}</p>
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
      
      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
            {t('auth.login.emailLabel')}
          </label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            className="ios-input w-full"
            placeholder="you@example.com"
            autoComplete="email"
          />
        </div>
        
        <div>
          <div className="flex justify-between items-center mb-1">
            <label htmlFor="password" className="block text-sm font-medium text-gray-700">
              {t('auth.login.passwordLabel')}
            </label>
            <Link
              href="/auth/forgot-password"
              className="text-sm text-ios-black hover:underline"
            >
              {t('auth.login.forgotPassword')}
            </Link>
          </div>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            className="ios-input w-full"
            placeholder="••••••••"
            autoComplete="current-password"
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
          {t('auth.login.loginButton')}
        </button>
      </form>
      
      <div className="mt-6">
        <div className="relative">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-gray-300"></div>
          </div>
          <div className="relative flex justify-center text-sm">
            <span className="px-2 bg-white text-gray-500">
              {t('auth.orContinueWith')}
            </span>
          </div>
        </div>
        
        <div className="mt-6 grid grid-cols-2 gap-3">
          <button
            onClick={handleGoogleLogin}
            className="ios-button-secondary flex justify-center items-center w-full"
            type="button"
          >
            <span className="sr-only">Google</span>
            <svg 
              xmlns="http://www.w3.org/2000/svg" 
              width="18" 
              height="18" 
              viewBox="0 0 18 18" 
              className="mr-2"
            >
              <path 
                fill="#4285F4" 
                d="M17.64 9.2c0-.637-.057-1.251-.164-1.84H9v3.481h4.844c-.209 1.125-.843 2.078-1.796 2.717v2.258h2.908c1.702-1.567 2.684-3.874 2.684-6.615z"
              />
              <path 
                fill="#34A853" 
                d="M9 18c2.43 0 4.467-.806 5.956-2.18l-2.908-2.259c-.806.54-1.837.86-3.048.86-2.344 0-4.328-1.584-5.036-3.711H.957v2.332A8.997 8.997 0 0 0 9 18z"
              />
              <path 
                fill="#FBBC05" 
                d="M3.964 10.71A5.41 5.41 0 0 1 3.682 9c0-.593.102-1.17.282-1.71V4.958H.957A8.996 8.996 0 0 0 0 9c0 1.452.348 2.827.957 4.042l3.007-2.332z"
              />
              <path 
                fill="#EA4335" 
                d="M9 3.58c1.321 0 2.508.454 3.44 1.345l2.582-2.58C13.463.891 11.426 0 9 0A8.997 8.997 0 0 0 .957 4.958L3.964 7.29C4.672 5.163 6.656 3.58 9 3.58z"
              />
            </svg>
            Google
          </button>

          <button
            onClick={handleAppleLogin}
            className="ios-button-secondary flex justify-center items-center w-full"
            type="button"
          >
            <span className="sr-only">Apple</span>
            <svg 
              xmlns="http://www.w3.org/2000/svg" 
              width="18" 
              height="18" 
              viewBox="0 0 18 18" 
              className="mr-2"
            >
              <path 
                d="M14.722 9.506c-.023-2.5 2.046-3.704 2.138-3.765-1.16-1.7-2.971-1.932-3.622-1.958-1.544-.157-3.012.905-3.796.905-.78 0-1.99-.883-3.268-.86-1.68.025-3.233.978-4.097 2.488-1.752 3.015-.447 7.483 1.255 9.93.834 1.202 1.818 2.55 3.12 2.5 1.25-.05 1.726-.805 3.24-.805 1.515 0 1.948.805 3.26.782 1.352-.023 2.206-1.215 3.035-2.425.951-1.397 1.341-2.747 1.368-2.818-.03-.013-2.624-1.005-2.65-3.988l.017.014zm-2.475-7.33c.692-.833 1.154-1.995 1.03-3.152-.995.04-2.197.663-2.91 1.495-.638.73-1.198 1.9-1.045 3.024 1.104.088 2.234-.574 2.926-1.366"
                fill="#000"
              />
            </svg>
            Apple
          </button>
        </div>
      </div>
      
      <div className="mt-8 text-center">
        <p className="text-gray-600">
          {t('auth.login.noAccount')}{' '}
          <Link href="/auth/register" className="text-ios-black font-medium hover:underline">
            {t('auth.login.register')}
          </Link>
        </p>
      </div>
    </div>
  );
};