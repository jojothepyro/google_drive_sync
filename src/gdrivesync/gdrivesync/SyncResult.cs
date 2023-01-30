namespace GDriveSync;

/// <summary>
/// Specifies the synchronization result.
/// </summary>
public enum SyncResult
{
	/// <summary>Synchronization successful.</summary>
	Success,
	/// <summary>Invalid arguments.</summary>
	InvalidArguments,
	/// <summary>Invalid configuration.</summary>
	InvalidConfiguration,
	/// <summary>Synchronization failed.</summary>
	Failed
}
