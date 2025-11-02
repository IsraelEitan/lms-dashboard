# LMS Dashboard - Frontend

**React 18 | TypeScript | Vite**

A modern, production-ready React frontend demonstrating best practices in component architecture, state management, type safety, and user experience design.

---

## 🌟 Features

### Core Functionality
- 📚 **Course Management**: Full CRUD operations with real-time updates
- 👥 **Student Management**: Manage student profiles with validation
- ✅ **Enrollment Management**: Assign students to courses with cascade delete support
- 📊 **Dashboard**: Real-time statistics with auto-refresh
- 🔄 **Pagination**: Efficient data browsing with page size controls
- 🎨 **Modern UI**: Clean, responsive design with professional UX

### Technical Features
- ⚡ **Lightning-Fast**: Built with Vite (instant HMR)
- 🎯 **Type-Safe**: 100% TypeScript coverage
- 📱 **Responsive**: Mobile-first design approach
- ♿ **Accessible**: Keyboard navigation and screen reader support
- 🎨 **Custom Components**: Reusable UI component library
- 🚀 **Optimized**: Code splitting and lazy loading
- 💅 **Modern CSS**: CSS Variables with dark mode support
- 🔔 **User Feedback**: Loading states, success/error notifications
- 🗑️ **Smart Dialogs**: Custom confirmation dialogs matching app theme

---

## 🛠️ Tech Stack

| Category | Technology | Purpose |
|----------|-----------|---------|
| **Framework** | React 18 | Modern hooks-based architecture |
| **Language** | TypeScript 5.5 | Type safety and IntelliSense |
| **Build Tool** | Vite 5 | Fast dev server, instant HMR |
| **Routing** | React Router 6 | Client-side navigation |
| **HTTP Client** | Axios | API communication with interceptors |
| **Styling** | CSS3 + Variables | Modern, maintainable styles |
| **Dev Tools** | ESLint + TypeScript | Code quality enforcement |

---

## Prerequisites

- Node.js 18+ and npm/yarn/pnpm
- Backend API running on `http://localhost:5000`

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment

Copy `.env.example` to `.env` and update if needed:

```bash
cp .env.example .env
```

The default configuration points to `http://localhost:5000/api`.

### 3. Start Development Server

```bash
npm run dev
```

The app will be available at `http://localhost:3000`.

### 4. Build for Production

```bash
npm run build
```

The production build will be in the `dist` directory.

### 5. Preview Production Build

```bash
npm run preview
```

## 📁 Project Structure

```
client/
├── src/
│   ├── components/                    # 🎨 Reusable UI Components
│   │   ├── Common/                   # Generic reusable components
│   │   │   ├── Alert.tsx            # Success/error notifications
│   │   │   ├── ConfirmDialog.tsx    # Custom confirmation dialog
│   │   │   ├── EmptyState.tsx       # Empty data state UI
│   │   │   ├── Loading.tsx          # Loading spinner component
│   │   │   ├── Modal.tsx            # Reusable modal wrapper
│   │   │   └── Pagination.tsx       # Pagination controls
│   │   │
│   │   ├── Courses/                  # Course-specific components
│   │   │   ├── CourseForm.tsx       # Create/edit course form
│   │   │   └── CourseList.tsx       # Course list with actions
│   │   │
│   │   ├── Students/                 # Student-specific components
│   │   │   ├── StudentForm.tsx      # Create/edit student form
│   │   │   └── StudentList.tsx      # Student list with actions
│   │   │
│   │   ├── Enrollments/              # Enrollment-specific components
│   │   │   ├── EnrollmentForm.tsx   # Enroll student in course
│   │   │   └── EnrollmentList.tsx   # Enrollment list with remove action
│   │   │
│   │   └── Layout/                   # Layout components
│   │       ├── Layout.tsx           # Main app layout wrapper
│   │       └── Sidebar.tsx          # Navigation sidebar
│   │
│   ├── pages/                        # 📄 Page Components (Route handlers)
│   │   ├── Dashboard.tsx            # Statistics overview page
│   │   ├── Courses.tsx              # Course management page
│   │   ├── Students.tsx             # Student management page
│   │   └── Enrollments.tsx          # Enrollment management page
│   │
│   ├── services/                     # 🌐 External Services
│   │   └── api.ts                   # Centralized API client (Axios)
│   │
│   ├── types/                        # 📐 TypeScript Type Definitions
│   │   └── index.ts                 # Shared types (Student, Course, etc.)
│   │
│   ├── styles/                       # 🎨 Global Styles
│   │   └── index.css                # CSS variables, animations, utilities
│   │
│   ├── App.tsx                       # Main app component (routing)
│   └── main.tsx                      # Entry point (ReactDOM render)
│
├── public/                           # Static assets (served as-is)
│   └── vite.svg
│
├── index.html                        # HTML template
├── vite.config.ts                    # Vite build configuration
├── tsconfig.json                     # TypeScript compiler options
├── tsconfig.node.json                # TypeScript for Node (Vite config)
├── package.json                      # Dependencies & scripts
└── README.md                         # This file
```

