import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.exec
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.nant
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.script
import jetbrains.buildServer.configs.kotlin.v2019_2.failureConditions.BuildFailureOnMetric
import jetbrains.buildServer.configs.kotlin.v2019_2.failureConditions.failOnMetricChange
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.finishBuildTrigger
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.vcs

/*
The settings script is an entry point for defining a TeamCity
project hierarchy. The script should contain a single call to the
project() function with a Project instance or an init function as
an argument.

VcsRoots, BuildTypes, Templates, and subprojects can be
registered inside the project using the vcsRoot(), buildType(),
template(), and subProject() methods respectively.

To debug settings scripts in command-line, run the

    mvnDebug org.jetbrains.teamcity:teamcity-configs-maven-plugin:generate

command and attach your debugger to the port 8000.

To debug in IntelliJ Idea, open the 'Maven Projects' tool window (View
-> Tool Windows -> Maven Projects), find the generate task node
(Plugins -> teamcity-configs -> teamcity-configs:generate), the
'Debug' option is available in the context menu for the task.
*/

version = "2019.2"

project {

    buildType(Version)
    buildType(DevelopMac)
    buildType(PackagingWindows)
    buildType(DevelopWin)
    buildType(PackageMacAndLinux)
    buildType(TestWindows)
    buildType(TestMac)
    buildType(GenerateDocumentation)

    features {
        feature {
            id = "PROJECT_EXT_1"
            type = "IssueTracker"
            param("secure:password", "")
            param("name", "GitHub.com")
            param("pattern", """#(\d+)""")
            param("authType", "anonymous")
            param("repository", "https://github.com/MonoGame/MonoGame")
            param("type", "GithubIssues")
            param("secure:accessToken", "")
            param("username", "")
        }
    }
    buildTypesOrder = arrayListOf(Version, DevelopWin, DevelopMac, TestWindows, TestMac, GenerateDocumentation, PackagingWindows, PackageMacAndLinux)
}

object DevelopMac : BuildType({
    name = "Build Mac"
    description = "Build code for Mac, iOS, and Linux."

    allowExternalStatus = true
    artifactRules = """
        Artifacts/**/iOS/**/*.nupkg
        Artifacts/**/Android/**/*.nupkg
        Artifacts/**/*.mpack
    """.trimIndent()
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    params {
        param("env.GIT_BRANCH", "%teamcity.build.branch%")
        param("env.BUILD_NUMBER", "%build.number%")
    }

    vcs {
        root(RelativeId("Develop"))

        checkoutMode = CheckoutMode.ON_SERVER
        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        exec {
            name = "dotnet tool restore"
            path = "dotnet"
            arguments = "tool restore"
            formatStderrAsError = true
        }
        exec {
            name = "Running Cake Script"
            path = "dotnet"
            arguments = "cake build.cake"
            formatStderrAsError = true
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${Version.id}"
            successfulOnly = true
            branchFilter = "+:*"
        }
    }

    features {
        feature {
            type = "teamcity.github.status"
            param("guthub_context", "Build Mac, iOS, and Linux")
            param("guthub_owner", "MonoGame")
            param("guthub_authentication_type", "token")
            param("guthub_guest", "true")
            param("guthub_repo", "MonoGame")
            param("github_report_on", "on start and finish")
            param("secure:github_access_token", "credentialsJSON:6be4e606-4738-4fe6-b476-503782a0a65f")
        }
    }

    dependencies {
        snapshot(Version) {
            onDependencyFailure = FailureAction.CANCEL
            onDependencyCancel = FailureAction.CANCEL
        }
    }

    requirements {
        equals("teamcity.agent.jvm.os.name", "Mac OS X")
        exists("DotNetCLI")
    }
})

