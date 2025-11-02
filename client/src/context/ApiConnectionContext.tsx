import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import axios from 'axios';

interface ApiConnectionContextType {
  isConnected: boolean;
  isChecking: boolean;
  lastError: string | null;
  checkConnection: () => Promise<void>;
  setConnectionStatus: (connected: boolean, error?: string) => void;
}

const ApiConnectionContext = createContext<ApiConnectionContextType | undefined>(undefined);

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5225/api/v1';

export const ApiConnectionProvider = ({ children }: { children: ReactNode }) => {
  const [isConnected, setIsConnected] = useState(true);
  const [isChecking, setIsChecking] = useState(false);
  const [lastError, setLastError] = useState<string | null>(null);

  const checkConnection = async () => {
    setIsChecking(true);
    try {
      // Try to hit the health check endpoint or dashboard stats
      await axios.get(`${API_BASE_URL.replace('/api/v1', '')}/healthz`, {
        timeout: 5000,
      });
      setIsConnected(true);
      setLastError(null);
    } catch (error) {
      setIsConnected(false);
      if (axios.isAxiosError(error)) {
        if (error.code === 'ERR_NETWORK' || error.code === 'ECONNREFUSED') {
          setLastError('Cannot connect to server');
        } else if (error.code === 'ECONNABORTED') {
          setLastError('Connection timeout');
        } else {
          setLastError(error.message || 'Unknown error');
        }
      } else {
        setLastError('Unknown error');
      }
    } finally {
      setIsChecking(false);
    }
  };

  const setConnectionStatus = (connected: boolean, error?: string) => {
    setIsConnected(connected);
    if (error) {
      setLastError(error);
    } else if (connected) {
      setLastError(null);
    }
  };

  useEffect(() => {
    // Initial connection check
    checkConnection();
  }, []);

  return (
    <ApiConnectionContext.Provider
      value={{
        isConnected,
        isChecking,
        lastError,
        checkConnection,
        setConnectionStatus,
      }}
    >
      {children}
    </ApiConnectionContext.Provider>
  );
};

export const useApiConnection = () => {
  const context = useContext(ApiConnectionContext);
  if (!context) {
    throw new Error('useApiConnection must be used within ApiConnectionProvider');
  }
  return context;
};

