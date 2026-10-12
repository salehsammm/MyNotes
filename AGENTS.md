# Project workflow

- After completing requested changes, verify them and commit your changes with a clear, descriptive message. Keep unrelated pending work out of the commit.
- Prepare a Release folder publish after changes using `dotnet publish MyNotes.csproj -c Release -o bin/Release/net9.0/publish` and report the output location.
- When deploying to the local IIS site, follow the staging and configuration-preservation instructions in `PROJECT.md`.
