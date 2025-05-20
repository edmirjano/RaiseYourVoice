import React from 'react';
import { motion } from 'framer-motion';
import { Alert } from './Alert/Alert';

type ApiErrorDisplayProps = {
  error: string | null;
  onRetry?: () => void;
  className?: string;
};

export const ApiErrorDisplay: React.FC<ApiErrorDisplayProps> = ({
  error,
  onRetry,
  className = '',
}) => {
  if (!error) return null;

  return (
    <motion.div
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -10 }}
      className={className}
    >
      <Alert
        variant="error"
        title="Error"
        onClose={onRetry ? undefined : () => {}}
      >
        <div className="flex flex-col">
          <p>{error}</p>
          {onRetry && (
            <button
              onClick={onRetry}
              className="mt-2 self-end text-sm font-medium text-red-700 hover:text-red-800"
            >
              Try Again
            </button>
          )}
        </div>
      </Alert>
    </motion.div>
  );
};