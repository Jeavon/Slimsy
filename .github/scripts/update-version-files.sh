#!/bin/bash
set -e

# Update Version Files Script
# Updates version numbers in umbraco-package.json and .csproj files
#
# Usage: ./update-version-files.sh <semver> <path_to_extension>
# Example: ./update-version-files.sh "7.0.0-alpha.1" "src/Slimsy"

SEMVER="$1"
PATH_TO_EXTENSION="$2"

if [ -z "$SEMVER" ] || [ -z "$PATH_TO_EXTENSION" ]; then
  echo "Error: Missing required arguments"
  echo "Usage: $0 <semver> <path_to_extension>"
  exit 1
fi

echo "Updating version to $SEMVER in $PATH_TO_EXTENSION"

# Update umbraco-package.json
jq '.version = "'"$SEMVER"'"' "$PATH_TO_EXTENSION/wwwroot/umbraco-package.json" > temp.json && mv temp.json "$PATH_TO_EXTENSION/wwwroot/umbraco-package.json"
echo "Updated version in $PATH_TO_EXTENSION/wwwroot/umbraco-package.json to $SEMVER"

# Convert semantic version to numeric file version (e.g., 7.0.0-alpha.1 -> 7.0.0.1)
if [[ $SEMVER == *"-"* ]]; then
  BASE_VERSION=$(echo "$SEMVER" | cut -d'-' -f1)
  PRERELEASE=$(echo "$SEMVER" | cut -d'-' -f2)
  # Extract number from prerelease (e.g., alpha.1 -> 1, beta.2 -> 2)
  PRERELEASE_NUM=$(echo "$PRERELEASE" | grep -o '[0-9]\+$' || echo "0")
  FILE_VERSION="${BASE_VERSION}.${PRERELEASE_NUM}"
else
  FILE_VERSION="${SEMVER}.0"
fi

# Function to update file version in .csproj files
update_file_version() {
  local CSPROJ_PATH=$1
  local FILE_VERSION=$2
  local INFORMATIONAL_VERSION=$3
  
  if [ ! -f "$CSPROJ_PATH" ]; then
    echo "Warning: $CSPROJ_PATH not found"
    return
  fi
  
  # Check if FileVersion element exists
  if grep -q "<FileVersion>" "$CSPROJ_PATH"; then
    # Update existing FileVersion
    sed -i "s|<FileVersion>.*</FileVersion>|<FileVersion>$FILE_VERSION</FileVersion>|" "$CSPROJ_PATH"
    echo "Updated FileVersion in $CSPROJ_PATH to $FILE_VERSION"
  else
    # Add FileVersion to the first PropertyGroup
    sed -i "0,/<PropertyGroup>/s|<PropertyGroup>|<PropertyGroup>\n\t\t<FileVersion>$FILE_VERSION</FileVersion>|" "$CSPROJ_PATH"
    echo "Added FileVersion to $CSPROJ_PATH with value $FILE_VERSION"
  fi
  
  # Check if InformationalVersion element exists
  if grep -q "<InformationalVersion>" "$CSPROJ_PATH"; then
    # Update existing InformationalVersion
    sed -i "s|<InformationalVersion>.*</InformationalVersion>|<InformationalVersion>$INFORMATIONAL_VERSION</InformationalVersion>|" "$CSPROJ_PATH"
    echo "Updated InformationalVersion in $CSPROJ_PATH to $INFORMATIONAL_VERSION"
  else
    # Add InformationalVersion to the first PropertyGroup
    sed -i "0,/<PropertyGroup>/s|<PropertyGroup>|<PropertyGroup>\n\t\t<InformationalVersion>$INFORMATIONAL_VERSION</InformationalVersion>|" "$CSPROJ_PATH"
    echo "Added InformationalVersion to $CSPROJ_PATH with value $INFORMATIONAL_VERSION"
  fi
}

# Update Slimsy project file
update_file_version "$PATH_TO_EXTENSION/Slimsy.csproj" "$FILE_VERSION" "$SEMVER"

echo "Version update completed successfully"
