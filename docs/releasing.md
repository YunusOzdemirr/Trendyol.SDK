# Releasing Trendyol.Sdk

Releases are built and published by GitHub Actions when a version tag is pushed.
The workflow uses NuGet.org Trusted Publishing (OIDC), so the repository does not
store a long-lived NuGet API key.

## One-time setup

### 1. Add the GitHub environment

In the GitHub repository, open **Settings > Environments** and create an
environment named `release`.

Recommended protection settings:

- Add a required reviewer for public releases.
- Restrict deployments to tags matching `v*`.
- Prevent environment administrators from bypassing protection rules, if that
  matches the repository's ownership model.

Add an environment secret named `NUGET_USER`. Its value must be the NuGet.org
profile name that owns the package, not an email address. This is an account
identifier, not an API key.

### 2. Add the NuGet.org trusted publishing policy

Sign in to NuGet.org and open **Trusted Publishing** from the account menu. Add a
GitHub Actions policy with these exact values:

| Field | Value |
|---|---|
| Repository owner | `YunusOzdemirr` |
| Repository | `Trendyol.SDK` |
| Workflow file | `release.yml` |
| Environment | `release` |

Select the individual or organization that owns `Trendyol.Sdk` as the policy
owner. The workflow file field must contain only `release.yml`, not the full
`.github/workflows/release.yml` path.

NuGet.org may mark a new policy as temporarily active until its first successful
publish. If so, run the first release within the activation window shown by
NuGet.org.

## Publishing a release

The tag is the release version and must point to a commit on the repository's
default branch. Supported examples:

- `v0.1.0-alpha.1` produces NuGet version `0.1.0-alpha.1` and a GitHub prerelease.
- `v0.1.0` produces NuGet version `0.1.0` and a latest GitHub release.

After committing the release-ready code and documentation:

```powershell
git tag -a v0.1.0-alpha.1 -m "Trendyol.Sdk 0.1.0-alpha.1"
git push origin main --follow-tags
```

The tag push starts `.github/workflows/release.yml`, which performs these steps:

1. Validate the semantic version tag and confirm that its commit is on `main`.
2. Restore, build, and run all Release tests.
3. Create the `.nupkg` and `.snupkg`, using the tag as the package version.
4. Store both files as a GitHub Actions artifact.
5. Exchange GitHub's OIDC token for a short-lived NuGet credential.
6. Publish the package and symbols to NuGet.org.
7. Publish the package to GitHub Packages using the workflow's `GITHUB_TOKEN`.
8. Create a GitHub Release with generated notes and both package files attached.

The `PackageVersion` in the project file remains useful for local builds, but
the release workflow deliberately overrides it with the pushed tag. A failed
build or test prevents publishing. The `release` environment's protection rules
are evaluated immediately before NuGet.org publishing.

The GitHub Packages copy is linked to this repository through the package's
`RepositoryUrl` metadata and appears in the repository's **Packages** section.
No personal access token or additional secret is required. Symbols remain on
NuGet.org's symbol server; GitHub Packages receives the primary `.nupkg` only.

## Backfilling an existing tag

When GitHub Packages publishing is added after a NuGet.org release already
exists, do not delete or recreate the tag. After the updated workflow is merged
into the default branch, open **Actions > Release > Run workflow**, enter the
existing tag (for example, `v0.1.0-alpha.1`), and start the run.

The workflow checks out that exact tag, rebuilds and tests it, skips the existing
immutable NuGet.org version, publishes it to GitHub Packages, and leaves an
existing GitHub Release unchanged.

## Failure and retry behavior

Open **Actions > Release**, select the failed run, and use **Re-run failed jobs**.
The NuGet push uses `--skip-duplicate`, so a retry can continue if the package was
already accepted before a later step failed. NuGet package versions are immutable;
never delete and recreate a published version tag with different source code.

If a release has incorrect package contents, unlist it on NuGet.org, fix the
source, increment the version, and publish a new tag.
