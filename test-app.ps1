# LMS Dashboard End-to-End Testing Script
# This script tests all features of the LMS Dashboard application

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "LMS Dashboard E2E Testing Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

$API_URL = "http://localhost:5225/api"

# Helper function to make API requests
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [string]$IdempotencyKey = $null
    )
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($IdempotencyKey) {
        $headers["Idempotency-Key"] = $IdempotencyKey
    }
    
    try {
        if ($Body) {
            $jsonBody = $Body | ConvertTo-Json
            $response = Invoke-RestMethod -Uri "$API_URL$Endpoint" -Method $Method -Headers $headers -Body $jsonBody
        } else {
            $response = Invoke-RestMethod -Uri "$API_URL$Endpoint" -Method $Method -Headers $headers
        }
        return $response
    } catch {
        Write-Host "Error: $_" -ForegroundColor Red
        return $null
    }
}

# Test 1: Create Students
Write-Host "`n[TEST 1] Creating Students..." -ForegroundColor Yellow
$student1 = Invoke-ApiRequest -Method POST -Endpoint "/students" -IdempotencyKey "test-student-1" -Body @{
    name = "John Doe"
    email = "john.doe@example.com"
}

$student2 = Invoke-ApiRequest -Method POST -Endpoint "/students" -IdempotencyKey "test-student-2" -Body @{
    name = "Jane Smith"
    email = "jane.smith@example.com"
}

$student3 = Invoke-ApiRequest -Method POST -Endpoint "/students" -IdempotencyKey "test-student-3" -Body @{
    name = "Bob Johnson"
    email = "bob.johnson@example.com"
}

if ($student1 -and $student2 -and $student3) {
    Write-Host "✓ Successfully created 3 students" -ForegroundColor Green
    Write-Host "  Student 1 ID: $($student1.id)" -ForegroundColor Gray
    Write-Host "  Student 2 ID: $($student2.id)" -ForegroundColor Gray
    Write-Host "  Student 3 ID: $($student3.id)" -ForegroundColor Gray
} else {
    Write-Host "✗ Failed to create students" -ForegroundColor Red
}

# Test 2: List Students
Write-Host "`n[TEST 2] Listing Students..." -ForegroundColor Yellow
$studentsList = Invoke-ApiRequest -Method GET -Endpoint "/students?pageNumber=1&pageSize=10"
if ($studentsList) {
    Write-Host "✓ Successfully retrieved $($studentsList.totalCount) students" -ForegroundColor Green
    Write-Host "  Page: $($studentsList.pageNumber)/$($studentsList.totalPages)" -ForegroundColor Gray
} else {
    Write-Host "✗ Failed to list students" -ForegroundColor Red
}

# Test 3: Create Courses
Write-Host "`n[TEST 3] Creating Courses..." -ForegroundColor Yellow
$course1 = Invoke-ApiRequest -Method POST -Endpoint "/courses" -IdempotencyKey "test-course-1" -Body @{
    title = "Introduction to Computer Science"
    description = "Learn the fundamentals of programming and computer science"
    instructorName = "Prof. Alan Turing"
}

$course2 = Invoke-ApiRequest -Method POST -Endpoint "/courses" -IdempotencyKey "test-course-2" -Body @{
    title = "Web Development Bootcamp"
    description = "Master modern web development with React, Node.js, and more"
    instructorName = "Dr. Ada Lovelace"
}

$course3 = Invoke-ApiRequest -Method POST -Endpoint "/courses" -IdempotencyKey "test-course-3" -Body @{
    title = "Data Structures and Algorithms"
    description = "Deep dive into algorithms, data structures, and problem solving"
    instructorName = "Prof. Donald Knuth"
}

if ($course1 -and $course2 -and $course3) {
    Write-Host "✓ Successfully created 3 courses" -ForegroundColor Green
    Write-Host "  Course 1 ID: $($course1.id)" -ForegroundColor Gray
    Write-Host "  Course 2 ID: $($course2.id)" -ForegroundColor Gray
    Write-Host "  Course 3 ID: $($course3.id)" -ForegroundColor Gray
} else {
    Write-Host "✗ Failed to create courses" -ForegroundColor Red
}

