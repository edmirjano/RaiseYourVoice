import React, { useEffect } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '../../contexts/AuthContext';

interface RoleGuardProps {
  children: React.ReactNode;
  allowedRoles: string[];
}

/**
 * RoleGuard component that redirects to access-denied if user doesn't have required role
 */
export const RoleGuard: React.FC<RoleGuardProps> = ({ children, allowedRoles }) => {
  const { user, isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    // Wait until auth state is loaded
    if (!isLoading) {
      // If not authenticated, redirect to login
      if (!isAuthenticated) {
        router.push({
          pathname: '/auth/login',
          query: { returnUrl: router.asPath }
        });
      } 
      // If authenticated but doesn't have required role, redirect to access-denied
      else if (user && !allowedRoles.includes(user.role)) {
        router.push('/access-denied');
      }
    }
  }, [isAuthenticated, isLoading, router, user, allowedRoles]);

  // Show nothing while checking auth
  if (isLoading || !isAuthenticated || (user && !allowedRoles.includes(user.role))) {
    return null;
  }

  // If authenticated and has required role, render children
  return <>{children}</>;
};