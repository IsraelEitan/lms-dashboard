using System;
using System.Threading;
using System.Threading.Tasks;
using Lms.Api.Application.Interfaces;
using Lms.Api.Application.Services;
using Lms.Api.Common.Results;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Presentation.Contracts;
using Moq;
using Xunit;

namespace Lms.UnitTests;

public class EnrollmentServiceTests
{
    private readonly Mock<ICourseService> _courses = new();
    private readonly Mock<IStudentService> _students = new();
    private readonly EnrollmentService _svc;

    public EnrollmentServiceTests()
    {
        _svc = new EnrollmentService(_courses.Object, _students.Object);
    }

    [Fact]
    public async Task Assign_Succeeds_IfStudentAndCourseExist()
    {
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        _courses.Setup(c => c.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CourseDto>.Success(new CourseDto(courseId, "C","T", null)));
        _students.Setup(s => s.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<StudentDto>.Success(new StudentDto(studentId, "N","e@e")));

        var result = await _svc.AssignAsync(studentId, courseId, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(studentId, result.Value!.StudentId);
        Assert.Equal(courseId, result.Value.CourseId);

        var dupe = await _svc.AssignAsync(studentId, courseId, CancellationToken.None);
        Assert.False(dupe.IsSuccess);
        Assert.Equal("enrollment.duplicate", dupe.Error.Code);
    }

    [Fact]
    public async Task Assign_Fails_IfCourseMissing()
    {
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        _courses.Setup(c => c.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CourseDto>.Failure(Errors.Course.NotFound(courseId)));
        _students.Setup(s => s.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<StudentDto>.Success(new StudentDto(studentId, "N","e@e")));

        var result = await _svc.AssignAsync(studentId, courseId, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("enrollment.course_missing", result.Error.Code);
    }

    [Fact]
    public async Task Assign_Fails_IfStudentMissing()
    {
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        _courses.Setup(c => c.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CourseDto>.Success(new CourseDto(courseId, "C","T", null)));
        _students.Setup(s => s.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<StudentDto>.Failure(Errors.Student.NotFound(studentId)));

        var result = await _svc.AssignAsync(studentId, courseId, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("enrollment.student_missing", result.Error.Code);
    }

    [Fact]
    public async Task Query_ByCourseOrStudent_Works()
    {
        var sid = Guid.NewGuid();
        var cid = Guid.NewGuid();
        _courses.Setup(c => c.GetByIdAsync(cid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CourseDto>.Success(new CourseDto(cid, "C","T", null)));
        _students.Setup(s => s.GetByIdAsync(sid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<StudentDto>.Success(new StudentDto(sid, "N","e@e")));

        await _svc.AssignAsync(sid, cid, CancellationToken.None);

        var all = await _svc.QueryAsync(new PagingQuery { Page = 1, PageSize = 10 }, CancellationToken.None);
        Assert.True(all.IsSuccess);
        Assert.Single(all.Value!.Items);

        var byCourse = await _svc.GetByCourseAsync(cid, CancellationToken.None);
        Assert.True(byCourse.IsSuccess);
        Assert.Single(byCourse.Value!);

        var byStudent = await _svc.GetByStudentAsync(sid, CancellationToken.None);
        Assert.True(byStudent.IsSuccess);
        Assert.Single(byStudent.Value!);
    }
}
