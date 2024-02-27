using System.Text.Json.Serialization;

namespace PreviewEnvironments.Contracts.AzureDevOps.v2;

public class PRCOAccount
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; }
}

public class PRCOAuthor
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("_links")]
    public PRCOLinks Links { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; }

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; }

    [JsonPropertyName("descriptor")]
    public string Descriptor { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}

public class PRCOAvatar
{
    [JsonPropertyName("href")]
    public string Href { get; set; }
}

public class PRCOCollection
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; }
}

public class PRCOComment
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("parentCommentId")]
    public int ParentCommentId { get; set; }

    [JsonPropertyName("author")]
    public PRCOAuthor Author { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("publishedDate")]
    public DateTime PublishedDate { get; set; }

    [JsonPropertyName("lastUpdatedDate")]
    public DateTime LastUpdatedDate { get; set; }

    [JsonPropertyName("lastContentUpdatedDate")]
    public DateTime LastContentUpdatedDate { get; set; }

    [JsonPropertyName("commentType")]
    public string CommentType { get; set; }

    [JsonPropertyName("usersLiked")]
    public object[] UsersLiked { get; set; }

    [JsonPropertyName("_links")]
    public PRCOLinks Links { get; set; }
}

public class PRCOCommitter
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}

public class PRCOCompletionOptions
{
    [JsonPropertyName("mergeCommitMessage")]
    public string MergeCommitMessage { get; set; }

    [JsonPropertyName("deleteSourceBranch")]
    public bool DeleteSourceBranch { get; set; }

    [JsonPropertyName("squashMerge")]
    public bool SquashMerge { get; set; }

    [JsonPropertyName("mergeStrategy")]
    public string MergeStrategy { get; set; }

    [JsonPropertyName("transitionWorkItems")]
    public bool TransitionWorkItems { get; set; }

    [JsonPropertyName("autoCompleteIgnoreConfigIds")]
    public object[] AutoCompleteIgnoreConfigIds { get; set; }
}

public class PRCOCreatedBy
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("_links")]
    public PRCOLinks Links { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; }

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; }

    [JsonPropertyName("descriptor")]
    public string Descriptor { get; set; }
}

public class PRCODetailedMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("html")]
    public string Html { get; set; }

    [JsonPropertyName("markdown")]
    public string Markdown { get; set; }
}

public class PRCOLastMergeCommit
{
    [JsonPropertyName("commitId")]
    public string CommitId { get; set; }

    [JsonPropertyName("author")]
    public PRCOAuthor Author { get; set; }

    [JsonPropertyName("committer")]
    public PRCOCommitter Committer { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class PRCOLastMergeSourceCommit
{
    [JsonPropertyName("commitId")]
    public string CommitId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class PRCOLastMergeTargetCommit
{
    [JsonPropertyName("commitId")]
    public string CommitId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class PRCOLinks
{
    [JsonPropertyName("avatar")]
    public PRCOAvatar Avatar { get; set; }

    [JsonPropertyName("self")]
    public PRCOSelf Self { get; set; }

    [JsonPropertyName("repository")]
    public PRCORepository Repository { get; set; }

    [JsonPropertyName("threads")]
    public PRCOThreads Threads { get; set; }

    [JsonPropertyName("pullRequests")]
    public PRCOPullRequests PullRequests { get; set; }
}

public class PRCOMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("html")]
    public string Html { get; set; }

    [JsonPropertyName("markdown")]
    public string Markdown { get; set; }
}

public class PRCOProject
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("revision")]
    public int Revision { get; set; }

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; }

    [JsonPropertyName("lastUpdateTime")]
    public DateTime LastUpdateTime { get; set; }

    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; }
}

public class PRCOPullRequest
{
    [JsonPropertyName("repository")]
    public PRCORepository Repository { get; set; }

    [JsonPropertyName("pullRequestId")]
    public int PullRequestId { get; set; }

