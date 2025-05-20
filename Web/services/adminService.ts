import apiClient, { handleApiError } from './apiClient';

/**
 * Get system statistics for admin dashboard
 */
export const getSystemStatistics = async (): Promise<any> => {
  try {
    const response = await apiClient.get('/admin/statistics');
    return response.data;
  } catch (error) {
    console.error('Error fetching system statistics:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get user management data
 */
export const getUsers = async (
  page: number = 1,
  pageSize: number = 10,
  role?: string,
  searchTerm?: string
): Promise<any> => {
  try {
    const params: any = { page, pageSize };
    if (role) params.role = role;
    if (searchTerm) params.search = searchTerm;
    
    const response = await apiClient.get('/admin/users', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching users:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update user role (admin only)
 */
export const updateUserRole = async (userId: string, role: string): Promise<void> => {
  try {
    await apiClient.put(`/admin/users/${userId}/role`, { role });
  } catch (error) {
    console.error(`Error updating role for user ${userId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Suspend a user (admin only)
 */
export const suspendUser = async (userId: string, reason: string): Promise<void> => {
  try {
    await apiClient.post(`/admin/users/${userId}/suspend`, { reason });
  } catch (error) {
    console.error(`Error suspending user ${userId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Unsuspend a user (admin only)
 */
export const unsuspendUser = async (userId: string): Promise<void> => {
  try {
    await apiClient.post(`/admin/users/${userId}/unsuspend`);
  } catch (error) {
    console.error(`Error unsuspending user ${userId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get reported content for moderation
 */
export const getReportedContent = async (
  page: number = 1,
  pageSize: number = 10,
  contentType?: 'post' | 'comment',
  status?: 'pending' | 'approved' | 'rejected'
): Promise<any> => {
  try {
    const params: any = { page, pageSize };
    if (contentType) params.contentType = contentType;
    if (status) params.status = status;
    
    const response = await apiClient.get('/admin/reported-content', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching reported content:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Approve reported content
 */
export const approveContent = async (contentId: string, contentType: 'post' | 'comment'): Promise<void> => {
  try {
    await apiClient.post(`/admin/reported-content/${contentId}/approve`, { contentType });
  } catch (error) {
    console.error(`Error approving ${contentType} ${contentId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Reject reported content
 */
export const rejectContent = async (contentId: string, contentType: 'post' | 'comment', reason: string): Promise<void> => {
  try {
    await apiClient.post(`/admin/reported-content/${contentId}/reject`, { contentType, reason });
  } catch (error) {
    console.error(`Error rejecting ${contentType} ${contentId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get system settings
 */
export const getSystemSettings = async (): Promise<any> => {
  try {
    const response = await apiClient.get('/admin/settings');
    return response.data;
  } catch (error) {
    console.error('Error fetching system settings:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update system settings
 */
export const updateSystemSettings = async (settings: any): Promise<void> => {
  try {
    await apiClient.put('/admin/settings', settings);
  } catch (error) {
    console.error('Error updating system settings:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get audit logs
 */
export const getAuditLogs = async (
  page: number = 1,
  pageSize: number = 10,
  userId?: string,
  action?: string,
  startDate?: Date,
  endDate?: Date
): Promise<any> => {
  try {
    const params: any = { page, pageSize };
    if (userId) params.userId = userId;
    if (action) params.action = action;
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    
    const response = await apiClient.get('/admin/audit-logs', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching audit logs:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get API keys (admin only)
 */
export const getApiKeys = async (): Promise<any[]> => {
  try {
    const response = await apiClient.get('/admin/api-keys');
    return response.data;
  } catch (error) {
    console.error('Error fetching API keys:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create API key (admin only)
 */
export const createApiKey = async (name: string, expiresInDays?: number): Promise<any> => {
  try {
    const response = await apiClient.post('/admin/api-keys', {
      name,
      expiresInDays
    });
    return response.data;
  } catch (error) {
    console.error('Error creating API key:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Revoke API key (admin only)
 */
export const revokeApiKey = async (id: string): Promise<void> => {
  try {
    await apiClient.delete(`/admin/api-keys/${id}`);
  } catch (error) {
    console.error(`Error revoking API key ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};