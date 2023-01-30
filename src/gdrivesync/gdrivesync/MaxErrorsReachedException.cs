namespace GDriveSync;

/// <summary>
/// Maximum errors reached exception.
/// </summary>
/// <seealso cref="System.Exception" />
public class MaxErrorsReachedException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MaxErrorsReachedException"/> class.
	/// </summary>
	public MaxErrorsReachedException()
		: base("Maximum number of errors has been reached, exiting run")
	{ }
}