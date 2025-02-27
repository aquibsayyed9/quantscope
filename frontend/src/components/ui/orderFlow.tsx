import React, { useState, useEffect } from 'react';
import { Search } from 'lucide-react';
import { Input } from './input';
import { api } from '../../services/api';
import type { OrderFlow } from '../../services/api';

// Define the type for the order state
interface OrderState {
  status: string;
  timestamp: string | number;
  details?: string;
}

// Define the type for the OrderFlowTimeline props
interface OrderFlowTimelineProps {
  order: {
    orderId: string;
    symbol: string;
    side: string;
    quantity: number;
    price: number;
    states: OrderState[];
  }
}

const OrderFlowTimeline = ({ order }: OrderFlowTimelineProps) => {
  const getStatusColor = (status: string): string => {
    switch (status) {
      case 'New': return 'bg-blue-500';
      case 'Ack': return 'bg-yellow-500';
      case 'Partial': return 'bg-orange-500';
      case 'Filled': return 'bg-green-500';
      case 'Rejected': return 'bg-red-500';
      default: return 'bg-gray-500';
    }
  };

  return (
    <div className="mt-4">
      <div className="flex items-center text-sm">
        <span className="font-medium">{`Order: ${order.orderId} - ${order.symbol}`}</span>
        <span className="ml-2 text-muted-foreground">
          {`${order.side} ${order.quantity}@${order.price.toFixed(2)}`}
        </span>
      </div>
      
      <div className="flex items-center mt-2 space-x-2">
        {order.states.map((state: OrderState, index: number) => (
          <React.Fragment key={`${state.status}-${state.timestamp}`}>
            <div className="flex flex-col items-center">
              <div className={`p-3 rounded ${getStatusColor(state.status)} text-white text-sm min-w-24`}>
                <div className="font-medium">{state.status}</div>
                <div className="text-xs">{new Date(state.timestamp).toLocaleTimeString()}</div>
                {state.details && (
                  <div className="text-xs mt-1">{state.details}</div>
                )}
              </div>
            </div>
            {index < order.states.length - 1 && (
              <div className="h-px w-8 bg-gray-300 mt-6" />
            )}
          </React.Fragment>
        ))}
      </div>
    </div>
  );
};

const OrderFlow = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [orders, setOrders] = useState<OrderFlow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);

  const fetchOrders = async (query = '', page = 1) => {
    try {
      setLoading(true);
      
      const params: Record<string, any> = {
        pageSize,
        pageNumber: page
      };

      if (query) {
        // Try to determine if the query is a symbol or orderId
        const isSymbol = /^[A-Za-z]+$/.test(query);
        if (isSymbol) {
          params.symbol = query.toUpperCase();
        } else {
          params.orderId = query;
        }
      }

      const data = await api.getOrderFlow(params);
      
      setOrders(data.orders);
      setPageNumber(data.currentPage);
      setError(null);
    } catch (err) {
      console.error('Error fetching orders:', err);
      setError(err instanceof Error ? err.message : 'An error occurred');
      setOrders([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // Debounce the search query
    const timeoutId = setTimeout(() => {
      setPageNumber(1); // Reset to first page on new search
      fetchOrders(searchQuery, 1);
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchQuery]);

  return (
    <div className="p-4">
      <div className="relative">
        <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search Order ID or Symbol..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-9"
        />
      </div>
      
      <div className="mt-6">
        <h3 className="text-lg font-medium mb-4">Latest Orders</h3>
        {loading && orders.length === 0 ? (
          <div className="text-center text-muted-foreground">Loading orders...</div>
        ) : error ? (
          <div className="text-center text-red-500">
            Error loading orders: {error}
            <br />
            <button 
              onClick={() => fetchOrders(searchQuery, pageNumber)}
              className="mt-2 text-blue-500 underline"
            >
              Try again
            </button>
          </div>
        ) : orders.length === 0 ? (
          <div className="text-center text-muted-foreground">No orders found</div>
        ) : (
          <>
            <div className="space-y-6">
              {orders.map(order => (
                <OrderFlowTimeline key={order.orderId} order={order} />
              ))}
            </div>
            {orders.length >= pageSize && (
              <button
                onClick={() => fetchOrders(searchQuery, pageNumber + 1)}
                className="mt-4 w-full p-2 text-center text-blue-500 hover:text-blue-600"
                disabled={loading}
              >
                {loading ? 'Loading...' : 'Load More'}
              </button>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default OrderFlow;