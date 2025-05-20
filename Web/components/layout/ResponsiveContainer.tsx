import React from 'react';

type ContainerSize = 'sm' | 'md' | 'lg' | 'xl' | 'full';

type ResponsiveContainerProps = {
  children: React.ReactNode;
  size?: ContainerSize;
  className?: string;
  padding?: boolean;
};

export const ResponsiveContainer: React.FC<ResponsiveContainerProps> = ({
  children,
  size = 'lg',
  className = '',
  padding = true,
}) => {
  const sizeClasses = {
    sm: 'max-w-3xl',
    md: 'max-w-4xl',
    lg: 'max-w-6xl',
    xl: 'max-w-7xl',
    full: 'max-w-full',
  };

  return (
    <div
      className={`mx-auto ${sizeClasses[size]} ${
        padding ? 'px-4 sm:px-6 lg:px-8' : ''
      } ${className}`}
    >
      {children}
    </div>
  );
};