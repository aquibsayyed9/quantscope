export const MessageTypes: Record<string, { label: string; color: string }> = {
    '0': { label: 'Heartbeat', color: 'bg-gray-100' },
    '1': { label: 'Test Request', color: 'bg-gray-100' },
    '2': { label: 'Resend Request', color: 'bg-yellow-100' },
    '4': { label: 'Sequence Reset', color: 'bg-yellow-100' },
    '5': { label: 'Logout', color: 'bg-red-100' },
    '8': { label: 'Execution Report', color: 'bg-blue-100' },
    'D': { label: 'New Order Single', color: 'bg-green-100' },
    'F': { label: 'Order Cancel Request', color: 'bg-red-100' },
    'G': { label: 'Order Cancel/Replace', color: 'bg-orange-100' },
    'AE': { label: 'Trade Capture Report', color: 'bg-purple-100' },
    'f': { label: 'Security Status', color: 'bg-gray-100' },
    'd': { label: 'Security Definition', color: 'bg-green-100' },
    'A': { label: 'Logon', color: 'bg-green-100' }
  };
  
  export const ExecTypes: Record<string, { label: string; color: string }> = {
    '0': { label: 'New', color: 'bg-green-100' },
    '4': { label: 'Canceled', color: 'bg-red-100' },
    '5': { label: 'Replace', color: 'bg-orange-100' },
    '8': { label: 'Rejected', color: 'bg-red-100' },
    'F': { label: 'Trade', color: 'bg-blue-100' },
    'D': { label: 'Restated', color: 'bg-orange-100' },
    '3': { label: 'Done For Day', color: 'bg-blue-100' },
  };