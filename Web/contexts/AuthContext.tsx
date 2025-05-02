import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useRouter } from 'next/router';
import { motion } from 'framer-motion';
import * as authService from '../services/authService';
import { LoginCredentials, RegisterData, AuthResponse } from '../services/authService';

// Define user type based on what's stored
export type User = {
  id: string;
  name: string;
  email: string;
  role: string;
  profilePicture?: string;
};

// Define the auth context state type
interface AuthContextState {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => void;
  forgotPassword: (email: string) => Promise<void>;
  resetPassword: (token: string, newPassword: string) => Promise<void>;
  loginWithGoogle: () => Promise<void>;
  loginWithApple: () => Promise<void>;
}

// Create the auth context with default values
const AuthContext = createContext<AuthContextState>({
  user: null,
  isLoading: true,
  isAuthenticated: false,
  login: async () => {},
  register: async () => {},
  logout: () => {},
  forgotPassword: async () => {},
  resetPassword: async () => {},
  loginWithGoogle: async () => {},
  loginWithApple: async () => {},
});

// Props for AuthProvider
interface AuthProviderProps {
  children: ReactNode;
}

// Create the Auth Provider component
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const router = useRouter();

  // Check for user data in localStorage on initial load
  useEffect(() => {
    const loadUser = () => {
      const user = authService.getCurrentUser();
      setUser(user);
      setIsLoading(false);
    };

    loadUser();
  }, []);

  // Login handler
  const login = async (credentials: LoginCredentials) => {
    setIsLoading(true);
    try {
      const response = await authService.login(credentials);
      setUser({
        id: response.userId,
        name: response.name,
        email: response.email,
        role: response.role,
      });
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  // Register handler
  const register = async (data: RegisterData) => {
    setIsLoading(true);
    try {
      const response = await authService.register(data);
      setUser({
        id: response.userId,
        name: response.name,
        email: response.email,
        role: response.role,
      });
    } catch (error) {
      console.error('Registration error:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  // Logout handler
  const logout = () => {
    authService.logout();
    setUser(null);
    router.push('/auth/login');
  };

  // Forgot password handler
  const forgotPassword = async (email: string) => {
    try {
      await authService.forgotPassword(email);
    } catch (error) {
      console.error('Forgot password error:', error);
      throw error;
    }
  };

  // Reset password handler
  const resetPassword = async (token: string, newPassword: string) => {
    try {
      await authService.resetPassword(token, newPassword);
    } catch (error) {
      console.error('Reset password error:', error);
      throw error;
    }
  };

  // Social login handlers
  const loginWithGoogle = async () => {
    try {
      // Redirect to Google OAuth endpoint
      window.location.href = `${process.env.NEXT_PUBLIC_API_URL}/auth/google`;
    } catch (error) {
      console.error('Google login error:', error);
      throw error;
    }
  };

  const loginWithApple = async () => {
    try {
      // Redirect to Apple OAuth endpoint
      window.location.href = `${process.env.NEXT_PUBLIC_API_URL}/auth/apple`;
    } catch (error) {
      console.error('Apple login error:', error);
      throw error;
    }
  };

  // Determine if user is authenticated
  const isAuthenticated = !!user;

  // Provide the auth context to children
  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated,
        login,
        register,
        logout,
        forgotPassword,
        resetPassword,
        loginWithGoogle,
        loginWithApple,
      }}
    >
      {isLoading ? (
        <div className="flex h-screen items-center justify-center">
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.9 }}
            transition={{ duration: 0.3 }}
            className="text-center p-8"
          >
            <div className="w-12 h-12 border-4 border-ios-black border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
            <p className="text-ios-black">Loading...</p>
          </motion.div>
        </div>
      ) : (
        children
      )}
    </AuthContext.Provider>
  );
};

// Create a hook for using the auth context
export const useAuth = () => useContext(AuthContext);

export default AuthContext;