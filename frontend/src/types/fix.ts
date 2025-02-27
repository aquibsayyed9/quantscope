export interface FixMessage {
    id: string;
    timestamp: string;
    sessionId: string;
    messageType: string;
    senderCompId: string;
    targetCompId: string;
    seqNum: number;
    rawMessage: string;
    parsedMessage: Record<string, string | number | boolean | null | undefined>;
  }
  
  export interface MonitoringStats {
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