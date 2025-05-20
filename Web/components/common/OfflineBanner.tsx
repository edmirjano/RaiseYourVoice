import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useOffline } from '../../contexts/OfflineContext';

type OfflineBannerProps = {
  className?: string;
  message?: string;
};

export const OfflineBanner: React.FC<OfflineBannerProps> = ({
  className = '',
  message = 'You are currently offline. Some features may be unavailable.',
}) => {
  const { isOffline } = useOffline();
  
  return (
    <AnimatePresence>
      {isOffline && (
        <motion.div
          initial={{ opacity: 0, y: -50 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -50 }}
          transition={{ duration: 0.3 }}
          className={`fixed top-0 inset-x-0 bg-red-500 text-white py-2 px-4 text-center z-50 ${className}`}
        >
          <div className="flex items-center justify-center">
            <svg
              className="h-5 w-5 mr-2"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
              />
            </svg>
            <span>{message}</span>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};