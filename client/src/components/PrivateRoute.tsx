import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import PageSpinner from './ui/PageSpinner';

interface PrivateRouteProps {
  children: React.ReactNode;
  requireAdmin?: boolean;
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ children, requireAdmin = false }) => {
  const { user, status } = useAuthStore();
  const location = useLocation();

  // Whether there is a session is only known once /users/me answers. Redirecting before that would
  // bounce every signed-in user to the login page on a page refresh.
  if (status === 'loading') {
    return <PageSpinner />;
  }

  if (status !== 'authenticated' || !user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requireAdmin && user.role !== 'Admin') {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};

export default PrivateRoute;
