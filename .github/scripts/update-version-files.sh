#!/bin/bash
set -e

# Update Version Files Script
# Updates version numbers in umbraco-package.json, package.manifest, and related files
#
# Usage: ./update-version-files.sh <semver> <path_to_extension>
# Example: ./update-version-files.sh "1.0.0-alpha.1" "src/Extension"

SEMVER="$1"
PATH_TO_EXTENSION="$2"

if [ -z "$SEMVER" ] || [ -z "$PATH_TO_EXTENSION" ]; then
  echo "Error: Missing required arguments"
  echo "Usage: $0 <semver> <path_to_extension>"
  exit 1
fi

echo "Updating version to $SEMVER in $PATH_TO_EXTENSION"

# Update umbraco-package.json if it exists
if [ -f "$PATH_TO_EXTENSION/wwwroot/umbraco-package.json" ]; then
  jq '.version = "'"$SEMVER"'"' "$PATH_TO_EXTENSION/wwwroot/umbraco-package.json" > temp.json && mv temp.json "$PATH_TO_EXTENSION/wwwroot/umbraco-package.json"
  echo "Updated version in $PATH_TO_EXTENSION/wwwroot/umbraco-package.json to $SEMVER"
fi

# Update package.manifest if it exists
if [ -f "$PATH_TO_EXTENSION/wwwroot/package.manifest" ]; then
  jq '.version = "'"$SEMVER"'"' "$PATH_TO_EXTENSION/wwwroot/package.manifest" > temp.json && mv temp.json "$PATH_TO_EXTENSION/wwwroot/package.manifest"
  echo "Updated version in $PATH_TO_EXTENSION/wwwroot/package.manifest to $SEMVER"
fi

# Convert semantic version to numeric file version (e.g., 1.0.0-alpha.1 -> 1.0.0.1)
if [[ $SEMVER == *"-"* ]]; then
  BASE_VERSION=$(echo "$SEMVER" | cut -d'-' -f1)
  PRERELEASE=$(echo "$SEMVER" | cut -d'-' -f2)
  # Extract number from prerelease (e.g., alpha.1 -> 1, beta.2 -> 2)
  PRERELEASE_NUM=$(echo "$PRERELEASE" | grep -o '[0-9]\+$' || echo "0")
  FILE_VERSION="${BASE_VERSION}.${PRERELEASE_NUM}"
else
  # Stable release: Find highest pre-release number for this version and add 1
  BASE_VERSION="$SEMVER"
  # Get all tags matching this version with pre-release suffix
  MAX_PRERELEASE=$(git tag -l "v${BASE_VERSION}-*" | grep -o '[0-9]\+$' | sort -n | tail -1)
  if [ -z "$MAX_PRERELEASE" ]; then
    # No pre-releases found, use 1
    FILE_VERSION="${BASE_VERSION}.1"
  else
    # Use max pre-release number + 1
    NEXT_NUM=$((MAX_PRERELEASE + 1))
    FILE_VERSION="${BASE_VERSION}.${NEXT_NUM}"
  fi
fi

echo "Version update completed successfully"
echo "SEMVER=$SEMVER"
echo "FILE_VERSION=$FILE_VERSION"

# Output for GitHub Actions
if [ -n "$GITHUB_OUTPUT" ]; then
  echo "file_version=$FILE_VERSION" >> "$GITHUB_OUTPUT"
fi
