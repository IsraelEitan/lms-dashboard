# LMS Dashboard - Comprehensive Testing Guide

This guide provides step-by-step instructions for testing all features of the LMS Dashboard application.

## Prerequisites

- Backend API running on `http://localhost:5225`
- Frontend UI running on `http://localhost:3000`
- Modern web browser (Chrome, Firefox, Edge, or Safari)

## Quick Start

### Option 1: Use the Startup Script (Recommended)

```powershell
.\start-app.ps1
```

This will start both servers in separate windows.

### Option 2: Manual Startup

**Terminal 1 (Backend):**
```bash
cd src/Lms.Api
dotnet run
```

**Terminal 2 (Frontend):**
```bash
cd client
npm install
npm run dev
```

Wait 10-15 seconds, then open `http://localhost:3000` in your browser.

## Automated API Testing

Run the automated API test script:

```powershell
.\test-app.ps1
```

This will test all backend API endpoints including:
- ✅ Creating students
- ✅ Creating courses
- ✅ Updating records
- ✅ Creating enrollments
- ✅ Listing all resources
- ✅ Getting single records
- ✅ Pagination
- ✅ Idempotency

## Manual UI Testing

### Test 1: Dashboard Page

1. **Navigate to Dashboard**
   - Open `http://localhost:3000` in your browser
   - You should see the dashboard with statistics cards

2. **Verify Dashboard Elements**
   - [ ] Dashboard title is displayed
   - [ ] Three statistics cards show (Courses, Students, Enrollments)
   - [ ] Cards show counts (may be 0 initially)
   - [ ] Quick action buttons are visible
   - [ ] All cards are clickable and navigate to respective pages

3. **Test Navigation**
   - [ ] Click on the Courses card → Should navigate to /courses
   - [ ] Click on Students card → Should navigate to /students
   - [ ] Click on Enrollments card → Should navigate to /enrollments
   - [ ] Use sidebar navigation to switch between pages

### Test 2: Student Management

#### 2.1 Create Students

1. **Navigate to Students Page**
   - Click "Students" in the sidebar
   - Should see Students page with "+ Add Student" button

2. **Open Create Student Form**
   - Click "+ Add Student" button
   - Modal dialog should appear with form

3. **Test Form Validation**
   - Try to submit empty form → Should see validation errors
   - Enter name with less than 2 characters → Should see error
   - Enter invalid email (e.g., "notanemail") → Should see error
   - [ ] Verify all validation messages appear correctly

4. **Create Valid Students**
   Create at least 3 students with the following data:
   
   **Student 1:**
   - Name: `John Doe`
   - Email: `john.doe@example.com`
   
   **Student 2:**
   - Name: `Jane Smith`
   - Email: `jane.smith@example.com`
   
   **Student 3:**
   - Name: `Bob Johnson`
   - Email: `bob.johnson@example.com`

5. **Verify Success**
   - [ ] Success message appears after creation
   - [ ] Modal closes automatically
   - [ ] New student appears in the list
   - [ ] Student details are correct

#### 2.2 List Students

1. **Verify Student List**
   - [ ] All created students are displayed
   - [ ] Student names are shown
   - [ ] Email addresses are displayed
   - [ ] Student IDs are shown as badges
   - [ ] Edit and Delete buttons are visible

#### 2.3 Edit Student

1. **Open Edit Form**
   - Click "Edit" button on any student
   - Modal should appear with pre-filled data

2. **Update Student**
   - Change name to `John Updated Doe`
   - Change email to `john.updated@example.com`
   - Click "Update Student"

3. **Verify Update**
   - [ ] Success message appears
   - [ ] Modal closes
   - [ ] Updated data is shown in the list

#### 2.4 Test Pagination (Optional)

If you have more than 10 students:
- [ ] Pagination controls appear at bottom
- [ ] "Next" button is enabled
- [ ] Click "Next" → Shows next page
- [ ] Page number updates correctly
- [ ] "Previous" button works

#### 2.5 Delete Student (Save for Last)

1. **Delete a Student**
   - Click "Delete" on any student
   - Confirm deletion in the browser alert

2. **Verify Deletion**
   - [ ] Success message appears
   - [ ] Student is removed from list
   - [ ] Total count updates

### Test 3: Course Management

#### 3.1 Create Courses

1. **Navigate to Courses Page**
   - Click "Courses" in sidebar
   - Should see Courses page with "+ Add Course" button

2. **Open Create Course Form**
   - Click "+ Add Course"
   - Modal with form should appear

3. **Test Form Validation**
   - Try to submit empty form → Should see validation errors
   - Enter title with less than 3 characters → Should see error
   - Enter description with less than 10 characters → Should see error
   - Leave instructor name empty → Should see error
   - [ ] Verify all validation messages

