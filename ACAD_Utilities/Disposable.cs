/*This is free and unencumbered software released into the public domain.
*
*Anyone is free to copy, modify, publish, use, compile, sell, or
*distribute this software, either in source code form or as a compiled
*binary, for any purpose, commercial or non-commercial, and by any
*means.
*
*In jurisdictions that recognize copyright laws, the author or authors
*of this software dedicate any and all copyright interest in the
*software to the public domain. We make this dedication for the benefit
*of the public at large and to the detriment of our heirs and
*successors. We intend this dedication to be an overt act of
*relinquishment in perpetuity of all present and future rights to this
*software under copyright law.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
*EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
*MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
*IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
*OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
*ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
*OTHER DEALINGS IN THE SOFTWARE.
*
*For more information, please refer to <https://unlicense.org>
*/

using System;

namespace ACAD_Utilities
{
	/// <summary>
	/// A wrapper for a disposable object. Allows the using statement to be used with a condition.
	/// </summary>
	public sealed class Disposable : IDisposable
	{
		#region Enums

		private enum DisposableType
		{
			flag,
			function,
			action,
			conditiionalAction,
		}

		#endregion
		
		#region Fields

		private readonly IDisposable disposableField;
		private readonly DisposableType typeField;
		private readonly bool disposeField;
		private readonly Func<bool> disposeFunctionField;
		private readonly Action disposeActionField;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="disposable">The disposable object to wrap.</param>
		/// <param name="dispose">The condition to dispose the disposable.</param>
		/// <exception cref="ArgumentNullException" />
		public Disposable(IDisposable disposable, bool dispose)
		{
			typeField = DisposableType.flag;
			disposableField = disposable ?? throw new ArgumentNullException("The disposable cannot be null");
			disposeField = dispose;
		}

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="disposable">The disposable object to wrap.</param>
		/// <param name="dispose">The condition to dispose the disposable.</param>
		/// <exception cref="ArgumentNullException" />
		public Disposable(IDisposable disposable, Func<bool> dispose)
		{
			typeField = DisposableType.function;
			disposableField = disposable ?? throw new ArgumentNullException("The disposable cannot be null");
			disposeFunctionField = dispose ?? throw new ArgumentNullException("The action cannot be null");
		}

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="action">The action to invoke on dispose.</param>
		/// <exception cref="ArgumentNullException" />
		public Disposable(Action action)
		{
			typeField = DisposableType.action;
			disposeActionField = action ?? throw new ArgumentNullException("The action cannot be null");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		/// <param name="dispose"></param>
		public Disposable(Action action, bool dispose)
		{
			typeField = DisposableType.conditiionalAction;
			disposeActionField = action ?? throw new ArgumentNullException("The action cannot be null");
			disposeField = dispose;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Releases unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			switch (typeField)
			{
				case DisposableType.flag:
					if (disposeField)
						disposableField.Dispose();
					break;
				case DisposableType.function:
					if (disposeFunctionField.Invoke())
						disposableField.Dispose();
					break;
				case DisposableType.action:
					disposeActionField.Invoke();
					break;
				case DisposableType.conditiionalAction:
					if (disposeField)
						disposeActionField.Invoke();
					break;
				default:
					break;
			}
		}

		#endregion
	}
}
