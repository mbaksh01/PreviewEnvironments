using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Contracts.AzureDevOps.v2;

namespace PreviewEnvironments.Application.Helpers;

public static class IdHelper
{
    public static string GetAzurePipelinesId(AzurePipelines pipeline)
    {
        return $"{pipeline.ProjectName}-{pipeline.BuildDefinitionId}";
    }
    
    public static string GetAzurePipelinesId(BuildCompleteContract contract)
    {
        return $"{contract.Resource.Project.Name}-{contract.Resource.Definition.Id}";
    }
}