import React from 'react';
import { Skeleton } from '../common/Skeleton/Skeleton';

interface FeedSkeletonProps {
  count?: number;
  className?: string;
}

export const FeedSkeleton: React.FC<FeedSkeletonProps> = ({
  count = 3,
  className = '',
}) => {
  return (
    <div className={`space-y-6 ${className}`}>
      {Array.from({ length: count }).map((_, index) => (
        <div key={index} className="ios-card p-4">
          {/* Post Header */}
          <div className="flex items-center mb-4">
            <Skeleton type="avatar" className="mr-3" />
            <div className="flex-1">
              <Skeleton type="text" className="w-1/3 mb-1" />
              <Skeleton type="text" className="w-1/4" />
            </div>
          </div>
          
          {/* Post Title and Content */}
          <Skeleton type="text" className="w-3/4 mb-2" />
          <Skeleton type="text" count={3} className="mb-4" />
          
          {/* Post Image */}
          <Skeleton type="rectangle" className="w-full h-48 mb-4" rounded />
          
          {/* Post Tags */}
          <div className="flex space-x-2 mb-4">
            <Skeleton type="text" className="w-16" />
            <Skeleton type="text" className="w-20" />
            <Skeleton type="text" className="w-14" />
          </div>
          
          {/* Post Footer */}
          <div className="flex justify-between pt-2 border-t border-gray-100">
            <div className="flex space-x-4">
              <Skeleton type="text" className="w-16" />
              <Skeleton type="text" className="w-20" />
            </div>
            <Skeleton type="text" className="w-8" />
          </div>
        </div>
      ))}
    </div>
  );
};