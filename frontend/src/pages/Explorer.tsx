import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { MessageTypeSelect } from '../components/ui/MessageTypeSelect';
import { X, Calendar, ChevronLeft, ChevronRight } from 'lucide-react';
import { api } from '../services/api';
import { FIX_TAGS } from '../types/fix-tags';
import { ExecTypes, MessageTypes } from '../constants/fix-message-types';

interface FixMessage {
  id: number;
  timestamp: string;
  msgType: string;
  sequenceNumber: number;
  senderCompID: string;
  targetCompID: string;
  sessionId: string;
  fields: Record<string, string>;
  createdAt: string;
  execType: string;
  price: number;
  orderQty: number;
  side: string;
  leavesQty: number;
  cumQty: number;
  symbol: string;
  ordStatus: string;
}

interface FilterParams {
  msgTypes: string[];
  orderId?: string;
  startTime?: string;
  endTime?: string;
  page: number;
  pageSize: number;
  skipHeartbeats?: boolean;
}

interface PaginationResponse {
  messages: FixMessage[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

// Helper function to map API FixMessage to our component's FixMessage
const mapApiMessageToComponentMessage = (apiMessage: any): FixMessage => {
  // Create default/empty fields object
  const fields: Record<string, string> = {};

  // If apiMessage has tag values anywhere, try to find them
  try {
    // Check if there's a fields property
    if (apiMessage.fields && typeof apiMessage.fields === 'object') {
      Object.assign(fields, apiMessage.fields);
    }
    // Check if there's a tags property
    else if (apiMessage.tags && typeof apiMessage.tags === 'object') {
      Object.assign(fields, apiMessage.tags);
    }
    // Check if there are any numeric keys directly on the object
    else {
      Object.keys(apiMessage).forEach(key => {
        if (/^\d+$/.test(key) && apiMessage[key] !== undefined) {
          fields[key] = String(apiMessage[key]);
        }
      });
    }
  } catch (err) {
    console.error('Error mapping FIX message fields:', err);
  }

  // Now construct our FixMessage with safe fallbacks for everything
  return {
    id: typeof apiMessage.id === 'number' ? apiMessage.id : 0,
    timestamp: typeof apiMessage.timestamp === 'string' ? apiMessage.timestamp : new Date().toISOString(),
    msgType: fields['35'] || 'Unknown',
    sequenceNumber: parseInt(fields['34'] || '0'),
    senderCompID: fields['49'] || '',
    targetCompID: fields['56'] || '',
    sessionId: typeof apiMessage.sessionId === 'string' ? apiMessage.sessionId : '',
    fields: fields,
    createdAt: typeof apiMessage.createdAt === 'string' ? apiMessage.createdAt :
      (typeof apiMessage.timestamp === 'string' ? apiMessage.timestamp : new Date().toISOString()),
    execType: fields['150'] || '',

    price: apiMessage.price || parseFloat(fields['44'] || '0'),
    orderQty: apiMessage.orderQty || parseFloat(fields['38'] || '0'),
    side: apiMessage.side || fields['54'] || '',
    leavesQty: apiMessage.leavesQty || parseFloat(fields['151'] || '0'),
    cumQty: apiMessage.cumQty || parseFloat(fields['14'] || '0'),
    symbol: apiMessage.symbol || fields['55'] || '',
    ordStatus: apiMessage.ordStatus || fields['39'] || ''
  };
};

// Helper function to format the side (1=Buy, 2=Sell)
const formatSide = (side: string) => {
  if (side === '1') return 'Buy';
  if (side === '2') return 'Sell';
  return side;
};

// Helper function to format prices and quantities
const formatNumber = (num: number) => {
  if (num === undefined || num === null) return '-';
  return num.toFixed(2);
};
const Explorer = () => {
  const [paginationData, setPaginationData] = useState<PaginationResponse>({
    messages: [],
    totalCount: 0,
    totalPages: 0,
    currentPage: 1,
    pageSize: 20
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedMessage, setSelectedMessage] = useState<FixMessage | null>(null);
  const [filters, setFilters] = useState<FilterParams>({
    msgTypes: [],
    page: 1,
    pageSize: 20,
    skipHeartbeats: false
  });

  useEffect(() => {
    const fetchMessages = async () => {
      try {
        setLoading(true);

        const data = await api.getMessages({
          msgTypes: filters.msgTypes,
          orderId: filters.orderId,
          startTime: filters.startTime,
          endTime: filters.endTime,
          page: filters.page,
          pageSize: filters.pageSize,
          skipHeartbeats: filters.skipHeartbeats ?? false
        });

        // Map the API response to our component's FixMessage format
        const mappedMessages = Array.isArray(data)
          ? data.map(mapApiMessageToComponentMessage)
          : []; // Handle case where data might not be an array

        // If we got a full page of results, there might be more
        // If we got less than a full page, we're on the last page
        const isFullPage = mappedMessages.length === filters.pageSize;
        // Calculate total pages based on response
        const calculatedTotalPages = isFullPage
          ? Math.max(filters.page + 1, 2) // At least one more page if we have a full page
          : filters.page; // Current page is the last page if not full

        // Estimate total count based on what we know
        const estimatedTotalCount = isFullPage
          ? calculatedTotalPages * filters.pageSize // Assume all pages are full
          : ((filters.page - 1) * filters.pageSize) + mappedMessages.length; // Actual count

        setPaginationData({
          messages: mappedMessages,
          totalCount: estimatedTotalCount,
          totalPages: calculatedTotalPages,
          currentPage: filters.page,
          pageSize: filters.pageSize
        });

        setError(null);
      } catch (err) {
        console.error('Error in fetchMessages:', err);
        setError(err instanceof Error ? err.message : 'Failed to fetch messages');
      } finally {
        setLoading(false);
      }
    };

    fetchMessages();
  }, [filters]);

  const handleFilterChange = (key: keyof FilterParams, value: string) => {
    if (key === 'startTime' || key === 'endTime') {
      // If the value is empty, set it to empty string
      // Otherwise convert to ISO string for backend
      const dateValue = value ? new Date(value).toISOString() : '';
      setFilters(prev => ({
        ...prev,
        [key]: dateValue,
        page: 1
      }));
    } else {
      setFilters(prev => ({
        ...prev,
        [key]: value,
        page: 1
      }));
    }
  };

  const handleToggleSkipHeartbeats = () => {
    setFilters(prev => ({
      ...prev,
      skipHeartbeats: !prev.skipHeartbeats,
      page: 1
    }));
  };

  const handleClearFilters = () => {
    setFilters({
      msgTypes: [],
      page: 1,
      pageSize: filters.pageSize,
      skipHeartbeats: false
    });
  };

  const handlePageChange = (newPage: number) => {
    if (newPage < 1 || newPage > paginationData.totalPages) return;

    setFilters(prev => ({
      ...prev,
      page: newPage
    }));
  };

  const handlePageSizeChange = (newSize: number) => {
    setFilters(prev => ({
      ...prev,
      pageSize: newSize,
      page: 1
    }));
  };

  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString();
  };

  const renderField = (tag: string, value: string) => {
    return (
      <div key={tag} className="py-1 border-b last:border-b-0">
        <div className="text-sm text-gray-900">
          <span className="font-medium">{FIX_TAGS[tag] || `Tag ${tag}`}:</span>{' '}
          {value}
        </div>
      </div>
    );
  };

  const MessageTypeDisplay = ({ msgType }: { msgType: string }) => {
    const messageInfo = MessageTypes[msgType] || { label: `MsgType: ${msgType}`, color: 'bg-gray-100' };

    return (
      <span className={`inline-block px-2 py-1 rounded text-sm ${messageInfo.color}`}>
        {messageInfo.label}
      </span>
    );
  };

  const ExecTypeDisplay = ({ execType }: { execType: string | null }) => {
    if (!execType) return null;

    const execInfo = ExecTypes[execType] || { label: `ExecType: ${execType}`, color: 'bg-gray-100' };

    return (
      <span className={`inline-block px-2 py-1 rounded text-sm ${execInfo.color}`}>
        {execInfo.label}
      </span>
    );
  };

  const renderPagination = () => {
    const { currentPage, totalPages, messages, pageSize } = paginationData;

    // If we have no messages, don't show pagination
    if (messages.length === 0) {
      return null;
    }

    // If we're not sure about total pages, just show a simpler next/prev pagination
    const showSimplePagination = messages.length < pageSize; // If we have less than a full page

    if (showSimplePagination) {
      return (
        <div className="flex items-center justify-between border-t border-gray-200 px-4 py-3 sm:px-6 mt-4">
          <div className="flex items-center justify-between w-full">
            <button
              onClick={() => handlePageChange(currentPage - 1)}
              disabled={currentPage === 1}
              className={`relative inline-flex items-center rounded-md border px-4 py-2 text-sm font-medium ${currentPage === 1
                ? 'border-gray-300 bg-gray-100 text-gray-400'
                : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50'
                }`}
            >
              Previous
            </button>
            <span className="text-sm text-gray-700">
              Page {currentPage}
            </span>
            <button
              onClick={() => handlePageChange(currentPage + 1)}
              disabled={messages.length < pageSize}
              className={`relative inline-flex items-center rounded-md border px-4 py-2 text-sm font-medium ${messages.length < pageSize
                ? 'border-gray-300 bg-gray-100 text-gray-400'
                : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50'
                }`}
            >
              Next
            </button>
          </div>
        </div>
      );
    }

    // Create an array of page numbers to show
    let pageNumbers = [];

    if (totalPages <= 7) {
      // If fewer than 7 pages, show all
      pageNumbers = Array.from({ length: totalPages }, (_, i) => i + 1);
    } else {
      // Always show first and last page, and some around current page
      if (currentPage <= 3) {
        pageNumbers = [1, 2, 3, 4, 5, '...', totalPages];
      } else if (currentPage >= totalPages - 2) {
        pageNumbers = [1, '...', totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
      } else {
        pageNumbers = [1, '...', currentPage - 1, currentPage, currentPage + 1, '...', totalPages];
      }
    }

    return (
      <div className="flex items-center justify-between border-t border-gray-200 px-4 py-3 sm:px-6 mt-4">
        <div className="flex flex-1 justify-between sm:hidden">
          <button
            onClick={() => handlePageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className={`relative inline-flex items-center rounded-md border px-4 py-2 text-sm font-medium ${currentPage === 1
              ? 'border-gray-300 bg-gray-100 text-gray-400'
              : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50'
              }`}
          >
            Previous
          </button>
          <button
            onClick={() => handlePageChange(currentPage + 1)}
            disabled={currentPage === totalPages || messages.length < pageSize}
            className={`relative ml-3 inline-flex items-center rounded-md border px-4 py-2 text-sm font-medium ${currentPage === totalPages || messages.length < pageSize
              ? 'border-gray-300 bg-gray-100 text-gray-400'
              : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50'
              }`}
          >
            Next
          </button>
        </div>
        <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
          <div>
            <p className="text-sm text-gray-700">
              Showing <span className="font-medium">{(currentPage - 1) * paginationData.pageSize + 1}</span> to{' '}
              <span className="font-medium">
                {Math.min(currentPage * paginationData.pageSize, paginationData.totalCount)}
              </span>{' '}
              of about <span className="font-medium">{paginationData.totalCount}+</span> results
            </p>
          </div>
          <div>
            <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm" aria-label="Pagination">
              <button
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1}
                className={`relative inline-flex items-center rounded-l-md px-2 py-2 ${currentPage === 1
                  ? 'bg-gray-100 text-gray-400'
                  : 'bg-white text-gray-500 hover:bg-gray-50'
                  }`}
              >
                <span className="sr-only">Previous</span>
                <ChevronLeft className="h-5 w-5" aria-hidden="true" />
              </button>

              {pageNumbers.map((pageNum, idx) => (
                pageNum === '...' ? (
                  <span
                    key={`ellipsis-${idx}`}
                    className="relative inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white"
                  >
                    ...
                  </span>
                ) : (
                  <button
                    key={`page-${pageNum}`}
                    onClick={() => handlePageChange(pageNum as number)}
                    aria-current={pageNum === currentPage ? 'page' : undefined}
                    className={`relative inline-flex items-center px-4 py-2 text-sm font-medium ${pageNum === currentPage
                      ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                      : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50'
                      }`}
                  >
                    {pageNum}
                  </button>
                )
              ))}

              <button
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage === totalPages || messages.length < pageSize}
                className={`relative inline-flex items-center rounded-r-md px-2 py-2 ${currentPage === totalPages || messages.length < pageSize
                  ? 'bg-gray-100 text-gray-400'
                  : 'bg-white text-gray-500 hover:bg-gray-50'
                  }`}
              >
                <span className="sr-only">Next</span>
                <ChevronRight className="h-5 w-5" aria-hidden="true" />
              </button>
            </nav>
          </div>
        </div>
      </div>
    );
  };

  const renderPageSizeSelector = () => {
    return (
      <div className="flex items-center space-x-2">
        <span className="text-sm text-gray-700">Show</span>
        <select
          className="border rounded-md px-2 py-1 text-sm"
          value={filters.pageSize}
          onChange={(e) => handlePageSizeChange(Number(e.target.value))}
        >
          <option value={10}>10</option>
          <option value={20}>20</option>
          <option value={50}>50</option>
          <option value={100}>100</option>
        </select>
        <span className="text-sm text-gray-700">entries</span>
      </div>
    );
  };

  const OrdStatusDisplay = ({ status }: { status: string | null }) => {
    if (!status) return null;

    const statusMap: Record<string, { label: string, color: string }> = {
      '0': { label: 'New', color: 'bg-blue-100 text-blue-800' },
      '1': { label: 'Partially Filled', color: 'bg-purple-100 text-purple-800' },
      '2': { label: 'Filled', color: 'bg-green-100 text-green-800' },
      '3': { label: 'Done for Day', color: 'bg-gray-100 text-gray-800' },
      '4': { label: 'Canceled', color: 'bg-red-100 text-red-800' },
      '5': { label: 'Replaced', color: 'bg-yellow-100 text-yellow-800' },
      '6': { label: 'Pending Cancel', color: 'bg-orange-100 text-orange-800' },
      '7': { label: 'Stopped', color: 'bg-gray-100 text-gray-800' },
      '8': { label: 'Rejected', color: 'bg-red-100 text-red-800' },
      '9': { label: 'Suspended', color: 'bg-gray-100 text-gray-800' },
      'A': { label: 'Pending New', color: 'bg-blue-100 text-blue-800' },
      'B': { label: 'Calculated', color: 'bg-gray-100 text-gray-800' },
      'C': { label: 'Expired', color: 'bg-red-100 text-red-800' },
      'D': { label: 'Accepted', color: 'bg-green-100 text-green-800' },
      'E': { label: 'Pending Replace', color: 'bg-yellow-100 text-yellow-800' }
    };

    const statusInfo = statusMap[status] || { label: `Status: ${status}`, color: 'bg-gray-100' };

    return (
      <span className={`inline-block px-2 py-1 rounded text-xs ${statusInfo.color}`}>
        {statusInfo.label}
      </span>
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900">Message Explorer</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filters</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Message Type</label>
              <MessageTypeSelect
                selected={filters.msgTypes}
                onChange={(types) => setFilters(prev => ({
                  ...prev,
                  msgTypes: types,
                  page: 1
                }))}
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Order ID</label>
              <input
                type="text"
                className="w-full px-3 py-2 border rounded-md"
                placeholder="Enter Order ID"
                value={filters.orderId}
                onChange={(e) => handleFilterChange('orderId', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Start Time</label>
              <div className="relative">
                <input
                  type="datetime-local"
                  className="w-full px-3 py-2 border rounded-md"
                  value={filters.startTime ? new Date(filters.startTime).toISOString().slice(0, 16) : ''}
                  onChange={(e) => handleFilterChange('startTime', e.target.value)}
                />
                <Calendar className="absolute right-2 top-2 h-5 w-5 text-gray-400" />
              </div>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">End Time</label>
              <div className="relative">
                <input
                  type="datetime-local"
                  className="w-full px-3 py-2 border rounded-md"
                  value={filters.endTime ? new Date(filters.endTime).toISOString().slice(0, 16) : ''}
                  onChange={(e) => handleFilterChange('endTime', e.target.value)}
                />
                <Calendar className="absolute right-2 top-2 h-5 w-5 text-gray-400" />
              </div>
            </div>
          </div>
          <div className="flex justify-end space-x-2 mt-4">
            <button
              className="px-4 py-2 border rounded-md hover:bg-gray-50"
              onClick={handleClearFilters}
            >
              Clear Filters
            </button>
            <button
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              onClick={() => setFilters(prev => ({ ...prev, page: 1 }))}
            >
              Search
            </button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div className="flex items-center space-x-4">
            {renderPageSizeSelector()}

            <div className="flex items-center ml-6">
              <input
                type="checkbox"
                id="skipHeartbeats"
                checked={filters.skipHeartbeats}
                onChange={handleToggleSkipHeartbeats}
                className="h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              />
              <label htmlFor="skipHeartbeats" className="ml-2 text-sm text-gray-700">
                Skip Heartbeats & Test Requests
              </label>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex justify-center items-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <span className="ml-2">Loading messages...</span>
            </div>
          ) : error ? (
            <div className="text-red-500 py-4">{error}</div>
          ) : paginationData.messages.length === 0 ? (
            <div className="text-gray-500 py-4">No messages found</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full border-collapse">
                <thead>
                  <tr className="border-b">
                    <th className="px-4 py-2 text-left">Time</th>
                    <th className="px-4 py-2 text-left">Type</th>
                    <th className="px-4 py-2 text-left">Symbol</th>
                    <th className="px-4 py-2 text-right">Price</th>
                    <th className="px-4 py-2 text-right">Qty</th>
                    <th className="px-4 py-2 text-center">Side</th>
                    <th className="px-4 py-2 text-center">Status</th>
                    <th className="px-4 py-2 text-right">Filled</th>
                    <th className="px-4 py-2 text-right">Leaves</th>
                    <th className="px-4 py-2 text-left">Order ID</th>
                    <th className="px-4 py-2 text-left">ExecType</th>
                    <th className="px-4 py-2 text-left">Fields</th>
                  </tr>
                </thead>
                <tbody>
                  {paginationData.messages.map((message) => (
                    <tr key={message.id} className="border-b hover:bg-gray-50">
                      <td className="px-4 py-2">{formatTimestamp(message.timestamp)}</td>
                      <td className="px-4 py-2">
                        <MessageTypeDisplay msgType={message.msgType} />
                      </td>
                      <td className="px-4 py-2">{message.symbol || '-'}</td>
                      <td className="px-4 py-2 text-right">{message.price ? formatNumber(message.price) : '-'}</td>
                      <td className="px-4 py-2 text-right">{message.orderQty ? formatNumber(message.orderQty) : '-'}</td>
                      <td className="px-4 py-2 text-center">
                        <span
                          className={`inline-block px-2 py-1 rounded text-xs font-medium ${message.side === '1' ? 'bg-green-100 text-green-800' :
                            message.side === '2' ? 'bg-red-100 text-red-800' : 'bg-gray-100'
                            }`}
                        >
                          {formatSide(message.side)}
                        </span>
                      </td>
                      <td className="px-4 py-2 text-center">
                        <OrdStatusDisplay status={message.ordStatus} />
                      </td>
                      <td className="px-4 py-2 text-right">{formatNumber(message.cumQty)}</td>
                      <td className="px-4 py-2 text-right">{formatNumber(message.leavesQty)}</td>
                      <td className="px-4 py-2">{message.fields['37'] || '-'}</td>
                      <td className="px-4 py-2">
                        <ExecTypeDisplay execType={message.execType} />
                      </td>
                      <td className="px-4 py-2">
                        <button
                          className="px-3 py-1 text-sm border rounded-md hover:bg-gray-50"
                          onClick={() => setSelectedMessage(message)}
                        >
                          View Fields
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>

              {renderPagination()}
            </div>
          )}
        </CardContent>
      </Card>

      {selectedMessage && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg w-full max-w-4xl max-h-[80vh] overflow-hidden">
            <div className="p-4 border-b flex justify-between items-center">
              <h3 className="text-lg font-medium flex items-center gap-2">
                <MessageTypeDisplay msgType={selectedMessage.msgType} />
                <span className="text-gray-500">(ID: {selectedMessage.id})</span>
              </h3>
              <button
                onClick={() => setSelectedMessage(null)}
                className="p-1 hover:bg-gray-100 rounded"
              >
                <X className="h-5 w-5" />
              </button>
            </div>
            <div className="p-4 overflow-y-auto max-h-[calc(80vh-4rem)]">
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <h4 className="font-medium mb-2">Order Details</h4>
                    <div className="bg-gray-50 p-3 rounded-md space-y-2">
                      <p><span className="font-medium">Time:</span> {formatTimestamp(selectedMessage.timestamp)}</p>
                      <p><span className="font-medium">Symbol:</span> {selectedMessage.symbol || '-'}</p>
                      <p><span className="font-medium">Order ID:</span> {selectedMessage.fields['37'] || '-'}</p>
                      <p><span className="font-medium">Side:</span> {formatSide(selectedMessage.side)}</p>
                      <p><span className="font-medium">Price:</span> {formatNumber(selectedMessage.price)}</p>
                      <p><span className="font-medium">Quantity:</span> {formatNumber(selectedMessage.orderQty)}</p>
                      <div className="flex items-center gap-2">
                        <span className="font-medium">Status:</span>
                        <OrdStatusDisplay status={selectedMessage.ordStatus} />
                      </div>
                      <div className="flex items-center gap-2">
                        <span className="font-medium">Exec Type:</span>
                        <ExecTypeDisplay execType={selectedMessage.execType} />
                      </div>
                    </div>
                  </div>
                  <div>
                    <h4 className="font-medium mb-2">Execution Details</h4>
                    <div className="bg-gray-50 p-3 rounded-md space-y-2">
                      <p><span className="font-medium">Sequence:</span> {selectedMessage.sequenceNumber}</p>
                      <p><span className="font-medium">Filled Qty:</span> {formatNumber(selectedMessage.cumQty)}</p>
                      <p><span className="font-medium">Remaining Qty:</span> {formatNumber(selectedMessage.leavesQty)}</p>
                      <p><span className="font-medium">Sender:</span> {selectedMessage.senderCompID}</p>
                      <p><span className="font-medium">Target:</span> {selectedMessage.targetCompID}</p>
                      <p><span className="font-medium">Session:</span> {selectedMessage.sessionId}</p>
                    </div>
                  </div>
                </div>
                <div>
                  <h4 className="font-medium mb-2">FIX Fields</h4>
                  <div className="bg-gray-50 p-3 rounded-md divide-y">
                    {Object.entries(selectedMessage.fields)
                      .sort(([a], [b]) => parseInt(a) - parseInt(b))
                      .map(([tag, value]) => renderField(tag, value))
                    }
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Explorer;