import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.nant
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.nuGetPack
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
    buildType(NuGetDevelop)
    buildType(PackageMacAndLinux)
    buildType(TestWindows)
    buildType(TestMac)
    buildType(GenerateDocumentation)
    buildType(PackageNuGet)

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
    buildTypesOrder = arrayListOf(Version, DevelopWin, DevelopMac, TestWindows, TestMac, GenerateDocumentation, PackagingWindows, PackageMacAndLinux, PackageNuGet, NuGetDevelop)
}

object DevelopMac : BuildType({
    name = "Build Mac"
    description = "Build code for Mac, iOS, and Linux."

    allowExternalStatus = true
    artifactRules = """
        MonoGame.Framework/bin => MonoGame.Framework.zip!/MonoGame.Framework/bin
        MonoGame.Framework.Content.Pipeline/bin => MonoGame.Framework.Content.Pipeline.zip!/MonoGame.Framework.Content.Pipeline/bin
        Tools/MGCB/bin => Tools.zip!/Tools/MGCB/bin
        Tools/Pipeline/bin => Tools.zip!/Tools/Pipeline/bin
        Test/bin => Test.zip!/Test/bin
    """.trimIndent()
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    params {
        param("system.nuget3path", "%teamcity.tool.NuGet.CommandLine.DEFAULT%/tools")
    }

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
            targets = "build_linux build_mac build_ios"
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
            type = "JetBrains.AssemblyInfo"
        }
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
    }
})

object DevelopWin : BuildType({
    name = "Build Windows"
    description = "Build code for all Windows platforms, Web and Android."

    allowExternalStatus = true
    artifactRules = """
        MonoGame.Framework/bin => MonoGame.Framework.zip!/MonoGame.Framework/bin
        MonoGame.Framework.Content.Pipeline/bin => MonoGame.Framework.Content.Pipeline.zip!/MonoGame.Framework.Content.Pipeline/bin
        Tools/2MGFX/bin => Tools.zip!/Tools/2MGFX/bin
        Tools/MGCB/bin => Tools.zip!/Tools/MGCB/bin
        Tools/Pipeline/bin => Tools.zip!/Tools/Pipeline/bin
        Test/bin => Test.zip!/Test/bin
    """.trimIndent()
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    params {
        param("system.nuget3path", "%teamcity.tool.NuGet.CommandLine.DEFAULT%/tools")
    }

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
            targets = "build_windows build_web build_android build_windows8 build_windowsphone81 build_windows10"
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
            param("guthub_context", "Build Windows, Web, Android, and OUYA")
            param("guthub_owner", "MonoGame")
            param("guthub_authentication_type", "token")
            param("guthub_guest", "true")
            param("guthub_username", "mgbot")
            param("guthub_repo", "MonoGame")
            param("github_report_on", "on start and finish")
            param("secure:github_access_token", "credentialsJSON:6be4e606-4738-4fe6-b476-503782a0a65f")
            param("secure:guthub_username", "credentialsJSON:638301b3-2489-409b-8e34-566fa418c654")
        }
        feature {
            type = "JetBrains.AssemblyInfo"
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
    }
})

object GenerateDocumentation : BuildType({
    name = "Generate Documentation"
    description = "Generate the SDK documentation."

    allowExternalStatus = true
    artifactRules = """Documentation\Output=>Documentation.zip"""
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    vcs {
        root(RelativeId("Develop"), "-:.", "+:Documentation", "+:CHANGELOG.md", "+:default.build", """+:ThirdParty\Dependencies\SharpDoc""")

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
            targets = "build_docs"
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

            artifacts {
                artifactRules = """
                    MonoGame.Framework.zip!**
                    MonoGame.Framework.Content.Pipeline.zip!**
                """.trimIndent()
            }
        }
        dependency(DevelopWin) {
            snapshot {
                onDependencyFailure = FailureAction.CANCEL
                onDependencyCancel = FailureAction.CANCEL
            }

            artifacts {
                artifactRules = """
                    MonoGame.Framework.zip!**
                    MonoGame.Framework.Content.Pipeline.zip!**
                """.trimIndent()
            }
        }
    }

    requirements {
        startsWith("teamcity.agent.name", "MonoGameWin")
    }
})

