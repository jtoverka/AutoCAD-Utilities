using System;

namespace ACAD_Utilities
{
	/// <summary>
	/// A wrapper for a disposable object. Allows the using statement to be used with a condition.
	/// </summary>
	public sealed class Disposable : IDisposable
	{
		#region Fields

		private readonly IDisposable disposable;
		private readonly bool dispose;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="disposable">The disposable object to wrap.</param>
		/// <param name="dispose">The condition </param>
		public Disposable(IDisposable disposable, bool dispose)
		{
			this.disposable = disposable;
			this.dispose = dispose;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Releases unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (dispose)
				disposable.Dispose();
		}

		#endregion
	}
}
