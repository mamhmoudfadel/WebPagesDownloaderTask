using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;


namespace WebPagesDownloader.Test;

public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] arguments)
        : base(new AutoMockDataAttribute(), arguments)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AutoMockDataAttribute : AutoDataAttribute
{

    public AutoMockDataAttribute(params string[] values)
        : base(() => CreateFixture(values))
    {

    }

    public static IFixture CreateFixture(params string[] values)
    {
        Fixture fixture = new Fixture();
        fixture.Customize(new CompositeCustomization(
           new StringCustomization(),
           new DateTimeCustomization()
           ));

        return fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
    }
}
public class StringCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<string>(x => x.FromFactory(() => $"String_{Guid.NewGuid().ToString()[..8]}"));
    }
}
public class DateTimeCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<DateTime>(x => x.FromFactory(() => GetRandomDate(new DateTime(2000, 1, 1), DateTime.Today)));
    }
    public static DateTime GetRandomDate(DateTime startDate, DateTime endDate)
    {
        Random random = new Random();

        // Calculate range in days
        int range = (endDate - startDate).Days;

        // Get random number within the range
        int randomDays = random.Next(range + 1); // +1 to include 'endDate'

        // Return startDate plus random number of days
        return startDate.AddDays(randomDays);
    }
}