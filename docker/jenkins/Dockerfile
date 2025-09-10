FROM jenkins/jenkins:lts-jdk17

# Disable setup wizard; we use JCasC
ENV JAVA_OPTS -Djenkins.install.runSetupWizard=false

USER jenkins

# Pre-install plugins
COPY plugins.txt /usr/share/jenkins/ref/plugins.txt
RUN jenkins-plugin-cli -f /usr/share/jenkins/ref/plugins.txt

# Configuration as Code
COPY casc/ /usr/share/jenkins/ref/casc/
ENV CASC_JENKINS_CONFIG=/usr/share/jenkins/ref/casc/jenkins.yaml

