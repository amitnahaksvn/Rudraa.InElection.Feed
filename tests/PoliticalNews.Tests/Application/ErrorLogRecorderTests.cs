using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Application.Abstractions;
using Application.Models;
using Application.Services;
using Domain.Entities;

namespace PoliticalNews.Tests.Application;

public class ErrorLogRecorderTests
{
    private static ErrorNotification BuildNotification() => new()
    {
        Environment = "Production",
        ApplicationName = "PoliticalNews.Tests",
        Provider = "AajTak",
        FeedOrApiName = "Home",
        SourceUrl = "https://example.com/feed",
        Operation = "RSS Feed Fetch",
        ExceptionType = "System.Net.Http.HttpRequestException",
        ErrorMessage = "network timeout",
        HttpStatusCode = 503,
        CorrelationId = "history-1",
        HangfireJobId = "job-1"
    };

    [Fact]
    public async Task RecordAsync_MapsNotificationFieldsOntoErrorLog_AndLeavesItUnsent()
    {
        var repo = new Mock<IErrorLogRepository>();
        ErrorLog? inserted = null;
        repo
            .Setup(r => r.InsertAsync(It.IsAny<ErrorLog>(), It.IsAny<CancellationToken>()))
            .Callback<ErrorLog, CancellationToken>((e, _) => inserted = e)
            .Returns(Task.CompletedTask);

        await ErrorLogRecorder.RecordAsync(repo.Object, BuildNotification(), NullLogger.Instance, CancellationToken.None);

        Assert.NotNull(inserted);
        Assert.Equal("AajTak", inserted!.Provider);
        Assert.Equal("Home", inserted.FeedOrApiName);
        Assert.Equal("RSS Feed Fetch", inserted.Source);
        Assert.Equal("network timeout", inserted.Message);
        Assert.Equal(503, inserted.HttpStatusCode);
        Assert.Equal("503", inserted.ErrorCode);
        Assert.Equal("history-1", inserted.CorrelationId);
        Assert.Equal("job-1", inserted.HangfireJobId);
        Assert.False(inserted.IsSent);
        Assert.Null(inserted.SentOn);
    }

    [Fact]
    public async Task RecordAsync_RepositoryThrows_IsSwallowedAndLogged()
    {
        var repo = new Mock<IErrorLogRepository>();
        repo
            .Setup(r => r.InsertAsync(It.IsAny<ErrorLog>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("mongo unavailable"));

        // Must not throw - recording a failure must never itself fail the caller's run.
        await ErrorLogRecorder.RecordAsync(repo.Object, BuildNotification(), NullLogger.Instance, CancellationToken.None);
    }

    [Fact]
    public async Task RecordIfAnyAsync_MultipleErrors_InsertsEachOne()
    {
        var repo = new Mock<IErrorLogRepository>();
        var errors = new List<ErrorNotification> { BuildNotification(), BuildNotification() };

        await ErrorLogRecorder.RecordIfAnyAsync(repo.Object, errors, NullLogger.Instance, "history-1", CancellationToken.None);

        repo.Verify(r => r.InsertAsync(It.IsAny<ErrorLog>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
