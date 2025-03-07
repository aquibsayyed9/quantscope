// Updated src/App.tsx
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import ProtectedRoute from './components/auth/ProtectedRoute';
import Layout from './components/layout/Layout';
import Dashboard from './pages/Dashboard';
import Explorer from './pages/Explorer';
import Connectors from './pages/Connectors';
import Login from './pages/Login';
import Register from './pages/Register';
import { AuthProvider } from './contexts/authContext';
import LandingPage from './pages/LandingPage';

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Public routes (no layout) */}
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          
          {/* Protected routes with layout */}
          <Route element={
            <ProtectedRoute>
              <Layout />
            </ProtectedRoute>
          }>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/explorer" element={<Explorer />} />
            <Route path="/connectors" element={<Connectors />} />
          </Route>
          
          {/* Redirect authenticated users from root to dashboard */}
          <Route path="/" element={
            <ProtectedRoute redirectTo="/dashboard">
              <LandingPage />
            </ProtectedRoute>
          } />
          
          {/* Redirect unmatched routes */}
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;