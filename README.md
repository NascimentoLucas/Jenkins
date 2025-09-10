Jenkins (Docker + JCasC)

A minimal, reproducible Jenkins setup using Docker, Jenkins Configuration as Code (JCasC), and a small set of core plugins.

**Structure**
- `compose.yaml` — Docker Compose service for Jenkins
- `.env` — Default ports and admin credentials (dev only)
- `.gitignore` — Ignore OS/IDE files and persistent data
- `docker/jenkins/` — Image build context
  - `Dockerfile` — Based on `jenkins/jenkins:lts-jdk17`, disables setup wizard, installs plugins, configures JCasC
  - `plugins.txt` — Preinstalled plugins
  - `casc/jenkins.yaml` — JCasC config (admin, auth, basic settings)
- `data/jenkins_home/` — Persistent Jenkins data (gitignored)

**Quick Start**
- Prerequisites: Docker Desktop (or Docker Engine) and Docker Compose v2.
- Update `.env` with desired ports and temporary admin credentials.
- Build and start Jenkins: `docker compose up -d --build`
- Open Jenkins at `http://localhost:8080`.

Jenkins is preconfigured via JCasC, so the setup wizard is skipped. The admin user/password comes from `.env` or your shell environment.

**Common Tasks**
- Change admin credentials:
  - Edit `.env` values or export env vars before `docker compose up`.
  - For existing data, update credentials in `docker/jenkins/casc/jenkins.yaml`, then rebuild and recreate the container.
- Update plugins:
  - Edit `docker/jenkins/plugins.txt`
  - Restart Jenkins: `docker compose up -d` (the container installs/updates plugins on start)
- Reset instance:
  - Stop: `docker compose down`
  - Remove data: delete `data/jenkins_home/` (this wipes all jobs/config)
  - Start fresh: `docker compose up -d --build`

**Notes**
- This is a minimal dev setup. For production, secure credentials (no plain-text), consider HTTPS, backups, and controlled plugin updates.
- If you need Docker inside Jenkins builds, add Docker-in-Docker or mount the host Docker socket per your environment.
