using Google.Apis.Drive.v3;
using System.Collections.Concurrent;

namespace GDriveSync;

/// <summary>
/// Directory synchronization service.
/// </summary>
public class DirectorySynchronizationService
{
	#region Members

	private const int MaxErrorCount = 10;

	private readonly DirectoryInfo _localRootDir;
	private readonly string _rootFolderId;
	private readonly IGoogleDriveService _googleDriveService;
	private readonly SyncMode _syncMode;
	private readonly IOutputWriter _outputWriter;

	private int _checkedDirCount;
	private int _createDirCount;
	private int _deleteDirCount;
	private int _checkedFileCount;
	private int _downloadFileCount;
	private int _overwriteFileCount;
	private int _deleteFileCount;
	private int _errorCount;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="DirectorySynchronizationService"/> class.
	/// </summary>
	/// <param name="localDirInfo">The local directory information.</param>
	/// <param name="rootFolderId">The root folder identifier.</param>
	/// <param name="googleDriveService">The Google drive service.</param>
	/// <param name="syncMode">The synchronize mode.</param>
	/// <param name="outputWriter">The output writer.</param>
	/// <exception cref="System.ArgumentNullException">
	/// localDirInfo
	/// or
	/// googleDriveService
	/// or
	/// outputWriter
	/// </exception>
	/// <exception cref="System.ArgumentException">rootFolderId</exception>
	public DirectorySynchronizationService(
		DirectoryInfo? localDirInfo,
		string rootFolderId,
		IGoogleDriveService googleDriveService,
		SyncMode syncMode,
		IOutputWriter outputWriter)
	{
		_localRootDir = localDirInfo ?? throw new ArgumentNullException(nameof(localDirInfo));
		if (string.IsNullOrWhiteSpace(rootFolderId))
		{
			throw new ArgumentException($"{nameof(rootFolderId)} must have a value", nameof(rootFolderId));
		}
		_rootFolderId = rootFolderId;
		_googleDriveService = googleDriveService ?? throw new ArgumentNullException(nameof(googleDriveService));
		_syncMode = syncMode;
		_outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Synchronizes the directory defined by the configuration.
	/// </summary>
	/// <returns>
	/// The results.
	/// </returns>
	public async Task<SyncResult> Sync()
	{
		SyncResult result;
		switch (_syncMode)
		{
			case SyncMode.OneWayToLocal:
				result = await OneWaySyncLocal();
				break;
			default:
				await _outputWriter.WriteErrorAsync($"Sync mode '{_syncMode}' is not implemented");
				result = SyncResult.InvalidArguments;
				break;
		}

		return result;
	}

	#endregion

	#region Helper Methods

	private async Task<SyncResult> OneWaySyncLocal()
	{
		var folderInfo = await _googleDriveService.GetFolderInfo(_rootFolderId);

		await OneWaySyncLocalCompareDirectory(_localRootDir, folderInfo);

		await _outputWriter.WriteLineAysnc("Sync complete:");
		await _outputWriter.WriteLineAysnc($"  Checked Dirs: {_checkedDirCount}");
		await _outputWriter.WriteLineAysnc($"  Created Dirs: {_createDirCount}");
		await _outputWriter.WriteLineAysnc($"  Deleted Dirs: {_deleteDirCount}");
		await _outputWriter.WriteLineAysnc($"  Checked Files: {_checkedFileCount}");
		await _outputWriter.WriteLineAysnc($"  Downloaded Files: {_downloadFileCount}");
		await _outputWriter.WriteLineAysnc($"  Overwritten Files: {_overwriteFileCount}");
		await _outputWriter.WriteLineAysnc($"  Deleted Files: {_deleteFileCount}");
		await _outputWriter.WriteLineAysnc($"  Errors: {_errorCount}");

		return SyncResult.Success;
	}

	private async Task OneWaySyncLocalCompareDirectory(DirectoryInfo dir, DriveFolderInfo folder)
	{
		try
		{
			// Compare directories
			var childDirs = dir.GetDirectories().Select(d => new ComparedDir(d)).ToList();
			foreach (var childFolder in folder.ChildFolders)
			{
				var correspondingDir = childDirs.FirstOrDefault(d => d.Dir.Name.Trim().Equals(childFolder.Name, StringComparison.InvariantCultureIgnoreCase));
				if (correspondingDir == null)
				{
					await _outputWriter.WriteLineAysnc($"++ Add dir '{dir.FullName}\\{childFolder.Name}'");
					correspondingDir = new ComparedDir(dir.CreateSubdirectory(childFolder.Name));
					Interlocked.Increment(ref _createDirCount);
				}

				var fullChildFolderInfo = await _googleDriveService.GetFolderInfo(childFolder);
				await OneWaySyncLocalCompareDirectory(correspondingDir.Dir, fullChildFolderInfo);

				correspondingDir.Compared = true;

				Interlocked.Increment(ref _checkedDirCount);
			}

			foreach (var missingDir in childDirs.Where(d => !d.Compared))
			{
				await _outputWriter.WriteLineAysnc($"-- Delete dir '{missingDir.Dir.FullName}'");
				missingDir.Dir.Delete(true);
				Interlocked.Increment(ref _deleteDirCount);
			}

			// Compare files
			var dirFiles = dir.GetFiles().Select(f => new ComparedFile(f)).ToList();
			foreach (var folderFile in folder.Files)
			{
				var dirFile = dirFiles.FirstOrDefault(f => f.File.Name.Equals(folderFile.Name, StringComparison.InvariantCultureIgnoreCase));
				if (dirFile == null)
				{
					await _outputWriter.WriteLineAysnc($"++ Download file '{dir.FullName}\\{folderFile.Name}'");
					if (!await _googleDriveService.DownloadFile(dir.FullName, folderFile))
					{
						IncrementErrors();
					}
					Interlocked.Increment(ref _downloadFileCount);
				}
				else
				{
					if (dirFile.File.LastWriteTime != folderFile.ModifiedTime)
					{
						await _outputWriter.WriteLineAysnc($"== Overwrite file '{dir.FullName}\\{folderFile.Name}'");
						dirFile.File.Delete();
						if (!await _googleDriveService.DownloadFile(dir.FullName, folderFile))
						{
							IncrementErrors();
						}
						Interlocked.Increment(ref _overwriteFileCount);
					}

					dirFile.Compared = true;
				}

				if (Interlocked.Increment(ref _checkedFileCount) % 1000 == 0)
				{
					await _outputWriter.WriteLineAysnc($"Processed {_checkedFileCount} files...");
				}
			}

			foreach (var missingFile in dirFiles.Where(f => !f.Compared))
			{
				await _outputWriter.WriteLineAysnc($"-- Delete file '{missingFile.File.FullName}'");
				missingFile.File.Delete();
				Interlocked.Increment(ref _deleteFileCount);
			}
		}
		catch (MaxErrorsReachedException)
		{
			throw;
		}
		catch (Exception ex)
		{
			await _outputWriter.WriteErrorAsync(ex.ToString());
			IncrementErrors();
		}
	}

	private void IncrementErrors()
	{
		Interlocked.Increment(ref _errorCount);
		if (_errorCount > MaxErrorCount)
		{
			throw new MaxErrorsReachedException();
		}
	}

	#endregion

	#region Helper Classes

	private class ComparedDir
	{
		public DirectoryInfo Dir { get; set; }
		public bool Compared { get; set; }

		public ComparedDir(DirectoryInfo dir)
		{
			Dir = dir;
			Compared = false;
		}
	}

	private class ComparedFile
	{
		public FileInfo File { get; set; }
		public bool Compared { get; set; }

		public ComparedFile(FileInfo file)
		{
			File = file;
			Compared = false;
		}
	}

	#endregion
}