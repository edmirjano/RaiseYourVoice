import apiClient from './apiClient';
import Cookies from 'js-cookie';

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
    
    // Also store in cookies for SSR
    Cookies.set('token', response.data.token, { secure: true, sameSite: 'strict' });
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
    
    // Also store in cookies for SSR
    Cookies.set('token', response.data.token, { secure: true, sameSite: 'strict' });
  }
  
  return response.data;
};

export const logout = () => {
  // Call the logout endpoint to invalidate the refresh token
  apiClient.post('/auth/logout', { 
    refreshToken: localStorage.getItem('refreshToken') 
  }).catch(err => {
    console.error('Error during logout:', err);
  });
  
  // Clear local storage and cookies
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('user');
  Cookies.remove('token');
};

export const refreshToken = async (refreshToken: string): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/auth/refresh-token', { 
    token: localStorage.getItem('token'),
    refreshToken 
  });
  
  if (response.data.token) {
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    
    // Also update in cookies for SSR
    Cookies.set('token', response.data.token, { secure: true, sameSite: 'strict' });
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
    
    // Also store in cookies for SSR
    Cookies.set('token', response.data.token, { secure: true, sameSite: 'strict' });
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