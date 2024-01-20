# How to deploy the application

The application needs to be hosted somewhere which is accessible to your source
control provider and the host needs to have access to docker.

## Building the application

Run the following command to build the PreviewEnvironments.API in docker.

⚠️ **Note:** This command must be ran from the src folder.

`docker build -t previewenvironments.api -f ./PreviewEnvironments.API/Dockerfile .`

## Running the application

Run the following command to run the preview environments image locally.

Windows:
```
docker run `
    -itd `
    --rm `
    -v "/var/run/docker.sock:/var/run/docker.sock" `
    -e ASPNETCORE_ENVIRONMENT=Development `
    --privileged=true `
    --name previewenvironments.api `
    previewenvironments.api
```

Linux/MacOS:
```
docker run \
    -it \
    --rm \
    -v "/var/run/docker.sock:/var/run/docker.sock" \
    -e ASPNETCORE_ENVIRONMENT=Development \
    --privileged=true \
    --name previewenvironments.api \
    previewenvironments.api
```

## Pushing the application to your registry

Run the following command, replacing [host] with the host of your container registry, to tag the preview environments image you build with
the host of your container registry.

`docker tag previewenvironments [host]/previewenvironments.api`

Then run the following command to to push the image to your container registry.

`docker push [host]/previewenvironments.api`