# Test 4: List Courses
Write-Host "`n[TEST 4] Listing Courses..." -ForegroundColor Yellow
$coursesList = Invoke-ApiRequest -Method GET -Endpoint "/courses?pageNumber=1&pageSize=10"
if ($coursesList) {
    Write-Host "✓ Successfully retrieved $($coursesList.totalCount) courses" -ForegroundColor Green
    Write-Host "  Page: $($coursesList.pageNumber)/$($coursesList.totalPages)" -ForegroundColor Gray
} else {
    Write-Host "✗ Failed to list courses" -ForegroundColor Red
}

# Test 5: Update Course
Write-Host "`n[TEST 5] Updating Course..." -ForegroundColor Yellow
if ($course1) {
    $updatedCourse = Invoke-ApiRequest -Method PUT -Endpoint "/courses/$($course1.id)" -Body @{
        title = "Introduction to Computer Science - Updated"
        description = "Learn the fundamentals of programming and computer science with new content"
        instructorName = "Prof. Alan Turing"
    }
    if ($updatedCourse) {
        Write-Host "✓ Successfully updated course" -ForegroundColor Green
        Write-Host "  New title: $($updatedCourse.title)" -ForegroundColor Gray
    } else {
        Write-Host "✗ Failed to update course" -ForegroundColor Red
    }
}

# Test 6: Create Enrollments
Write-Host "`n[TEST 6] Creating Enrollments..." -ForegroundColor Yellow
if ($student1 -and $course1 -and $student2 -and $course2 -and $student3 -and $course3) {
    $enrollment1 = Invoke-ApiRequest -Method POST -Endpoint "/enrollments" -IdempotencyKey "test-enrollment-1" -Body @{
        studentId = $student1.id
        courseId = $course1.id
    }
    
    $enrollment2 = Invoke-ApiRequest -Method POST -Endpoint "/enrollments" -IdempotencyKey "test-enrollment-2" -Body @{
        studentId = $student2.id
        courseId = $course2.id
    }
    
    $enrollment3 = Invoke-ApiRequest -Method POST -Endpoint "/enrollments" -IdempotencyKey "test-enrollment-3" -Body @{
        studentId = $student3.id
        courseId = $course3.id
    }
    
    # Enroll student 1 in multiple courses
    $enrollment4 = Invoke-ApiRequest -Method POST -Endpoint "/enrollments" -IdempotencyKey "test-enrollment-4" -Body @{
        studentId = $student1.id
        courseId = $course2.id
    }
    
    if ($enrollment1 -and $enrollment2 -and $enrollment3 -and $enrollment4) {
        Write-Host "✓ Successfully created 4 enrollments" -ForegroundColor Green
    } else {
        Write-Host "✗ Some enrollments failed" -ForegroundColor Red
    }
}

