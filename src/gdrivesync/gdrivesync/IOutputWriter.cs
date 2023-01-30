namespace GDriveSync;

/// <summary>
/// Defines a output writer.
/// </summary>
public interface IOutputWriter
{
	/// <summary>
	/// Writes the specified value to the output asynchronously.
	/// </summary>
	/// <param name="value">The value.</param>
	Task WriteAsync(string value);

	/// <summary>
	/// Writes the specified value to the output including a newline asynchronously.
	/// </summary>
	/// <param name="value">The value.</param>
	Task WriteLineAysnc(string value);

	/// <summary>
	/// Writes the specified value as an error to the output asynchronously.
	/// </summary>
	/// <param name="value">The value.</param>
	Task WriteErrorAsync(string value);
}
