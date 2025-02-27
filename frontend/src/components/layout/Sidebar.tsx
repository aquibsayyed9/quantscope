import { Link, useLocation } from 'react-router-dom';
import { LayoutDashboard, MessageSquare, Settings, CableIcon } from 'lucide-react';

const Sidebar = () => {
  const location = useLocation();
  
  const navItems = [
    {
      path: '/',
      label: 'Dashboard',
      icon: <LayoutDashboard />,
    },
    {
      path: '/explorer',
      label: 'Message Explorer',
      icon: <MessageSquare />,
    },
    {
      path: '/connectors',
      label: 'Connectors',
      icon: <CableIcon />,
    },
    {
      path: '/settings',
      label: 'Settings',
      icon: <Settings />,
    },
  ];

  return (
    <aside className="fixed left-0 top-0 h-screen w-64 bg-slate-900 text-white p-4">
      <div className="mb-8">
        <h1 className="text-xl font-bold text-white flex items-center gap-2">
          FIX Analyzer
        </h1>
      </div>
      <nav className="space-y-2">
        {navItems.map((item) => (
          <Link
            key={item.path}
            to={item.path}
            className={`flex items-center gap-3 px-4 py-2 rounded-lg transition-colors ${
              location.pathname === item.path 
                ? 'bg-slate-800 text-white' 
                : 'text-slate-300 hover:bg-slate-800 hover:text-white'
            }`}
          >
            <span className="w-5 h-5">{item.icon}</span>
            <span className="text-sm font-medium">{item.label}</span>
          </Link>
        ))}
      </nav>
    </aside>
  );
};

export default Sidebar;