4. **Create Valid Courses**
   Create at least 3 courses:
   
   **Course 1:**
   - Title: `Introduction to Computer Science`
   - Description: `Learn the fundamentals of programming and computer science`
   - Instructor: `Prof. Alan Turing`
   
   **Course 2:**
   - Title: `Web Development Bootcamp`
   - Description: `Master modern web development with React, Node.js, and databases`
   - Instructor: `Dr. Ada Lovelace`
   
   **Course 3:**
   - Title: `Data Structures and Algorithms`
   - Description: `Deep dive into algorithms, data structures, and problem solving`
   - Instructor: `Prof. Donald Knuth`

5. **Verify Success**
   - [ ] Success message appears
   - [ ] Modal closes
   - [ ] Course appears in list
   - [ ] All details are correct

#### 3.2 List Courses

1. **Verify Course List**
   - [ ] All created courses displayed
   - [ ] Course titles shown
   - [ ] Descriptions visible
   - [ ] Instructor names displayed
   - [ ] Edit and Delete buttons present

#### 3.3 Edit Course

1. **Open Edit Form**
   - Click "Edit" on any course
   - Modal appears with pre-filled data

2. **Update Course**
   - Update title, description, or instructor
   - Click "Update Course"

3. **Verify Update**
   - [ ] Success message
   - [ ] Modal closes
   - [ ] Changes reflected in list

#### 3.4 Delete Course (Save for Last)

1. **Delete a Course**
   - Click "Delete" on any course
   - Confirm deletion

2. **Verify Deletion**
   - [ ] Success message
   - [ ] Course removed from list

### Test 4: Enrollment Management

#### 4.1 Prerequisites Check

Before testing enrollments:
- [ ] At least 2 students exist
- [ ] At least 2 courses exist
- If not, create them first

#### 4.2 Create Enrollments

1. **Navigate to Enrollments Page**
   - Click "Enrollments" in sidebar
   - Should see "+ Enroll Student" button

2. **Open Enrollment Form**
   - Click "+ Enroll Student"
   - Modal with dropdowns should appear

3. **Test Form Validation**
   - Try to submit without selecting student → Should see error
   - Try to submit without selecting course → Should see error
   - [ ] Verify validation messages

4. **Create Valid Enrollments**
   Create multiple enrollments:
   
   **Enrollment 1:**
   - Student: `John Doe`
   - Course: `Introduction to Computer Science`
   
   **Enrollment 2:**
   - Student: `Jane Smith`
   - Course: `Web Development Bootcamp`
   
   **Enrollment 3:**
   - Student: `Bob Johnson`
   - Course: `Data Structures and Algorithms`
   
   **Enrollment 4:**
   - Student: `John Doe` (same student, different course)
   - Course: `Web Development Bootcamp`

5. **Verify Success**
   - [ ] Success message appears
   - [ ] Modal closes
   - [ ] Enrollment appears in list
   - [ ] Student and course details shown correctly

#### 4.3 List Enrollments

1. **Verify Enrollment List**
   - [ ] All enrollments displayed
   - [ ] Student names shown
   - [ ] Student emails shown
   - [ ] Course titles shown
   - [ ] Instructor names shown
   - [ ] Enrollment dates displayed
   - [ ] Dates formatted correctly
   - [ ] Remove button present

#### 4.4 Test Duplicate Enrollment Prevention

1. **Try to Create Duplicate**
   - Try enrolling same student in same course again
   - Should receive error message
   - [ ] Error is handled gracefully

#### 4.5 Remove Enrollment

1. **Delete an Enrollment**
   - Click "Remove" on any enrollment
   - Confirm deletion

2. **Verify Removal**
   - [ ] Success message appears
   - [ ] Enrollment removed from list

### Test 5: User Experience Features

#### 5.1 Loading States

1. **Observe Loading Indicators**
   - When lists load → Spinner should appear
   - When creating records → Button shows "Saving..."
   - When deleting → Button shows "Deleting..."
   - [ ] All loading states work correctly

#### 5.2 Error Handling

1. **Test Error Scenarios**
   - Stop the backend server
   - Try to create a student
   - Should see error message
   - [ ] Error message is user-friendly
   - [ ] Application doesn't crash

2. **Restart Backend and Retry**
   - Start backend again
   - Retry the operation
   - [ ] Works after backend is back

#### 5.3 Empty States

1. **Test Empty States**
   - Delete all records of one type
   - Should see helpful empty state message
   - [ ] Empty state icon displayed
   - [ ] Helpful message shown
   - [ ] Call-to-action button present

#### 5.4 Success Notifications