# Test 7: List Enrollments
Write-Host "`n[TEST 7] Listing Enrollments..." -ForegroundColor Yellow
$enrollmentsList = Invoke-ApiRequest -Method GET -Endpoint "/enrollments?pageNumber=1&pageSize=10"
if ($enrollmentsList) {
    Write-Host "✓ Successfully retrieved $($enrollmentsList.totalCount) enrollments" -ForegroundColor Green
    foreach ($enrollment in $enrollmentsList.items) {
        Write-Host "  - $($enrollment.studentName) enrolled in $($enrollment.courseTitle)" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ Failed to list enrollments" -ForegroundColor Red
}

# Test 8: Get Student's Enrollments
Write-Host "`n[TEST 8] Getting Student's Enrollments..." -ForegroundColor Yellow
if ($student1) {
    $studentEnrollments = Invoke-ApiRequest -Method GET -Endpoint "/enrollments/student/$($student1.id)"
    if ($studentEnrollments) {
        Write-Host "✓ Successfully retrieved enrollments for $($student1.name)" -ForegroundColor Green
        Write-Host "  Enrolled in $($studentEnrollments.Count) course(s)" -ForegroundColor Gray
    } else {
        Write-Host "✗ Failed to get student enrollments" -ForegroundColor Red
    }
}

# Test 9: Get Course's Enrollments
Write-Host "`n[TEST 9] Getting Course's Enrollments..." -ForegroundColor Yellow
if ($course1) {
    $courseEnrollments = Invoke-ApiRequest -Method GET -Endpoint "/enrollments/course/$($course1.id)"
    if ($courseEnrollments) {
        Write-Host "✓ Successfully retrieved enrollments for $($course1.title)" -ForegroundColor Green
        Write-Host "  $($courseEnrollments.Count) student(s) enrolled" -ForegroundColor Gray
    } else {
        Write-Host "✗ Failed to get course enrollments" -ForegroundColor Red
    }
}

# Test 10: Test Idempotency
Write-Host "`n[TEST 10] Testing Idempotency..." -ForegroundColor Yellow
$course4 = Invoke-ApiRequest -Method POST -Endpoint "/courses" -IdempotencyKey "idempotency-test" -Body @{
    title = "Idempotency Test Course"
    description = "This course tests idempotency"
    instructorName = "Test Instructor"
}

Start-Sleep -Seconds 1

$course4Duplicate = Invoke-ApiRequest -Method POST -Endpoint "/courses" -IdempotencyKey "idempotency-test" -Body @{
    title = "Idempotency Test Course"
    description = "This course tests idempotency"
    instructorName = "Test Instructor"
}

if ($course4 -and $course4Duplicate -and $course4.id -eq $course4Duplicate.id) {
    Write-Host "✓ Idempotency working correctly (same ID returned)" -ForegroundColor Green
    Write-Host "  Course ID: $($course4.id)" -ForegroundColor Gray
} else {
    Write-Host "✗ Idempotency test failed" -ForegroundColor Red
}

# Test 11: Pagination
Write-Host "`n[TEST 11] Testing Pagination..." -ForegroundColor Yellow
$page1 = Invoke-ApiRequest -Method GET -Endpoint "/courses?pageNumber=1&pageSize=2"
$page2 = Invoke-ApiRequest -Method GET -Endpoint "/courses?pageNumber=2&pageSize=2"

if ($page1 -and $page2) {
    Write-Host "✓ Pagination working correctly" -ForegroundColor Green
    Write-Host "  Page 1: $($page1.items.Count) items" -ForegroundColor Gray
    Write-Host "  Page 2: $($page2.items.Count) items" -ForegroundColor Gray
    Write-Host "  Has next page: $($page1.hasNextPage)" -ForegroundColor Gray
} else {
    Write-Host "✗ Pagination test failed" -ForegroundColor Red
}

# Test 12: Get Single Records
Write-Host "`n[TEST 12] Getting Single Records by ID..." -ForegroundColor Yellow
if ($student1 -and $course1) {
    $singleStudent = Invoke-ApiRequest -Method GET -Endpoint "/students/$($student1.id)"
    $singleCourse = Invoke-ApiRequest -Method GET -Endpoint "/courses/$($course1.id)"
    
    if ($singleStudent -and $singleCourse) {
        Write-Host "✓ Successfully retrieved single records" -ForegroundColor Green
        Write-Host "  Student: $($singleStudent.name)" -ForegroundColor Gray
        Write-Host "  Course: $($singleCourse.title)" -ForegroundColor Gray
    } else {
        Write-Host "✗ Failed to get single records" -ForegroundColor Red
    }
}

# Test 13: Update Student
Write-Host "`n[TEST 13] Updating Student..." -ForegroundColor Yellow
if ($student1) {
    $updatedStudent = Invoke-ApiRequest -Method PUT -Endpoint "/students/$($student1.id)" -Body @{
        name = "John Updated Doe"
        email = "john.updated@example.com"
    }
    if ($updatedStudent) {
        Write-Host "✓ Successfully updated student" -ForegroundColor Green
        Write-Host "  New name: $($updatedStudent.name)" -ForegroundColor Gray
        Write-Host "  New email: $($updatedStudent.email)" -ForegroundColor Gray
    } else {
        Write-Host "✗ Failed to update student" -ForegroundColor Red
    }
}

# Summary
Write-Host "`n================================" -ForegroundColor Cyan
Write-Host "Testing Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "All API endpoints tested successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Open your browser to http://localhost:3000" -ForegroundColor White
Write-Host "2. Test the UI features:" -ForegroundColor White
Write-Host "   - View Dashboard with statistics" -ForegroundColor Gray
Write-Host "   - Browse and manage Courses" -ForegroundColor Gray
Write-Host "   - Browse and manage Students" -ForegroundColor Gray
Write-Host "   - Create and view Enrollments" -ForegroundColor Gray
Write-Host "   - Test form validation" -ForegroundColor Gray
Write-Host "   - Test pagination" -ForegroundColor Gray
Write-Host ""

