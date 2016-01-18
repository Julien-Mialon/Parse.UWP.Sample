// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if IOS
using PreserveAttribute = Foundation.PreserveAttribute;
#elif ANDROID
using PreserveAttribute = Android.Runtime.PreserveAttribute;
#endif

namespace Parse.Internal {
  /// <summary>
  /// Provides a List implementation that can delegate to any other
  /// list, regardless of its value type. Used for coercion of
  /// lists when returning them to users.
  /// </summary>
  /// <typeparam name="TOut">The resulting type of value in the list.</typeparam>
  /// <typeparam name="TIn">The original type of value in the list.</typeparam>
#if MONO
  [Preserve(AllMembers = true)]
#endif
  class FlexibleListWrapper<TOut, TIn> : IList<TOut> {
    private IList<TIn> toWrap;
    public FlexibleListWrapper(IList<TIn> toWrap) {
      this.toWrap = toWrap;
    }

    public int IndexOf(TOut item) {
      return toWrap.IndexOf((TIn)ParseClient.ConvertTo<TIn>(item));
    }

    public void Insert(int index, TOut item) {
      toWrap.Insert(index, (TIn)ParseClient.ConvertTo<TIn>(item));
    }

    public void RemoveAt(int index) {
      toWrap.RemoveAt(index);
    }

    public TOut this[int index] {
      get {
        return (TOut)ParseClient.ConvertTo<TOut>(toWrap[index]);
      }
      set {
        toWrap[index] = (TIn)ParseClient.ConvertTo<TIn>(value);
      }
    }

    public void Add(TOut item) {
      toWrap.Add((TIn)ParseClient.ConvertTo<TIn>(item));
    }

    public void Clear() {
      toWrap.Clear();
    }

    public bool Contains(TOut item) {
      return toWrap.Contains((TIn)ParseClient.ConvertTo<TIn>(item));
    }

    public void CopyTo(TOut[] array, int arrayIndex) {
      toWrap.Select(item => (TOut)ParseClient.ConvertTo<TOut>(item))
          .ToList().CopyTo(array, arrayIndex);
    }

    public int Count {
      get { return toWrap.Count; }
    }

    public bool IsReadOnly {
      get { return toWrap.IsReadOnly; }
    }

    public bool Remove(TOut item) {
      return toWrap.Remove((TIn)ParseClient.ConvertTo<TIn>(item));
    }

    public IEnumerator<TOut> GetEnumerator() {
      foreach (var item in (IEnumerable)toWrap) {
        yield return (TOut)ParseClient.ConvertTo<TOut>(item);
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }
  }
}
