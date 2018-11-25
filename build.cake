var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");
var solution = "./imagini.sln";
var testProject = "./Tests/Tests.csproj";

Task("Restore").Does(() => {
    DotNetCoreRestore();
});

Task("Clean").Does(() => {
    DotNetCoreClean(solution);
    CleanDirectory("./docs/coverage/");
});

Task("Build").Does(() => {
    DotNetCoreBuild(solution, new DotNetCoreBuildSettings {
        Configuration = configuration
    });
}).OnError(ex => {
    Error("Build Failed");
    throw ex;
});

Task("Test").Does(() => {
    DotNetCoreTest(testProject, new DotNetCoreTestSettings {
        NoBuild = true,
        Configuration = configuration,
        ArgumentCustomization = (b) => b
            .Append("/p:CollectCoverage=true")
            .Append("/p:CoverletOutputFormat=opencover")
            .Append("/p:CoverletOutput=./coverage.xml")
            .Append("/p:Exclude=\"[SDL*]*\"")
    });
});

Task("ReportCoverage").Does(() => {
    var param = "\"-reports:./Tests/coverage.xml\" " +
        "\"-targetdir:./docs/coverage/\" " +
        "\"-sourcedirs:./Imagini.Core/;./Imagini.2D/;./Imagini.Audio/\" " +
        "\"-reporttypes:HTML;Badges\"";
    Information("Running 'reportgenerator " + param + "'");
    StartProcess("reportgenerator", new ProcessSettings {
        Arguments = param
    });
    // DotNetCoreTool(testProject, "reportgenerator", param);
});

Task("InstallTools").Does(() => {
    StartProcess("dotnet", new ProcessSettings {
        Arguments = "tool install --global dotnet-reportgenerator-globaltool"
    });
});

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("ReportCoverage");


Task("Full")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("ReportCoverage");

Task("CI")
    .IsDependentOn("InstallTools")
    .IsDependentOn("Full");

RunTarget(target);
