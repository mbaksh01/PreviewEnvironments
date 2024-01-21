using System.Net.Http.Headers;
using System.Text;
using PreviewEnvironments.Application.Extensions;

namespace PreviewEnvironments.Application.Test.Unit.Extensions;

public class HttpRequestMessageExtensionsTests
{
    [Fact]
    public void WithBasicAuthorization_Should_Add_Basic_Auth_To_Headers()
    {
        // Arrange
        using HttpRequestMessage message = new();
        const string accessToken = "test-access-token";
        
        string expectedHeaderValue =
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{accessToken}"));

        // Act
        _ = message.WithBasicAuthorization(accessToken);

        // Assert
        AuthenticationHeaderValue? authHeader = message.Headers.Authorization;

        authHeader.Should().NotBeNull();

        using (new AssertionScope())
        {
            authHeader!.Scheme.Should().Be("Basic");
            authHeader.Parameter.Should().Be(expectedHeaderValue);
        }
    }
    
    [Fact]
    public async Task WithBody_Should_Add_Body_As_Json()
    {
        // Arrange
        using HttpRequestMessage message = new();
        object body = new { name = "tester" };
        const string expectedBody = "{\"name\":\"tester\"}";
        const string expectedContentType = "application/json";

        // Act
        _ = message.WithJsonBody(body);

        // Assert
        StringContent? content = message.Content as StringContent;

        content.Should().NotBeNull();

        using (new AssertionScope())
        {
            (await content!.ReadAsStringAsync()).Should().Be(expectedBody);
            content.Headers.ContentType!.MediaType.Should().Be(expectedContentType);
        }
    }
}