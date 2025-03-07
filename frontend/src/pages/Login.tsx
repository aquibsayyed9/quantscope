import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/authContext';
import { Card, CardContent, CardHeader, CardTitle, CardFooter } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Alert, AlertDescription } from '../components/ui/alert';
import { Loader2 } from 'lucide-react';

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const { login, error, loading } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();

    interface LocationState {
        from: {
            pathname: string;
        };
    }

    const from = (location.state as LocationState)?.from?.pathname || '/';
    
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const response = await login(email, password);
            localStorage.setItem('token', response.token);
            // Small delay to ensure auth state is updated
            setTimeout(() => {
                navigate(from, { replace: true });
            }, 100);
        } catch {
            // Error is handled in auth context
        }
    };

    return (
        <div className="flex justify-center items-center min-h-screen bg-slate-50">
            <div className="w-full max-w-md px-4">
                <div className="text-center mb-8">
                    <h1 className="text-3xl font-bold text-slate-900">FIX Analyzer</h1>
                    <p className="text-slate-500 mt-2">Sign in to access your dashboard</p>
                </div>
                
                <Card className="border-none shadow-lg">
                    <CardHeader className="pb-4">
                        <CardTitle className="text-xl text-center text-slate-800">Welcome back</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <form onSubmit={handleSubmit} className="space-y-4">
                            {error && (
                                <Alert variant="destructive" className="bg-red-50 text-red-800 border border-red-200">
                                    <AlertDescription>{error}</AlertDescription>
                                </Alert>
                            )}
                            
                            <div className="space-y-2">
                                <Label htmlFor="email" className="text-sm font-medium text-slate-700">
                                    Email address
                                </Label>
                                <Input
                                    id="email"
                                    type="email"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    required
                                    placeholder="name@company.com"
                                    className="w-full rounded-md border-slate-300 focus:border-blue-500 focus:ring-blue-500"
                                />
                            </div>
                            
                            <div className="space-y-2">
                                <div className="flex items-center justify-between">
                                    <Label htmlFor="password" className="text-sm font-medium text-slate-700">
                                        Password
                                    </Label>
                                    <a href="#" className="text-sm font-medium text-blue-600 hover:text-blue-500">
                                        Forgot password?
                                    </a>
                                </div>
                                <Input
                                    id="password"
                                    type="password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    required
                                    className="w-full rounded-md border-slate-300 focus:border-blue-500 focus:ring-blue-500"
                                />
                            </div>
                            
                            <Button
                                type="submit"
                                disabled={loading}
                                className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-md transition-colors"
                            >
                                {loading ? (
                                    <>
                                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                        Signing in...
                                    </>
                                ) : (
                                    'Sign In'
                                )}
                            </Button>
                        </form>
                    </CardContent>
                    <CardFooter className="flex justify-center border-t border-slate-100 pt-4">
                        <p className="text-sm text-slate-600">
                            Don't have an account?{' '}
                            <a href="/register" className="font-medium text-blue-600 hover:text-blue-500">
                                Create one now
                            </a>
                        </p>
                    </CardFooter>
                </Card>
                
                <p className="mt-8 text-center text-xs text-slate-500">
                    &copy; {new Date().getFullYear()} FIX Analyzer. All rights reserved.
                </p>
            </div>
        </div>
    );
};

export default Login;