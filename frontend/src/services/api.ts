import { MonitoringStats, FixMessage } from '../types/fix';

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

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  user: {
    id: number;
    email: string;
  };
}

// Helper function to get auth headers
// const getAuthHeaders = (): HeadersInit => {
//   const token = localStorage.getItem('token');
//   const headers: Record<string, string> = {
//     'Accept': 'application/json',
//     'Content-Type': 'application/json',
//   };

//   if (token) {
//     headers['Authorization'] = `Bearer ${token}`;
//   }

//   return headers;
// };

// Secure fetch wrapper for authenticated API calls
const securedFetch = async (url: string, options: RequestInit = {}): Promise<Response> => {
  const token = localStorage.getItem('token');

  if (!token) {
    console.error('Authentication token missing for request to:', url);
    throw new Error('Authentication required');
  }

  try {
    const headers = {
      ...options.headers,
      'Authorization': `Bearer ${token}`,
      'Accept': 'application/json',
      'Content-Type': 'application/json',
    };

    const response = await fetch(url, {
      ...options,
      headers,
    });

    if (response.status === 401) {
      console.error('Authentication failed (401) for request to:', url);
      localStorage.removeItem('token');
      window.location.href = '/login';
      throw new Error('Authentication expired');
    }

    if (!response.ok) {
      throw new Error(`API request failed: ${response.statusText}`);
    }

    return response;
  } catch (error) {
    console.error('Error during API request to', url, error);
    throw error;
  }
};

export const api = {
  // Authentication methods
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Login failed');
    }

    const data = await response.json();
    localStorage.setItem('token', data.token);
    return data;
  },

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Registration failed');
    }

    const data = await response.json();
    localStorage.setItem('token', data.token);
    return data;
  },

  async getCurrentUser(): Promise<{ id: number; email: string }> {
    const response = await securedFetch(`${API_BASE_URL}/auth/me`);
    return response.json();
  },

  logout(): void {
    localStorage.removeItem('token');
  },

  // Data methods - all secured with authentication
  async getMessages(params: GetMessagesParams): Promise<FixMessage[]> {
    const queryString = new URLSearchParams();

    // Only add parameters that have values
    if (params.page) queryString.append('page', params.page.toString());
    if (params.pageSize) queryString.append('pageSize', params.pageSize.toString());
    if (params.skipHeartbeats) queryString.append('skipHeartbeats', params.skipHeartbeats.toString());
    if (Array.isArray(params.msgTypes) && params.msgTypes.length > 0) {
      params.msgTypes.forEach(type => {
        queryString.append('msgTypes', type);
      });
    }
    if (params.orderId) queryString.append('orderId', params.orderId);
    if (params.startTime) queryString.append('startTime', params.startTime);
    if (params.endTime) queryString.append('endTime', params.endTime);

    const response = await securedFetch(`${API_BASE_URL}/messages?${queryString}`);
    return response.json();
  },

  async getMessagesWithPagination(params: GetMessagesParams): Promise<MessagesPaginationResponse> {
    const queryString = new URLSearchParams();

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

    const response = await securedFetch(`${API_BASE_URL}/messages/paginated?${queryString}`);
    return response.json();
  },

  async getMonitoringStats(): Promise<MonitoringStats> {
    const response = await securedFetch(`${API_BASE_URL}/fixlog/monitoring`);
    return response.json();
  },

  async getConnectors(type?: Connector['type']): Promise<Connector[]> {
    const params = type ? `?type=${type}` : '';
    try {
      const response = await securedFetch(`${API_BASE_URL}/connectors${params}`);
      return response.json();
    } catch (error) {
      console.error("Failed to fetch connectors:", error);
      return [];
    }
  },

  async getConnector(id: number): Promise<Connector> {
    const response = await securedFetch(`${API_BASE_URL}/connectors/${id}`);
    return response.json();
  },

  async createConnector(request: CreateConnectorRequest): Promise<Connector> {
    const response = await securedFetch(`${API_BASE_URL}/connectors`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
    return response.json();
  },

  async updateConnector(id: number, request: UpdateConnectorRequest): Promise<void> {
    await securedFetch(`${API_BASE_URL}/connectors/${id}`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
  },

  async deleteConnector(id: number): Promise<void> {
    await securedFetch(`${API_BASE_URL}/connectors/${id}`, {
      method: 'DELETE',
    });
  },

  async toggleConnector(id: number): Promise<void> {
    await securedFetch(`${API_BASE_URL}/connectors/${id}/toggle`, {
      method: 'POST',
    });
  },

  async uploadFixFiles(files: File[]): Promise<{ sessionId: string, message: string }> {
    const token = localStorage.getItem('token');

    if (!token) {
      throw new Error('Authentication required');
    }

    const formData = new FormData();
    files.forEach(file => {
      formData.append('files', file);
    });

    const response = await fetch(`${API_BASE_URL}/files/upload`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
      body: formData,
    });

    if (response.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
      throw new Error('Authentication expired');
    }

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

    const response = await securedFetch(`${API_BASE_URL}/OrderFlow?${queryString}`);
    return response.json();
  }
};