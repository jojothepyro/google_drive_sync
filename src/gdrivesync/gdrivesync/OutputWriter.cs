namespace GDriveSync;

/// <summary>
/// Output writer.
/// </summary>
/// <seealso cref="GDriveSync.IOutputWriter" />
public class OutputWriter : IOutputWriter
{
	/// <summary>
	/// Writes the specified value to the output asynchronously.
	/// </summary>
	/// <param name="value">The value.</param>
	public async Task WriteAsync(string value)
	{
		await Console.Out.WriteAsync(value);
	}

	/// <summary>
	/// Writes the specified value as an error to the output asynchronously.
	/// </summary>
	/// <param name="value">The value.</param>
	public async Task WriteErrorAsync(string value)
	{
		await Console.Error.WriteLineAsync(value);
	}

	/// <summary>
	/// Writes the specified value to the output including a newline asynchronously.
	/// </summary>
	/// <param name="value">The value.</param>
	public async Task WriteLineAysnc(string value)
	{
		await Console.Out.WriteLineAsync(value);
	}
}