    [JsonPropertyName("codeReviewId")]
    public int CodeReviewId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("createdBy")]
    public PRCOCreatedBy CreatedBy { get; set; }

    [JsonPropertyName("creationDate")]
    public DateTime CreationDate { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("sourceRefName")]
    public string SourceRefName { get; set; }

    [JsonPropertyName("targetRefName")]
    public string TargetRefName { get; set; }

    [JsonPropertyName("mergeStatus")]
    public string MergeStatus { get; set; }

    [JsonPropertyName("isDraft")]
    public bool IsDraft { get; set; }

    [JsonPropertyName("mergeId")]
    public string MergeId { get; set; }

    [JsonPropertyName("lastMergeSourceCommit")]
    public PRCOLastMergeSourceCommit LastMergeSourceCommit { get; set; }

    [JsonPropertyName("lastMergeTargetCommit")]
    public PRCOLastMergeTargetCommit LastMergeTargetCommit { get; set; }

    [JsonPropertyName("lastMergeCommit")]
    public PRCOLastMergeCommit LastMergeCommit { get; set; }

    [JsonPropertyName("reviewers")]
    public PRCOReviewer[] Reviewers { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("completionOptions")]
    public PRCOCompletionOptions CompletionOptions { get; set; }

    [JsonPropertyName("supportsIterations")]
    public bool SupportsIterations { get; set; }

    [JsonPropertyName("artifactId")]
    public string ArtifactId { get; set; }
}

public class PRCOPullRequests
{
    [JsonPropertyName("href")]
    public string Href { get; set; }
}

public class PRCORepository
{
    [JsonPropertyName("href")]
    public string Href { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("project")]
    public PRCOProject Project { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("remoteUrl")]
    public string RemoteUrl { get; set; }

    [JsonPropertyName("sshUrl")]
    public string SshUrl { get; set; }

    [JsonPropertyName("webUrl")]
    public string WebUrl { get; set; }

    [JsonPropertyName("isDisabled")]
    public bool IsDisabled { get; set; }

    [JsonPropertyName("isInMaintenance")]
    public bool IsInMaintenance { get; set; }
}

public class PRCOResource
{
    [JsonPropertyName("comment")]
    public PRCOComment Comment { get; set; }

    [JsonPropertyName("pullRequest")]
    public PRCOPullRequest PullRequest { get; set; }
}

public class PRCOResourceContainers
{
    [JsonPropertyName("collection")]
    public PRCOCollection Collection { get; set; }

    [JsonPropertyName("account")]
    public PRCOAccount Account { get; set; }

    [JsonPropertyName("project")]
    public PRCOProject Project { get; set; }
}

public class PRCOReviewer
{
    [JsonPropertyName("reviewerUrl")]
    public string ReviewerUrl { get; set; }

    [JsonPropertyName("vote")]
    public int Vote { get; set; }

    [JsonPropertyName("votedFor")]
    public PRCOVotedFor[] VotedFor { get; set; }

    [JsonPropertyName("hasDeclined")]
    public bool HasDeclined { get; set; }

    [JsonPropertyName("isFlagged")]
    public bool IsFlagged { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("_links")]
    public PRCOLinks Links { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; }

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; }

    [JsonPropertyName("isRequired")]
    public bool? IsRequired { get; set; }

    [JsonPropertyName("isContainer")]
    public bool? IsContainer { get; set; }
}

public class PullRequestCommentedOnContract
{
    [JsonPropertyName("subscriptionId")]
    public string SubscriptionId { get; set; }

    [JsonPropertyName("notificationId")]
    public int NotificationId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("eventType")]
    public string EventType { get; set; }

    [JsonPropertyName("publisherId")]
    public string PublisherId { get; set; }

    [JsonPropertyName("message")]
    public PRCOMessage Message { get; set; }

    [JsonPropertyName("detailedMessage")]
    public PRCODetailedMessage DetailedMessage { get; set; }

    [JsonPropertyName("resource")]
    public PRCOResource Resource { get; set; }

    [JsonPropertyName("resourceVersion")]
    public string ResourceVersion { get; set; }

    [JsonPropertyName("resourceContainers")]
    public PRCOResourceContainers ResourceContainers { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }
}

public class PRCOSelf
{
    [JsonPropertyName("href")]
    public string Href { get; set; }
}

public class PRCOThreads
{
    [JsonPropertyName("href")]
    public string Href { get; set; }
}

public class PRCOVotedFor
{
    [JsonPropertyName("reviewerUrl")]
    public string ReviewerUrl { get; set; }

    [JsonPropertyName("vote")]
    public int Vote { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("_links")]
    public PRCOLinks Links { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; }

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; }

    [JsonPropertyName("isContainer")]
    public bool IsContainer { get; set; }
}
