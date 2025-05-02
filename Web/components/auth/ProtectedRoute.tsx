import React, { useEffect } from 'react';
import { useRouter } from 'next/router';
import { motion } from 'framer-motion';
import { useAuth } from '../../contexts/AuthContext';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
}

/**
 * Protects a route by requiring authentication and optional role-based permissions
 */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ 
  children, 
  requiredRoles = [] 
}) => {
  const { isAuthenticated, user, isLoading } = useAuth();
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
      // If roles are required and user doesn't have any of the required roles
      else if (
        requiredRoles.length > 0 && 
        user && 
        !requiredRoles.includes(user.role)
      ) {
        router.push('/access-denied');
      }
    }
  }, [isAuthenticated, isLoading, requiredRoles, router, user]);

  // Show loading state while checking auth
  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          exit={{ opacity: 0, scale: 0.9 }}
          transition={{ duration: 0.3 }}
          className="text-center p-8"
        >
          <div className="w-12 h-12 border-4 border-ios-black border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-ios-black">Authenticating...</p>
        </motion.div>
      </div>
    );
  }

  // If not authenticated or doesn't have required roles, don't render children
  // (redirect will happen from useEffect)
  if (!isAuthenticated || (requiredRoles.length > 0 && user && !requiredRoles.includes(user.role))) {
    return null;
  }

  // If authenticated and has required role (or no role required), render children
  return <>{children}</>;
};

export default ProtectedRoute;