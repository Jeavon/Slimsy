#!/bin/bash
set -e

# Merge Main Branch to Dev
# Merges the current main branch back to its corresponding dev branch
#
# Usage: ./merge-to-develop.sh <semver>
# Example: ./merge-to-develop.sh "7.0.0"

SEMVER="$1"

if [ -z "$SEMVER" ]; then
  echo "Error: Missing required argument"
  echo "Usage: $0 <semver>"
  exit 1
fi

# Extract version from main branch (e.g., main-v7 -> v7)
CURRENT_BRANCH=${GITHUB_REF#refs/heads/}
VERSION_SUFFIX=$(echo "$CURRENT_BRANCH" | sed 's/main-//')
DEV_BRANCH="dev-$VERSION_SUFFIX"

echo "Merging $CURRENT_BRANCH into $DEV_BRANCH after release $SEMVER"

# Configure git
git config user.name "github-actions[bot]"
git config user.email "github-actions[bot]@users.noreply.github.com"

# Fetch the dev branch
git fetch origin "$DEV_BRANCH"

# Checkout dev branch
git checkout "$DEV_BRANCH"

# Merge main branch into dev
git merge --no-ff "origin/$CURRENT_BRANCH" -m "chore: merge $CURRENT_BRANCH into $DEV_BRANCH after release $SEMVER"

# Push changes
git push origin "$DEV_BRANCH"

echo "Successfully merged main to dev branch"
