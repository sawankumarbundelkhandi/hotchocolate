using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Helpers;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Configuration = IsLocalBuild ? Debug : Release;

    [CI] readonly AzurePipelines DevOpsPipeLine;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetBuildSonarSolution(AllSolutionFile);
            DotNetRestore(c => c.SetProjectFile(AllSolutionFile));
            BuildCodeGenServer();
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            if (!InvokedTargets.Contains(Restore))
            {
                DotNetBuildSonarSolution(AllSolutionFile);
            }

            DotNetBuild(c => c
                .SetProjectFile(AllSolutionFile)
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetVersion(GitVersion.SemVer));
        });

    Target Reset => _ => _
        .Executes(() =>
        {
            TryDelete(AllSolutionFile);
            TryDelete(SonarSolutionFile);
            TryDelete(TestSolutionFile);
            TryDelete(PackSolutionFile);

            DotNetBuildSonarSolution(AllSolutionFile);
            DotNetRestore(c => c.SetProjectFile(AllSolutionFile));
        });

    void BuildCodeGenServer(bool restore = false)
    {
        DotNetBuild(c => c
            .SetNoRestore(!restore)
            .SetProjectFile(RootDirectory / "src/StrawberryShake/CodeGeneration/src/CodeGeneration.CSharp.Server/StrawberryShake.CodeGeneration.CSharp.Server.csproj")
            .SetOutputDirectory(RootDirectory / "src/StrawberryShake/Tooling/src/.server")
            .SetConfiguration(Configuration));

        DotNetBuild(c => c
            .SetNoRestore(!restore)
            .SetProjectFile(RootDirectory / "src/StrawberryShake/CodeGeneration/src/CodeGeneration.CSharp.Server/StrawberryShake.CodeGeneration.CSharp.Server.csproj")
                .SetOutputDirectory(RootDirectory / "src/StrawberryShake/SourceGenerator/src/.server")
                .SetConfiguration(Configuration));
    }
}
