using System;
using System.Threading;
using System.Threading.Tasks;
using Lms.Api.Application.Services;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Presentation.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Lms.UnitTests;

public class StudentServiceTests
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly StudentService _svc;

    public StudentServiceTests()
    {
        _svc = new StudentService(_cache);
    }

    [Fact]
    public async Task CreateStudent_Succeeds_WhenValid()
    {
        var dto = new CreateStudentDto { Name = "Stu", Email = "a@b.com" };
        var result = await _svc.CreateAsync(dto, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("Stu", result.Value.Name);
    }

    [Fact]
    public async Task CreateStudent_Fails_MissingOrInvalidFields()
    {
        var res1 = await _svc.CreateAsync(new CreateStudentDto { Name = null!, Email = "abc@b" }, CancellationToken.None);
        Assert.False(res1.IsSuccess);
        Assert.Equal("common.validation", res1.Error.Code);

        var res2 = await _svc.CreateAsync(new CreateStudentDto { Name = "Name", Email = null! }, CancellationToken.None);
        Assert.False(res2.IsSuccess);
        Assert.Equal("student.invalid_email", res2.Error.Code);
    }

    [Fact]
    public async Task QueryStudent_Pagination_Works()
    {
        for (int i = 1; i <= 10; i++)
        {
            await _svc.CreateAsync(new CreateStudentDto { Name = $"S{i}", Email = $"x{i}@b.com" }, CancellationToken.None);
        }

        var query = new PagingQuery { Page = 2, PageSize = 3 };
        var result = await _svc.QueryAsync(query, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Items.Count);
        Assert.Equal(10, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetById_ReturnsOr404()
    {
        var dto = new CreateStudentDto { Name = "Test", Email = "x@b.com" };
        var add = await _svc.CreateAsync(dto, CancellationToken.None);
        var ok = await _svc.GetByIdAsync(add.Value.Id, CancellationToken.None);
        Assert.True(ok.IsSuccess);
        var nf = await _svc.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.False(nf.IsSuccess);
        Assert.Equal(404, nf.Error.HttpStatus);
    }

    [Fact]
    public async Task DeleteStudent_WorksOr404()
    {
        var dto = new CreateStudentDto { Name = "DelMe", Email = "a@b.com" };
        var s = await _svc.CreateAsync(dto, CancellationToken.None);
        var del = await _svc.DeleteAsync(s.Value.Id, CancellationToken.None);
        Assert.True(del.IsSuccess);
        var nf = await _svc.DeleteAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.False(nf.IsSuccess);
        Assert.Equal(404, nf.Error.HttpStatus);
    }
}
