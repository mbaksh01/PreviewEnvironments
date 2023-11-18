using System.Text.Json.Serialization;

namespace PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

internal sealed class PullRequestThread
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
    ///   <item>
    ///     <term>wontFix</term>
    ///     <description>The thread status is resolved as won't fix.</description>
    ///   </item>
    /// </list>
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; set; }
}

internal sealed class Comment
{
    /// <summary>
    /// Parent id of comment.
    /// </summary>
    [JsonPropertyName("parentCommentId")]
    public int ParentCommentId { get; set; }

    /// <summary>
    /// Message to display on pull request.
    /// </summary>
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    /// <summary>
    /// Type of comment. Supported types below:
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