1. **Verify Auto-Dismiss**
   - Create any record
   - Success message should appear
   - Wait 3 seconds
   - [ ] Message auto-dismisses

### Test 6: Responsive Design

#### 6.1 Desktop View (1920x1080)

- [ ] Sidebar visible and full width
- [ ] All content properly laid out
- [ ] Tables fit nicely
- [ ] Modals centered

#### 6.2 Tablet View (768x1024)

1. **Resize Browser Window**
   - Resize to approximately tablet size
   - [ ] Layout adjusts properly
   - [ ] Content remains readable
   - [ ] Buttons remain accessible

#### 6.3 Mobile View (375x667)

1. **Test Mobile Layout**
   - Resize to mobile dimensions
   - [ ] Sidebar collapses or becomes scrollable
   - [ ] Tables become scrollable
   - [ ] Buttons stack properly
   - [ ] Forms remain usable

### Test 7: Navigation & Routing

#### 7.1 Sidebar Navigation

- [ ] Click each sidebar link
- [ ] Active link is highlighted
- [ ] URL updates correctly
- [ ] Page content changes

#### 7.2 Browser Navigation

- [ ] Click browser back button → Returns to previous page
- [ ] Click browser forward button → Goes forward
- [ ] Refresh page → Stays on same page
- [ ] Bookmark a page → Returns to correct page

### Test 8: Form Behavior

#### 8.1 Modal Interactions

- [ ] ESC key closes modal
- [ ] Click outside modal closes it
- [ ] X button closes modal
- [ ] Cancel button closes modal
- [ ] Form data clears when closed

#### 8.2 Form Validation

For each form:
- [ ] Required fields show error when empty
- [ ] Email validation works
- [ ] Character limits enforced
- [ ] Error messages clear when field is corrected
- [ ] Can't submit invalid form

### Test 9: Data Consistency

#### 9.1 Cross-Page Verification

1. **Create Data Flow**
   - Create student on Students page
   - Go to Enrollments page
   - [ ] New student appears in dropdown

2. **Update Data Flow**
   - Update student name
   - Go to Enrollments page
   - [ ] Updated name shows in existing enrollments

3. **Dashboard Updates**
   - Note counts on dashboard
   - Create new records
   - Return to dashboard
   - [ ] Counts update correctly

### Test 10: Performance

#### 10.1 Load Times

- [ ] Dashboard loads in < 2 seconds
- [ ] Page transitions are instant
- [ ] Forms open immediately
- [ ] No visible lag in UI

#### 10.2 Large Data Sets

If you create 20+ records:
- [ ] Pagination works smoothly
- [ ] List rendering is fast
- [ ] No performance degradation

## API Testing with Swagger

1. **Open Swagger UI**
   - Navigate to `http://localhost:5225/swagger`
   - Should see interactive API documentation

2. **Test Endpoints**
   - [ ] Try "GET /api/students"
   - [ ] Try "POST /api/courses" with valid data
   - [ ] Try "PUT /api/students/{id}" to update
   - [ ] Review response schemas

## Testing Checklist Summary

### Core Features
- [x] Create students
- [x] List students (with pagination)
- [x] Edit students
- [x] Delete students
- [x] Create courses
- [x] List courses (with pagination)
- [x] Edit courses
- [x] Delete courses
- [x] Assign students to courses
- [x] List enrollments
- [x] Remove enrollments
- [x] Dashboard with statistics

### UI/UX Features
- [x] Form validation
- [x] Loading states
- [x] Success/error messages
- [x] Empty states
- [x] Responsive design
- [x] Modal interactions
- [x] Navigation

### Technical Features
- [x] Pagination
- [x] API error handling
- [x] Client-side routing
- [x] Data consistency
- [x] Auto-dismiss notifications

## Known Issues / Limitations

Document any issues found:

1. Issue: _____________
   - Steps to reproduce: _____________
   - Expected: _____________
   - Actual: _____________

## Browser Compatibility

Test in multiple browsers:

- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Edge (latest)
- [ ] Safari (latest, macOS only)

## Conclusion

If all tests pass:
- ✅ Application is fully functional
- ✅ All assignment requirements met
- ✅ Ready for demonstration/submission

## Troubleshooting

### Backend Not Starting
```bash
cd src/Lms.Api
dotnet clean
dotnet build
dotnet run
```

### Frontend Not Starting
```bash
cd client
rm -rf node_modules
npm install
npm run dev
```

### Port Already in Use
- Backend: Change port in `src/Lms.Api/Properties/launchSettings.json`
- Frontend: Change port in `client/vite.config.ts`

### CORS Errors
Check that `src/Lms.Api/Program.cs` includes your frontend URL in CORS policy.

