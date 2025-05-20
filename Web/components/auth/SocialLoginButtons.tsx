import React from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';

interface SocialLoginButtonsProps {
  onGoogleLogin: () => void;
  onAppleLogin: () => void;
}

export const SocialLoginButtons: React.FC<SocialLoginButtonsProps> = ({
  onGoogleLogin,
  onAppleLogin
}) => {
  const { t } = useTranslation('common');
  
  return (
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
        <motion.button
          whileHover={{ scale: 1.03 }}
          whileTap={{ scale: 0.97 }}
          onClick={onGoogleLogin}
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
        </motion.button>

        <motion.button
          whileHover={{ scale: 1.03 }}
          whileTap={{ scale: 0.97 }}
          onClick={onAppleLogin}
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
        </motion.button>
      </div>
    </div>
  );
};