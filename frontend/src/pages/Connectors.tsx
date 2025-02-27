import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import FileUpload from '../components/ui/fileUpload';
import { Loader2 } from 'lucide-react';
import { api, Connector, CreateConnectorRequest } from '../services/api';

export interface FileUploadConfig {
  directory: string;
  pattern: string;
  scanIntervalMs: number;
  processSubDirectories: boolean;
}

const Connectors = () => {
  const [_connectors, setConnectors] = useState<Connector[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeConnector, setActiveConnector] = useState<Connector | null>(null);

  const [fileConfig, setFileConfig] = useState<FileUploadConfig>({
    directory: '',
    pattern: '*.fix',
    scanIntervalMs: 5000,
    processSubDirectories: true
  });

  useEffect(() => {
    loadConnectors();
  }, []);

  const loadConnectors = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await api.getConnectors();
      setConnectors(data);
      
      // Find active FileUpload connector if exists
      const fileUploadConnector = data.find(c => c.type === 'FileUpload');
      if (fileUploadConnector) {
        setActiveConnector(fileUploadConnector);
        // Type assertion to make TypeScript happy
        setFileConfig(fileUploadConnector.configuration as FileUploadConfig);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load connectors');
    } finally {
      setLoading(false);
    }
  };

  // const handleFileConfigChange = (key: keyof FileUploadConfig, value: string | number | boolean) => {
  //   setFileConfig(prev => ({
  //     ...prev,
  //     [key]: value
  //   }));
  // };

  const handleSaveFileConfig = async () => {
    try {
      setSaving(true);
      setError(null);

      const request: CreateConnectorRequest = {
        name: 'File Upload Connector',
        type: 'FileUpload',
        configuration: fileConfig,
        isActive: true
      };

      if (activeConnector) {
        // Update existing connector
        await api.updateConnector(activeConnector.id, {
          configuration: fileConfig
        });
      } else {
        // Create new connector
        const newConnector = await api.createConnector(request);
        setActiveConnector(newConnector);
      }

      await loadConnectors();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save configuration');
    } finally {
      setSaving(false);
    }
  };

  const handleToggleConnector = async (connector: Connector) => {
    try {
      setError(null);
      await api.toggleConnector(connector.id);
      await loadConnectors();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to toggle connector');
    }
  };

  const renderFileUploadTab = () => (
    <Card>
      <CardHeader>
        <div className="flex justify-between items-center">
          <CardTitle>File Upload Configuration</CardTitle>
          {activeConnector && (
            <button
              className={`px-3 py-1 rounded-full text-sm ${
                activeConnector.isActive 
                  ? 'bg-green-100 text-green-800' 
                  : 'bg-gray-100 text-gray-800'
              }`}
              onClick={() => handleToggleConnector(activeConnector)}
            >
              {activeConnector.isActive ? 'Active' : 'Inactive'}
            </button>
          )}
        </div>
      </CardHeader>
      <CardContent>
        <FileUpload 
          fileConfig={fileConfig}
          // onFileConfigChange={handleFileConfigChange}
          onSaveConfig={handleSaveFileConfig}
          saving={saving}
          error={error}
        />
      </CardContent>
    </Card>
  );

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900">Connectors</h1>
      </div>

      <Tabs defaultValue="fileUpload" className="space-y-4">
        <TabsList>
          <TabsTrigger value="fileUpload">File Upload</TabsTrigger>
          <TabsTrigger value="nats">NATS</TabsTrigger>
          <TabsTrigger value="kafka">Kafka</TabsTrigger>
          <TabsTrigger value="fixGateway">FIX Gateway</TabsTrigger>
        </TabsList>

        <TabsContent value="fileUpload">
          {renderFileUploadTab()}
        </TabsContent>
        
        <TabsContent value="nats">
          <Card>
            <CardHeader>
              <CardTitle>NATS Configuration</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-gray-500">NATS configuration coming soon...</div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="kafka">
          <Card>
            <CardHeader>
              <CardTitle>Kafka Configuration</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-gray-500">Kafka configuration coming soon...</div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="fixGateway">
          <Card>
            <CardHeader>
              <CardTitle>FIX Gateway Configuration</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-gray-500">FIX Gateway configuration coming soon...</div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default Connectors;