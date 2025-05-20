import React from 'react';

type SkeletonType = 'text' | 'circle' | 'rectangle' | 'card' | 'avatar' | 'button';

type SkeletonProps = {
  type?: SkeletonType;
  width?: string | number;
  height?: string | number;
  className?: string;
  count?: number;
  circle?: boolean;
  rounded?: boolean;
};

export const Skeleton: React.FC<SkeletonProps> = ({
  type = 'text',
  width,
  height,
  className = '',
  count = 1,
  circle = false,
  rounded = false,
}) => {
  const baseClasses = 'animate-pulse bg-gray-200';
  
  const getTypeClasses = () => {
    switch (type) {
      case 'text':
        return 'h-4 rounded';
      case 'circle':
        return 'rounded-full';
      case 'rectangle':
        return rounded ? 'rounded-lg' : '';
      case 'card':
        return 'rounded-lg';
      case 'avatar':
        return 'rounded-full';
      case 'button':
        return 'rounded-full';
      default:
        return '';
    }
  };

  const getStyles = () => {
    const styles: React.CSSProperties = {};
    
    if (width) {
      styles.width = typeof width === 'number' ? `${width}px` : width;
    } else if (type === 'text') {
      styles.width = '100%';
    } else if (type === 'circle' || type === 'avatar') {
      styles.width = '40px';
    } else if (type === 'button') {
      styles.width = '80px';
    }
    
    if (height) {
      styles.height = typeof height === 'number' ? `${height}px` : height;
    } else if (type === 'circle' || type === 'avatar') {
      styles.height = '40px';
    } else if (type === 'button') {
      styles.height = '36px';
    } else if (type === 'card') {
      styles.height = '200px';
    }
    
    return styles;
  };

  const renderSkeleton = (key: number) => (
    <div
      key={key}
      className={`${baseClasses} ${getTypeClasses()} ${circle ? 'rounded-full' : ''} ${className}`}
      style={getStyles()}
    ></div>
  );

  if (count === 1) {
    return renderSkeleton(0);
  }

  return (
    <div className="space-y-2">
      {Array.from({ length: count }).map((_, index) => renderSkeleton(index))}
    </div>
  );
};