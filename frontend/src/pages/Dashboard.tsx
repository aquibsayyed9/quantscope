import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { MessageCircle, Activity, AlertTriangle, Clock, Users, BarChart } from 'lucide-react';
import { api } from '../services/api';
import OrderFlow from '../components/ui/orderFlow';

// Updated interface to match backend DTO
interface MonitoringStats {
  messageRates: Array<{
    sessionKey: string;
    messageCount: number;
  }>;
  lastMessageTimestamps: Array<{
    sessionKey: string;
    lastTimestamp: string;
  }>;
  sequenceResets: number;
  rejectedMessages: number;
  exchangeResets: number;
  sessionMessages: number;
}

const Dashboard = () => {
  const [stats, setStats] = useState<MonitoringStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const data = await api.getMonitoringStats();
        setStats(data);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch stats');
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
    const interval = setInterval(fetchStats, 30000);
    return () => clearInterval(interval);
  }, []);

  // Calculate metrics
  const activeSessions = stats?.messageRates.length ?? 0;
  const totalMessages = stats?.sessionMessages ?? 0;
  const totalErrors = (stats?.rejectedMessages ?? 0) + (stats?.sequenceResets ?? 0);
  const sessionHealth = activeSessions > 0 ? 
    ((activeSessions - (stats?.exchangeResets ?? 0)) / activeSessions * 100) : 0;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>
      
      {/* Original Stats Row */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Messages
            </CardTitle>
            <MessageCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {loading ? (
                <span className="text-muted-foreground">Loading...</span>
              ) : error ? (
                <span className="text-red-500">Error</span>
              ) : (
                totalMessages.toLocaleString()
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Sessions
            </CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {loading ? (
                <span className="text-muted-foreground">Loading...</span>
              ) : error ? (
                <span className="text-red-500">Error</span>
              ) : (
                activeSessions
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Errors
            </CardTitle>
            <AlertTriangle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {loading ? (
                <span className="text-muted-foreground">Loading...</span>
              ) : error ? (
                <span className="text-red-500">Error</span>
              ) : (
                totalErrors
              )}
            </div>
            {!loading && !error && totalErrors > 0 && (
              <p className="text-sm text-muted-foreground mt-1">
                {stats?.rejectedMessages} rejected, {stats?.sequenceResets} resets
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Active Sessions Overview */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between pb-2">
          <CardTitle className="text-sm font-medium">Active Sessions Overview</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {loading ? (
              <div className="text-muted-foreground">Loading session data...</div>
            ) : error ? (
              <div className="text-red-500">Failed to load session data</div>
            ) : (
              stats?.messageRates.map((session, index) => (
                <div key={index} className="flex items-center justify-between p-2 bg-slate-50 rounded-lg text-sm">
                  <div className="flex-1 min-w-0">
                    <div className="font-medium truncate">{session.sessionKey}</div>
                    <div className="text-muted-foreground">
                      {session.messageCount} msgs | Last: {
                        new Date(stats.lastMessageTimestamps
                          .find(t => t.sessionKey === session.sessionKey)!
                          .lastTimestamp).toLocaleTimeString()
                      }
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </CardContent>
      </Card>

      {/* Enhanced Stats Row */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Latest Session Health
            </CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {loading ? "Loading..." : `${sessionHealth.toFixed(1)}%`}
            </div>
            <p className="text-sm text-muted-foreground">
              {stats?.exchangeResets ?? 0} exchange resets
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Message Rate
            </CardTitle>
            <BarChart className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {loading ? "Loading..." : stats?.messageRates.reduce((sum, rate) => sum + rate.messageCount, 0)}
            </div>
            <p className="text-sm text-muted-foreground">Messages per session</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Sequence Gaps
            </CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {loading ? "Loading..." : stats?.sequenceResets ?? 0}
            </div>
            <p className="text-sm text-muted-foreground">Total sequence resets</p>
          </CardContent>
        </Card>
      </div>

      {/* Separator */}
      <div className="border-t border-slate-200" />

      {/* Order Flow Section */}
      <Card className="col-span-3">
        <CardHeader>
          <CardTitle className="text-lg font-medium">Order Flow Timeline</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-96 w-full overflow-y-auto">
            <OrderFlow />
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default Dashboard;