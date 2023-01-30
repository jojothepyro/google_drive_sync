namespace GDriveSync;

/// <summary>
/// Defines a Google drive service.
/// </summary>
public interface IGoogleDriveService
{
	/// <summary>
	/// Gets the folder information.
	/// </summary>
	/// <param name="folderId">The folder identifier.</param>
	/// <returns>The drive folder information.</returns>
	Task<DriveFolderInfo> GetFolderInfo(string folderId);

	/// <summary>
	/// Gets the folder information.
	/// </summary>
	/// <param name="folderInfo">The folder information.</param>
	/// <returns>The drive folder information.</returns>
	Task<DriveFolderInfo> GetFolderInfo(DriveChildFolderInfo folderInfo);

	/// <summary>
	/// Downloads the file to the specified path.
	/// </summary>
	/// <param name="dirPath">The directory path.</param>
	/// <param name="fileInfo">The file information.</param>
	/// <returns>If the download was successful.</returns>
	Task<bool> DownloadFile(string dirPath, DriveFileInfo fileInfo);
}