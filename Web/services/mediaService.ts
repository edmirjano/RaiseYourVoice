import apiClient, { handleApiError } from './apiClient';
import { convertToWebP } from '../components/common/MediaConverter/WebPConverter';

/**
 * Upload a single file to the server
 */
export const uploadFile = async (file: File, folder: string = 'general'): Promise<string> => {
  try {
    // Convert image to WebP if it's an image
    let fileToUpload = file;
    if (file.type.startsWith('image/') && !file.type.includes('webp')) {
      try {
        fileToUpload = await convertToWebP(file);
      } catch (conversionError) {
        console.warn('Failed to convert image to WebP, using original format:', conversionError);
      }
    }
    
    const formData = new FormData();
    formData.append('file', fileToUpload);
    formData.append('folder', folder);
    
    const response = await apiClient.post<{ url: string }>('/media/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    
    return response.data.url;
  } catch (error) {
    console.error('Error uploading file:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Upload multiple files to the server
 */
export const uploadFiles = async (files: File[], folder: string = 'general'): Promise<string[]> => {
  try {
    // Process files in parallel
    const uploadPromises = files.map(file => uploadFile(file, folder));
    return await Promise.all(uploadPromises);
  } catch (error) {
    console.error('Error uploading multiple files:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Delete a file from the server
 */
export const deleteFile = async (url: string): Promise<void> => {
  try {
    await apiClient.delete('/media/delete', {
      data: { url }
    });
  } catch (error) {
    console.error('Error deleting file:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get a signed URL for a file (for temporary access to private files)
 */
export const getSignedUrl = async (url: string, expiresInMinutes: number = 60): Promise<string> => {
  try {
    const response = await apiClient.get<{ signedUrl: string }>('/media/signed-url', {
      params: { url, expiresInMinutes }
    });
    return response.data.signedUrl;
  } catch (error) {
    console.error('Error getting signed URL:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Optimize an image URL for different sizes
 */
export const getOptimizedImageUrl = (url: string, width?: number, height?: number, format: 'webp' | 'jpeg' | 'png' | 'original' = 'webp'): string => {
  // If it's not a URL from our domain, return as is
  if (!url.includes('raiseyourvoice.al') && !url.includes('localhost')) {
    return url;
  }
  
  // For demonstration, we're just returning the original URL
  // In a real implementation, this would call an image optimization service
  // or use Next.js Image component
  return url;
};