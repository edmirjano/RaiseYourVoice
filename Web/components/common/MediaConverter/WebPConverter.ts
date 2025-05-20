/**
 * Utility for converting images to WebP format
 */
export const convertToWebP = async (file: File): Promise<File> => {
  return new Promise((resolve, reject) => {
    // Check if the file is an image
    if (!file.type.startsWith('image/')) {
      reject(new Error('File is not an image'));
      return;
    }

    // Skip if already WebP
    if (file.type === 'image/webp') {
      resolve(file);
      return;
    }

    const reader = new FileReader();
    reader.onload = (event) => {
      const img = new Image();
      img.onload = () => {
        const canvas = document.createElement('canvas');
        canvas.width = img.width;
        canvas.height = img.height;
        const ctx = canvas.getContext('2d');
        
        if (!ctx) {
          reject(new Error('Could not get canvas context'));
          return;
        }
        
        ctx.drawImage(img, 0, 0);
        
        // Convert to WebP
        canvas.toBlob(
          (blob) => {
            if (!blob) {
              reject(new Error('Could not convert to WebP'));
              return;
            }
            
            // Create a new file with the same name but .webp extension
            const fileName = file.name.replace(/\.[^/.]+$/, '.webp');
            const webpFile = new File([blob], fileName, {
              type: 'image/webp',
              lastModified: Date.now(),
            });
            
            resolve(webpFile);
          },
          'image/webp',
          0.8 // Quality
        );
      };
      
      img.onerror = () => {
        reject(new Error('Could not load image'));
      };
      
      img.src = event.target?.result as string;
    };
    
    reader.onerror = () => {
      reject(new Error('Could not read file'));
    };
    
    reader.readAsDataURL(file);
  });
};

/**
 * Batch convert multiple images to WebP
 */
export const batchConvertToWebP = async (files: File[]): Promise<File[]> => {
  const promises = files.map((file) => {
    if (file.type.startsWith('image/')) {
      return convertToWebP(file).catch(() => file); // Fallback to original if conversion fails
    }
    return Promise.resolve(file); // Return non-image files as is
  });
  
  return Promise.all(promises);
};