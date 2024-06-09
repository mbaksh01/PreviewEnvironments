# Configuration Files

Configuration (JSON) files are used to define details about how your preview
environment should be managed and deployed.

## Schema

### Root

| Property Name | Description                                                           | Required |
|---------------|-----------------------------------------------------------------------|----------|
| Deployment    | Stores configuration related to how the container should be deployed. | ✅        |
| GitProvider   | Stored the value of the git provider used by your project.            | ✅        |
| BuildServer   | Stores the name of the build server used by your project.             | ✅        |

### Deployment

| Property Name             | Description                                                                                                                                                              | Required | Default Value |
|---------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------|---------------|
| ContainerHostAddress      | The host address where the deployed container will be accessible. This value is used to send a message to the pull request with a link to where the container is hosted. | ✅        |               |
| ImageName                 | The name of the docker image containing your application.                                                                                                                | ✅        |               |
| ImageRegistry             | The docker registry where the docker image can be pulled from.                                                                                                           | ✅        |               |
| AllowedDeploymentPorts    | A list of ports which can be used to deploy a container. When empty a random port between 10,000 and 60,000 is chosen.                                                   | ❌        |               |
| ContainerTimeoutSeconds   | Stores how long the container should be running for in seconds.                                                                                                          | ✅        |               |
| CreateContainerRetryCount | Stores how many retries should occur when trying to create a container.                                                                                                  | ✅        |               |
| ColdStartEnabled          | If true containers will be paused when created and will start when a user navigates to the environment link.                                                             | ❌        | false         |

### Azure Repos (Optional, required when GitProvider is AzureRepos)

| Property Name         | Description                                                                                                  | Required | Default Value                                  |
|-----------------------|--------------------------------------------------------------------------------------------------------------|----------|------------------------------------------------|
| Base Address          | Address of Azure DevOps.                                                                                     | ❌        | https://dev.azure.com                          |
| Organization Name     | Name of the organization containing the repository where the pull request message and status will be posted. | ✅        |                                                |
| Project Name          | Name of the project containing the repository where pull request messages and status will be posted.         | ✅        |                                                |
| Repository Name       | Name of the repository where pull request messages and status will be posted.                                | ✅        |                                                |
| Personal Access Token | Access token which will be used when calling the Azure DevOps API.                                           | ❌        | Value of AzAccessToken environmental variable. |

### Azure Pipelines (Optional, required when BuildServer is AzurePipelines)

| Property Name       | Description                                                                 | Required |
|---------------------|-----------------------------------------------------------------------------|----------|
| Project Name        | Name of the project containing the pipeline which will trigger the webhook. | ✅        |
| Build Definition Id | Id of the build definition which will trigger the webhook.                  | ✅        |
