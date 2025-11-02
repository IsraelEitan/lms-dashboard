using System;
using System.Threading;
using System.Threading.Tasks;
using Lms.Api.Application.Services;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Presentation.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Lms.UnitTests;

public class CourseServiceTests
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly CourseService _svc;

    public CourseServiceTests()
    {
        _svc = new CourseService(_cache);
    }

    [Fact]
    public async Task CreateCourse_Succeeds_WhenValid()
    {
        var dto = new CreateUpdateCourseDto { Code = "C99", Title = "Title", Description = "Desc" };
        var result = await _svc.CreateAsync(dto, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("C99", result.Value.Code);
    }

    [Fact]
    public async Task CreateCourse_Fails_WhenMissingCode()
    {
        var dto = new CreateUpdateCourseDto { Code = null!, Title = "Valid", Description = null };
        var result = await _svc.CreateAsync(dto, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("course.code_required", result.Error.Code);
    }

    [Fact]
    public async Task CreateCourse_Fails_WhenDuplicateCode()
    {
        var dto = new CreateUpdateCourseDto { Code = "C97", Title = "A", Description = null };
        await _svc.CreateAsync(dto, CancellationToken.None);
        var result2 = await _svc.CreateAsync(dto, CancellationToken.None);
        Assert.False(result2.IsSuccess);
        Assert.Equal("course.duplicate_code", result2.Error.Code);
    }

    [Fact]
    public async Task QueryCourse_PaginationWorks()
    {
        for (int i = 1; i <= 15; i++)
        {
            await _svc.CreateAsync(new CreateUpdateCourseDto { Code = $"C{i}", Title = $"T{i}", Description = null }, CancellationToken.None);
        }

        var query = new PagingQuery { Page = 2, PageSize = 5 };
        var result = await _svc.QueryAsync(query, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Items.Count);
        Assert.Equal(15, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetById_ReturnsItem_Or404()
    {
        var dto = new CreateUpdateCourseDto { Code = "X1", Title = "TT", Description = "" };
        var add = await _svc.CreateAsync(dto, CancellationToken.None);
        var ok = await _svc.GetByIdAsync(add.Value.Id, CancellationToken.None);
        Assert.True(ok.IsSuccess);
        var notFound = await _svc.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.False(notFound.IsSuccess);
        Assert.Equal(404, notFound.Error.HttpStatus);
    }

    [Fact]
    public async Task UpdateCourse_ValidAndInvalidCases()
    {
        var dto = new CreateUpdateCourseDto { Code = "UPDATE", Title = "T", Description = null };
        var create = await _svc.CreateAsync(dto, CancellationToken.None);
        var upd = await _svc.UpdateAsync(create.Value.Id, new CreateUpdateCourseDto { Code = "UPDATE", Title = "T2", Description = "d" }, CancellationToken.None);
        Assert.True(upd.IsSuccess);
        var badUpd = await _svc.UpdateAsync(Guid.NewGuid(), dto, CancellationToken.None);
        Assert.False(badUpd.IsSuccess);
        Assert.Equal(404, badUpd.Error.HttpStatus);
    }

    [Fact]
    public async Task DeleteCourse_Works_Or404()
    {
        var dto = new CreateUpdateCourseDto { Code = "DEL", Title = "T", Description = null };
        var cr = await _svc.CreateAsync(dto, CancellationToken.None);
        var delOk = await _svc.DeleteAsync(cr.Value.Id, CancellationToken.None);
        Assert.True(delOk.IsSuccess);
        var del404 = await _svc.DeleteAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.False(del404.IsSuccess);
        Assert.Equal(404, del404.Error.HttpStatus);
    }
}
