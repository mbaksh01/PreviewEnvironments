# Decision Record for Preview Environment Commands

Title: Implementing commands to interact with preview environments.

## Context and Problem Statement

Currently all preview environment management is automated and handled though
webhooks provided by the build server. This is great as a developer does not
have to remember to deploy a container, but poses a problem when a container has
been stopped and it needs to be restarted. The introduction of commands will
allow developers to interact with preview environment without having to manually
send API request when they want to restart or get information about a container.

## Decision

We have decided to implement commands to improve efficiency when using preview
environments.

## Rational

The decision was made based on the following factors:

1. User feedback: When a preview environment has been stopped the only way to
restart it is by re-queuing the build. This means that a user has to wait for
the build to complete before they can access the preview environment. This was
often pointed out as a pain point for users.

2. Ease of use: By implementing commands we are able to give users access to
their preview environment directly from the pull request page. This means they
can make decision like extending the lifetime of the container or restarting a
container right from the pull request page.

3. Future development: With the introduction of commands, there will be an
opportunity to implement new feature which will give users more control over an
individual preview environment.

## Implementation Plan

The following steps will be taken to implement the use of commands:

1. Container tracking: Currently container are tracked by the preview
environment manager but this will need to be moved to its own service. This will
allow tracked containers to be accessed and managed by multiple services.

2. Command parsing: To handle incoming commands we plan to use the
CommandLineParser NuGet package which will allow us to abstract away the logic
associated with parsing and validating a command. This will also make it easier
to add more commands in the future.

3. Feature implementation: Add the logic required to process each command.

## Conclusion

Adding the ability to use commands to interact with preview environments will
improve user experience and will allow for the growth of the project.
