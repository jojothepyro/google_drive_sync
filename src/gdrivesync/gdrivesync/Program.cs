using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace GDriveSync;

public class Program
{
	#region Private Members

	private static readonly IServiceCollection Services = new ServiceCollection()
		.AddSingleton<IOutputWriter, OutputWriter>();

	private static readonly Option<DirectoryInfo?> LocalDirOption = new (
		name: "--local",
		description: "The local directory path.",
		parseArgument: ParseLocalDirectoryArg)
	{ IsRequired = true };

	private static readonly Option<string?> DrivefolderIdOption = new (
		name: "--driveFolderId",
		description: "The ID of the Google drive folder to start searching from.")
	{ IsRequired = true };

	private static readonly Option<SyncMode> SyncModeOption = new(
		name: "--mode",
		description: "The sync mode to use.",
		getDefaultValue: () => SyncMode.OneWayToLocal);

	private static readonly Option<string[]> ExcludeExtensionsOption = new(
		name: "--excludeExt",
		description: "Extensions to exclude during sync")
	{
		AllowMultipleArgumentsPerToken = true,
		Arity = ArgumentArity.ZeroOrMore
	};

	#endregion

	#region Public Methods

	/// <summary>
	/// Main entry point for the application.
	/// </summary>
	/// <param name="args">The arguments.</param>
	/// <returns>Status code of application result.</returns>
	public static async Task<int> Main(string[] args)
	{
		var syncCommand = new Command("sync", "Sync the cloud and local directory")
		{
			LocalDirOption,
			DrivefolderIdOption,
			SyncModeOption,
			ExcludeExtensionsOption
		};
		syncCommand.SetHandler(async (context) =>
		{
			var service = BuildDirectorySynchronizationService(context);
			context.ExitCode = (int) await service.Sync();
		});

		var rootCommand = new RootCommand("Application to sync files from Google Drive to local directory")
		{
			syncCommand
		};

		return await rootCommand.InvokeAsync(args);
	}

	#endregion

	#region Helper Methods

	private static DirectoryInfo? ParseLocalDirectoryArg(ArgumentResult argResult)
	{
		DirectoryInfo? result;
		if (argResult.Tokens.Count == 0)
		{
			argResult.ErrorMessage = "No local directory specified";
			result = null;
		}
		else
		{
			string? dirPath = argResult.Tokens.Single().Value;
			if (!Directory.Exists(dirPath))
			{
				argResult.ErrorMessage = $"Directory '{dirPath}' does not exist";
				result = null;
			}
			else
			{
				result = new DirectoryInfo(dirPath);
			}
		}

		return result;
	}

	private static DirectorySynchronizationService BuildDirectorySynchronizationService(InvocationContext context)
	{
		var localDir = context.ParseResult.GetValueForOption(LocalDirOption);
		var driveFolderId = context.ParseResult.GetValueForOption(DrivefolderIdOption);
		var syncMode = context.ParseResult.GetValueForOption(SyncModeOption);
		var excludedExtensions = context.ParseResult.GetValueForOption(ExcludeExtensionsOption);

		Services
			//.AddSingleton<ILocalDirectoryService>(new LocalDirectoryService(localDir))
			.AddSingleton<IGoogleDriveService>(serviceProvider => new GoogleDriveService(
				excludedExtensions,
				serviceProvider.GetService<IOutputWriter>()))
			.AddSingleton(serviceProvider => new DirectorySynchronizationService(
				localDir,
				driveFolderId,
				serviceProvider.GetService<IGoogleDriveService>(),
				syncMode,
				serviceProvider.GetService<IOutputWriter>()));

		return Services
			.BuildServiceProvider()
			.GetService< DirectorySynchronizationService>();
	}

	#endregion
}