object DevelopWin : BuildType({
    name = "Build Windows"
    description = "Build code for all Windows platforms, Web and Android."

    allowExternalStatus = true
    artifactRules = """
        Artifacts/**/*.nupkg
        Artifacts/**/*.vsix
        Artifacts/**/*.mpack
    """.trimIndent()
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    params {
        param("env.GIT_BRANCH", "%teamcity.build.branch%")
        param("env.BUILD_NUMBER", "%build.number%")
    }

    vcs {
        root(RelativeId("Develop"))

        checkoutMode = CheckoutMode.ON_SERVER
        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        exec {
            name = "dotnet tool restore"
            path = "dotnet"
            arguments = "tool restore"
            formatStderrAsError = true
        }
        exec {
            name = "Running Cake Script"
            path = "dotnet-cake"
            arguments = "build.cake"
            formatStderrAsError = true
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${Version.id}"
            successfulOnly = true
            branchFilter = "+:*"
        }
    }

    features {
        feature {
            type = "teamcity.github.status"
            param("guthub_context", "Build Windows, Web, and Android")
            param("guthub_owner", "MonoGame")
            param("guthub_authentication_type", "token")
            param("guthub_guest", "true")
            param("guthub_username", "mgbot")
            param("guthub_repo", "MonoGame")
            param("github_report_on", "on start and finish")
            param("secure:github_access_token", "credentialsJSON:6be4e606-4738-4fe6-b476-503782a0a65f")
            param("secure:guthub_username", "credentialsJSON:638301b3-2489-409b-8e34-566fa418c654")
        }
    }

    dependencies {
        snapshot(Version) {
            onDependencyFailure = FailureAction.CANCEL
            onDependencyCancel = FailureAction.CANCEL
        }
    }

    requirements {
        startsWith("teamcity.agent.name", "MonoGameWin")
        exists("DotNetCLI")
    }
})

object GenerateDocumentation : BuildType({
    name = "Generate Documentation"
    description = "Generate the SDK documentation."

    allowExternalStatus = true
    artifactRules = """Documentation\_site=>Documentation.zip"""
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    vcs {
        root(RelativeId("Develop"))

        checkoutMode = CheckoutMode.ON_SERVER
        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        exec {
            name = "Running docfx metadata"
            path = "docfx"
            arguments = "metadata"
            formatStderrAsError = true
            workingDir = "Documentation"
        }
        exec {
            name = "Running docfx build"
            path = "docfx"
            arguments = "build"
            formatStderrAsError = true
            workingDir = "Documentation"
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${Version.id}"
            successfulOnly = true
            branchFilter = "+:*"
        }
    }

    features {
        feature {
            type = "teamcity.github.status"
            param("guthub_context", "Generate Documentation")
            param("guthub_owner", "MonoGame")
            param("guthub_authentication_type", "token")
            param("guthub_guest", "true")
            param("guthub_username", "mgbot")
            param("guthub_repo", "MonoGame")
            param("github_report_on", "on start and finish")
            param("secure:github_access_token", "credentialsJSON:6be4e606-4738-4fe6-b476-503782a0a65f")
            param("secure:guthub_username", "credentialsJSON:638301b3-2489-409b-8e34-566fa418c654")
        }
    }

    dependencies {
        dependency(DevelopMac) {
            snapshot {
                onDependencyFailure = FailureAction.CANCEL
                onDependencyCancel = FailureAction.CANCEL
            }
        }
        dependency(DevelopWin) {
            snapshot {
                onDependencyFailure = FailureAction.CANCEL
                onDependencyCancel = FailureAction.CANCEL
            }
        }
    }

    requirements {
        startsWith("teamcity.agent.name", "MonoGameWin")
    }
})

object PackageMacAndLinux : BuildType({
    name = "Package Mac and Linux"
    description = "Create the Mac and Linux SDK packages."
    paused = true

    allowExternalStatus = true
    artifactRules = """
        Installers/Pipeline.MacOS.pkg
        Installers/MonoGame.pkg
        Installers/Linux/monogame-sdk.deb=>Linux
        Installers/Linux/monogame-sdk.run=>Linux
    """.trimIndent()
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    vcs {
        root(RelativeId("Develop"))

        checkoutMode = CheckoutMode.ON_SERVER
        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        nant {
            name = "Running NAnt Script"
            mode = nantFile {
                path = "default.build"
            }
            targets = "build_installer"
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${Version.id}"
            successfulOnly = true
            branchFilter = "+:*"
        }
    }

    features {
        feature {
            type = "teamcity.github.status"
            param("guthub_context", "Package Mac and Linux")
            param("guthub_owner", "MonoGame")
            param("guthub_authentication_type", "token")
            param("guthub_guest", "true")
            param("guthub_repo", "MonoGame")
            param("github_report_on", "on start and finish")
            param("secure:github_access_token", "credentialsJSON:6be4e606-4738-4fe6-b476-503782a0a65f")
        }
    }

    dependencies {
        snapshot(GenerateDocumentation) {
            onDependencyFailure = FailureAction.CANCEL
            onDependencyCancel = FailureAction.CANCEL
        }
        snapshot(TestWindows) {
            onDependencyFailure = FailureAction.CANCEL
            onDependencyCancel = FailureAction.CANCEL
        }
        artifacts(DevelopMac) {
            artifactRules = """
                MonoGame.Framework.zip!**
                MonoGame.Framework.Content.Pipeline.zip!**
                Tools.zip!**
            """.trimIndent()
        }
        artifacts(DevelopWin) {
            artifactRules = """
                MonoGame.Framework.zip!**
                MonoGame.Framework.Content.Pipeline.zip!**
                Tools.zip!**
            """.trimIndent()
        }
    }

    requirements {
        equals("teamcity.agent.jvm.os.name", "Mac OS X")
    }
})

