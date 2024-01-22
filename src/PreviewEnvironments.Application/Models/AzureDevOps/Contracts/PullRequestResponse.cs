using System.Text.Json.Serialization;

namespace PreviewEnvironments.Application.Models.AzureDevOps.Contracts;

internal sealed class PullRequestResponse
{
    public Repository Repository { get; set; }
    
    public int PullRequestId { get; set; }
    
    public int CodeReviewId { get; set; }
    
    public string Status { get; set; }
    
    public CreatedBy CreatedBy { get; set; }
    
    public string CreationDate { get; set; }
    
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public string SourceRefName { get; set; }
    
    public string TargetRefName { get; set; }
    
    public string MergeStatus { get; set; }
    
    public string MergeId { get; set; }
    
    public Commit LastMergeSourceCommit { get; set; }
    
    public Commit LastMergeTargetCommit { get; set; }
    
    public LastMergeCommit LastMergeCommit { get; set; }
    
    public Reviewers[] Reviewers { get; set; }
    
    public Uri Url { get; set; }
    
    [JsonPropertyName("_links")]
    public Dictionary<string, Link> Links { get; set; }
    
    public bool SupportsIterations { get; set; }
    
    public string ArtifactId { get; set; }
}

internal sealed  class Repository
{
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public Uri Url { get; set; }
    
    public Project Project { get; set; }
    
    public string RemoteUrl { get; set; }
}

internal sealed class Project
{
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string Url { get; set; }
    
    public string State { get; set; }
    
    public int Revision { get; set; }
}

internal sealed class CreatedBy
{
    public string Id { get; set; }
    
    public string DisplayName { get; set; }
    
    public string UniqueName { get; set; }
    
    public string Url { get; set; }
    
    public string ImageUrl { get; set; }
}

internal sealed class Commit
{
    public string CommitId { get; set; }
    
    public string Url { get; set; }
}

internal sealed class LastMergeCommit
{
    public string CommitId { get; set; }
    
    public Author Author { get; set; }
    
    public Committer Committer { get; set; }

    public string Comment { get; set; }
    
    public string Url { get; set; }
}

internal sealed class Author
{
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string Date { get; set; }
}

internal sealed class Committer
{
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string Date { get; set; }
}

internal sealed class Reviewers
{
    public string ReviewerUrl { get; set; }
    
    public int Vote { get; set; }
    
    public string Id { get; set; }

    public string DisplayName { get; set; }
    
    public string UniqueName { get; set; }
    
    public string Url { get; set; }
    
    public string ImageUrl { get; set; }
}

internal sealed class Link
{
    public Uri Href { get; set; }
}