### Component Architecture

**Feature-Based Organization**:
- Components grouped by feature (Courses, Students, Enrollments)
- Common components shared across features
- Each feature has its own form and list components

**Component Hierarchy**:
```
App (routing)
  └── Layout (sidebar + content area)
      ├── Dashboard (statistics cards)
      ├── Courses Page
      │   ├── CourseList (table)
      │   ├── CourseForm (modal)
      │   └── ConfirmDialog (delete)
      ├── Students Page
      │   ├── StudentList (table)
      │   ├── StudentForm (modal)
      │   └── ConfirmDialog (delete)
      └── Enrollments Page
          ├── EnrollmentList (table)
          ├── EnrollmentForm (modal)
          └── ConfirmDialog (delete)
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## Features Overview

### Dashboard
- View system statistics
- Quick action buttons
- Overview of courses, students, and enrollments

### Courses
- List all courses with pagination
- Add new courses
- Edit existing courses
- Delete courses
- Form validation

### Students
- List all students with pagination
- Add new students
- Edit student information
- Delete students
- Email validation

### Enrollments
- List all enrollments with pagination
- Enroll students in courses
- Remove enrollments
- View student and course details

---

## 🎯 Key Design Decisions

### 1. **TypeScript for Type Safety**

**Decision**: Use TypeScript throughout the application.

**Example**:
```typescript
// ✅ DO: Strong typing prevents bugs
interface Student {
  id: string;
  name: string;
  email: string;
}

const loadStudents = async (): Promise<PagedResponse<Student>> => {
  const response = await apiService.getStudents();
  return response;  // TypeScript ensures correct shape
};

// ❌ DON'T: Untyped JavaScript
const loadStudents = async () => {
  const response = await apiService.getStudents();
  return response;  // Could be anything!
};
```

**Benefits**:
- ✅ Catch errors at compile-time, not runtime
- ✅ IntelliSense and auto-completion
- ✅ Self-documenting code
- ✅ Safer refactoring

---

### 2. **Centralized API Service**

**Decision**: All API calls go through a single `apiService` in `services/api.ts`.

**Implementation**:
```typescript
// services/api.ts
class ApiService {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1',
      headers: { 'Content-Type': 'application/json' }
    });
  }

  async getStudents(page = 1, pageSize = 10): Promise<PagedResponse<Student>> {
    const response = await this.client.get<PagedResponse<Student>>('/students', {
      params: { page, pageSize }
    });
    return response.data;
  }
}

export const apiService = new ApiService();
```

**Benefits**:
- ✅ Single source of truth for API configuration
- ✅ Easy to add interceptors for auth/logging
- ✅ Consistent error handling
- ✅ Simple to mock for testing

---

### 3. **Component Composition Pattern**

**Decision**: Build complex UIs from small, focused components.

**Example**:
```typescript
// ✅ DO: Small, reusable components
<Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Add Student">
  <StudentForm onSubmit={handleSubmit} initialData={selectedStudent} />
</Modal>