object PackagingWindows : BuildType({
    name = "Package Windows"
    description = "Create the Windows SDK packaging."
    paused = true

    allowExternalStatus = true
    artifactRules = """Installers\Windows\MonoGameSetup.exe"""
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    vcs {
        root(RelativeId("Develop"))

        checkoutMode = CheckoutMode.ON_SERVER
        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        nant {
            name = "Running NAnt Script"
            mode = nantFile {
                path = "default.build"
            }
            targets = "build_installer"
            param("dotNetCoverage.dotCover.filters", """
                +:MonoGame.Framework
                +:MonoGame.Framework.Content.Pipeline
            """.trimIndent())
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${Version.id}"
            successfulOnly = true
            branchFilter = "+:*"
        }
    }

    features {
        feature {
            type = "teamcity.github.status"
            param("guthub_context", "Package Windows SDK")
            param("guthub_owner", "MonoGame")
            param("guthub_authentication_type", "token")
            param("guthub_guest", "true")
            param("guthub_username", "mgbot")
            param("guthub_repo", "MonoGame")
            param("github_report_on", "on start and finish")
            param("secure:github_access_token", "credentialsJSON:6be4e606-4738-4fe6-b476-503782a0a65f")
            param("secure:guthub_username", "credentialsJSON:638301b3-2489-409b-8e34-566fa418c654")
        }
    }

    dependencies {
        snapshot(GenerateDocumentation) {
            onDependencyFailure = FailureAction.CANCEL
            onDependencyCancel = FailureAction.CANCEL
        }
        snapshot(TestWindows) {
            onDependencyFailure = FailureAction.CANCEL
            onDependencyCancel = FailureAction.CANCEL
        }
        artifacts(DevelopMac) {
            artifactRules = "MonoGame.Framework.zip!**"
        }
        artifacts(DevelopWin) {
            artifactRules = """
                MonoGame.Framework.zip!**
                MonoGame.Framework.Content.Pipeline.zip!**
                Tools.zip!**
            """.trimIndent()
        }
    }

    requirements {
        startsWith("teamcity.agent.name", "MonoGameWin")
    }
})

object TestMac : BuildType({
    name = "Test Mac"
    description = "Run the Mac unit tests."

    allowExternalStatus = true
    artifactRules = "Test/bin/Linux/AnyCPU/Debug/TestResult.xml"
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    params {
        param("env.GIT_BRANCH", "%teamcity.build.branch%")
        param("env.BUILD_NUMBER", "%build.number%")
    }

    vcs {
        root(RelativeId("Develop"))

        checkoutMode = CheckoutMode.ON_SERVER
        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        exec {
            name = "dotnet tool restore"
            path = "dotnet"
            arguments = "tool restore"
            formatStderrAsError = true
        }
        exec {
            name = "Running Cake Script"
            path = "dotnet"
            arguments = "cake build.cake --build-target=Test"
            formatStderrAsError = false
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${Version.id}"
            successfulOnly = true
            branchFilter = "+:*"
        }
    }

    failureConditions {
        failOnMetricChange {
            enabled = false
            metric = BuildFailureOnMetric.MetricType.TEST_COUNT
            threshold = 1100
            units = BuildFailureOnMetric.MetricUnit.DEFAULT_UNIT
            comparison = BuildFailureOnMetric.MetricComparison.LESS
            compareTo = value()
            param("anchorBuild", "lastSuccessful")
        }
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.BUILD_DURATION
            threshold = 300
            units = BuildFailureOnMetric.MetricUnit.DEFAULT_UNIT
            comparison = BuildFailureOnMetric.MetricComparison.MORE
            compareTo = value()
            param("anchorBuild", "lastSuccessful")
        }
    }

    features {
        feature {
            type = "teamcity.github.status"
            param("guthub_context", "Test Mac")
            param("guthub_owner", "MonoGame")
            param("guthub_authentication_type", "token")
            param("guthub_guest", "true")
            param("guthub_repo", "MonoGame")
            param("github_report_on", "on start and finish")
            param("secure:github_access_token", "credentialsJSON:6be4e606-4738-4fe6-b476-503782a0a65f")
        }
    }

    dependencies {
        dependency(DevelopMac) {
            snapshot {
                onDependencyFailure = FailureAction.CANCEL
                onDependencyCancel = FailureAction.CANCEL
            }

            artifacts {
                artifactRules = "Test.zip!**"
                enabled = false
            }
        }
    }

    requirements {
        equals("teamcity.agent.jvm.os.name", "Mac OS X")
    }
})

