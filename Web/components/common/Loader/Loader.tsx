import React from 'react';
import { motion } from 'framer-motion';

type LoaderSize = 'sm' | 'md' | 'lg';
type LoaderType = 'spinner' | 'dots' | 'pulse';

type LoaderProps = {
  size?: LoaderSize;
  type?: LoaderType;
  className?: string;
  text?: string;
};

export const Loader: React.FC<LoaderProps> = ({
  size = 'md',
  type = 'spinner',
  className = '',
  text,
}) => {
  const sizeClasses = {
    sm: 'h-4 w-4',
    md: 'h-8 w-8',
    lg: 'h-12 w-12',
  };

  const renderLoader = () => {
    switch (type) {
      case 'spinner':
        return (
          <div
            className={`border-t-transparent rounded-full animate-spin border-2 border-ios-black ${sizeClasses[size]} ${className}`}
          ></div>
        );
      case 'dots':
        return (
          <div className={`flex space-x-1 ${className}`}>
            {[0, 1, 2].map((i) => (
              <motion.div
                key={i}
                className={`bg-ios-black rounded-full ${
                  size === 'sm' ? 'h-1.5 w-1.5' : size === 'md' ? 'h-2 w-2' : 'h-3 w-3'
                }`}
                animate={{ y: [0, -6, 0] }}
                transition={{
                  duration: 0.6,
                  repeat: Infinity,
                  delay: i * 0.1,
                  ease: 'easeInOut',
                }}
              />
            ))}
          </div>
        );
      case 'pulse':
        return (
          <motion.div
            className={`bg-ios-black rounded-full ${sizeClasses[size]} ${className}`}
            animate={{ scale: [1, 1.2, 1] }}
            transition={{ duration: 1.5, repeat: Infinity }}
          />
        );
      default:
        return null;
    }
  };

  return (
    <div className="flex flex-col items-center justify-center">
      {renderLoader()}
      {text && <p className="mt-2 text-sm text-gray-600">{text}</p>}
    </div>
  );
};