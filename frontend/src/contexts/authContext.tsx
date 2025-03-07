import React, { createContext, useState, useEffect, ReactNode } from 'react';
import { api, AuthResponse } from '../services/api';

interface User {
  id: number;
  email: string;
}

interface AuthContextType {
  user: User | null;
  loading: boolean;
  error: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<AuthResponse>;
  register: (email: string, password: string) => Promise<AuthResponse>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType>({
    user: null,
    loading: false,
    error: null,
    isAuthenticated: false,
    login: async (email: string, password: string): Promise<AuthResponse> => {
      throw new Error('Not implemented');  // or return a dummy AuthResponse
    },
    register: async (email: string, password: string): Promise<AuthResponse> => {
      throw new Error('Not implemented');  // or return a dummy AuthResponse
    },
    logout: () => {},
  });

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Check if user is already logged in
    const token = localStorage.getItem('token');
    if (!token) {
      setLoading(false);
      return;
    }

    // Load user data
    const loadUser = async () => {
      try {
        const userData = await api.getCurrentUser();
        setUser(userData);
      } catch (err) {
        console.error('Failed to load user', err);
        localStorage.removeItem('token');
      } finally {
        setLoading(false);
      }
    };

    loadUser();
  }, []);

//   const login = async (email: string, password: string) => {
//     setLoading(true);
//     setError(null);

//     try {
//       const response = await api.login({ email, password });
//       setUser(response.user);
//     } catch (err) {
//       setError(err instanceof Error ? err.message : 'Login failed');
//       throw err;
//     } finally {
//       setLoading(false);
//     }
//   };

const login = async (email: string, password: string): Promise<AuthResponse> => {
    setLoading(true);
    setError(null);
  
    try {
      const response = await api.login({ email, password });
      setUser(response.user);
      return response; // Now we can return this
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const register = async (email: string, password: string) => {
    setLoading(true);
    setError(null);

    try {
      const response = await api.register({ email, password });
      setUser(response.user);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Registration failed');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    api.logout();
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        error,
        isAuthenticated: !!user,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

// Custom hook for easier usage
export const useAuth = () => React.useContext(AuthContext);