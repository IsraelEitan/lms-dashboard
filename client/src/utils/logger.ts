/**
 * Frontend Logger Utility
 * Provides consistent, colorful console logging across the application
 */

type LogLevel = 'info' | 'success' | 'warning' | 'error' | 'debug';

interface LogConfig {
  level: LogLevel;
  emoji: string;
  color: string;
  bgColor: string;
}

const logConfigs: Record<LogLevel, LogConfig> = {
  info: {
    level: 'info',
    emoji: 'ℹ️',
    color: '#3b82f6',
    bgColor: '#dbeafe',
  },
  success: {
    level: 'success',
    emoji: '✅',
    color: '#10b981',
    bgColor: '#d1fae5',
  },
  warning: {
    level: 'warning',
    emoji: '⚠️',
    color: '#f59e0b',
    bgColor: '#fef3c7',
  },
  error: {
    level: 'error',
    emoji: '❌',
    color: '#ef4444',
    bgColor: '#fee2e2',
  },
  debug: {
    level: 'debug',
    emoji: '🔍',
    color: '#8b5cf6',
    bgColor: '#ede9fe',
  },
};

class Logger {
  private isDevelopment = import.meta.env.DEV;

  private log(level: LogLevel, message: string, ...args: any[]) {
    if (!this.isDevelopment && level === 'debug') {
      return; // Skip debug logs in production
    }

    const config = logConfigs[level];
    const timestamp = new Date().toLocaleTimeString();
    
    const styles = [
      `color: ${config.color}`,
      `background: ${config.bgColor}`,
      'padding: 2px 6px',
      'border-radius: 3px',
      'font-weight: bold',
    ].join(';');

    console.log(
      `%c${config.emoji} ${level.toUpperCase()} %c ${timestamp} %c ${message}`,
      styles,
      'color: #6b7280; font-weight: normal;',
      'color: inherit;',
      ...args
    );
  }

  info(message: string, ...args: any[]) {
    this.log('info', message, ...args);
  }

  success(message: string, ...args: any[]) {
    this.log('success', message, ...args);
  }

  warning(message: string, ...args: any[]) {
    this.log('warning', message, ...args);
  }

  error(message: string, error?: any, ...args: any[]) {
    this.log('error', message, ...args);
    if (error) {
      console.error('Error details:', error);
    }
  }

  debug(message: string, ...args: any[]) {
    this.log('debug', message, ...args);
  }

  // API-specific logging
  apiRequest(method: string, url: string, data?: any) {
    this.info(`🌐 API ${method} → ${url}`, data);
  }

  apiResponse(method: string, url: string, status: number, data?: any) {
    if (status >= 200 && status < 300) {
      this.success(`🌐 API ${method} ← ${url} [${status}]`, data);
    } else if (status >= 400) {
      this.error(`🌐 API ${method} ← ${url} [${status}]`, data);
    } else {
      this.info(`🌐 API ${method} ← ${url} [${status}]`, data);
    }
  }

  apiError(method: string, url: string, error: any) {
    this.error(`🌐 API ${method} ✗ ${url}`, error);
  }

  // Component lifecycle logging
  componentMount(componentName: string) {
    this.debug(`🔄 Component mounted: ${componentName}`);
  }

  componentUnmount(componentName: string) {
    this.debug(`🔄 Component unmounted: ${componentName}`);
  }
}

export const logger = new Logger();

