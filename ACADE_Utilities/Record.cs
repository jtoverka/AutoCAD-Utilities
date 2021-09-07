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
using System.ComponentModel;
using System.Xml.Serialization;

namespace ACADE_Utilities
{
    /// <summary>
    /// Represents a 2-record, or a pair.
    /// </summary>
    /// <typeparam name="T1">The type of the record's first component.</typeparam>
    /// <typeparam name="T2">The type of the record's second component.</typeparam>
    [Serializable]
    public class Record<T1, T2> : INotifyPropertyChanged
    {
        #region Fields

        private T1 item1Field = default;
        private T2 item2Field = default;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's first component.
        /// </summary>
        [XmlElement]
        public T1 Item1
        {
            get { return item1Field; }
            set
            {
                if (item1Field.Equals(value))
                    return;

                item1Field = value;
                OnPropertyChanged(this, new(nameof(Item1)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's second component.
        /// </summary>
        [XmlElement]
        public T2 Item2
        {
            get { return item2Field; }
            set
            {
                if (item2Field.Equals(value))
                    return;

                item2Field = value;
                OnPropertyChanged(this, new(nameof(Item2)));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invokes a property changed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The PropertyChangedEventArgs that contains the event's data.</param>
        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        #endregion

        #region Delegates, Events, Handlers

        /// <summary>
        /// Invoked on property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    /// <summary>
    /// Represents a 3-record, or a triple.
    /// </summary>
    /// <typeparam name="T1">The type of the record's first component.</typeparam>
    /// <typeparam name="T2">The type of the record's second component.</typeparam>
    /// <typeparam name="T3">The type of the record's third component.</typeparam>
    [Serializable]
    public class Record<T1, T2, T3> : INotifyPropertyChanged
    {
        #region Fields

        private T1 item1Field = default;
        private T2 item2Field = default;
        private T3 item3Field = default;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's first component.
        /// </summary>
        [XmlElement]
        public T1 Item1
        {
            get { return item1Field; }
            set
            {
                if (item1Field.Equals(value))
                    return;

                item1Field = value;
                OnPropertyChanged(this, new(nameof(Item1)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's second component.
        /// </summary>
        [XmlElement]
        public T2 Item2
        {
            get { return item2Field; }
            set
            {
                if (item2Field.Equals(value))
                    return;

                item2Field = value;
                OnPropertyChanged(this, new(nameof(Item2)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's third component.
        /// </summary>
        [XmlElement]
        public T3 Item3
        {
            get { return item3Field; }
            set
            {
                if (item3Field.Equals(value))
                    return;

                item3Field = value;
                OnPropertyChanged(this, new(nameof(Item3)));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invokes a property changed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The PropertyChangedEventArgs that contains the event's data.</param>
        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        #endregion

        #region Delegates, Events, Handlers

        /// <summary>
        /// Invoked on property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    /// <summary>
    /// Represents a 4-record, or a quadruple.
    /// </summary>
    /// <typeparam name="T1">The type of the record's first component.</typeparam>
    /// <typeparam name="T2">The type of the record's second component.</typeparam>
    /// <typeparam name="T3">The type of the record's third component.</typeparam>
    /// <typeparam name="T4">The type of the record's fourth component.</typeparam>
    [Serializable]
    public class Record<T1, T2, T3, T4> : INotifyPropertyChanged
    {
        #region Fields

        private T1 item1Field = default;
        private T2 item2Field = default;
        private T3 item3Field = default;
        private T4 item4Field = default;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's first component.
        /// </summary>
        [XmlElement]
        public T1 Item1
        {
            get { return item1Field; }
            set
            {
                if (item1Field.Equals(value))
                    return;

                item1Field = value;
                OnPropertyChanged(this, new(nameof(Item1)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's second component.
        /// </summary>
        [XmlElement]
        public T2 Item2
        {
            get { return item2Field; }
            set
            {
                if (item2Field.Equals(value))
                    return;

                item2Field = value;
                OnPropertyChanged(this, new(nameof(Item2)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's third component.
        /// </summary>
        [XmlElement]
        public T3 Item3
        {
            get { return item3Field; }
            set
            {
                if (item3Field.Equals(value))
                    return;

                item3Field = value;
                OnPropertyChanged(this, new(nameof(Item3)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's fourth component.
        /// </summary>
        [XmlElement]
        public T4 Item4
        {
            get { return item4Field; }
            set
            {
                if (item4Field.Equals(value))
                    return;

                item4Field = value;
                OnPropertyChanged(this, new(nameof(Item4)));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invokes a property changed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The PropertyChangedEventArgs that contains the event's data.</param>
        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        #endregion

        #region Delegates, Events, Handlers

        /// <summary>
        /// Invoked on property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    /// <summary>
    /// Represents a 5-record, or a quintuple.
    /// </summary>
    /// <typeparam name="T1">The type of the record's first component.</typeparam>
    /// <typeparam name="T2">The type of the record's second component.</typeparam>
    /// <typeparam name="T3">The type of the record's third component.</typeparam>
    /// <typeparam name="T4">The type of the record's fourth component.</typeparam>
    /// <typeparam name="T5">The type of the record's fifth component.</typeparam>
    [Serializable]
    public class Record<T1, T2, T3, T4, T5> : INotifyPropertyChanged
    {
        #region Fields

        private T1 item1Field = default;
        private T2 item2Field = default;
        private T3 item3Field = default;
        private T4 item4Field = default;
        private T5 item5Field = default;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's first component.
        /// </summary>
        [XmlElement]
        public T1 Item1
        {
            get { return item1Field; }
            set
            {
                if (item1Field.Equals(value))
                    return;

                item1Field = value;
                OnPropertyChanged(this, new(nameof(Item1)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's second component.
        /// </summary>
        [XmlElement]
        public T2 Item2
        {
            get { return item2Field; }
            set
            {
                if (item2Field.Equals(value))
                    return;

                item2Field = value;
                OnPropertyChanged(this, new(nameof(Item2)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's third component.
        /// </summary>
        [XmlElement]
        public T3 Item3
        {
            get { return item3Field; }
            set
            {
                if (item3Field.Equals(value))
                    return;

                item3Field = value;
                OnPropertyChanged(this, new(nameof(Item3)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's fourth component.
        /// </summary>
        [XmlElement]
        public T4 Item4
        {
            get { return item4Field; }
            set
            {
                if (item4Field.Equals(value))
                    return;

                item4Field = value;
                OnPropertyChanged(this, new(nameof(Item4)));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Record{T1, T2, T3, T4, T5}"/> object's fifth component.
        /// </summary>
        [XmlElement]
		public T5 Item5
        {
            get { return item5Field; }
            set
            {
                if (item5Field.Equals(value))
                    return;

                item5Field = value;
                OnPropertyChanged(this, new(nameof(Item5)));
            }
        }

		#endregion

		#region Methods

        /// <summary>
        /// Invokes a property changed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The PropertyChangedEventArgs that contains the event's data.</param>
        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
            PropertyChanged?.Invoke(sender, e);
		}

		#endregion
		
        #region Delegates, Events, Handlers

        /// <summary>
        /// Invoked on property changed.
        /// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
