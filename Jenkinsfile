pipeline {
  agent any

  options {
    timestamps()
    ansiColor('xterm')
    durabilityHint('PERFORMANCE_OPTIMIZED')
    buildDiscarder(logRotator(numToKeepStr: '20'))
    timeout(time: 60, unit: 'MINUTES')
  }

  environment {
    UNITY_EXECUTABLE = "${env.UNITY_2022_3}"               // define this globally or here
    UNITY_PROJECT    = "${env.WORKSPACE}"
    UNITY_ASSET      = "${env.WORKSPACE}\\Assets"
    UNITY_LOG        = "${env.LOG_PATH}\\unity\\SampleAutoBuild.log" // define LOG_PATH globally or here
    APP_NAME         = "SampleAutoBuild"
    // Optional: DEV_TOOL must exist on the agent if you use Copy Agent Assets
    DEV_TOOL = "${env.DEV_TOOL}"
  }

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Copy Agent Assets') {
      when { expression { return env.DEV_TOOL && env.DEV_TOOL.trim() } }
      steps {
        bat """
        if not exist "%UNITY_ASSET%" mkdir "%UNITY_ASSET%"
        xcopy "%DEV_TOOL%\\*" "%UNITY_ASSET%\\" /E /I /Y
        """
      }
    }

    stage('Inject Secret Files') {
      steps {
        withCredentials([
          file(credentialsId: 'SampleAutoBuild-android-keystore-binary', variable: 'KEYSTORE_FILE_TMP'),
          file(credentialsId: 'SampleAutoBuild-android-keystore-json',   variable: 'KEYSTORE_JSON_TMP')
        ]) {
          bat """
          if not exist "%UNITY_ASSET%" mkdir "%UNITY_ASSET%"
          copy /Y "%KEYSTORE_FILE_TMP%" "%UNITY_ASSET%\\build_data_keystore.keystore"
          copy /Y "%KEYSTORE_JSON_TMP%" "%UNITY_ASSET%\\build_config.json"
          """
        }
      }
    }

    stage('Unity Build') {
      steps {
        bat """
        "%UNITY_EXECUTABLE%" ^
          -batchmode -nographics -quit ^
          -projectPath "%UNITY_PROJECT%" ^
          -buildTarget Android ^
          -executeMethod Nascimento.Dev.Build.BuildScript.BuildMethod ^
          -logFile "%UNITY_LOG%"
        """
      }
    }

    stage('Collect Logs / Artifacts') {
      steps {
        // Copy log back into the workspace so 'archiveArtifacts' can find it
        bat """
        if exist "%UNITY_LOG%" copy /Y "%UNITY_LOG%" "%WORKSPACE%\\unity.log"
        """
        archiveArtifacts artifacts: 'unity.log', fingerprint: true, allowEmptyArchive: true
      }
    }
  }

  post {
    always {
      // Show last lines of the log in the build console (if present)
      script {
        if (fileExists('unity.log')) {
          powershell 'Get-Content unity.log -Tail 200 | Out-String | Write-Host'
        }
      }
      // Securely remove injected secrets from the workspace
      bat """
      del /F /Q "%UNITY_ASSET%\\build_data_keystore.keystore" 2>nul
      del /F /Q "%UNITY_ASSET%\\build_config.json" 2>nul
      """
    }
  }
}
