using NSubstitute;
using Library.ApplicationCore;
using Library.ApplicationCore.Entities;
using Library.ApplicationCore.Enums;
using Library.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace Library.UnitTests.ApplicationCore.LoanServiceTests;

public class ReturnLoanTest
{
    private readonly ILoanRepository _mockLoanRepository;
    private readonly LoanService _loanService;
    private readonly JsonData _jsonData;
    private readonly JsonLoanRepository _jsonLoanRepository;

    public ReturnLoanTest()
    {
        _mockLoanRepository = Substitute.For<ILoanRepository>();
        _loanService = new LoanService(_mockLoanRepository);

        var configuration = new ConfigurationBuilder()
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

    [Fact(DisplayName = "LoanService.ReturnLoan: Returns LoanNotFound if loan is not found")]
    public async Task ReturnLoan_ReturnsLoanNotFound()
    {
        // Arrange
        var loanId = 1;
        _mockLoanRepository.GetLoan(loanId).Returns((Loan?)null);

        // Act
        LoanReturnStatus returnStatus = await _loanService.ReturnLoan(loanId);

        // Assert
        Assert.Equal(LoanReturnStatus.LoanNotFound, returnStatus);
    }

    [Fact(DisplayName = "LoanService.ReturnLoan: Returns AlreadyReturned if loan is already returned")]
    public async Task ReturnLoan_ReturnsAlreadyReturned()
    {
        // Arrange
        var patron = PatronFactory.CreateCurrentPatron();
        var loan = LoanFactory.CreateReturnedLoanForPatron(patron);
        var loanId = loan.Id;
        _mockLoanRepository.GetLoan(loanId).Returns(loan);

        // Act
        LoanReturnStatus returnStatus = await _loanService.ReturnLoan(loanId);

        // Assert
        Assert.Equal(LoanReturnStatus.AlreadyReturned, returnStatus);
    }

    [Fact(DisplayName = "LoanService.ReturnLoan: Returns Success and updates return date for a patron with current membership")]
    public async Task ReturnLoan_ReturnsSuccessAndUpdateReturnDate()
    {
        // Arrange
        var patron = PatronFactory.CreateCurrentPatron();
        var loan = LoanFactory.CreateCurrentLoanForPatron(patron);
        var loanId = loan.Id;
        _mockLoanRepository.GetLoan(loanId).Returns(loan);

        // Act
        LoanReturnStatus returnStatus = await _loanService.ReturnLoan(loanId);

        // Assert
        Assert.Equal(LoanReturnStatus.Success, returnStatus);
        Assert.NotNull(loan.ReturnDate);
    }

    [Fact(DisplayName = "LoanService.ReturnLoan: Returns Success and updates return date for an expired loan")]
    public async Task ReturnLoan_ReturnsSuccessAndUpdateReturnDateForExpiredLoan()
    {
        // Arrange
        var patron = PatronFactory.CreateCurrentPatron();
        var loan = LoanFactory.CreateExpiredLoanForPatron(patron);
        var loanId = loan.Id;
        _mockLoanRepository.GetLoan(loanId).Returns(loan);

        // Act
        LoanReturnStatus returnStatus = await _loanService.ReturnLoan(loanId);

        // Assert
        Assert.Equal(LoanReturnStatus.Success, returnStatus);
        Assert.NotNull(loan.ReturnDate);
    }

    [Fact(DisplayName = "LoanService.ReturnLoan: Returns Success and updates return date for a patron with expired membership")]
    public async Task ReturnLoan_ReturnsSuccessAndUpdateReturnDateForExpiredPatron()
    {
        // Arrange
        var patron = PatronFactory.CreateExpiredPatron();
        var loan = LoanFactory.CreateCurrentLoanForPatron(patron);
        var loanId = loan.Id;
        _mockLoanRepository.GetLoan(loanId).Returns(loan);

        // Act
        LoanReturnStatus returnStatus = await _loanService.ReturnLoan(loanId);

        // Assert
        Assert.Equal(LoanReturnStatus.Success, returnStatus);
        Assert.NotNull(loan.ReturnDate);
    }

    [Fact(DisplayName = "JsonLoanRepository.GetLoan: Returns loan when valid ID is found")]
    public async Task JsonLoanRepository_GetLoan_ReturnsLoanWhenValidIdFound()
    {
        // Arrange
        var expectedLoanId = 1;
        var expectedLoan = new Loan
        {
            Id = expectedLoanId,
            BookItemId = 17,
            PatronId = 22,
            ReturnDate = null
        };
        _mockLoanRepository.GetLoan(expectedLoanId).Returns(expectedLoan);

        // Act
        var actualLoan = await _jsonLoanRepository.GetLoan(expectedLoanId);

        // Assert
        Assert.NotNull(actualLoan);
        Assert.Equal(expectedLoanId, actualLoan.Id);
        Assert.Equal(expectedLoan.BookItemId, actualLoan.BookItemId);
        Assert.Equal(expectedLoan.PatronId, actualLoan.PatronId);
        Assert.Null(actualLoan.ReturnDate);
    }
}
