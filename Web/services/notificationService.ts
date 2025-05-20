import apiClient from './apiClient';

export type Notification = {
  id: string;
  title: string;
  content: string;
  type: string;
  sentBy: string;
  sentAt: string;
  expiresAt?: string;
  readStatus: 'Unread' | 'Read' | 'Dismissed';
};

export type DeviceTokenRequest = {
  deviceToken: string;
  deviceType: string;
};

export const getUserNotifications = async (): Promise<Notification[]> => {
  const response = await apiClient.get<Notification[]>('/notifications');
  return response.data;
};

export const getNotification = async (id: string): Promise<Notification> => {
  const response = await apiClient.get<Notification>(`/notifications/${id}`);
  return response.data;
};

export const markAsRead = async (id: string): Promise<void> => {
  await apiClient.put(`/notifications/${id}/read`);
};

export const dismissNotification = async (id: string): Promise<void> => {
  await apiClient.put(`/notifications/${id}/dismiss`);
};

export const registerDeviceToken = async (deviceToken: string, deviceType: string): Promise<void> => {
  await apiClient.post('/notifications/device-token', {
    deviceToken,
    deviceType
  });
};

export const removeDeviceToken = async (deviceToken: string): Promise<void> => {
  await apiClient.delete(`/notifications/device-token/${deviceToken}`);
};

// For admin/moderator use
export const createNotification = async (notification: Partial<Notification>): Promise<Notification> => {
  const response = await apiClient.post<Notification>('/notifications', notification);
  return response.data;
};

export const broadcastNotification = async (notification: Partial<Notification>): Promise<Notification> => {
  const response = await apiClient.post<Notification>('/notifications/broadcast', notification);
  return response.data;
};

// Admin notification template management
export async function getNotificationTemplates() {
  return apiClient.get('/admin/notification-templates').then(r => r.data);
}

export async function createNotificationTemplate(data) {
  return apiClient.post('/admin/notification-templates', data).then(r => r.data);
}

export async function updateNotificationTemplate(id, data) {
  return apiClient.put(`/admin/notification-templates/${id}`, data).then(r => r.data);
}

export async function deleteNotificationTemplate(id) {
  return apiClient.delete(`/admin/notification-templates/${id}`).then(r => r.data);
}