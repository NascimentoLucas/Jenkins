FROM jenkins/jenkins:lts-jdk21

# Disable setup wizard; we use JCasC
ENV JAVA_OPTS -Djenkins.install.runSetupWizard=false

USER jenkins

# Pre-install plugins
COPY plugins.txt /usr/share/jenkins/ref/plugins.txt
RUN jenkins-plugin-cli -f /usr/share/jenkins/ref/plugins.txt

# Configuration as Code
COPY casc/ /usr/share/jenkins/ref/casc/
ENV CASC_JENKINS_CONFIG=/usr/share/jenkins/ref/casc/jenkins.yaml

# On every container start, ensure plugins from plugins.txt are installed/updated
USER root
COPY custom-entry.sh /usr/local/bin/custom-entry.sh
RUN sed -i 's/\r$//' /usr/local/bin/custom-entry.sh \
    && chmod +x /usr/local/bin/custom-entry.sh
USER jenkins

# Use wrapper that updates plugins, then starts Jenkins
CMD ["/usr/local/bin/custom-entry.sh"]