// ❌ DON'T: Monolithic components
// Everything in one 500-line component
```

**Benefits**:
- ✅ Easy to test individual components
- ✅ Reusable across pages
- ✅ Clear separation of concerns
- ✅ Easier to maintain

---

### 4. **Custom Hooks for State Management**

**Decision**: Use React hooks (`useState`, `useEffect`, `useLocation`) for state.

**Example**:
```typescript
const Dashboard = () => {
  const [stats, setStats] = useState<Stats | null>(null);
  const [loading, setLoading] = useState(true);
  const location = useLocation();

  useEffect(() => {
    // Reload stats when navigating back to dashboard
    loadStats();
  }, [location.pathname]);  // ✅ Dependency ensures refresh
};
```

**Benefits**:
- ✅ No external state management library needed (simpler)
- ✅ React's built-in features are sufficient
- ✅ Less boilerplate than Redux/MobX

---

### 5. **Pagination with Reset on Create**

**Decision**: When creating a new item, reset to page 1 so user sees it immediately.

**Implementation**:
```typescript
const handleSubmit = async (data: any) => {
  if (selectedStudent) {
    // Update: Stay on current page
    await apiService.updateStudent(selectedStudent.id, data);
    loadStudents();  // Reload current page
  } else {
    // Create: Go to page 1
    await apiService.createStudent(data);
    setCurrentPage(1);
    await loadStudents(1);  // ✅ User sees new item
  }
};
```

**Rationale**:
- ✅ **UX**: Users expect to see newly created items
- ✅ **Intuitive**: No confusion about "where did my item go?"

---

### 6. **Custom Confirmation Dialog (Not Native confirm())**

**Decision**: Build custom `ConfirmDialog` component instead of using `window.confirm()`.

**Example**:
```typescript
<ConfirmDialog
  isOpen={confirmDelete.isOpen}
  title="Delete Student"
  message={
    <span>
      Are you sure you want to delete <strong>{student?.name}</strong>?
      <br /><br />
      This action will also remove all enrollments and cannot be undone.
    </span>
  }
  confirmText="Delete Student"
  type="danger"
  onConfirm={handleDeleteConfirm}
  onCancel={handleDeleteCancel}
  isLoading={!!deletingId}
/>
```

**Rationale**:
- ✅ **Professional**: Matches app design system
- ✅ **Accessible**: Better keyboard and screen reader support
- ✅ **UX**: Loading states, better messaging
- ✅ **Consistent**: Same look & feel as other modals

**Comparison**:
```typescript
// ❌ Native dialog (ugly, blocking, inconsistent)
if (window.confirm('Delete student?')) {
  deleteStudent(id);
}

// ✅ Custom dialog (beautiful, non-blocking, branded)
<ConfirmDialog ... />
```

---

### 7. **CSS Variables for Theming**

**Decision**: Use CSS custom properties for consistent styling.

**Implementation**:
```css
:root {
  /* Colors */
  --primary-color: #4f46e5;
  --secondary-color: #10b981;
  --danger-color: #ef4444;
  
  /* Typography */
  --font-family: system-ui, -apple-system, sans-serif;
  
  /* Spacing */
  --spacing-sm: 0.5rem;
  --spacing-md: 1rem;
  --spacing-lg: 2rem;
}

.btn-primary {
  background-color: var(--primary-color);  /* ✅ Easy to theme */
}
```

**Benefits**:
- ✅ **Consistency**: Colors defined once
- ✅ **Themeable**: Easy to add dark mode
- ✅ **Maintainable**: Change one variable, update everywhere

---

### 8. **Loading and Error States**

**Decision**: Always show loading and error states for better UX.

**Example**:
```typescript
const [loading, setLoading] = useState(true);
const [error, setError] = useState<string | null>(null);

useEffect(() => {
  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiService.getStudents();
      setStudents(data);
    } catch (err) {
      setError('Failed to load students');
    } finally {
      setLoading(false);
    }
  };
  load();
}, []);

// UI
if (loading) return <Loading />;
if (error) return <Alert type="error" message={error} />;
return <StudentList students={students} />;
```

**Benefits**:
- ✅ **User Feedback**: Users know what's happening
- ✅ **Professional**: Handles edge cases gracefully
- ✅ **Accessibility**: Screen readers announce states

---

### 9. **Responsive Design with Mobile-First**

**Decision**: Build for mobile first, then enhance for larger screens.

**Implementation**:
```css
/* ✅ Mobile first (default) */
.container {
  padding: 1rem;
}

