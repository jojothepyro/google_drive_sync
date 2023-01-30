using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.IO;
using System.Text;

namespace GDriveSync;

public class GoogleDriveService : IGoogleDriveService
{
	#region Private Members

	private DriveService _driveService;
	private readonly string _exclusionQuery;
	private readonly IOutputWriter _outputWriter;

	#endregion

	#region Properties

	protected DriveService DriveService
	{
		get
		{
			if(_driveService == null)
			{
				_driveService = CreateDriveService();
			}
			return _driveService;
		}
	}

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="GoogleDriveService" /> class.
	/// </summary>
	/// <param name="exclusionExtensions">The exclusion extensions.</param>
	/// <param name="outputWriter">The output writer.</param>
	/// <exception cref="System.ArgumentNullException">outputWriter</exception>
	/// <exception cref="System.ArgumentException">rootFolderId</exception>
	public GoogleDriveService(string[] exclusionExtensions, IOutputWriter outputWriter)
	{
		if(exclusionExtensions == null)
		{
			_exclusionQuery = string.Empty;
		}
		else
		{
			var sb = new StringBuilder();
			foreach(var exclusionExtension in exclusionExtensions)
			{
				sb.Append($" and fileExtension != '{exclusionExtension.TrimStart('.').Trim(' ')}'");
			}

			_exclusionQuery = sb.ToString();
		}

		_outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Gets the folder information.
	/// </summary>
	/// <param name="folderId">The folder identifier.</param>
	/// <returns>The drive folder information.</returns>
	/// <exception cref="System.ArgumentException">folderId</exception>
	public async Task<DriveFolderInfo> GetFolderInfo(string folderId)
	{
		if (string.IsNullOrWhiteSpace(folderId))
		{
			throw new ArgumentException($"{nameof(folderId)} is required", nameof(folderId));
		}

		var folderInfo = await DriveService.Files.Get(folderId).ExecuteAsync();

		return await GetFolderInfo(new DriveChildFolderInfo(folderInfo));
	}

	/// <summary>
	/// Gets the folder information.
	/// </summary>
	/// <param name="folderInfo">The folder information.</param>
	/// <returns>The drive folder information.</returns>
	public async Task<DriveFolderInfo> GetFolderInfo(DriveChildFolderInfo folderInfo)
	{
		if (folderInfo == null)
		{
			throw new ArgumentNullException(nameof(folderInfo));
		}

		var listRequest = DriveService.Files.List();
		listRequest.Q = $"'{folderInfo.Id}' in parents" + _exclusionQuery;
		listRequest.Fields = "files(id,name,mimeType,modifiedTime,fileExtension)";
		var listResults = await listRequest.ExecuteAsync();

		var childFolders = new List<DriveChildFolderInfo>();
		var files = new List<DriveFileInfo>();

		if (listResults != null)
		{
			foreach (var listResult in listResults.Files)
			{
				if (listResult.MimeType == "application/vnd.google-apps.folder")
				{
					childFolders.Add(new DriveChildFolderInfo(listResult));
				}
				else
				{
					files.Add(new DriveFileInfo(listResult));
				}
			}
		}

		return new DriveFolderInfo(folderInfo.Id, folderInfo.Name, childFolders, files);
	}

	/// <summary>
	/// Downloads the file to the specified path.
	/// </summary>
	/// <param name="dirPath">The directory path.</param>
	/// <param name="fileInfo">The file information.</param>
	public async Task<bool> DownloadFile(string dirPath, DriveFileInfo fileInfo)
	{
		bool result;
		var filePath = Path.Combine(dirPath, fileInfo.Name);

		try
		{
			using (var fileStream = File.Create(filePath))
			{
				await DriveService.Files.Get(fileInfo.Id).DownloadAsync(fileStream);
			}

			if (fileInfo.ModifiedTime.HasValue)
			{
				File.SetLastWriteTime(filePath, fileInfo.ModifiedTime.Value);
			}
			result = true;
		}
		catch (Exception ex)
		{
			await _outputWriter.WriteErrorAsync($"Error while trying to download file '{fileInfo.Id}' to '{filePath}'");
			await _outputWriter.WriteErrorAsync(ex.ToString());
			result= false;
		}

		return result;
	}

	#endregion

	#region Helper Methods

	private static DriveService CreateDriveService()
	{
		var userCreds = GoogleWebAuthorizationBroker.AuthorizeAsync(
			new ClientSecrets
			{
				ClientId = "60433414972-d1a8eti75bbe95mtgfv6t4oe6tghifkn.apps.googleusercontent.com",
				ClientSecret = "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"
			},
			new[] { DriveService.Scope.DriveReadonly, DriveService.Scope.DriveMetadataReadonly },
			"jojothepyro",
			CancellationToken.None)
			.Result;

		return new DriveService(new BaseClientService.Initializer()
		{
			HttpClientInitializer = userCreds,
			ApplicationName = "gdrivesync"
		});
	}

	#endregion
}