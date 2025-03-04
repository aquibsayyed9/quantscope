import { MonitoringStats, FixMessage } from '../types/fix';

// const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || 'http://localhost:7263/api';
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7263/api';

export interface Connector {
  id: number;
  name: string;
  type: 'FileUpload' | 'Nats' | 'Kafka' | 'FixGateway';
  isActive: boolean;
  configuration: unknown;
  createdAt: string;
  lastModifiedAt?: string;
  lastConnectedAt?: string;
  status: string;
  errorMessage?: string;
}

export interface CreateConnectorRequest {
  name: string;
  type: Connector['type'];
  configuration: unknown;
  isActive?: boolean;
}

export interface UpdateConnectorRequest {
  name?: string;
  configuration?: unknown;
  isActive?: boolean;
}

export interface OrderFlowState {
  status: string;
  timestamp: string;
  details?: string;
}

export interface OrderFlow {
  orderId: string;
  symbol: string;
  side: string;
  quantity: number;
  price: number;
  states: OrderFlowState[];
  createdAt: string;
}

export interface OrderFlowResponse {
  orders: OrderFlow[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
}

export interface MessagesPaginationResponse {
  messages: FixMessage[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export interface GetMessagesParams {
  msgTypes?: string[];
  orderId?: string;
  startTime?: string;
  endTime?: string;
  page: number;
  pageSize: number;
  skipHeartbeats: boolean;
}

export const api = {
  async getMessages(params: GetMessagesParams): Promise<FixMessage[]> {
    const queryString = new URLSearchParams();
    
    // Only add parameters that have values
    if (params.page) queryString.append('page', params.page.toString());
    if (params.pageSize) queryString.append('pageSize', params.pageSize.toString());
    if (params.skipHeartbeats) queryString.append('skipHeartbeats', params.skipHeartbeats.toString());
    if (Array.isArray(params.msgTypes) && params.msgTypes.length > 0) {
      // Add each message type as a separate query parameter with the same key
      params.msgTypes.forEach(type => {
        queryString.append('msgTypes', type);
      });
    }
    if (params.orderId) queryString.append('orderId', params.orderId);
    if (params.startTime) queryString.append('startTime', params.startTime);
    if (params.endTime) queryString.append('endTime', params.endTime);
    
    const response = await fetch(`${API_BASE_URL}/messages?${queryString}`, {
      headers: {
        'Accept': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch messages');
    }

    return response.json();
  },

  // Updated method that would return pagination metadata
  // Note: This assumes your backend API supports this response format
  async getMessagesWithPagination(params: GetMessagesParams): Promise<MessagesPaginationResponse> {
    const queryString = new URLSearchParams();
    
    // Only add parameters that have values
    if (params.page) queryString.append('page', params.page.toString());
    if (params.pageSize) queryString.append('pageSize', params.pageSize.toString());
    if (Array.isArray(params.msgTypes) && params.msgTypes.length > 0) {
      params.msgTypes.forEach(type => {
        queryString.append('msgTypes', type);
      });
    }
    if (params.orderId) queryString.append('orderId', params.orderId);
    if (params.startTime) queryString.append('startTime', params.startTime);
    if (params.endTime) queryString.append('endTime', params.endTime);
    
    const response = await fetch(`${API_BASE_URL}/messages/paginated?${queryString}`, {
      headers: {
        'Accept': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch messages');
    }

    return response.json();
  },

  async getMonitoringStats(): Promise<MonitoringStats> {
    const response = await fetch(`${API_BASE_URL}/fixlog/monitoring`, {
      headers: {
        'Accept': 'application/json',
      },
    });
    if (!response.ok) {
      throw new Error('Failed to fetch monitoring stats');
    }
    return response.json();
  },

  // New connector endpoints
  async getConnectors(type?: Connector['type']): Promise<Connector[]> {
    const params = type ? `?type=${type}` : '';
    const response = await fetch(`${API_BASE_URL}/connectors${params}`, {
      headers: {
        'Accept': 'application/json',
      },
    });
    if (!response.ok) {
      //throw new Error('Failed to fetch connectors');
      console.log("Failed to fetch connectors");
      return []
    }
    return response.json();
  },

  async getConnector(id: number): Promise<Connector> {
    const response = await fetch(`${API_BASE_URL}/connectors/${id}`, {
      headers: {
        'Accept': 'application/json',
      },
    });
    if (!response.ok) {
      throw new Error('Failed to fetch connector');
    }
    return response.json();
  },

  async createConnector(request: CreateConnectorRequest): Promise<Connector> {
    const response = await fetch(`${API_BASE_URL}/connectors`, {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });
    if (!response.ok) {
      throw new Error('Failed to create connector');
    }
    return response.json();
  },

  async updateConnector(id: number, request: UpdateConnectorRequest): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/connectors/${id}`, {
      method: 'PUT',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });
    if (!response.ok) {
      throw new Error('Failed to update connector');
    }
  },

  async deleteConnector(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/connectors/${id}`, {
      method: 'DELETE',
      headers: {
        'Accept': 'application/json',
      },
    });
    if (!response.ok) {
      throw new Error('Failed to delete connector');
    }
  },

  async toggleConnector(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/connectors/${id}/toggle`, {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
      },
    });
    if (!response.ok) {
      throw new Error('Failed to toggle connector');
    }
  },

  async uploadFixFiles(files: File[]): Promise<{ sessionId: string, message: string }> {
    const formData = new FormData();
    files.forEach(file => {
        formData.append('files', file); // Changed from 'file' to 'files' to match backend
    });

    const response = await fetch(`${API_BASE_URL}/files/upload`, {
        method: 'POST',
        body: formData,
    });

    if (!response.ok) {
        throw new Error('Failed to upload files');
    }
    return response.json();
  },

  async getOrderFlow(params: {
    orderId?: string;
    symbol?: string;
    pageSize?: number;
    pageNumber?: number;
    trackingMode?: string;
    clOrderId?: string;
  }): Promise<OrderFlowResponse> {
    const queryString = new URLSearchParams();
    
    if (params.orderId) queryString.append('orderId', params.orderId);
    queryString.append('trackingMode', params.trackingMode ?? 'OrderId');
    if (params.symbol) queryString.append('symbol', params.symbol);
    if (params.pageSize) queryString.append('pageSize', params.pageSize.toString());
    if (params.pageNumber) queryString.append('pageNumber', params.pageNumber.toString());

    const response = await fetch(`${API_BASE_URL}/OrderFlow?${queryString}`, {
      headers: {
        'Accept': 'application/json',
      },
    });
    
    if (!response.ok) {
      throw new Error('Failed to fetch order flow');
    }
    
    return response.json();
  },
};