object NuGetDevelop : BuildType({
    name = "NuGet Publish Develop"
    description = "Create and publish the develop NuGet packages."

    allowExternalStatus = true
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    vcs {
        root(RelativeId("Develop"), "-:.", "+:NuGetPackages")

        checkoutMode = CheckoutMode.ON_SERVER
        cleanCheckout = true
    }

    steps {
        nuGetPack {
            toolPath = "%teamcity.tool.NuGet.CommandLine.DEFAULT%"
            paths = """NuGetPackages\*.nuspec"""
            version = "%build.number%-develop"
            baseDir = customPath {
                path = """MonoGame.Framework\bin\"""
            }
            outputDir = """NuGetPackages\Output"""
            cleanOutputDir = true
            publishPackages = true
            param("nugetCustomPath", "%teamcity.tool.NuGet.CommandLine.DEFAULT%")
            param("nugetPathSelector", "%teamcity.tool.NuGet.CommandLine.DEFAULT%")
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${Version.id}"
            successfulOnly = true
            branchFilter = "+:refs/heads/develop"
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
            """.trimIndent()
        }
    }

    requirements {
        startsWith("system.agent.name", "MonoGameWin")
    }
})

object PackageMacAndLinux : BuildType({
    name = "Package Mac and Linux"
    description = "Create the Mac and Linux SDK packages."

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

object PackageNuGet : BuildType({
    name = "Package NuGet"
    description = "Create the develop NuGet packages."

    allowExternalStatus = true
    buildNumberPattern = "${Version.depParamRefs.buildNumber}"

    vcs {
        root(RelativeId("Develop"), "-:.", "+:NuGetPackages")

        checkoutMode = CheckoutMode.ON_SERVER
        cleanCheckout = true
        showDependenciesChanges = true
    }

    steps {
        nuGetPack {
            toolPath = "%teamcity.tool.NuGet.CommandLine.DEFAULT%"
            paths = """NuGetPackages\*.nuspec"""
            version = "%build.number%-develop"
            baseDir = customPath {
                path = """MonoGame.Framework\bin\"""
            }
            outputDir = """NuGetPackages\Output"""
            cleanOutputDir = true
            param("nugetCustomPath", "%teamcity.tool.NuGet.CommandLine.DEFAULT%")
            param("nugetPathSelector", "%teamcity.tool.NuGet.CommandLine.DEFAULT%")
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${GenerateDocumentation.id}"
            successfulOnly = true
            branchFilter = """
                +:*
                -:refs/heads/develop
                -:refs/heads/master
            """.trimIndent()
        }
    }

    features {
        feature {
            type = "teamcity.github.status"
            param("guthub_context", "Package NuGet")
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
            artifactRules = "MonoGame.Framework.zip!**"
        }
        artifacts(DevelopWin) {
            artifactRules = """
                MonoGame.Framework.zip!**
                MonoGame.Framework.Content.Pipeline.zip!**
            """.trimIndent()
        }
    }

    requirements {
        startsWith("system.agent.name", "MonoGameWin")
    }
})

object PackagingWindows : BuildType({
    name = "Package Windows"
    description = "Create the Windows SDK packaging."

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

    vcs {
        root(RelativeId("Develop"), "-:.", "+:MonoGame.Framework.Content.Pipeline", "+:MonoGame.Framework", "+:Test", "+:Tools", "+:default.build")

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
            targets = "run_tests"
            reduceTestFeedback = true
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

    vcs {
        root(RelativeId("Develop"), "-:.", "+:MonoGame.Framework.Content.Pipeline", "+:MonoGame.Framework", "+:Test", "+:Tools", "+:default.build")

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
            targets = "run_tests"
            reduceTestFeedback = true
            param("dotNetCoverage.dotCover.home.path", "%teamcity.tool.JetBrains.dotCover.CommandLineTools.bundled%")
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
        text("VersionPatch", "0", label = "Patch Version Number", description = "The patch version number or zero.",
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
