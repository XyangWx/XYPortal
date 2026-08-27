# CI integration for the proto drift check

The vendored .proto files under
`XYPortal.EvGRPC/src/XYPortal.EvGRPC.gRPC/proto/evgrpc/` are
expected to match the SHA-256 fingerprints listed in
`.upstream.sha256` next to them. The local script
`scripts/proto-drift-check.sh` verifies this.

Two CI configs wire the same script into the two remotes this
repo is published to:

## GitHub Actions

File: `.github/workflows/evgrpc-proto-check.yml`

Trigger:
  - `push` to `master`, `main`, `develop`
  - `pull_request` against those branches
  - `workflow_dispatch` (manual run)

Path filter (only run when these change):
  - `XYPortal.EvGRPC/src/XYPortal.EvGRPC.gRPC/proto/**`
  - `scripts/proto-drift-check.sh`
  - the workflow file itself

Image: `ubuntu-latest`. The script uses only bash + sha256sum,
no .NET SDK required. Job timeout 5 minutes (typical run < 5 s).

## GitLab CI (mksword)

File: `.gitlab-ci.yml`

Trigger:
  - merge requests
  - pushes to `master` / `main` / `develop`

Same path filter as GitHub.

Image: `alpine:latest` (smaller, faster).

## What the script does

`scripts/proto-drift-check.sh` is cwd-agnostic — it resolves
paths relative to its own location, so CI just needs
`bash scripts/proto-drift-check.sh`. The exit code drives the
job: 0 on pass, non-zero on any hash mismatch (with a
human-readable hint pointing to the manifest bump command).

## Adding a new claim name

The Decorator's `TryClaim` probe order is the single source of
truth for the access-token claim convention. If a future CI
integration adds a new claim type (e.g. `mks_token`), add it to
the probe list and bump this doc.
