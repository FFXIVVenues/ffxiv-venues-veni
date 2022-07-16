cd FFXIVVenues.VeniKi
docker build ./ -t ocridion/ffxivvenues.veni:latest --build-arg NUGET_REPO_PASSWORD=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
docker push ocridion/ffxivvenues.veni:latest


# docker run ocridion/ffxivvenues.veni:latest --env-file ./veni.env