# Decision Record for Automatic Container Restarts

Title: Automatic container restarts via API redirects.

## Context and Problem Statement

When sharing preview environment links with clients often they would attempt to
use the link after the container had been automatically stopped. An initial fix
for this was to increase the container expiration time so that they would stay
on for longer but after increasing the expiration time to one day we still
experienced this issue often. This would mean our client would realise the link
is no longer working, email one of the developers, and they would have to find
the associated pull request and trigger a restart for the preview environment
before they can resend the link.

## Decision

I have decided to implement automatic container restarts to improve client
satisfaction and reduce developer load.

## Rational

The decision was made based on the following factors:

1. Improved automation: As well as a user being able to restart a container,
they can also be restarted by the preview environments API which reduces the
need for developer interaction as containers will restart on their own if needed.

2. Reduced uptime: With automatic restarts in place the expiration time for each
preview environment can be reduced as the application will detect if someone is
trying to reach a preview environment which is not running and it will start it
for them. This way the container expiration can be the average session time for
a preview environment.

## Implementation Plan

The following steps will be taken to implement the use of automatic restarts:

1. Link generation: Two links will get generated when a container is created.
The first link will navigate to an API endpoint with an id which can be used to
identify the correct preview environment and get its actual link.

2. Link usage: One link will navigate the user to the API which will be able to
start the preview environment if needed and the other will take the user to the
running instance of the preview environment.

## Conclusion

By introducing the use of automatic restarts, preview environments can be run
more efficiently and can improve the developer and client experience.
