# Error Handling Guide

This guide explains how error handling works in the LMS Dashboard application.

---

## Overview

The application implements a comprehensive error handling strategy that ensures users have a smooth experience even when things go wrong. The system handles two main categories of errors:

1. **Network Errors** - Backend server unavailable or connection issues
2. **Application Errors** - Runtime errors in React components

---

## Network Error Handling

### The Problem

Without proper error handling, if the backend is down, users would see:
- Multiple browser alert() dialogs (one for each failed API call)
- Confusing error messages in the console
- A broken UI that keeps trying to load data
- No clear way to recover

### The Solution

We implemented a centralized error handling system using:
- **ApiConnectionContext** - Tracks backend connection status
- **ServerErrorPage** - User-friendly error page with auto-retry
- **NetworkError** - Custom error type for network failures
- **API Interceptors** - Detect and handle connection failures

### How It Works

#### 1. Connection Monitoring

The `ApiConnectionContext` monitors the connection status:

```typescript
// client/src/context/ApiConnectionContext.tsx
export const ApiConnectionProvider = ({ children }) => {
  const [isConnected, setIsConnected] = useState(true);
  const [lastError, setLastError] = useState<string | null>(null);

  const checkConnection = async () => {
    try {
      await axios.get(`${API_BASE_URL}/healthz`);
      setIsConnected(true);
    } catch (error) {
      setIsConnected(false);
      setLastError('Cannot connect to server');
    }
  };
  
  // ...
};
```

#### 2. API Service Integration

The API service notifies the context about connection failures:

```typescript
// client/src/services/api.ts
this.client.interceptors.response.use(
  (response) => {
    // Success - notify connected
    if (this.connectionStatusCallback) {
      this.connectionStatusCallback(true);
    }
    return response;
  },
  (error) => {
    // Check for network errors
    if (error.code === 'ERR_NETWORK' || error.code === 'ECONNREFUSED') {
      const networkError = new NetworkError('Unable to connect to server');
      
      // Notify disconnected - this will trigger ServerErrorPage
      if (this.connectionStatusCallback) {
        this.connectionStatusCallback(false, networkError.message);
      }
      
      throw networkError;
    }
    // ... handle other errors
  }
);
```

#### 3. Server Error Page

When disconnected, the entire app is replaced with a friendly error page:

```typescript
// client/src/App.tsx
function AppContent() {
  const { isConnected, checkConnection } = useApiConnection();

  // Show error page if not connected
  if (!isConnected) {
    return <ServerErrorPage onRetry={checkConnection} />;
  }

  // Normal app rendering
  return <BrowserRouter>...</BrowserRouter>;
}
```

### Features

**User-Friendly UI:**
- Clear explanation of the problem
- Helpful troubleshooting steps
- Technical details in an expandable section

**Auto-Retry:**
- Automatically retries connection every 5 seconds
- Shows countdown timer
- User can also manually retry

**Single Error Display:**
- No multiple alerts
- One consistent error page
- Prevents error spam

---

## Application Error Handling

### Error Boundary

React Error Boundaries catch runtime errors in the component tree:

```typescript
// client/src/components/Common/ErrorBoundary.tsx
class ErrorBoundary extends Component {
  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error };
  }

  render() {
    if (this.state.hasError) {
      return <div>Something went wrong...</div>;
    }
    return this.props.children;
  }
}
```

**What It Catches:**
- Rendering errors
- Component lifecycle errors
- Constructor errors

**What It Doesn't Catch:**
- Event handlers (use try-catch)
- Async code (use try-catch)
- Server-side rendering errors
- Errors in the error boundary itself

### API Error Handling

API errors (4xx, 5xx) are handled differently from network errors:

```typescript
try {
  const data = await apiService.getStudents();
  setStudents(data.items);
} catch (error) {
  if (error instanceof NetworkError) {
    // Network error - handled by ApiConnectionContext
    // ServerErrorPage will be shown automatically
  } else {
    // API error (validation, not found, etc.)
    setError(error.title || 'An error occurred');
  }
}
```

**API Error Types:**
- **400 Bad Request** - Validation errors, displayed inline
- **404 Not Found** - Resource not found, show appropriate message
- **409 Conflict** - Duplicate resource, show conflict message
- **422 Unprocessable Entity** - Business logic error
- **500 Internal Server Error** - Server error, show generic message

---

## Error Flow Diagram

```
User Action (e.g., Load Dashboard)
         ↓
    API Request
         ↓
    ┌────────────────┐
    │  Is Backend    │
    │   Running?     │
    └────┬───────┬───┘
         │       │
         No      Yes
         │       │
         ↓       ↓
   Network    Success
    Error      Response
         │       │
         ↓       ↓
 ConnectionStatus  Update UI
   = false         with Data
         │
         ↓
  ServerErrorPage
   (with auto-retry)
```

---

## Testing Error Handling

### Test Network Errors

**Scenario 1: Backend Not Running**

1. Stop the backend server
2. Try to access the application
3. **Expected Result:** ServerErrorPage appears with auto-retry
4. Start the backend
5. **Expected Result:** Page automatically recovers and loads data

**Scenario 2: Backend Crashes During Use**

1. Start with backend running
2. Load some data
3. Stop the backend
4. Navigate to another page (triggers API call)
5. **Expected Result:** ServerErrorPage appears immediately

### Test Application Errors

**Simulate Component Error:**

