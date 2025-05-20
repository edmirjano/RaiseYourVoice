import React from 'react';
import { motion } from 'framer-motion';
import { Loader } from './Loader/Loader';

type LoadingIndicatorProps = {
  isLoading: boolean;
  text?: string;
  fullScreen?: boolean;
  overlay?: boolean;
  className?: string;
  size?: 'sm' | 'md' | 'lg';
  type?: 'spinner' | 'dots' | 'pulse';
};

export const LoadingIndicator: React.FC<LoadingIndicatorProps> = ({
  isLoading,
  text,
  fullScreen = false,
  overlay = false,
  className = '',
  size = 'md',
  type = 'spinner',
}) => {
  if (!isLoading) return null;

  const containerClasses = `
    flex flex-col items-center justify-center
    ${fullScreen ? 'fixed inset-0 z-50' : 'w-full h-full'}
    ${overlay ? 'bg-white bg-opacity-80' : ''}
    ${className}
  `;

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      transition={{ duration: 0.2 }}
      className={containerClasses}
    >
      <Loader size={size} type={type} text={text} />
    </motion.div>
  );
};