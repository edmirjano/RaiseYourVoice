import apiClient, { handleApiError } from './apiClient';

export type User = {
  id: string;
  name: string;
  email: string;
  role: string;
  profilePicture?: string;
  bio?: string;
  joinDate: Date;
  lastLogin?: Date;
  preferredLanguage: string;
  notificationSettings: {
    emailNotifications: boolean;
    pushNotifications: boolean;
    newPostNotifications: boolean;
    commentReplies: boolean;
    mentionNotifications: boolean;
    eventReminders: boolean;
  };
};

/**
 * Get the current user's profile
 */
export const getCurrentUser = async (): Promise<User> => {
  try {
    const response = await apiClient.get<User>('/users/me');
    return response.data;
  } catch (error) {
    console.error('Error fetching current user:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get a user by ID
 */
export const getUserById = async (id: string): Promise<User> => {
  try {
    const response = await apiClient.get<User>(`/users/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching user ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update user profile
 */
export const updateUserProfile = async (userData: Partial<User>): Promise<User> => {
  try {
    const response = await apiClient.put<User>('/users/profile', userData);
    return response.data;
  } catch (error) {
    console.error('Error updating user profile:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Change password
 */
export const changePassword = async (currentPassword: string, newPassword: string): Promise<void> => {
  try {
    await apiClient.post('/auth/password/change', {
      currentPassword,
      newPassword
    });
  } catch (error) {
    console.error('Error changing password:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update notification settings
 */
export const updateNotificationSettings = async (settings: User['notificationSettings']): Promise<void> => {
  try {
    await apiClient.put('/users/notification-settings', settings);
  } catch (error) {
    console.error('Error updating notification settings:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Search users by query
 */
export const searchUsers = async (query: string): Promise<User[]> => {
  try {
    const response = await apiClient.get<User[]>(`/users/search?q=${encodeURIComponent(query)}`);
    return response.data;
  } catch (error) {
    console.error('Error searching users:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get posts by user ID
 */
export const getUserPosts = async (userId: string): Promise<any[]> => {
  try {
    const response = await apiClient.get(`/posts/user/${userId}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching posts for user ${userId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get donations by current user
 */
export const getUserDonations = async (): Promise<any[]> => {
  try {
    const response = await apiClient.get('/donations/user');
    return response.data;
  } catch (error) {
    console.error('Error fetching user donations:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Delete user account
 */
export const deleteUserAccount = async (): Promise<void> => {
  try {
    await apiClient.delete('/users/account');
  } catch (error) {
    console.error('Error deleting user account:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Upload profile picture
 */
export const uploadProfilePicture = async (file: File): Promise<string> => {
  try {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await apiClient.post<{ url: string }>('/users/profile-picture', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    
    return response.data.url;
  } catch (error) {
    console.error('Error uploading profile picture:', error);
    throw new Error(handleApiError(error));
  }
};