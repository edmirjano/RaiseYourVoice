import apiClient, { handleApiError } from './apiClient';

/**
 * Get all translations for a specific language
 */
export const getAllTranslations = async (language: string): Promise<Record<string, string>> => {
  try {
    const response = await apiClient.get<Record<string, string>>('/localizations', {
      headers: {
        'Accept-Language': language
      }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching translations for language ${language}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get translations for a specific category and language
 */
export const getTranslationsByCategory = async (
  category: string,
  language: string
): Promise<Record<string, string>> => {
  try {
    const response = await apiClient.get<Record<string, string>>(`/localizations/category/${category}`, {
      headers: {
        'Accept-Language': language
      }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching translations for category ${category} and language ${language}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get a single translation by key and language
 */
export const getTranslation = async (key: string, language: string): Promise<string> => {
  try {
    const response = await apiClient.get<{ key: string; value: string }>(`/localizations/${key}`, {
      headers: {
        'Accept-Language': language
      }
    });
    return response.data.value;
  } catch (error) {
    console.error(`Error fetching translation for key ${key} and language ${language}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Set a translation (admin only)
 */
export const setTranslation = async (
  key: string,
  language: string,
  value: string,
  category?: string,
  description?: string
): Promise<void> => {
  try {
    await apiClient.post('/localizations', {
      key,
      language,
      value,
      category,
      description
    });
  } catch (error) {
    console.error(`Error setting translation for key ${key} and language ${language}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Set user's preferred language
 */
export const setPreferredLanguage = async (language: string): Promise<void> => {
  try {
    // Store in localStorage for client-side
    localStorage.setItem('preferredLanguage', language);
    
    // Also update in cookies for SSR
    document.cookie = `preferredLanguage=${language}; path=/; max-age=31536000; SameSite=Strict`;
    
    // Update user preferences if logged in
    if (localStorage.getItem('token')) {
      await apiClient.put('/users/preferences', {
        preferredLanguage: language
      });
    }
  } catch (error) {
    console.error(`Error setting preferred language to ${language}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get user's preferred language
 */
export const getPreferredLanguage = (): string => {
  // Check localStorage first
  const localStorageLanguage = localStorage.getItem('preferredLanguage');
  if (localStorageLanguage) {
    return localStorageLanguage;
  }
  
  // Check cookies
  const cookies = document.cookie.split(';');
  for (const cookie of cookies) {
    const [name, value] = cookie.trim().split('=');
    if (name === 'preferredLanguage') {
      return value;
    }
  }
  
  // Default to browser language or English
  return navigator.language.split('-')[0] || 'en';
};