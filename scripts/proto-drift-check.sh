#!/usr/bin/env bash
# scripts/proto-drift-check.sh
#
# Detects accidental edits to vendored .proto files that were not
# accompanied by an update of `.upstream.sha256`.
#
# Workflow (XYPortal.EvGRPC plan-evgrpc.md Step 1.3):
#   - Every .proto under XYPortal.EvGRPC/src/XYPortal.EvGRPC.gRPC/proto/evgrpc/
#     has its expected SHA-256 listed in
#     "$REPO_ROOT/XYPortal.EvGRPC/src/XYPortal.EvGRPC.gRPC/proto/evgrpc/.upstream.sha256"
#   - CI / local: `sha256sum -c` on that file.
#   - Failure means a .proto was edited without bumping the manifest;
#     bump the manifest in the same commit that edits a .proto:
#       cd "$REPO_ROOT/XYPortal.EvGRPC/src/XYPortal.EvGRPC.gRPC/proto/evgrpc"
#       sha256sum *.proto > .upstream.sha256
#
# Exits 0 on pass, non-zero on hash mismatch or missing manifest.

set -euo pipefail

# Resolve paths relative to this script so it works from
# any cwd (CI, repo root, deep submodule checkout).
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
MANIFEST=""$REPO_ROOT/XYPortal.EvGRPC/src/XYPortal.EvGRPC.gRPC/proto/evgrpc/.upstream.sha256""

if [[ ! -f "$MANIFEST" ]]; then
    echo "ERROR: manifest not found: $MANIFEST" >&2
    exit 2
fi

echo "Running proto drift check against $MANIFEST"
if (cd "$REPO_ROOT/XYPortal.EvGRPC/src/XYPortal.EvGRPC.gRPC/proto/evgrpc" && sha256sum -c "$MANIFEST"); then
    echo "OK: all vendored .proto files match the manifest"
else
    echo "FAIL: at least one vendored .proto file has been edited" >&2
    echo "      without bumping $MANIFEST." >&2
    echo "      Run: (cd "$REPO_ROOT/XYPortal.EvGRPC/src/XYPortal.EvGRPC.gRPC/proto/evgrpc" && sha256sum *.proto > .upstream.sha256)" >&2
    exit 1
fi
