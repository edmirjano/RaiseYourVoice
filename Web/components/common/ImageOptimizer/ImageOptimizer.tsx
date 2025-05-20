import React, { useState, useEffect } from 'react';
import { useInView } from 'react-intersection-observer';

type ImageFormat = 'webp' | 'jpeg' | 'png' | 'original';

type ImageOptimizerProps = {
  src: string;
  alt: string;
  width?: number;
  height?: number;
  className?: string;
  objectFit?: 'cover' | 'contain' | 'fill' | 'none' | 'scale-down';
  format?: ImageFormat;
  quality?: number;
  lazy?: boolean;
  placeholder?: string | boolean;
  onLoad?: () => void;
  onError?: () => void;
};

export const ImageOptimizer: React.FC<ImageOptimizerProps> = ({
  src,
  alt,
  width,
  height,
  className = '',
  objectFit = 'cover',
  format = 'webp',
  quality = 80,
  lazy = true,
  placeholder = true,
  onLoad,
  onError,
}) => {
  const [isLoaded, setIsLoaded] = useState(false);
  const [error, setError] = useState(false);
  const { ref, inView } = useInView({
    triggerOnce: true,
    rootMargin: '200px 0px',
  });

  // Function to optimize image URL
  const getOptimizedUrl = () => {
    // If it's an external URL that doesn't support optimization, return as is
    if (src.startsWith('http') && !src.includes('raiseyourvoice.al')) {
      return src;
    }

    // For demonstration, we're just returning the original URL
    // In a real implementation, this would call an image optimization service
    // or use Next.js Image component
    return src;
  };

  const handleImageLoad = () => {
    setIsLoaded(true);
    if (onLoad) onLoad();
  };

  const handleImageError = () => {
    setError(true);
    if (onError) onError();
  };

  const imageStyles: React.CSSProperties = {
    objectFit,
  };

  if (width) imageStyles.width = width;
  if (height) imageStyles.height = height;

  return (
    <div
      ref={ref}
      className={`relative overflow-hidden ${className}`}
      style={{ width, height }}
    >
      {/* Placeholder */}
      {placeholder && !isLoaded && !error && (
        <div
          className="absolute inset-0 bg-gray-200 animate-pulse"
          style={{ width, height }}
        ></div>
      )}

      {/* Image */}
      {(inView || !lazy) && !error && (
        <img
          src={getOptimizedUrl()}
          alt={alt}
          onLoad={handleImageLoad}
          onError={handleImageError}
          className={`transition-opacity duration-300 ${
            isLoaded ? 'opacity-100' : 'opacity-0'
          }`}
          style={imageStyles}
          width={width}
          height={height}
          loading={lazy ? 'lazy' : 'eager'}
        />
      )}

      {/* Error fallback */}
      {error && (
        <div
          className="flex items-center justify-center bg-gray-100 text-gray-400"
          style={{ width, height }}
        >
          <svg
            className="h-8 w-8"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
            />
          </svg>
        </div>
      )}
    </div>
  );
};