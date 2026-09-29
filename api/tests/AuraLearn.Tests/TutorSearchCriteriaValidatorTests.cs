using AuraLearn.Application.Interfaces;
using AuraLearn.Application.Services;
using FluentValidation;
using Xunit;

namespace AuraLearn.Tests;

/// <summary>
/// Tests del validador de criterios de búsqueda del catálogo de tutores.
/// </summary>
public class TutorSearchCriteriaValidatorTests
{
    private readonly TutorSearchCriteriaValidator _sut = new();

    public static TheoryData<int, int> InvalidPaging => new()
    {
        { 0, 12 },   // page < 1
        { 1, 0 },    // pageSize < 1
        { 1, 51 },   // pageSize > MaxPageSize (50)
    };

    [Theory]
    [MemberData(nameof(InvalidPaging))]
    public void Validate_WithInvalidPaging_Fails(int page, int pageSize)
    {
        var criteria = new TutorSearchCriteria(null, null, null, null, null, page, pageSize);
        var result = _sut.Validate(criteria);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithValidDefaults_IsValid()
    {
        var criteria = new TutorSearchCriteria(null, null, null, null, null, 1, 12);
        var result = _sut.Validate(criteria);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithRatingOutOfRange_IsInvalid()
    {
        var criteria = new TutorSearchCriteria(null, null, MinRating: 6, null, null, 1, 12);
        var result = _sut.Validate(criteria);
        Assert.False(result.IsValid);
    }
}
