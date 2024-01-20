# Setup a Project with Preview Environments

## Azure DevOps

Learn how to setup a Azure DevOps project to work with preview environments.

### Prerequisites

- The preview environments API must be hosted in a location accessible to Azure DevOps.
- A container registry must be hosted somewhere accessible to the build agent running the Docker push command.

### Service Connection

A service connection to the docker registry you want to push your images to
needs to be configured.

1. Navigate to the project containing the pipeline which will push the Docker image.
2. Open the project settings.
3. Navigate to 'Service Connections' under the 'Pipelines' section.
4. Click 'New Service Connection'.
5. Select 'Docker Registry', fill out the necessary details, and click 'Save'.

### Build Pipeline

A build pipeline which pushes your application's Docker image needs to be configured.

1. Navigate to the project where you added your service connection.
2. Navigate to the 'Pipelines' section of the project.
3. Create or edit an existing pipeline.
4. Add a Docker task to build and push your image. Example below:
   ```yaml
   - task: Docker@2
     displayName: Build and Push Docker Image
     inputs:
       containerRegistry: 'MyContainerRegistry' # Name of your service connection here.
       repository: 'myapp' # Name of your docker image.
       command: 'buildAndPush'
       Dockerfile: '$(Build.SourcesDirectory)/src/myApp/Dockerfile' # Path to your Dockerfile.
       buildContext: '$(Build.SourcesDirectory)/src' # Build context.
       tags: 'pr-$(System.PullRequest.PullRequestId)' # The docker image tag must be the pull request number with 'pr-' prefixed.
   ```
5. (Optional): Add another task to clean the image from the build agent. Example below:
   ```yaml
   - script: 'docker rmi mycr.domain.com/myapp:pr-$(System.PullRequest.PullRequestId)' # Adjust image name as needed.
     displayName: Remove Image From Agent
   ```
   **⚠️ Note:** If your using this task then its recommended to use variables to
   reduce the repetition of the docker image name and docker image tag.

6. Run your pipeline and ensure that the image is pushed, named and tagged correctly.

### Webhooks

Webhooks are required to notify the preview environments API of when a build is
complete or when a pull request is updated.

1. Navigate to the project you want to add support for preview environments.
2. Open the project settings.
3. Navigate to 'Service hooks' under the 'General' section.
4. Click +, select 'Web Hooks', and click 'Next'.
5. Set the trigger to 'Build completed', set the 'Build pipeline' to the name of
   the pipeline which builds and pushes the docker image, set the 'Build status'
   to 'Succeeded', and click 'Next'.
6. Set the URL to the host address of the preview environments API with
   '/vstfs/buildComplete' appended.
7. Test the connection by clicking 'Test' and then click 'Finish'.

Repeat the above steps to setup the pull request updated webhook using the
following values:

- Trigger: 'Pull request updated'.
- Repository: Your repository name.
- Change: 'Status changed'.
- Url: Host address of the preview environments API with '/vstfs/pullRequestUpdated' appended.

### Custom Pull Request Status

The preview environments API posts a custom status check which can be added to
your pull request status checks.

⚠️ **Note:** The custom status can only be setup after at least one successful
deployment of a preview environment has happened.

1. Navigate to the project which supports preview environments.
2. Open the project settings.
3. Navigate to 'Repositories' under the 'Repos' section.
4. Select the repository which is configured to use preview environments.
5. Select the 'Policies' tab and select the branch you want to add the status to.
6. Navigate to 'Status Checks' and click +.
7. In the 'Status to check' dropdown select 'preview-environments/deployment-status'.
8. Configure any additional settings and click 'Save'.

The preview environment deployment status will now appear on any pull request
created which is merging into the branch you selected in step 5.