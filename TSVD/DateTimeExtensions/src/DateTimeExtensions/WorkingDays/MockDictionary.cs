// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Coyote.Tasks;

namespace Nekara
{
    /// <summary>
    /// A MockDictionary, which implements IDictionary that can be controlled during testing.
    /// </summary>
    public class MockDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        private int sharedEntry;
        private bool IsWrite = false;

        protected IDictionary<TKey, TValue> InnerDictionary
        {
            get;
            private set;
        }

        public int Count => this.InnerDictionary.Count;

        public bool IsReadOnly => this.InnerDictionary.IsReadOnly;

        public ICollection<TKey> Keys => this.InnerDictionary.Keys;

        public ICollection<TValue> Values => this.InnerDictionary.Values;

        public MockDictionary()
        {
            this.InnerDictionary = new Dictionary<TKey, TValue>();
        }

        public MockDictionary(IDictionary<TKey, TValue> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.InnerDictionary = new Dictionary<TKey, TValue>(source);
        }

        public TValue this[TKey key]
        {
            get
            {
                var TaskId = (int)Task.CurrentId;
                this.sharedEntry = TaskId;

                Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

                if (this.sharedEntry != TaskId && this.IsWrite)
                {
                    Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in get Value");
                }

                return this.InnerDictionary[key];
            }
            set
            {
                var TaskId = (int)Task.CurrentId;
                this.sharedEntry = TaskId;
                this.IsWrite = true;
                Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

                if (this.sharedEntry != TaskId)
                {
                    Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in set Value");
                }

                this.IsWrite = false;
                this.InnerDictionary[key] = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            this.IsWrite = true;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in Add()");
            }

            this.IsWrite = false;

            this.InnerDictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {

            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId && this.IsWrite)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in ContainsKey()");
            }

            return this.InnerDictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {

            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            this.IsWrite = true;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in Remove()");
            }

            this.IsWrite = false;
            return this.InnerDictionary.Remove(key);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            this.IsWrite = true;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in Add(KeyValuePair)");
            }

            this.IsWrite = false;
            this.InnerDictionary.Add(item);
        }

        public void Clear()
        {
            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            this.IsWrite = true;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in Clear");
            }

            this.IsWrite = false;
            this.InnerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId && this.IsWrite)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in Contains(KeyValuePair)");
            }

            return this.InnerDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {

            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId && this.IsWrite)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in CopyTo()");
            }

            this.InnerDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            this.IsWrite = true;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in Remove()");
            }

            this.IsWrite = false;
            return this.InnerDictionary.Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId && this.IsWrite)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in GetEnumerator()");
            }

            return this.InnerDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId && this.IsWrite)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in GetEnumerator()");
            }

            return this.InnerDictionary.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var TaskId = (int)Task.CurrentId;
            this.sharedEntry = TaskId;
            Microsoft.Coyote.Runtime.SchedulingPoint.Interleave();

            if (this.sharedEntry != TaskId && this.IsWrite)
            {
                Microsoft.Coyote.Specifications.Specification.Assert(false, "Race in TryGetValue()");
            }

            return this.InnerDictionary.TryGetValue(key, out value);
        }
    }
}
