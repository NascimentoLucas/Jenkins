#!/usr/bin/env bash
set -euo pipefail

echo "[entry] Ensuring required plugins are installed/updated..."
# Install/update plugins listed in the baked plugins.txt into JENKINS_HOME.
# --latest true updates to the latest compatible versions for this Jenkins core.
if ! jenkins-plugin-cli -f /usr/share/jenkins/ref/plugins.txt --latest true; then
  echo "[entry] Plugin installation/update failed; continuing to start Jenkins." >&2
fi

echo "[entry] Starting Jenkins..."
exec /usr/local/bin/jenkins.sh

