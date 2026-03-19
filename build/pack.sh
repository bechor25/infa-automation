#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# pack.sh — Build and pack all publishable NuGet packages
#
# Usage:
#   ./build/pack.sh                          # uses version from Directory.Build.props
#   ./build/pack.sh 2.1.0                    # override version
#   ./build/pack.sh 2.1.0 https://feed.url   # pack + push to internal feed
#   NUGET_API_KEY=xyz ./build/pack.sh 2.1.0 https://feed.url
# ─────────────────────────────────────────────────────────────────────────────

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
OUTPUT_DIR="$ROOT_DIR/nupkg"

VERSION="${1:-}"
FEED_URL="${2:-}"

PROJECTS=(
  "src/Company.Automation.Contracts/Company.Automation.Contracts.csproj"
  "src/Company.Automation.Configuration/Company.Automation.Configuration.csproj"
  "src/Company.Automation.Core/Company.Automation.Core.csproj"
  "src/Company.Automation.Reporting/Company.Automation.Reporting.csproj"
  "src/Company.Automation.UI/Company.Automation.UI.csproj"
  "src/Company.Automation.API/Company.Automation.API.csproj"
  "src/Company.Automation.TestHost/Company.Automation.TestHost.csproj"
)

# Clean output directory
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

echo "==> Building and packing framework packages..."
echo "    Output: $OUTPUT_DIR"
[[ -n "$VERSION" ]] && echo "    Version: $VERSION"

for proj in "${PROJECTS[@]}"; do
  echo ""
  echo "  Packing $proj..."
  PACK_ARGS=(
    dotnet pack "$ROOT_DIR/$proj"
    --configuration Release
    --output "$OUTPUT_DIR"
    --no-build
  )
  [[ -n "$VERSION" ]] && PACK_ARGS+=("/p:PackageVersion=$VERSION")
  "${PACK_ARGS[@]}"
done

echo ""
echo "==> Packages written to $OUTPUT_DIR:"
ls -1 "$OUTPUT_DIR"

# Push to feed if URL provided
if [[ -n "$FEED_URL" ]]; then
  echo ""
  echo "==> Pushing packages to $FEED_URL..."
  API_KEY="${NUGET_API_KEY:-}"
  if [[ -z "$API_KEY" ]]; then
    echo "WARNING: NUGET_API_KEY is not set. Push may fail if feed requires authentication."
  fi

  for pkg in "$OUTPUT_DIR"/*.nupkg; do
    echo "  Pushing $pkg..."
    dotnet nuget push "$pkg" \
      --source "$FEED_URL" \
      ${API_KEY:+--api-key "$API_KEY"} \
      --skip-duplicate
  done
  echo ""
  echo "==> All packages published."
fi

echo ""
echo "Done."
