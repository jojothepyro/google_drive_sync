namespace GDriveSync;

/// <summary>
/// Local directory service.
/// </summary>
public class LocalDirectoryService : ILocalDirectoryService
{
	#region Private Members

	private readonly DirectoryInfo _localRootDir;

	#endregion

	#region Properties

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="LocalDirectoryService"/> class.
	/// </summary>
	/// <param name="localDirInfo">The local directory information.</param>
	/// <exception cref="System.ArgumentNullException">localDirInfo</exception>
	public LocalDirectoryService(DirectoryInfo? localDirInfo)
	{
		_localRootDir = localDirInfo ?? throw new ArgumentNullException(nameof(localDirInfo));
	}

	#endregion

	#region Public Methods

	#endregion

	#region Helper Methods

	#endregion
}