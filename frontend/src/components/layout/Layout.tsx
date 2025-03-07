// src/components/layout/Layout.tsx
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';

const Layout = () => {
  return (
    <div className="min-h-screen bg-slate-50">
      <Sidebar />
      <div className="pl-64"> {/* Add padding for sidebar */}
        <header className="h-16 bg-white border-b flex items-center px-6 fixed top-0 right-0 left-64">
        </header>
        <main className="pt-16 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default Layout;