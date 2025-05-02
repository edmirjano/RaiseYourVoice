import apiClient from './apiClient';

export type LoginCredentials = {
  email: string;
  password: string;
};

export type RegisterData = {
  name: string;
  email: string;
  password: string;
  profilePicture?: string;
  bio?: string;
  preferredLanguage?: string;
};

export type AuthResponse = {
  userId: string;
  name: string;
  email: string;
  role: string;
  token: string;
  refreshToken: string;
};

export const login = async (credentials: LoginCredentials): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/auth/login', credentials);
  
  // Store the auth token in local storage for later use
  if (response.data.token) {
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    localStorage.setItem('user', JSON.stringify({
      id: response.data.userId,
      name: response.data.name,
      email: response.data.email,
      role: response.data.role
    }));
  }
  
  return response.data;
};

export const register = async (data: RegisterData): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/auth/register', data);
  
  // Store the auth token in local storage for later use
  if (response.data.token) {
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    localStorage.setItem('user', JSON.stringify({
      id: response.data.userId,
      name: response.data.name,
      email: response.data.email,
      role: response.data.role
    }));
  }
  
  return response.data;
};

export const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('user');
};

export const refreshToken = async (refreshToken: string): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/auth/refresh-token', { refreshToken });
  
  if (response.data.token) {
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('refreshToken', response.data.refreshToken);
  }
  
  return response.data;
};

export const getCurrentUser = () => {
  if (typeof window === 'undefined') return null;
  
  const userStr = localStorage.getItem('user');
  if (!userStr) return null;
  
  try {
    return JSON.parse(userStr);
  } catch (error) {
    console.error('Error parsing user data:', error);
    return null;
  }
};

/**
 * Send a password reset email to the specified email address
 */
export const forgotPassword = async (email: string): Promise<void> => {
  await apiClient.post('/auth/forgot-password', { email });
};

/**
 * Reset password using token from email
 */
export const resetPassword = async (token: string, newPassword: string): Promise<void> => {
  await apiClient.post('/auth/reset-password', { token, newPassword });
};

/**
 * Handle OAuth callback from Google/Apple login
 */
export const handleOAuthCallback = async (token: string): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/auth/oauth-callback', { token });
  
  if (response.data.token) {
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    localStorage.setItem('user', JSON.stringify({
      id: response.data.userId,
      name: response.data.name,
      email: response.data.email,
      role: response.data.role
    }));
  }
  
  return response.data;
};

/**
 * Verify email address with verification token
 */
export const verifyEmail = async (token: string): Promise<void> => {
  await apiClient.post('/auth/verify-email', { token });
};

/**
 * Check if current token is valid
 */
export const validateToken = async (): Promise<boolean> => {
  try {
    await apiClient.get('/auth/validate-token');
    return true;
  } catch (error) {
    return false;
  }
};