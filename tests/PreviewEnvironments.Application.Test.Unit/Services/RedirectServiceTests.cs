using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Services;

public class RedirectServiceTests
{
    private readonly IRedirectService _sut = new RedirectService();

    [Fact]
    public void Add_Should_Return_Correct_Uri()
    {
        // Arrange
        string key = new('A', 12);;

        // Act
        Uri actualUri = _sut.Add(
            key,
            new Uri("https://temp.uri.com"),
            new Uri("https://test.application.com"));

        // Assert
        actualUri
            .Should()
            .BeEquivalentTo(new Uri($"https://test.application.com/environments/{key}"));
    }
    
    [Fact]
    public void GetRedirectUri_Should_Return_Correct_Uri()
    {
        // Arrange
        string key = new('A', 12);
        Uri expectedUri = new("https://temp.uri.com");
        
        _sut.Add(
            key,
            expectedUri,
            new Uri("https://test.application.com"));
        
        // Act
        Uri? actualUri = _sut.GetRedirectUri(key);

        // Assert
        actualUri.Should().BeEquivalentTo(expectedUri);
    }
}