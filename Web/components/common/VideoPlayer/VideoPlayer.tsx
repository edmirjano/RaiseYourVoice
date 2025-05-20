import React, { useState, useRef, useEffect } from 'react';

type VideoPlayerProps = {
  src: string;
  poster?: string;
  width?: number | string;
  height?: number | string;
  className?: string;
  autoPlay?: boolean;
  muted?: boolean;
  loop?: boolean;
  controls?: boolean;
  onPlay?: () => void;
  onPause?: () => void;
  onEnd?: () => void;
  onError?: (error: any) => void;
};

export const VideoPlayer: React.FC<VideoPlayerProps> = ({
  src,
  poster,
  width = '100%',
  height = 'auto',
  className = '',
  autoPlay = false,
  muted = false,
  loop = false,
  controls = true,
  onPlay,
  onPause,
  onEnd,
  onError,
}) => {
  const [isPlaying, setIsPlaying] = useState(autoPlay);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const videoRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    const handlePlay = () => {
      setIsPlaying(true);
      if (onPlay) onPlay();
    };

    const handlePause = () => {
      setIsPlaying(false);
      if (onPause) onPause();
    };

    const handleEnded = () => {
      setIsPlaying(false);
      if (onEnd) onEnd();
    };

    const handleError = (e: any) => {
      setError('Error loading video');
      setIsLoading(false);
      if (onError) onError(e);
    };

    const handleLoadedData = () => {
      setIsLoading(false);
    };

    video.addEventListener('play', handlePlay);
    video.addEventListener('pause', handlePause);
    video.addEventListener('ended', handleEnded);
    video.addEventListener('error', handleError);
    video.addEventListener('loadeddata', handleLoadedData);

    return () => {
      video.removeEventListener('play', handlePlay);
      video.removeEventListener('pause', handlePause);
      video.removeEventListener('ended', handleEnded);
      video.removeEventListener('error', handleError);
      video.removeEventListener('loadeddata', handleLoadedData);
    };
  }, [onPlay, onPause, onEnd, onError]);

  // Check if the source is a WebM video
  const isWebM = src.toLowerCase().endsWith('.webm');

  return (
    <div className={`relative ${className}`}>
      {isLoading && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-100">
          <div className="w-10 h-10 border-4 border-ios-black border-t-transparent rounded-full animate-spin"></div>
        </div>
      )}

      {error && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-100 text-red-500">
          <div className="text-center">
            <svg
              className="h-10 w-10 mx-auto mb-2"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <p>{error}</p>
          </div>
        </div>
      )}

      <video
        ref={videoRef}
        className={`w-full h-auto rounded-lg ${error ? 'hidden' : ''}`}
        width={width}
        height={height}
        poster={poster}
        autoPlay={autoPlay}
        muted={muted}
        loop={loop}
        controls={controls}
        playsInline
      >
        <source src={src} type={isWebM ? 'video/webm' : 'video/mp4'} />
        Your browser does not support the video tag.
      </video>
    </div>
  );
};