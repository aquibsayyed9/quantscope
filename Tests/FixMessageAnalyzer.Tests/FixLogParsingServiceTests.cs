using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Services;
using FixMessageAnalyzer.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace FixMessageAnalyzer.Tests;
[TestClass]
public class FixLogParsingServiceTests
{
    private FixLogParsingService _parser;
    private Mock<ILogger<FixLogParsingService>> _loggerMock;
    private Mock<ILogger<FixFieldMapper>> _mapperLoggerMock;
    private Mock<FixMonitoringService> _monitoringServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<FixLogParsingService>>();
        _mapperLoggerMock = new Mock<ILogger<FixFieldMapper>>();
        _monitoringServiceMock = new Mock<FixMonitoringService>();

        var fieldMapper = new FixFieldMapper(_mapperLoggerMock.Object);
        _parser = new FixLogParsingService(_monitoringServiceMock.Object, fieldMapper, _loggerMock.Object);
    }

    private string BuildFixMessage(string timestamp, Dictionary<string, string> fields)
    {
        var fixFields = fields.Select(kv => $"{kv.Key}={kv.Value}");
        return $"{timestamp} - {string.Join("\x01", fixFields)}";
    }

    [TestMethod]
    public void ParseFixLogLine_NewOrderSingle_ParsesCorrectly()
    {
        // Arrange
        var fields = new Dictionary<string, string>
        {
            {"8", "FIXT.1.1"},
            {"35", "D"},  // New Order Single
            {"34", "1234"},
            {"49", "SENDER"},
            {"56", "TARGET"},
            {"11", "OrderID123"},  // ClOrdID
            {"55", "AAPL"},       // Symbol
            {"54", "1"},          // Side (1=Buy)
            {"44", "150.50"},     // Price
            {"38", "100"}         // OrderQty
        };

        var logLine = BuildFixMessage("20250110 12:00:13.228", fields);

        // Act
        var result = _parser.ParseFixLogLine(logLine);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("D", result.MsgType);
        Assert.AreEqual(1234, result.SequenceNumber);
        Assert.AreEqual("SENDER", result.SenderCompID);
        Assert.AreEqual("TARGET", result.TargetCompID);
        Assert.AreEqual("OrderID123", result.Fields["11"]);
        Assert.AreEqual("AAPL", result.Fields["55"]);
    }

    [TestMethod]
    public void ParseFixLogLine_ExecutionReport_ParsesCorrectly()
    {
        // Arrange
        var fields = new Dictionary<string, string>
        {
            {"8", "FIXT.1.1"},
            {"35", "8"},   // Execution Report
            {"34", "5678"},
            {"49", "SENDER"},
            {"56", "TARGET"},
            {"37", "ExecID123"},  // OrderID
            {"17", "ExecID456"},  // ExecID
            {"150", "0"},         // ExecType (0=New)
            {"39", "0"},          // OrdStatus (0=New)
            {"55", "AAPL"},       // Symbol
            {"54", "1"},          // Side (1=Buy)
            {"151", "100"},       // LeavesQty
            {"14", "0"}           // CumQty
        };

        var logLine = BuildFixMessage("20250110 12:00:13.228", fields);

        // Act
        var result = _parser.ParseFixLogLine(logLine);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("8", result.MsgType);
        Assert.AreEqual(5678, result.SequenceNumber);
        Assert.AreEqual("0", result.Fields["150"]); // ExecType
        Assert.AreEqual("0", result.Fields["39"]);  // OrdStatus
    }

    [TestMethod]
    public void ParseFixLogLine_InvalidTimestamp_ReturnsNull()
    {
        // Arrange
        var logLine = "Invalid timestamp - 8=FIXT.1.1\x0135=D\x0134=1234";

        // Act
        var result = _parser.ParseFixLogLine(logLine);

        // Assert
        Assert.IsNull(result);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [TestMethod]
    public void ParseFixLogLine_MissingRequiredFields_ReturnsMessageWithUnknowns()
    {
        // Arrange
        var fields = new Dictionary<string, string>
        {
            {"8", "FIXT.1.1"},
            {"55", "AAPL"}  // Only Symbol field
        };

        var logLine = BuildFixMessage("20250110 12:00:13.228", fields);

        // Act
        var result = _parser.ParseFixLogLine(logLine);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Unknown", result.MsgType);
        Assert.AreEqual(-1, result.SequenceNumber);
        Assert.AreEqual("Unknown", result.SenderCompID);
        Assert.AreEqual("Unknown", result.TargetCompID);
        Assert.AreEqual("AAPL", result.Fields["55"]);
    }

    [TestMethod]
    public void ParseFixLogLine_EmptyMessage_ReturnsNull()
    {
        // Arrange
        var logLine = "20250110 12:00:13.228 - ";

        // Act
        var result = _parser.ParseFixLogLine(logLine);

        // Assert
        Assert.IsNull(result);
    }
}