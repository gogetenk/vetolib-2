using FluentAssertions;
using Vetolib.Notifications.Templates;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class StockLowAlertEmailTemplateTests
{
    [Fact]
    public void Subject_English_ContainsItemName()
    {
        var subject = StockLowAlertEmailTemplate.Subject("Amoxicillin 250mg", "en");

        subject.Should().Contain("Amoxicillin 250mg");
        subject.Should().Contain("Low stock alert");
    }

    [Fact]
    public void Subject_Arabic_ContainsItemName()
    {
        var subject = StockLowAlertEmailTemplate.Subject("Amoxicillin 250mg", "ar");

        subject.Should().Contain("Amoxicillin 250mg");
    }

    [Fact]
    public void HtmlBody_English_ContainsQuantityAndThreshold()
    {
        var body = StockLowAlertEmailTemplate.HtmlBody("Doxycycline 100mg", 3, 10, "en");

        body.Should().Contain("Doxycycline 100mg");
        body.Should().Contain("3");
        body.Should().Contain("10");
        body.Should().Contain("restock");
    }

    [Fact]
    public void HtmlBody_Arabic_ContainsQuantityAndThreshold()
    {
        var body = StockLowAlertEmailTemplate.HtmlBody("Doxycycline 100mg", 3, 10, "ar");

        body.Should().Contain("Doxycycline 100mg");
        body.Should().Contain("3");
        body.Should().Contain("10");
    }

    [Fact]
    public void PlainTextBody_English_ContainsQuantityAndThreshold()
    {
        var body = StockLowAlertEmailTemplate.PlainTextBody("Vaccine A", 5, 20, "en");

        body.Should().Contain("Vaccine A");
        body.Should().Contain("5");
        body.Should().Contain("20");
        body.Should().Contain("restock");
    }

    [Fact]
    public void PlainTextBody_Arabic_ContainsQuantityAndThreshold()
    {
        var body = StockLowAlertEmailTemplate.PlainTextBody("Vaccine A", 5, 20, "ar");

        body.Should().Contain("Vaccine A");
        body.Should().Contain("5");
        body.Should().Contain("20");
    }
}
