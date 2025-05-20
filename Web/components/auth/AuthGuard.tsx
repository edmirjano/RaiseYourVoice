import React, { useEffect } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '../../contexts/AuthContext';

interface AuthGuardProps {
  children: React.ReactNode;
}

/**
 * AuthGuard component that redirects to login if user is not authenticated
 */
export const AuthGuard: React.FC<AuthGuardProps> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    // Wait until auth state is loaded
    if (!isLoading && !isAuthenticated) {
      // Store the current URL so we can redirect back after login
      router.push({
        pathname: '/auth/login',
        query: { returnUrl: router.asPath }
      });
    }
  }, [isAuthenticated, isLoading, router]);

  // Show nothing while checking auth
  if (isLoading || !isAuthenticated) {
    return null;
  }

  // If authenticated, render children
  return <>{children}</>;
};