/* Tablet */
@media (min-width: 768px) {
  .container {
    padding: 2rem;
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .container {
    max-width: 1200px;
    margin: 0 auto;
  }
}
```

**Benefits**:
- ✅ **Performance**: Mobile users get minimal CSS
- ✅ **Progressive Enhancement**: Works on all devices
- ✅ **Future-Proof**: Mobile traffic increasing

---

### 10. **Real-Time Dashboard Updates**

**Decision**: Dashboard stats update when navigating back from other pages.

**Implementation**:
```typescript
const Dashboard = () => {
  const location = useLocation();

  useEffect(() => {
    // Reload whenever location changes
    loadStats();
  }, [location.pathname]);  // ✅ Detects navigation
};
```

**Rationale**:
- ✅ **Accuracy**: Always shows current data
- ✅ **UX**: Users see their changes reflected
- ✅ **Simple**: No need for global state management

---

## 🌐 API Integration

### Centralized API Service

All backend communication goes through `services/api.ts`:

```typescript
export const apiService = {
  // Students
  getStudents: (page, pageSize) => Promise<PagedResponse<Student>>,
  getStudent: (id) => Promise<Student>,
  createStudent: (data) => Promise<Student>,
  updateStudent: (id, data) => Promise<Student>,
  deleteStudent: (id) => Promise<void>,
  
  // Courses
  getCourses: (page, pageSize) => Promise<PagedResponse<Course>>,
  // ... etc
};
```

### Error Handling Strategy

**Consistent Error Format**:
```typescript
interface ApiError {
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

// In components
try {
  await apiService.createStudent(data);
  setSuccess('Student created successfully');
} catch (err) {
  const apiError = err as ApiError;
  setError(apiError.title || 'Failed to create student');
}
```

**Benefits**:
- ✅ RFC 7807 ProblemDetails from backend
- ✅ User-friendly error messages
- ✅ Validation errors displayed per field

---

## 🎨 Design Principles

### User Experience
- **Feedback**: Loading states, success/error messages
- **Clarity**: Clear labels, helpful placeholders
- **Consistency**: Same patterns across all pages
- **Forgiveness**: Confirmation dialogs for destructive actions
- **Efficiency**: Keyboard shortcuts, pagination

### Visual Design
- **Modern**: Clean, minimalist interface
- **Accessible**: High contrast, keyboard navigation
- **Responsive**: Mobile-first, works on all devices
- **Professional**: Consistent spacing, typography, colors

### Performance
- **Fast Initial Load**: Vite's optimized bundling
- **Smooth Interactions**: CSS animations, no jank
- **Optimized Images**: Minimal assets
- **Code Splitting**: Lazy loading for routes (future)

---

## 🌐 Browser Support

| Browser | Version | Support |
|---------|---------|---------|
| Chrome | Latest | ✅ Full |
| Firefox | Latest | ✅ Full |
| Safari | Latest | ✅ Full |
| Edge | Latest | ✅ Full |
| Mobile Safari | iOS 13+ | ✅ Full |
| Chrome Mobile | Latest | ✅ Full |

---

## 🛠️ Development Best Practices

When adding new features:

### ✅ DO:
- Use TypeScript types for all props and state
- Create small, focused components (< 200 lines)
- Add loading and error states for async operations
- Use meaningful variable and component names
- Extract repeated logic into custom hooks
- Add comments for complex business logic
- Test manually before committing

### ❌ DON'T:
- Use `any` type (defeats the purpose of TypeScript)
- Create massive components (split into smaller ones)
- Ignore error states (handle all edge cases)
- Hardcode API URLs (use environment variables)
- Skip prop validation (use TypeScript interfaces)

### Example: Adding a New Feature

```typescript
// 1. Define types
interface Teacher {
  id: string;
  name: string;
  department: string;
}

// 2. Add API service method
async getTeachers(): Promise<PagedResponse<Teacher>> {
  const response = await this.client.get('/teachers');
  return response.data;
}

// 3. Create component
const Teachers = () => {
  const [teachers, setTeachers] = useState<PagedResponse<Teacher>>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const data = await apiService.getTeachers();
        setTeachers(data);
      } catch (err) {
        setError('Failed to load teachers');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading) return <Loading />;
  if (error) return <Alert type="error" message={error} />;
  return <TeacherList teachers={teachers.items} />;
};
```

---

## 📚 Additional Resources

- [React Documentation](https://react.dev/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [Vite Guide](https://vitejs.dev/guide/)
- [React Router Docs](https://reactrouter.com/)
- [Axios Documentation](https://axios-http.com/docs/intro)

---

## 🤝 Contributing

Contributions welcome! Please:
1. Follow existing code patterns
2. Use TypeScript strictly (no `any`)
3. Test your changes manually
4. Update types if API contracts change

---

<div align="center">

**Built with modern React best practices to demonstrate production-ready frontend development**

[Backend README](../../src/Lms.Api/README.md) • [Main README](../../README.md)

</div>

