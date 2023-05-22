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

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;

namespace AcDbWrapper
{
	/// <summary>
	/// A container of methods to check the validity of AutoCAD structs and objects.
	/// </summary>
	public static class Check
	{
		#region Static Methods

		/// <summary>
		/// Check Object Id for errors.
		/// </summary>
		/// <param name="id">ObjectId to check.</param>
		/// <param name="error">Flag to throw error.</param>
		/// <returns><see langword="true"/> if the objectId is valid; Otherwise, <see langword="false"/>.</returns>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="InvalidOperationException"/>
		public static bool Validate(this ObjectId id, bool error)
		{
			if (error)
			{
				if (id.IsNull)
					throw new NullReferenceException("Transaction is null");

				if (id.IsEffectivelyErased || id.IsErased)
					throw new InvalidOperationException("Object Id is erased");

				if (!id.IsValid)
					throw new InvalidOperationException("Object Id is not valid");
			}
			else
			{
				if (id.IsNull || id.IsEffectivelyErased || id.IsErased || !id.IsValid)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Check Object Id for errors.
		/// </summary>
		/// <param name="id">ObjectId to check.</param>
		/// <param name="objectClass">RXClass to compare to.</param>
		/// <param name="error">Flag to throw error.</param>
		/// <returns><see langword="true"/> if the objectId is valid; Otherwise, <see langword="false"/>.</returns>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="AccessViolationException"/>
		/// <exception cref="ArgumentException"/>
		public static bool Validate(this ObjectId id, RXClass objectClass, bool error)
		{
			if (error)
			{
				if (id.IsNull)
					throw new NullReferenceException("Transaction is null");

				if (id.IsEffectivelyErased || id.IsErased)
					throw new InvalidOperationException("Object Id is erased");

				if (!id.IsValid)
					throw new InvalidOperationException("Object Id is not valid");

				if (id.ObjectClass != objectClass)
					throw new ArgumentException($"Object Id is not a {objectClass.Name}");
			}
			else
			{
				if (id.IsNull || id.IsEffectivelyErased || id.IsErased || !id.IsValid || id.ObjectClass != objectClass)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Check transaction for errors.
		/// </summary>
		/// <param name="transaction">Transaction to check.</param>
		/// <param name="argument">Flag whether it is an argument check.</param>
		/// <returns><see langword="true"/> if the transaction is valid; Otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="ObjectDisposedException"/>
		public static bool Validate(this Transaction transaction, bool argument)
		{
			if (argument)
			{
				if (transaction == null)
					throw new ArgumentNullException("Transaction is null");
			}
			else
			{
				if (transaction == null)
					throw new NullReferenceException("Transaction is null");
			}

			if (transaction.IsDisposed)
				throw new ObjectDisposedException("Transaction has been disposed");

			return true;
		}

		/// <summary>
		/// Check transaction for errors.
		/// </summary>
		/// <param name="transaction">Transaction to check.</param>
		/// <param name="argument">Flag whether it is an argument check.</param>
		/// <param name="error">Flag whether to throw an error.</param>
		/// <returns><see langword="true"/> if the transaction is valid; Otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="ObjectDisposedException"/>
		public static bool Validate(this Transaction transaction, bool argument, bool error)
		{
			if (error)
			{
				if (argument)
				{
					if (transaction == null)
						throw new ArgumentNullException("Transaction is null");
				}
				else
				{
					if (transaction == null)
						throw new NullReferenceException("Transaction is null");
				}

				if (transaction.IsDisposed)
					throw new ObjectDisposedException("Transaction has been disposed");
			}
			else
			{
				if (transaction == null || transaction.IsDisposed)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Check database for errors.
		/// </summary>
		/// <param name="database">Database to check.</param>
		/// <param name="argument">Flag whether it is an argument check.</param>
		/// <returns><see langword="true"/> if the database is valid; Otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="ObjectDisposedException"/>
		public static bool Validate(this Database database, bool argument)
		{
			if (argument)
			{
				if (database == null)
					throw new ArgumentNullException("Database is null");
			}
			else
			{
				if (database == null)
					throw new NullReferenceException("Database is null");
			}

			if (database.IsBeingDestroyed)
				throw new InvalidOperationException("Database cannot be used, it is being destroyed");

			if (database.IsDisposed)
				throw new ObjectDisposedException("Database has been disposed");

			return true;
		}

		/// <summary>
		/// Check database for errors.
		/// </summary>
		/// <param name="database">Database to check.</param>
		/// <param name="argument">Flag whether it is an argument check.</param>
		/// <param name="error">Flag whether to throw an error.</param>
		/// <returns><see langword="true"/> if the database is valid; Otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="NullReferenceException"/>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="ObjectDisposedException"/>
		public static bool Validate(this Database database, bool argument, bool error)
		{
			if (error)
			{
				if (argument)
				{
					if (database == null)
						throw new ArgumentNullException("Database is null");
				}
				else
				{
					if (database == null)
						throw new NullReferenceException("Database is null");
				}

				if (database.IsBeingDestroyed)
					throw new InvalidOperationException("Database cannot be used, it is being destroyed");

				if (database.IsDisposed)
					throw new ObjectDisposedException("Database has been disposed");
			}
			else
			{
				if (database == null || database.IsBeingDestroyed || database.IsDisposed)
					return false;
			}

			return true;
		}

		#endregion
	}
}
