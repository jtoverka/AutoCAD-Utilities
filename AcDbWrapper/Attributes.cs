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

using System.Collections.Generic;
using System.ComponentModel;

namespace AcDbWrapper
{
	/// <summary>
	/// XData wrapper into an easy to use format.
	/// </summary>
	public class Attributes : Dictionary<string, DataPoint>, INotifyPropertyChanged
	{
		#region Fields

		private string m_Prefix = string.Empty;
		private string m_Suffix = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the xdata attribute prefix.
        /// </summary>
        public string Prefix
		{
			get { return m_Prefix; }
			set
			{
				value ??= string.Empty;

                if (value != m_Prefix)
                {
                    m_Prefix = value;
                    InvokePropertyChanged(nameof(Prefix));
                }
            }
		}

		/// <summary>
		/// Gets or sets the xdata attribute suffix.
		/// </summary>
		public string Suffix
		{
			get { return m_Suffix; }
			set
			{
				value ??= string.Empty;

				if (value != m_Suffix)
				{
					m_Suffix = value;
					InvokePropertyChanged(nameof(Suffix));
                }
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		public Attributes() : base() { }

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="capacity">The initial number of elements that the <see cref="Attributes"/> can contain.</param>
		public Attributes(int capacity) : base(capacity) { }

        /// <summary>
		/// Initializes a new instance of this class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{string}"/> implementation to use when comparing keys, or null to use the default.</param>
        public Attributes(IEqualityComparer<string> comparer) : base(comparer) { }

        /// <summary>
		/// Initializes a new instance of this class.
        /// </summary>
		/// <param name="capacity">The initial number of elements that the <see cref="Attributes"/> can contain.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{string}"/> implementation to use when comparing keys, or null to use the default.</param>
        public Attributes(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer) { }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="attributes">The <see cref="IDictionary{string, AcDbWrapper.Attrib}"/> whose elements are copied to XData.</param>
        public Attributes(IDictionary<string, DataPoint> attributes) : base(attributes) { }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="attributes">The <see cref="IDictionary{string, AcDbWrapper.Attrib}"/> whose elements are copied to XData.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{string}"/> implementation to use when comparing keys, or null to use the default.</param>
        public Attributes(IDictionary<string, DataPoint> attributes, IEqualityComparer<string> comparer) : base(attributes, comparer) { }

        #endregion

        #region Delegates, Events, Handlers

		/// <inheritdoc cref="INotifyPropertyChanged.PropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Invokes the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		/// <param name="property">The property that invoked the event.</param>
        protected void InvokePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new(property));
        }

        #endregion
	}
}