```typescript
// Temporarily add to any component
throw new Error('Test error');
```

**Expected Result:** ErrorBoundary catches it and shows error UI

### Test API Errors

**Scenario: Validation Error**

1. Try to create a student with invalid email
2. **Expected Result:** Inline error message, no page crash

**Scenario: Duplicate Resource**

1. Try to create a course with an existing code
2. **Expected Result:** 409 Conflict error shown in alert

---

## Best Practices for Developers

### 1. Never Use alert() for Errors

**Bad:**
```typescript
catch (error) {
  alert('Error occurred!'); // ❌ Multiple alerts if backend is down
}
```

**Good:**
```typescript
catch (error) {
  logger.error('Error occurred', error); // ✅ Logged for debugging
  setError(error.title || 'An error occurred'); // ✅ Show in UI
}
```

### 2. Let NetworkError Propagate

**Bad:**
```typescript
catch (error) {
  if (error instanceof NetworkError) {
    setError('Cannot connect'); // ❌ Prevents ServerErrorPage
  }
}
```

**Good:**
```typescript
catch (error) {
  if (error instanceof NetworkError) {
    // ✅ Don't handle it - let ApiConnectionContext handle it
    // ServerErrorPage will appear automatically
    return;
  }
  // Handle other errors
  setError(error.title);
}
```

### 3. Always Log Errors

```typescript
catch (error) {
  logger.error('Failed to load data', error); // ✅ Helps debugging
  // ... handle error
}
```

### 4. Provide Helpful Error Messages

**Bad:**
```typescript
setError('Error'); // ❌ Not helpful
```

**Good:**
```typescript
setError('Unable to load students. Please try again.'); // ✅ Clear and actionable
```

### 5. Handle Loading States

```typescript
const [loading, setLoading] = useState(false);
const [error, setError] = useState<string | null>(null);

const loadData = async () => {
  setLoading(true);
  setError(null); // ✅ Clear previous errors
  try {
    const data = await apiService.getData();
    setData(data);
  } catch (error) {
    if (!(error instanceof NetworkError)) {
      setError(error.title || 'Failed to load data');
    }
  } finally {
    setLoading(false); // ✅ Always clear loading state
  }
};
```

---

## Error Messages Reference

### Network Errors

| Error Code | User Message | Technical Cause |
|------------|-------------|-----------------|
| ERR_NETWORK | "Unable to connect to the server. Please check if the backend is running." | Network unreachable, server not responding |
| ECONNREFUSED | "Unable to connect to the server. Please check if the backend is running." | Connection refused by server |
| ECONNABORTED | "Connection timeout" | Request took longer than 10 seconds |

### API Errors

| Status Code | Example Message | When It Occurs |
|-------------|-----------------|----------------|
| 400 | "Invalid email format" | Validation error |
| 404 | "Student not found" | Resource doesn't exist |
| 409 | "A course with code CS101 already exists" | Duplicate resource |
| 422 | "Student is already enrolled in this course" | Business logic violation |
| 500 | "An unexpected error occurred" | Server error |

---

## Configuration

### API Timeout

Default timeout is 10 seconds. To change:

```typescript
// client/src/services/api.ts
this.client = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000, // 15 seconds
});
```

### Auto-Retry Interval

Default is 5 seconds. To change:

```typescript
// client/src/components/Common/ServerErrorPage.tsx
<ServerErrorPage 
  onRetry={checkConnection}
  autoRetry={true}
  retryInterval={10000} // 10 seconds
/>
```

### Disable Auto-Retry

```typescript
<ServerErrorPage 
  onRetry={checkConnection}
  autoRetry={false} // User must manually retry
/>
```

---

## Troubleshooting

### ServerErrorPage Shows Even When Backend is Running

**Possible Causes:**
1. CORS policy blocking requests
2. Backend running on different port than expected
3. Health check endpoint not responding

**Solution:**
1. Check browser console for CORS errors
2. Verify `VITE_API_URL` in `.env` matches backend URL
3. Ensure backend `/healthz` endpoint is accessible

### Multiple Error Alerts Still Appearing

**Possible Cause:** Old code using `alert()` instead of error state

**Solution:** Search codebase for `alert(` and replace with proper error handling

### Error Boundary Not Catching Errors

**Possible Cause:** Error occurs in event handler or async code

**Solution:** Wrap event handlers and async code in try-catch:

```typescript
const handleClick = async () => {
  try {
    await doSomething();
  } catch (error) {
    logger.error('Error in handler', error);
    setError('Operation failed');
  }
};
```

---

## Summary

**Key Features:**
- ✅ Single, user-friendly error page for server issues
- ✅ No multiple error dialogs
- ✅ Auto-retry with countdown
- ✅ ErrorBoundary for component errors
- ✅ Comprehensive logging
- ✅ Clear error messages

**Developer Guidelines:**
- Never use `alert()` for errors
- Let `NetworkError` propagate to `ApiConnectionContext`
- Always log errors with `logger.error()`
- Provide clear, actionable error messages
- Handle loading and error states properly

---

For more information, see:
- [Frontend README](../client/README.md)
- [Testing Guide](TESTING_GUIDE.md)
- Source: `client/src/context/ApiConnectionContext.tsx`
- Source: `client/src/components/Common/ServerErrorPage.tsx`
- Source: `client/src/components/Common/ErrorBoundary.tsx`

