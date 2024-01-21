using PreviewEnvironments.Application.Extensions;

namespace PreviewEnvironments.Application.Test.Unit.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void WithFallback_Should_Use_Token_When_Token_Is_Not_Null_Or_Whitespace()
    {
        // Arrange
        const string token = "test-access-token";

        // Act
        string? actualToken = token.WithFallback(null);

        // Assert
        actualToken.Should().NotBeNull();
        actualToken.Should().Be(token);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void WithFallback_Should_Use_Fallback_When_Token_Is_Null_Or_Whitespace(
        string? initialToken)
    {
        // Arrange
        const string expectedToken = "test-access-token";

        // Act
        string? actualToken = initialToken.WithFallback(expectedToken);

        // Assert
        actualToken.Should().NotBeNull();
        actualToken.Should().Be(expectedToken);
    }
}