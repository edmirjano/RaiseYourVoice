import apiClient from './apiClient';

export type User = {
  id: string;
  name: string;
  email: string;
  role: string;
  profilePicture?: string;
  bio?: string;
  preferredLanguage?: string;
  notificationSettings?: {
    emailNotifications: boolean;
    pushNotifications: boolean;
    newPostNotifications: boolean;
    commentReplies: boolean;
    mentionNotifications: boolean;
    eventReminders: boolean;
  };
};

export const getCurrentUser = async (): Promise<User> => {
  const response = await apiClient.get<User>('/users/me');
  return response.data;
};

export const getUserById = async (id: string): Promise<User> => {
  const response = await apiClient.get<User>(`/users/${id}`);
  return response.data;
};

export const updateUserProfile = async (userData: Partial<User>): Promise<User> => {
  const response = await apiClient.put<User>('/users/profile', userData);
  return response.data;
};

export const changePassword = async (currentPassword: string, newPassword: string): Promise<void> => {
  await apiClient.post('/auth/password/change', {
    currentPassword,
    newPassword
  });
};

export const updateNotificationSettings = async (settings: any): Promise<void> => {
  await apiClient.put('/users/notification-settings', settings);
};

export const searchUsers = async (query: string): Promise<User[]> => {
  const response = await apiClient.get<User[]>(`/users/search?q=${encodeURIComponent(query)}`);
  return response.data;
};

export const getUserPosts = async (userId: string): Promise<any[]> => {
  const response = await apiClient.get(`/posts/user/${userId}`);
  return response.data;
};

export const getUserDonations = async (): Promise<any[]> => {
  const response = await apiClient.get('/donations/user');
  return response.data;
};