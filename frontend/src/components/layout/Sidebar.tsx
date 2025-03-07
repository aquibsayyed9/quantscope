import { Link, useLocation } from 'react-router-dom';
import { 
  LayoutDashboard, 
  MessageSquare, 
  Settings, 
  CableIcon, 
  LogIn, 
  LogOut, 
  UserPlus,
  ChevronRight
} from 'lucide-react';
import { useAuth } from '../../contexts/authContext';
import { cn } from '../../lib/utils';
import { Avatar, AvatarFallback } from '../ui/avatar';

const Sidebar = () => {
  const location = useLocation();
  const { isAuthenticated, user, logout } = useAuth();

  const navItems = [
    {
      path: '/',
      label: 'Dashboard',
      icon: <LayoutDashboard className="w-5 h-5" />,
    },
    {
      path: '/explorer',
      label: 'Message Explorer',
      icon: <MessageSquare className="w-5 h-5" />,
    },
    {
      path: '/connectors',
      label: 'Connectors',
      icon: <CableIcon className="w-5 h-5" />,
    },
    {
      path: '/settings',
      label: 'Settings',
      icon: <Settings className="w-5 h-5" />,
    },
  ];

  type NavItem = {
    path: string;
    label: string;
    icon: React.ReactNode;
    onClick?: undefined;
  } | {
    path: string;
    label: string;
    icon: React.ReactNode;
    onClick: () => void;
  };

  // Auth navigation items
  const authItems: NavItem[] = isAuthenticated
    ? [
      {
        path: '#',
        label: 'Logout',
        icon: <LogOut className="w-5 h-5" />,
        onClick: logout,
      },
    ]
    : [
      {
        path: '/login',
        label: 'Login',
        icon: <LogIn className="w-5 h-5" />,
      },
      {
        path: '/register',
        label: 'Register',
        icon: <UserPlus className="w-5 h-5" />,
      },
    ];

  // Get user initials for avatar
  const getUserInitials = () => {
    if (!user?.email) return '?';
    return user.email.charAt(0).toUpperCase();
  };

  return (
    <aside className="fixed left-0 top-0 h-screen w-64 bg-slate-900 text-white flex flex-col">
      {/* Logo Area */}
      <div className="px-6 py-6 border-b border-slate-800">
        <h1 className="text-xl font-bold text-white flex items-center gap-2">
          <span className="bg-blue-600 text-white p-1 rounded">FIX</span>
          Analyzer
        </h1>
      </div>
      
      {/* Main Navigation */}
      <nav className="flex-1 py-6 px-3 space-y-1 overflow-y-auto">
        <div className="mb-2 px-3">
          <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">
            Main
          </p>
        </div>
        
        {navItems.map((item) => (
          <Link
            key={item.path}
            to={item.path}
            className={cn(
              "flex items-center gap-3 px-3 py-2.5 rounded-lg transition-all group",
              location.pathname === item.path
                ? "bg-slate-800 text-white"
                : "text-slate-300 hover:bg-slate-800/50 hover:text-white"
            )}
          >
            <span className={cn(
              "transition-colors", 
              location.pathname === item.path ? "text-blue-400" : "text-slate-400 group-hover:text-blue-400"
            )}>
              {item.icon}
            </span>
            <span className="text-sm font-medium flex-1">{item.label}</span>
            {location.pathname === item.path && (
              <ChevronRight className="w-4 h-4 text-blue-400" />
            )}
          </Link>
        ))}
      </nav>
      
      {/* User Section */}
      <div className="border-t border-slate-800 p-4">
        {isAuthenticated && user ? (
          <div className="flex items-center gap-3 mb-4">
            <Avatar className="h-10 w-10 bg-slate-700 text-white">
              <AvatarFallback>{getUserInitials()}</AvatarFallback>
            </Avatar>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-white truncate">
                {user.email}
              </p>
              <p className="text-xs text-slate-400">
                User ID: {user.id}
              </p>
            </div>
          </div>
        ) : null}
        
        {/* Auth Links */}
        <div className="space-y-1">
          {authItems.map((item) =>
            item.onClick ? (
              <button
                key={item.label}
                onClick={item.onClick}
                className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors text-slate-300 hover:bg-slate-800 hover:text-white text-left"
              >
                <span className="text-slate-400">{item.icon}</span>
                <span className="text-sm font-medium">{item.label}</span>
              </button>
            ) : (
              <Link
                key={item.path}
                to={item.path}
                className={cn(
                  "flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors",
                  location.pathname === item.path
                    ? "bg-slate-800 text-white"
                    : "text-slate-300 hover:bg-slate-800 hover:text-white"
                )}
              >
                <span className="text-slate-400">{item.icon}</span>
                <span className="text-sm font-medium">{item.label}</span>
              </Link>
            )
          )}
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;