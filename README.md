Jenkins (Docker + JCasC)

A minimal, reproducible Jenkins setup using Docker, Jenkins Configuration as Code (JCasC), and a small set of core plugins.

**Structure**
- `docker-compose.yaml`: Docker Compose service for Jenkins
- `.env.example`: Example ports and admin credentials (copy to `.env`)
- `.gitignore`: Ignore OS/IDE files and persistent data
- `docker/jenkins/`: Image build context
  - `Dockerfile`: Based on `jenkins/jenkins:lts-jdk17`, disables setup wizard, installs plugins, configures JCasC
  - `plugins.txt`: Preinstalled plugins
  - `casc/jenkins.yaml`: JCasC config (admin, auth, basic settings)
- Docker named volume `jenkins_home`: Persistent Jenkins data (not committed)

**Quick Start**
- Prerequisites: Docker Desktop (or Docker Engine) and Docker Compose v2.
- Copy env template: `cp .env.example .env` (then edit values)
- Build and start Jenkins: `docker compose up -d --build`
- Open Jenkins at `http://localhost:8080` (or your mapped port).

Jenkins is preconfigured via JCasC, so the setup wizard is skipped. The admin user/password is read from `.env` or your shell environment.

**Common Tasks**
- Change admin credentials:
  - Edit `.env` values or export env vars before `docker compose up`.
  - For existing data, update credentials in `docker/jenkins/casc/jenkins.yaml`, then rebuild and recreate the container.
- Update plugins:
  - Edit `docker/jenkins/plugins.txt`
  - Restart Jenkins: `docker compose up -d` (the container installs/updates plugins on start)
- Reset instance:
  - Stop: `docker compose down`
  - Remove data: `docker volume rm jenkins_home` (wipes all jobs/config)
  - Start fresh: `docker compose up -d --build`

**Notes**
- This is a minimal dev setup. For production, secure credentials (no plain-text), consider HTTPS, backups, and controlled plugin updates.
- If you need Docker inside Jenkins builds, add Docker-in-Docker or mount the host Docker socket per your environment.

**Configuration**
- `JENKINS_HTTP_PORT` (default `8080` if not set): host port mapped to Jenkins UI.
- `JENKINS_AGENT_PORT` (default `50000`): JNLP agent port exposure.
- `JENKINS_ADMIN_ID` / `JENKINS_ADMIN_PASSWORD`: admin bootstrap credentials read by JCasC.
- `JENKINS_URL`: external URL used by Jenkins (defaults to `http://localhost:${JENKINS_HTTP_PORT}/`).
- Set these in `.env` or export as environment variables before `docker compose up`.

**What This Showcases**
- Dockerized Jenkins LTS with reproducible provisioning via JCasC.
- Minimal, curated plugin set managed by `jenkins-plugin-cli`.
- Declarative `Jenkinsfile` example that builds a Unity Android project.
- Secure secret handling using Jenkins Credentials + `withCredentials` (no secrets in VCS).
- Clean developer UX: single `docker compose up -d --build` to start.

**Example Files**
- `BuildScript.cs`: Example Unity build script I use in my projects, it is copied to unity project before open it. Adapt paths, targets, and signing to your needs, or remove if not using Unity.
- `Jenkinsfile`: Example declarative pipeline I use to build projects (Unity Android showcase). Replace with your own pipeline if your stack differs.

**Architecture**
```
Developer (local)
   |
   |  docker compose up -d --build
   v
+---------------------------+
| Docker Compose            |
|  - service: jenkins       |
|  - image: jenkins:lts     |
|  - ports: 8080, 50000     |
|  - volume: jenkins_home   |
+------------+--------------+
             |
             v
        +----+-----------------------------+
        | Jenkins container                |
        |  - JCasC: casc/jenkins.yaml      |
        |  - Plugins: docker/jenkins/plugins.txt
        |  - Entrypoint: custom-entry.sh   |
        |  - Admin from env (.env/.ENV VAR)|
        +----------------+------------------+
                         |
                         | Pipeline runs (Jenkinsfile)
                         v
        +----------------+------------------+
        | Build Stage (Unity example)       |
        |  - withCredentials injects files  |
        |  - Unity CLI builds Android APK   |
        |  - Logs/artifacts archived        |
        +-----------------------------------+
```

**Troubleshooting**
- Port in use: Change `JENKINS_HTTP_PORT` or stop the conflicting service, then `docker compose up -d`.
- Reset everything: `docker compose down && docker volume rm jenkins_home && docker compose up -d --build`.
- Plugins didn’t update: Restart the container; the entry script re-applies `plugins.txt` with `--latest`.
- Apply JCasC changes: Edit `docker/jenkins/casc/jenkins.yaml` and recreate the container. You can also use Manage Jenkins → Configuration as Code → Reload.

**License**
- MIT. See `LICENSE`.
