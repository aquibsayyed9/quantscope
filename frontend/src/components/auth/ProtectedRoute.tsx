// src/components/auth/ProtectedRoute.tsx
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/authContext';

interface ProtectedRouteProps {
  children: React.ReactNode;
  redirectTo?: string;
}

const ProtectedRoute = ({ children, redirectTo = '/login' }: ProtectedRouteProps) => {
    const { isAuthenticated, loading, user } = useAuth();
    const location = useLocation();
  
    // Show loading state while checking authentication
    if (loading) {
      return <div className="flex justify-center items-center h-screen">Loading...</div>;
    }
  
    // Check both isAuthenticated flag and user object existence
    if (!isAuthenticated || !user) {
      // Redirect to login and save the current location
      return <Navigate to={redirectTo} state={{ from: location }} replace />;
    }
  
    return <>{children}</>;
  };

export default ProtectedRoute;