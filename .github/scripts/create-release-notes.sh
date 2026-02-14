#!/bin/bash
set -e

# Create or Update GitHub Release with Installation Notes
# Creates a GitHub release with auto-generated notes and adds installation instructions
#
# Usage: ./create-release-notes.sh <semver> <package_id> [github_nuget_source]
# Example: ./create-release-notes.sh "7.0.0-alpha.1" "Our.Umbraco.Slimsy" "https://nuget.pkg.github.com/Jeavon/index.json"

SEMVER="$1"
PACKAGE_ID="$2"
GITHUB_NUGET_SOURCE="${3:-}"

if [ -z "$SEMVER" ] || [ -z "$PACKAGE_ID" ]; then
  echo "Error: Missing required arguments"
  echo "Usage: $0 <semver> <package_id> [github_nuget_source]"
  exit 1
fi

echo "Processing GitHub release for v$SEMVER (package: $PACKAGE_ID)"

# Determine installation command based on release type
if [[ "$SEMVER" == *"-"* ]]; then
  # Pre-release: include source if provided
  if [ -n "$GITHUB_NUGET_SOURCE" ]; then
    INSTALL_CMD="dotnet add package $PACKAGE_ID --version $SEMVER --source $GITHUB_NUGET_SOURCE"
  else
    INSTALL_CMD="dotnet add package $PACKAGE_ID --version $SEMVER"
  fi
else
  # Stable release: no source needed (assumes nuget.org)
  INSTALL_CMD="dotnet add package $PACKAGE_ID --version $SEMVER"
fi

# Check if release already exists
if gh release view "v$SEMVER" >/dev/null 2>&1; then
  echo "Release v$SEMVER already exists, updating notes..."
  RELEASE_NOTES=$(gh release view "v$SEMVER" --json body -q .body)
  if [[ ! "$RELEASE_NOTES" == *"dotnet add package"* ]]; then
    echo "$RELEASE_NOTES" > notes.md
    echo "" >> notes.md
    echo "## Installation" >> notes.md
    echo "" >> notes.md
    echo '```bash' >> notes.md
    echo "$INSTALL_CMD" >> notes.md
    echo '```' >> notes.md
    gh release edit "v$SEMVER" --notes-file notes.md
    rm notes.md
    echo "Release notes updated successfully"
  else
    echo "Installation instructions already present, skipping update"
  fi
else
  echo "Creating new release v$SEMVER..."
  PRERELEASE_FLAG=""
  if [[ "$SEMVER" == *"-"* ]]; then
    PRERELEASE_FLAG="--prerelease"
  fi
  
  # Create release with generated notes first
  gh release create "v$SEMVER" \
    --title "v$SEMVER" \
    --generate-notes \
    $PRERELEASE_FLAG
  
  # Get the generated notes and append installation instructions
  RELEASE_NOTES=$(gh release view "v$SEMVER" --json body -q .body)
  echo "$RELEASE_NOTES" > notes.md
  echo "" >> notes.md
  echo "## Installation" >> notes.md
  echo "" >> notes.md
  echo '```bash' >> notes.md
  echo "$INSTALL_CMD" >> notes.md
  echo '```' >> notes.md
  gh release edit "v$SEMVER" --notes-file notes.md
  rm notes.md
  echo "Release created successfully"
fi
