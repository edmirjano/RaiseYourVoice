/**
 * Note: Full video conversion to WebM is complex and typically done server-side.
 * This is a simplified client-side approach with limitations.
 * For production, consider using a server-side conversion service.
 */

/**
 * Check if a video file is already in WebM format
 */
export const isWebMVideo = (file: File): boolean => {
  return file.type === 'video/webm';
};

/**
 * Check if the browser supports WebM
 */
export const isWebMSupported = (): boolean => {
  const video = document.createElement('video');
  return video.canPlayType('video/webm') !== '';
};

/**
 * Check if MediaRecorder API is available for potential conversion
 */
export const isMediaRecorderSupported = (): boolean => {
  return typeof MediaRecorder !== 'undefined';
};

/**
 * Get video dimensions
 * This is useful for maintaining aspect ratio during conversion
 */
export const getVideoDimensions = (file: File): Promise<{ width: number; height: number }> => {
  return new Promise((resolve, reject) => {
    const video = document.createElement('video');
    video.preload = 'metadata';
    
    video.onloadedmetadata = () => {
      URL.revokeObjectURL(video.src);
      resolve({
        width: video.videoWidth,
        height: video.videoHeight
      });
    };
    
    video.onerror = () => {
      URL.revokeObjectURL(video.src);
      reject(new Error('Could not load video metadata'));
    };
    
    video.src = URL.createObjectURL(file);
  });
};

/**
 * For client-side use, we'll provide a function to check if a video needs conversion
 * and suggest using a server-side service for actual conversion
 */
export const checkVideoFormatCompatibility = async (file: File): Promise<{
  isCompatible: boolean;
  needsConversion: boolean;
  message: string;
}> => {
  // If it's already WebM, it's compatible
  if (isWebMVideo(file)) {
    return {
      isCompatible: true,
      needsConversion: false,
      message: 'Video is already in WebM format'
    };
  }
  
  // Check if browser supports WebM
  if (!isWebMSupported()) {
    return {
      isCompatible: false,
      needsConversion: true,
      message: 'Your browser does not support WebM video format'
    };
  }
  
  // For MP4 and other formats, suggest server-side conversion
  return {
    isCompatible: true, // Most browsers can play MP4
    needsConversion: true,
    message: 'Video will be converted to WebM format on the server for optimal performance'
  };
};

/**
 * Prepare video for upload - this function would be called before sending to the server
 * It doesn't actually convert the video, but prepares metadata for server-side conversion
 */
export const prepareVideoForUpload = async (file: File): Promise<{
  file: File;
  metadata: {
    originalFormat: string;
    dimensions: { width: number; height: number };
    needsConversion: boolean;
  };
}> => {
  const dimensions = await getVideoDimensions(file);
  const compatibility = await checkVideoFormatCompatibility(file);
  
  return {
    file,
    metadata: {
      originalFormat: file.type,
      dimensions,
      needsConversion: compatibility.needsConversion
    }
  };
};