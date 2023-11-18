# Data Flow

1. Create a PR
2. Build code into a docker file.
3. Push code to pr image registry.
4. Send API message to say build has complete.
5. If the container exists, remove container, pull new image, deploy preview env
otherwise pull image and deploy preview env.
6. POST message to PR to say preview deployed with link to hosted version.
7. When PR complete or abandoned, remove container and image.

# Application logic

- Start application.
- Start registry.
- Listen for complete builds associated with PRs.
- Deploy preview env.
- Add time out to stop container after a specified period of time.
- Clean up after a PR has been closed or abandoned.

# Consumer Requirements

- The application must run using a docker image.
- The consumer is in charge of building and pushing images.
- The consumer must setup webhooks to trigger the deployment/cleanup of images.

## Application Requirements

- The application must listen for messages to deploy/cleanup images/envs.
- The application must host a registry as a location to store pr images.
- The application must post a message to the PR with a link to the preview env.

# Ideas

## Business Hours

Allow user to specify a start time and end time in which containers will be
active. This will stop containers after the end time and start them again when
the start time is hit.

## Commands

Allow users to execute commands on the preview environments using comments in
the PR, like `/preview-env restart` to restart an env.

# Competitors

## Azure Static Web Apps

Only works with static web apps E.g. React, Vue, Blazor.

## JenkinsX

Have to deploy to k8s which is not idea for small companies.

## Azure DevTest Labs

Build on VM rather than docker container. More bulky and not light weight.
Designed for applications rather than web apps.

## Harness CI

# Creating the sample app

- Run agent locally
- Run docker locally
- Run registry locally

Once the top has been complete, the pipeline can be ran.

## Pipeline Setup

1. Service connection to docker registry. No credentials required so i just used
'1' and '1' for the id and password.
2. YAML pipeline containing a build and push docker step to build the image and
push to locally running docker image.
