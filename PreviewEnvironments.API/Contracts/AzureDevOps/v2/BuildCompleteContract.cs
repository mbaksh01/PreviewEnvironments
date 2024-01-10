using System.Text.Json.Serialization;

namespace PreviewEnvironments.API.Contracts.AzureDevOps.v2;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public partial class BuildCompleteContract
{
    [JsonPropertyName("subscriptionId")]
    public Guid SubscriptionId { get; set; }

    [JsonPropertyName("notificationId")]
    public long NotificationId { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("eventType")]
    public string EventType { get; set; }

    [JsonPropertyName("publisherId")]
    public string PublisherId { get; set; }

    [JsonPropertyName("message")]
    public BCMessage Message { get; set; }

    [JsonPropertyName("detailedMessage")]
    public BCMessage DetailedMessage { get; set; }

    [JsonPropertyName("resource")]
    public BCResource Resource { get; set; }

    [JsonPropertyName("resourceVersion")]
    public string ResourceVersion { get; set; }

    [JsonPropertyName("resourceContainers")]
    public BCResourceContainers ResourceContainers { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate { get; set; }
}

public partial class BCMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("html")]
    public string Html { get; set; }

    [JsonPropertyName("markdown")]
    public string Markdown { get; set; }
}

public partial class BCResource
{
    [JsonPropertyName("_links")]
    public BCResourceLinks Links { get; set; }

    [JsonPropertyName("properties")]
    public BCProperties Properties { get; set; }

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    [JsonPropertyName("validationResults")]
    public object[] ValidationResults { get; set; }

    [JsonPropertyName("plans")]
    public BCPlan[] Plans { get; set; }

    [JsonPropertyName("templateParameters")]
    public BCTemplateParameters TemplateParameters { get; set; }

    [JsonPropertyName("triggerInfo")]
    public BCTriggerInfo? TriggerInfo { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("buildNumber")]
    public string BuildNumber { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; }

    [JsonPropertyName("queueTime")]
    public DateTimeOffset QueueTime { get; set; }

    [JsonPropertyName("startTime")]
    public DateTimeOffset StartTime { get; set; }

    [JsonPropertyName("finishTime")]
    public DateTimeOffset FinishTime { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("definition")]
    public BCDefinition Definition { get; set; }

    [JsonPropertyName("buildNumberRevision")]
    public long BuildNumberRevision { get; set; }

    [JsonPropertyName("project")]
    public BCProject? Project { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("sourceBranch")]
    public string SourceBranch { get; set; }

    [JsonPropertyName("sourceVersion")]
    public string SourceVersion { get; set; }

    [JsonPropertyName("queue")]
    public BCQueue Queue { get; set; }

    [JsonPropertyName("priority")]
    public string Priority { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    [JsonPropertyName("requestedFor")]
    public BCLastChangedBy RequestedFor { get; set; }

    [JsonPropertyName("requestedBy")]
    public BCLastChangedBy RequestedBy { get; set; }

    [JsonPropertyName("lastChangedDate")]
    public DateTimeOffset LastChangedDate { get; set; }

    [JsonPropertyName("lastChangedBy")]
    public BCLastChangedBy LastChangedBy { get; set; }

    [JsonPropertyName("parameters")]
    public string Parameters { get; set; }

    [JsonPropertyName("orchestrationPlan")]
    public BCPlan OrchestrationPlan { get; set; }

    [JsonPropertyName("logs")]
    public BCLogs Logs { get; set; }

    [JsonPropertyName("repository")]
    public BCRepository Repository { get; set; }

    [JsonPropertyName("retainedByRelease")]
    public bool RetainedByRelease { get; set; }

    [JsonPropertyName("triggeredByBuild")]
    public object? TriggeredByBuild { get; set; }

    [JsonPropertyName("appendCommitMessageToRunName")]
    public bool AppendCommitMessageToRunName { get; set; }
}

public partial class BCDefinition
{
    [JsonPropertyName("drafts")]
    public object[] Drafts { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("queueStatus")]
    public string QueueStatus { get; set; }

    [JsonPropertyName("revision")]
    public long Revision { get; set; }

    [JsonPropertyName("project")]
    public BCProject Project { get; set; }
}

public partial class BCProject
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("revision")]
    public long Revision { get; set; }

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; }

    [JsonPropertyName("lastUpdateTime")]
    public DateTimeOffset LastUpdateTime { get; set; }
}

public partial class BCLastChangedBy
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("_links")]
    public BCLastChangedByLinks Links { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; }

    [JsonPropertyName("imageUrl")]
    public Uri ImageUrl { get; set; }

    [JsonPropertyName("descriptor")]
    public string Descriptor { get; set; }
}

public partial class BCLastChangedByLinks
{
    [JsonPropertyName("avatar")]
    public BCBadge Avatar { get; set; }
}

public partial class BCBadge
{
    [JsonPropertyName("href")]
    public Uri Href { get; set; }
}

public partial class BCResourceLinks
{
    [JsonPropertyName("self")]
    public BCBadge Self { get; set; }

    [JsonPropertyName("web")]
    public BCBadge Web { get; set; }

    [JsonPropertyName("sourceVersionDisplayUri")]
    public BCBadge SourceVersionDisplayUri { get; set; }

    [JsonPropertyName("timeline")]
    public BCBadge Timeline { get; set; }

    [JsonPropertyName("badge")]
    public BCBadge Badge { get; set; }
}

public partial class BCLogs
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }
}

public partial class BCPlan
{
    [JsonPropertyName("planId")]
    public Guid PlanId { get; set; }
}

public partial class BCProperties
{
}

public partial class BCQueue
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("pool")]
    public BCPool Pool { get; set; }
}

public partial class BCPool
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("isHosted")]
    public bool IsHosted { get; set; }
}

public partial class BCRepository
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("clean")]
    public object? Clean { get; set; }

    [JsonPropertyName("checkoutSubmodules")]
    public bool CheckoutSubmodules { get; set; }
}

public partial class BCTemplateParameters
{
    [JsonPropertyName("AssociatedWorkItems")]
    public string AssociatedWorkItems { get; set; }
}

public partial class BCTriggerInfo
{
    [JsonPropertyName("pr.number")]
    public int PrNumber { get; set; }

    [JsonPropertyName("pr.isFork")]
    public string PrIsFork { get; set; }

    [JsonPropertyName("pr.triggerRepository")]
    public Guid PrTriggerRepository { get; set; }

    [JsonPropertyName("pr.triggerRepository.Type")]
    public string PrTriggerRepositoryType { get; set; }
}

public partial class BCResourceContainers
{
    [JsonPropertyName("collection")]
    public BCAccount Collection { get; set; }

    [JsonPropertyName("account")]
    public BCAccount Account { get; set; }

    [JsonPropertyName("project")]
    public BCAccount Project { get; set; }
}

public partial class BCAccount
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("baseUrl")]
    public Uri BaseUrl { get; set; }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.