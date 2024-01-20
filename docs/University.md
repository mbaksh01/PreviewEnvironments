# Consumer Requirements

- The application must run using a docker image.
- The consumer is in charge of building and pushing images.
- The consumer must setup webhooks to trigger the deployment/cleanup of images.

## Application Requirements

- The application must listen for messages to deploy/cleanup images/envs.
- The application must host a registry as a location to store pr images.
- The application must post a message to the PR with a link to the preview env.

# Competitors

## Azure Static Web Apps

Only works with static web apps E.g. React, Vue, Blazor.

## JenkinsX

Have to deploy to k8s which is not idea for small companies.

## Azure DevTest Labs

Build on VM rather than docker container. More bulky and not light weight.
Designed for applications rather than web apps.

## Harness CI