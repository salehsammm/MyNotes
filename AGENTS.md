# Project workflow

- After completing requested changes, verify them and commit your changes with a clear, descriptive message. Keep unrelated pending work out of the commit.
- After changes, publish a Release build to `bin/Release/net9.0/publish` and deploy it to the existing local IIS site at `http://localhost:5050`, following the staging and configuration-preservation instructions in `PROJECT.md`.
- Back up the deployed files, preserve machine-local configuration, use `app_offline.htm` while copying, and verify the affected routes after deployment.
