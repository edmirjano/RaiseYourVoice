import apiClient, { handleApiError } from './apiClient';
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

export type User = {
  id: string;
  name: string;
  email: string;
  role: string;
  profilePicture?: string;
  bio?: string;
};

/**
 * Login with email and password
 */
export const login = async (credentials: LoginCredentials): Promise<AuthResponse> => {
  try {
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
  } catch (error) {
    console.error('Login error:', error);
    throw error;
  }
};

/**
 * Register a new user
 */
export const register = async (data: RegisterData): Promise<AuthResponse> => {
  try {
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
  } catch (error) {
    console.error('Registration error:', error);
    throw error;
  }
};

/**
 * Logout the current user
 */
export const logout = async (): Promise<void> => {
  try {
    // Call the logout endpoint to invalidate the refresh token
    const refreshTokenValue = localStorage.getItem('refreshToken');
    if (refreshTokenValue) {
      await apiClient.post('/auth/logout', { refreshToken: refreshTokenValue });
    }
  } catch (error) {
    console.error('Logout error:', error);
  } finally {
    // Clear local storage and cookies regardless of API call success
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    Cookies.remove('token');
  }
};

/**
 * Refresh the access token using a refresh token
 */
export const refreshToken = async (refreshTokenValue: string): Promise<AuthResponse> => {
  try {
    const response = await apiClient.post<AuthResponse>('/auth/refresh-token', { 
      token: localStorage.getItem('token'),
      refreshToken: refreshTokenValue 
    });
    
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('refreshToken', response.data.refreshToken);
      
      // Also update in cookies for SSR
      Cookies.set('token', response.data.token, { secure: true, sameSite: 'strict' });
    }
    
    return response.data;
  } catch (error) {
    console.error('Token refresh error:', error);
    throw error;
  }
};

/**
 * Get the current user from local storage
 */
export const getCurrentUser = (): User | null => {
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
 * Check if the current user is authenticated
 */
export const isAuthenticated = (): boolean => {
  return !!getCurrentUser() && !!localStorage.getItem('token');
};

/**
 * Send a password reset email to the specified email address
 */
export const forgotPassword = async (email: string): Promise<void> => {
  try {
    await apiClient.post('/auth/forgot-password', { email });
  } catch (error) {
    console.error('Forgot password error:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Reset password using token from email
 */
export const resetPassword = async (token: string, newPassword: string): Promise<void> => {
  try {
    await apiClient.post('/auth/reset-password', { token, newPassword });
  } catch (error) {
    console.error('Reset password error:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Handle OAuth callback from Google/Apple login
 */
export const handleOAuthCallback = async (token: string): Promise<AuthResponse> => {
  try {
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
  } catch (error) {
    console.error('OAuth callback error:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Verify email address with verification token
 */
export const verifyEmail = async (token: string): Promise<void> => {
  try {
    await apiClient.post('/auth/verify-email', { token });
  } catch (error) {
    console.error('Email verification error:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Check if current token is valid
 */
export const validateToken = async (): Promise<boolean> => {
  try {
    await apiClient.get('/auth/validate-token');
    return true;
  } catch (error) {
    console.error('Token validation error:', error);
    return false;
  }
};

/**
 * Update user profile
 */
export const updateProfile = async (userData: Partial<User>): Promise<User> => {
  try {
    const response = await apiClient.put<User>('/users/profile', userData);
    
    // Update user in local storage
    const currentUser = getCurrentUser();
    if (currentUser) {
      const updatedUser = { ...currentUser, ...response.data };
      localStorage.setItem('user', JSON.stringify(updatedUser));
    }
    
    return response.data;
  } catch (error) {
    console.error('Update profile error:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Change password
 */
export const changePassword = async (currentPassword: string, newPassword: string): Promise<void> => {
  try {
    await apiClient.post('/auth/password/change', { currentPassword, newPassword });
  } catch (error) {
    console.error('Change password error:', error);
    throw new Error(handleApiError(error));
  }
};