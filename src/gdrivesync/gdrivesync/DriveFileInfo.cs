namespace GDriveSync;

/// <summary>
/// Drive file information.
/// </summary>
public class DriveFileInfo
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
	/// Gets the modified time.
	/// </summary>
	/// <value>
	/// The modified time.
	/// </value>
	public DateTime? ModifiedTime { get; }

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="DriveFileInfo"/> class.
	/// </summary>
	/// <param name="id">The identifier.</param>
	/// <param name="name">The name.</param>
	/// <exception cref="System.ArgumentNullException">
	/// id
	/// or
	/// name
	/// </exception>
	public DriveFileInfo(Google.Apis.Drive.v3.Data.File? file)
	{
		if (file == null)
		{
			throw new ArgumentNullException(nameof(file));
		}

		Id = file.Id;
		Name = file.Name.Trim();
		ModifiedTime = file.ModifiedTime;
	}

	#endregion
}