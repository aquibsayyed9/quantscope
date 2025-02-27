import React, { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from './card';
import { Upload, Loader2, FileUp } from 'lucide-react';
import { api } from '../../services/api';

interface FileUploadProps {
    fileConfig: {
      directory: string;
      pattern: string;
      scanIntervalMs: number;
      processSubDirectories: boolean;
    };
    // onFileConfigChange: (key: string, value: string | number | boolean) => void;
    onSaveConfig: () => Promise<void>;
    saving: boolean;
    error: string | null;
  }

  const FileUpload: React.FC<FileUploadProps> = ({ 
    // fileConfig, 
    // onFileConfigChange, 
    // onSaveConfig, 
    // saving, 
    error 
  }) => {
  const [files, setFiles] = useState([]);
  const [uploading, setUploading] = useState(false);

  const handleFileChange = (e: any) => {
    const selectedFiles: any = Array.from(e.target.files);
    if (selectedFiles.length > 5) {
      alert('Please select up to 5 files only');
      return;
    }
    setFiles(selectedFiles);
  };

  const handleFileUpload = async () => {
    try {
      setUploading(true);
      
      // Upload each file
      await Promise.all(
        files.map(file => api.uploadFixFiles([file]))
      );
      
      setFiles([]);
      alert('Files uploaded successfully!');
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to upload files');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Directory Scanner Section */}
      {/*
      <Card>
        <CardHeader>
          <div className="flex items-center space-x-2">
            <FolderOpen className="h-5 w-5 text-blue-500" />
            <CardTitle>Directory Scanner</CardTitle>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Directory Path</label>
              <input
                type="text"
                className="w-full px-3 py-2 border rounded-md"
                placeholder="/path/to/fix/logs"
                value={fileConfig.directory}
                onChange={(e) => onFileConfigChange('directory', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">File Pattern</label>
              <input
                type="text"
                className="w-full px-3 py-2 border rounded-md"
                placeholder="*.fix"
                value={fileConfig.pattern}
                onChange={(e) => onFileConfigChange('pattern', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Scan Interval (ms)</label>
              <input
                type="number"
                className="w-full px-3 py-2 border rounded-md"
                placeholder="5000"
                value={fileConfig.scanIntervalMs}
                onChange={(e) => onFileConfigChange('scanIntervalMs', parseInt(e.target.value))}
              />
            </div>
            <div className="space-y-2">
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  className="rounded border-gray-300"
                  checked={fileConfig.processSubDirectories}
                  onChange={(e) => onFileConfigChange('processSubDirectories', e.target.checked)}
                />
                <span className="text-sm font-medium">Process Subdirectories</span>
              </label>
            </div>

            <div className="flex justify-end">
              <button
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 flex items-center gap-2 disabled:opacity-50"
                onClick={onSaveConfig}
                disabled={saving}
              >
                {saving ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Upload className="h-4 w-4" />
                )}
                {saving ? 'Saving...' : 'Save Configuration'}
              </button>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="relative py-4">
        <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-gray-300"></div>
        </div>
        <div className="relative flex justify-center">
            <span className="bg-white px-4 text-sm text-gray-500">
            OR
            </span>
        </div>
        </div>
      */}

      
      {/* Direct File Upload Section */}
      <Card>
        <CardHeader>
          <div className="flex items-center space-x-2">
            <FileUp className="h-5 w-5 text-blue-500" />
            <CardTitle>Direct File Upload</CardTitle>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div className="border-2 border-dashed rounded-lg p-6 text-center">
              <input
                type="file"
                multiple
                accept=".fix,.log,.txt,.out,.csv,.in,.data,.dat"
                onChange={handleFileChange}
                className="hidden"
                id="file-upload"
              />
              <label 
                htmlFor="file-upload"
                className="cursor-pointer flex flex-col items-center space-y-2"
              >
                <Upload className="h-8 w-8 text-gray-400" />
                <span className="text-sm text-gray-600">
                  Click to select files or drag and drop
                </span>
                <span className="text-xs text-gray-500">
                  Up to 5 files supported
                </span>
              </label>
            </div>

            {files.length > 0 && (
              <div className="space-y-2">
                <div className="text-sm font-medium">Selected Files:</div>
                <ul className="text-sm text-gray-600">
                  {files.map((file: any, index) => (
                    <li key={index} className="flex items-center space-x-2">
                      <FileUp className="h-4 w-4" />
                      <span>{file.name}</span>
                    </li>
                  ))}
                </ul>
                <button
                  className="w-full px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 flex items-center justify-center gap-2 disabled:opacity-50"
                  onClick={handleFileUpload}
                  disabled={uploading}
                >
                  {uploading ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <Upload className="h-4 w-4" />
                  )}
                  {uploading ? 'Uploading...' : 'Upload Files'}
                </button>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {error && (
        <div className="text-red-600 text-sm">
          {error}
        </div>
      )}
    </div>
  );
};

export default FileUpload;