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
    CleanDirectory("./docs/api/");
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

Task("ApiDoc").Does(() => {
    DotNetCoreTool(solution, "doc", "-f Html -s ./Imagini.Core/ -o ./docs/api/core/");
    DotNetCoreTool(solution, "doc", "-f Html -s ./Imagini.2D/ -o ./docs/api/2d/");
    DotNetCoreTool(solution, "doc", "-f Html -s ./Imagini.ImageSharp/ -o ./docs/api/imagesharp/");
    CopyFile("docs/index.css", "docs/api/core/index.css");
    CopyFile("docs/index.css", "docs/api/2d/index.css");
    CopyFile("docs/index.css", "docs/api/imagesharp/index.css");
});

Task("ReportCoverage").Does(() => {
    var param = "\"-reports:./Tests/coverage.xml\" " +
        "\"-targetdir:./docs/coverage/\" " +
        "\"-sourcedirs:./Imagini.Core/;./Imagini.2D/\" " +
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
    StartProcess("dotnet", new ProcessSettings {
        Arguments = "tool install --global dotbook"
    });
});

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("ReportCoverage")
    .IsDependentOn("ApiDoc");


Task("CleanBuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Default");

Task("CI")
    .IsDependentOn("InstallTools")
    .IsDependentOn("CleanBuild");

RunTarget(target);
