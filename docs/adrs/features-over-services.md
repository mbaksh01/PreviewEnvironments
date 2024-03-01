# Decision Record For Migrating From Services To Features

Title: Migrating from services to features.

## Context and Problem Statement

When writing the tests for the `PreviewEnvironmentManager` service (a service
which manages starting containers, stopping containers, and expiring containers)
there is a lot of setup required to test small features. To combat this I am
thinking of splitting the `PreviewEnvironmentManager` into features which only
handle one task. In this case we will have a start container feature, stop
container feature and a expire containers feature.

## Decision

I have decided to breakdown my services into features.

## Rational

The decision was made based on the following factors:

1. Improved testability: By moving to features, test files will get
significantly smaller (~3 times smaller). There will also mean that each feature
will only require the services it uses.

2. SOLID principles: Moving to features means that this project will better
follow SOLID practices as each feature will only have one reason to change (If
there is a change to how a feature works).

## Implementation Plan

The following steps will be taken to implement the use of commands:

1. Create the feature files and copy the different methods from the
`PreviewEnvironmentManager` to those feature files.

2. Rearrange the tests to match the new project structure and copy the test to
their right locations.

3. Run the tests and ensure they are working as expected.

## Conclusion

By choosing features over services, testability will be improved, SOLID
practices will be better adhered to, and code complexity will be reduced.
