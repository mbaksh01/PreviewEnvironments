using System.Text.Json.Serialization;

namespace PreviewEnvironments.Application.Models.AzureDevOps.Contracts;

/// <summary>
/// Model used to create a thread on a pull request.
/// Learn more here: https://learn.microsoft.com/en-us/rest/api/azure/devops/git/pull-request-threads?view=azure-devops-rest-7.1
/// </summary>
internal sealed class PullRequestThreadRequest
{
    /// <summary>
    /// Comments to post.
    /// </summary>
    [JsonPropertyName("comments")]
    public required Comment[] Comments { get; set; }

    /// <summary>
    /// Thread status. Supported types below:
    /// <list type="bullet">
    ///   <item>
    ///     <term>active</term>
    ///     <description>The thread status is active.</description>
    ///   </item>
    ///   <item>
    ///     <term>byDesign</term>
    ///     <description>The thread status is resolved as by design.</description>
    ///   </item>
    ///   <item>
    ///     <term>closed</term>
    ///     <description>The thread status is closed.</description>
    ///   </item>
    ///   <item>
    ///     <term>fixed</term>
    ///     <description>The thread status is resolved as fixed.</description>
    ///   </item>
    ///   <item>
    ///     <term>pending</term>
    ///     <description>The thread status is pending.</description>
    ///   </item>
    ///   <item>
    ///     <term>unknow</term>
    ///     <description>The thread status is unknown.</description>
    ///   </item>
    ///  <item>
    ///    <term>wontFix</term>
    ///    <description>The thread status is resolved as won't fix.</description>
    ///  </item>
    /// </list>
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; set; }
}

/// <summary>
/// Model used to represent a single comment on a thread.
/// </summary>
internal sealed class Comment
{
    /// <summary>
    /// The Id of the parent comment. This is used for replies.
    /// </summary>
    [JsonPropertyName("parentCommentId")]
    public int ParentCommentId { get; set; }

    /// <summary>
    /// The comment content.
    /// </summary>
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    /// <summary>
    /// The comment type at the time of creation. Supported types below:
    /// <list type="bullet">
    ///   <item>
    ///     <term>codeChange</term>
    ///     <description>The comment comes as a result of a code change.</description>
    ///   </item>
    ///   <item>
    ///     <term>system</term>
    ///     <description>The comment represents a system message.</description>
    ///   </item>
    ///   <item>
    ///     <term>text</term>
    ///     <description>This is a regular user comment.</description>
    ///   </item>
    ///   <item>
    ///     <term>unknow</term>
    ///     <description>The comment type is not known.</description>
    ///   </item>
    /// </list>
    /// </summary>
    [JsonPropertyName("commentType")]
    public required string CommentType { get; set; }
}
