using System.Text.Json.Serialization;

namespace PreviewEnvironments.Contracts.AzureDevOps.v1;

public sealed class PullRequestUpdatedContract
{
    [JsonPropertyName("subscriptionId")]
    public Guid SubscriptionId { get; set; }

    [JsonPropertyName("notificationId")]
    public int NotificationId { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("eventType")]
    public string EventType { get; set; }

    [JsonPropertyName("publisherId")]
    public string PublisherId { get; set; }

    [JsonPropertyName("message")]
    public PRMessage Message { get; set; }

    [JsonPropertyName("detailedMessage")]
    public PRMessage DetailedMessage { get; set; }

    [JsonPropertyName("resource")]
    public PrResource Resource { get; set; }

    [JsonPropertyName("resourceVersion")]
    public string ResourceVersion { get; set; }

    [JsonPropertyName("resourceContainers")]
    public PRResourceContainers ResourceContainers { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate { get; set; }
}

public sealed class PRMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("html")]
    public string Html { get; set; }

    [JsonPropertyName("markdown")]
    public string Markdown { get; set; }
}

public sealed class PrResource
{
    [JsonPropertyName("repository")]
    public PRRepository Repository { get; set; }

    [JsonPropertyName("pullRequestId")]
    public int PullRequestId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("createdBy")]
    public PRCreatedBy CreatedBy { get; set; }

    [JsonPropertyName("creationDate")]
    public DateTimeOffset CreationDate { get; set; }

    [JsonPropertyName("closedDate")]
    public DateTimeOffset ClosedDate { get; set; }

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

    [JsonPropertyName("mergeId")]
    public Guid MergeId { get; set; }

    [JsonPropertyName("lastMergeSourceCommit")]
    public PRCommit LastMergeSourceCommit { get; set; }

    [JsonPropertyName("lastMergeTargetCommit")]
    public PRCommit LastMergeTargetCommit { get; set; }

    [JsonPropertyName("lastMergeCommit")]
    public PRCommit LastMergeCommit { get; set; }

    [JsonPropertyName("reviewers")]
    public PRReviewer[] Reviewers { get; set; }

    [JsonPropertyName("commits")]
    public PRCommit[] Commits { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("_links")]
    public PRLinks Links { get; set; }
}

public sealed class PRCommit
{
    [JsonPropertyName("commitId")]
    public string CommitId { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }
}

public sealed class PRCreatedBy
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; }

    [JsonPropertyName("imageUrl")]
    public Uri ImageUrl { get; set; }
}

public sealed class PRLinks
{
    [JsonPropertyName("web")]
    public PRStatuses Web { get; set; }

    [JsonPropertyName("statuses")]
    public PRStatuses Statuses { get; set; }
}

public sealed class PRStatuses
{
    [JsonPropertyName("href")]
    public Uri Href { get; set; }
}

public sealed class PRRepository
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("project")]
    public PRProject Project { get; set; }

    [JsonPropertyName("defaultBranch")]
    public string DefaultBranch { get; set; }

    [JsonPropertyName("remoteUrl")]
    public Uri RemoteUrl { get; set; }
}

public sealed class PRProject
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; }

    [JsonPropertyName("lastUpdateTime")]
    public DateTimeOffset LastUpdateTime { get; set; }
}

public sealed class PRReviewer
{
    [JsonPropertyName("reviewerUrl")]
    public object ReviewerUrl { get; set; }

    [JsonPropertyName("vote")]
    public int Vote { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; }

    [JsonPropertyName("imageUrl")]
    public Uri ImageUrl { get; set; }

    [JsonPropertyName("isContainer")]
    public bool IsContainer { get; set; }
}

public sealed class PRResourceContainers
{
    [JsonPropertyName("collection")]
    public PRAccount Collection { get; set; }

    [JsonPropertyName("account")]
    public PRAccount Account { get; set; }

    [JsonPropertyName("project")]
    public PRAccount Project { get; set; }
}

public sealed class PRAccount
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}
