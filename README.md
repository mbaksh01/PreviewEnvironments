# Setup

Learn how to setup a Azure DevOps Project with Preview Environments [here](./docs/Setup.md).

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

# Ideas

## Business Hours

Allow user to specify a start time and end time in which containers will be
active. This will stop containers after the end time and start them again when
the start time is hit.

## Commands

Allow users to execute commands on the preview environments using comments in
the PR, like `/preview-env restart` to restart an env.

# Creating the sample app

- Run agent locally
- Run docker locally
- Run registry locally

Once the top has been complete, the pipeline can be ran.
