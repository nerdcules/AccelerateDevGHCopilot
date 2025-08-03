using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Library.Infrastructure.Data;
using Library.ApplicationCore.Entities;

namespace Library.UnitTests.Infrastructure.JsonLoanRepositoryTests;

public class GetLoanTest
{
    private readonly JsonData _jsonData;
    private readonly JsonLoanRepository _jsonLoanRepository;

    public GetLoanTest()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"JsonPaths:Authors", Path.Combine("Json", "Authors.json")},
                {"JsonPaths:Books", Path.Combine("Json", "Books.json")},
                {"JsonPaths:BookItems", Path.Combine("Json", "BookItems.json")},
                {"JsonPaths:Patrons", Path.Combine("Json", "Patrons.json")},
                {"JsonPaths:Loans", Path.Combine("Json", "Loans.json")}
            })
            .Build();

        _jsonData = new JsonData(configuration);
        _jsonLoanRepository = new JsonLoanRepository(_jsonData);
    }

    [Fact(DisplayName = "JsonLoanRepository.GetLoan: Returns loan when valid ID is found")]
    public async Task GetLoan_ReturnsLoanWhenValidIdFound()
    {
        // Arrange
        var expectedLoanId = 1;

        // Act
        var actualLoan = await _jsonLoanRepository.GetLoan(expectedLoanId);

        // Assert
        Assert.NotNull(actualLoan);
        Assert.Equal(expectedLoanId, actualLoan.Id);
        Assert.Equal(17, actualLoan.BookItemId);
        Assert.Equal(22, actualLoan.PatronId);
        Assert.Null(actualLoan.ReturnDate);
    }

    [Fact(DisplayName = "JsonLoanRepository.GetLoan: Returns null when loan ID is not found")]
    public async Task GetLoan_ReturnsNullWhenLoanIdNotFound()
    {
        // Arrange
        var nonExistentLoanId = 999;

        // Act
        var actualLoan = await _jsonLoanRepository.GetLoan(nonExistentLoanId);

        // Assert
        Assert.Null(actualLoan);
    }
}