object TestWindows : BuildType({
    name = "Test Windows"
    description = "Run the Windows unit tests."

    allowExternalStatus = true
    artifactRules = """
        Test\bin\Windows\AnyCPU\Debug\CapturedFrames=>TestResults.Windows.zip
        Test\bin\Windows\AnyCPU\Debug\Diffs=>TestResults.Windows.zip
        Test\bin\Windows\AnyCPU\Debug\MonoGameTests.xml=>TestResults.Windows.zip
        Test\bin\Windows\AnyCPU\Debug\CapturedFrames => Test\bin\Windows\AnyCPU\Debug\CapturedFrames
        Test\bin\Windows\AnyCPU\Debug\Diffs => Test\bin\Windows\AnyCPU\Debug\Diffs
    """.trimIndent()
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    params {
        param("env.GIT_BRANCH", "%teamcity.build.branch%")
        param("env.BUILD_NUMBER", "%build.number%")
    }

    vcs {
        root(RelativeId("Develop"))

        checkoutMode = CheckoutMode.ON_SERVER
        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        exec {
            name = "dotnet tool restore"
            path = "dotnet"
            arguments = "tool restore"
            formatStderrAsError = true
        }
        exec {
            name = "Running Cake Script"
            path = "dotnet-cake"
            arguments = """build.cake -build-target="Test""""
            formatStderrAsError = true
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${Version.id}"
            successfulOnly = true
            branchFilter = "+:*"
        }
    }

    failureConditions {
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.TEST_COUNT
            threshold = 1200
            units = BuildFailureOnMetric.MetricUnit.DEFAULT_UNIT
            comparison = BuildFailureOnMetric.MetricComparison.LESS
            compareTo = value()
            param("anchorBuild", "lastSuccessful")
        }
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.BUILD_DURATION
            threshold = 300
            units = BuildFailureOnMetric.MetricUnit.DEFAULT_UNIT
            comparison = BuildFailureOnMetric.MetricComparison.MORE
            compareTo = value()
            stopBuildOnFailure = true
            param("anchorBuild", "lastSuccessful")
        }
    }

    features {
        feature {
            type = "teamcity.github.status"
            param("guthub_context", "Test Windows")
            param("guthub_owner", "MonoGame")
            param("guthub_authentication_type", "token")
            param("guthub_guest", "true")
            param("guthub_username", "mgbot")
            param("guthub_repo", "MonoGame")
            param("github_report_on", "on start and finish")
            param("secure:github_access_token", "credentialsJSON:6be4e606-4738-4fe6-b476-503782a0a65f")
            param("secure:guthub_username", "credentialsJSON:638301b3-2489-409b-8e34-566fa418c654")
        }
    }

    dependencies {
        dependency(DevelopWin) {
            snapshot {
                onDependencyFailure = FailureAction.CANCEL
                onDependencyCancel = FailureAction.CANCEL
            }

            artifacts {
                artifactRules = "Test.zip!**"
                enabled = false
            }
        }
    }

    requirements {
        startsWith("teamcity.agent.name", "MonoGameWin")
    }
})

object Version : BuildType({
    name = "Kickoff Build"
    description = "Trick to generate a single build version for entire build chain."

    buildNumberPattern = "%VersionMajor%.%VersionMinor%.%VersionPatch%.%VersionBuildCounter%"

    params {
        text("VersionMajor", "3", label = "Major Version Number", description = "The major version number.",
              regex = """^\d+${'$'}""")
        text("VersionMinor", "8", label = "Minor Version Number", description = "The minor version number.",
              regex = """^\d+${'$'}""")
        text("VersionPatch", "1", label = "Patch Version Number", description = "The patch version number or zero.",
              regex = """^\d+${'$'}""")
        text("VersionBuildCounter", "%build.counter%", label = "Version Build Counter", description = "This should be a unique number which increments with each build.", allowEmpty = false)
    }

    vcs {
        root(RelativeId("Develop"))

        checkoutMode = CheckoutMode.MANUAL
        cleanCheckout = true
        showDependenciesChanges = true
    }

    triggers {
        vcs {
            branchFilter = """
                +:*
                -:refs/heads/master
            """.trimIndent()
        }
    }

    requirements {
        startsWith("system.agent.name", "MonoGameWin")
    }
})
