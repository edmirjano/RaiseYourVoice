import apiClient, { handleApiError } from './apiClient';

export type Notification = {
  id: string;
  title: string;
  content: string;
  type: string;
  sentBy: string;
  sentAt: string;
  expiresAt?: string;
  targetAudience: {
    type: 'AllUsers' | 'SpecificUsers' | 'ByRole' | 'ByTopic' | 'ByRegion';
    userIds?: string[];
    targetRoles?: string[];
    topics?: string[];
    regions?: string[];
  };
  deliveryStatus: 'Queued' | 'Sent' | 'Failed';
  readStatus: 'Unread' | 'Read' | 'Dismissed';
};

export type NotificationTemplate = {
  id?: string;
  title: string;
  content: string;
  type: string;
  language: string;
  description?: string;
  createdAt?: string;
  updatedAt?: string;
};

export type DeviceTokenRequest = {
  deviceToken: string;
  deviceType: string;
};

/**
 * Get notifications for the current user
 */
export const getUserNotifications = async (): Promise<Notification[]> => {
  try {
    const response = await apiClient.get<Notification[]>('/notifications');
    return response.data;
  } catch (error) {
    console.error('Error fetching user notifications:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get a specific notification by ID
 */
export const getNotification = async (id: string): Promise<Notification> => {
  try {
    const response = await apiClient.get<Notification>(`/notifications/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching notification ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Mark a notification as read
 */
export const markAsRead = async (id: string): Promise<void> => {
  try {
    await apiClient.put(`/notifications/${id}/read`);
  } catch (error) {
    console.error(`Error marking notification ${id} as read:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Dismiss a notification
 */
export const dismissNotification = async (id: string): Promise<void> => {
  try {
    await apiClient.put(`/notifications/${id}/dismiss`);
  } catch (error) {
    console.error(`Error dismissing notification ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Register a device token for push notifications
 */
export const registerDeviceToken = async (deviceToken: string, deviceType: string): Promise<void> => {
  try {
    await apiClient.post('/notifications/device-token', {
      deviceToken,
      deviceType
    });
  } catch (error) {
    console.error('Error registering device token:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Remove a device token
 */
export const removeDeviceToken = async (deviceToken: string): Promise<void> => {
  try {
    await apiClient.delete(`/notifications/device-token/${deviceToken}`);
  } catch (error) {
    console.error(`Error removing device token ${deviceToken}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create a notification (admin/moderator only)
 */
export const createNotification = async (notification: Partial<Notification>): Promise<Notification> => {
  try {
    const response = await apiClient.post<Notification>('/notifications', notification);
    return response.data;
  } catch (error) {
    console.error('Error creating notification:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Broadcast a notification to all users (admin/moderator only)
 */
export const broadcastNotification = async (notification: Partial<Notification>): Promise<Notification> => {
  try {
    const response = await apiClient.post<Notification>('/notifications/broadcast', notification);
    return response.data;
  } catch (error) {
    console.error('Error broadcasting notification:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get notification templates (admin only)
 */
export const getNotificationTemplates = async (): Promise<NotificationTemplate[]> => {
  try {
    const response = await apiClient.get<NotificationTemplate[]>('/admin/notification-templates');
    return response.data;
  } catch (error) {
    console.error('Error fetching notification templates:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create a notification template (admin only)
 */
export const createNotificationTemplate = async (template: Partial<NotificationTemplate>): Promise<NotificationTemplate> => {
  try {
    const response = await apiClient.post<NotificationTemplate>('/admin/notification-templates', template);
    return response.data;
  } catch (error) {
    console.error('Error creating notification template:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update a notification template (admin only)
 */
export const updateNotificationTemplate = async (id: string, template: Partial<NotificationTemplate>): Promise<NotificationTemplate> => {
  try {
    const response = await apiClient.put<NotificationTemplate>(`/admin/notification-templates/${id}`, template);
    return response.data;
  } catch (error) {
    console.error(`Error updating notification template ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Delete a notification template (admin only)
 */
export const deleteNotificationTemplate = async (id: string): Promise<void> => {
  try {
    await apiClient.delete(`/admin/notification-templates/${id}`);
  } catch (error) {
    console.error(`Error deleting notification template ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};