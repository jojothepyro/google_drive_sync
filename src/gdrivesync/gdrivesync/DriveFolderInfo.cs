namespace GDriveSync;

/// <summary>
/// Drive folder information.
/// </summary>
public class DriveFolderInfo
{
	#region Properties

	/// <summary>
	/// Gets the identifier.
	/// </summary>
	/// <value>
	/// The identifier.
	/// </value>
	public string Id { get; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	/// <value>
	/// The name.
	/// </value>
	public string Name { get; }

	/// <summary>
	/// Gets the child folders.
	/// </summary>
	/// <value>
	/// The child folders.
	/// </value>
	public IReadOnlyList<DriveChildFolderInfo> ChildFolders { get; }

	/// <summary>
	/// Gets the files.
	/// </summary>
	/// <value>
	/// The files.
	/// </value>
	public IReadOnlyList<DriveFileInfo> Files { get; }

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="DriveFolderInfo"/> class.
	/// </summary>
	/// <param name="id">The identifier.</param>
	/// <param name="name">The name.</param>
	/// <param name="childFolders">The child folders.</param>
	/// <param name="files">The files.</param>
	/// <exception cref="System.ArgumentNullException">
	/// id
	/// or
	/// name
	/// </exception>
	public DriveFolderInfo(string id, string name, IEnumerable<DriveChildFolderInfo> childFolders, IEnumerable<DriveFileInfo> files)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException($"{nameof(id)} cannot be null or empty.", nameof(id));
		}
		Id = id;

		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException($"{nameof(name)} cannot be null or empty.", nameof(name));
		}
		Name = name;

		ChildFolders = childFolders == null 
			? Enumerable.Empty<DriveChildFolderInfo>().ToList().AsReadOnly()
			: childFolders.ToList().AsReadOnly();
		Files = files == null
			? Enumerable.Empty<DriveFileInfo>().ToList().AsReadOnly()
			: files.ToList().AsReadOnly();
	}

	#endregion
}
