#addin "nuget:?package=Cake.Boots&version=1.1.0.712-preview2"

var TARGET = Argument("t", Argument("target", "ci"));

var NUGET_VERSION = EnvironmentVariable("NUGET_VERSION") ?? "3.0.0";
var GIT_SHA = Argument("gitSha", EnvironmentVariable("GIT_SHA") ?? "");
var GIT_BRANCH_NAME = Argument("gitBranch", EnvironmentVariable("GIT_BRANCH_NAME") ?? "");

Task("prepare")
	.Does(async () =>
{
	// Setup latest Xamarin.Android SDK
	await Boots (Product.XamarinAndroid, ReleaseChannel.Stable);

	// Update .csproj nuget versions
	XmlPoke("./Xamarin.Essentials/Xamarin.Essentials.csproj", "/Project/PropertyGroup/PackageVersion", NUGET_VERSION);
});

Task("libs")
	.IsDependentOn("prepare")
	.Does(() =>
{
	MSBuild("./Xamarin.Essentials/Xamarin.Essentials.csproj", new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/libs.binlog")
		.SetConfiguration("Release")
		.WithRestore());
});

Task("nugets")
	.IsDependentOn("prepare")
	.IsDependentOn("libs")
	.IsDependentOn("docs")
	.Does(() =>
{
	MSBuild("./Xamarin.Essentials/Xamarin.Essentials.csproj", new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/nugets.binlog")
		.SetConfiguration("Release")
		.WithRestore()
		.WithProperty("PackageOutputPath", MakeAbsolute(new FilePath("./output/")).FullPath)
		.WithTarget("Pack"));
});

Task("tests")
	.IsDependentOn("libs")
	.Does(() =>
{
	var failed = 0;

	foreach (var csproj in GetFiles("./Tests/**/*.csproj")) {
		try {
			DotNetTest(csproj.FullPath, new DotNetTestSettings {
				Configuration = "Release",
				Loggers = new [] { $"trx;LogFileName={csproj.GetFilenameWithoutExtension()}.trx" }
			});
		} catch (Exception) {
			failed++;
		}
	}

	var output = $"./output/test-results/";
	EnsureDirectoryExists(output);
	CopyFiles($"./tests/**/TestResults/*.trx", output);

	if (failed > 0)
		throw new Exception($"{failed} tests have failed.");
});

Task("samples")
	.IsDependentOn("nugets")
	.Does(() =>
{
	MSBuild("./Xamarin.Essentials.sln", new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/samples.binlog")
		.SetConfiguration("Release")
		.WithRestore());
});

Task("docs")
	.IsDependentOn("libs")
	.WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	MSBuild("./Xamarin.Essentials/Xamarin.Essentials.csproj", new MSBuildSettings()
		.EnableBinaryLogger("./output/binlogs/docs.binlog")
		.SetConfiguration("Release")
		.WithRestore()
		.WithTarget("mdocupdatedocs"));
});

Task("ci")
	.IsDependentOn("libs")
	.IsDependentOn("nugets")
	.IsDependentOn("tests")
	.IsDependentOn("samples");

Task("ci-release")
	.IsDependentOn("libs")
	.IsDependentOn("nugets");

RunTarget(TARGET);
