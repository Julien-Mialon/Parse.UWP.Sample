﻿// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Parse.Internal {
  /// <summary>
  /// A <c>ParseEncoder</c> can be used to transform objects such as <see cref="ParseObject"/> into JSON
  /// data structures.
  /// </summary>
  /// <seealso cref="ParseDecoder"/>
  internal abstract class ParseEncoder {
    public static bool IsValidType(object value) {
      return value == null ||
          value.GetType().IsPrimitive() ||
          value is string ||
          value is ParseObject ||
          value is ParseACL ||
          value is ParseFile ||
          value is ParseGeoPoint ||
          value is ParseRelationBase ||
          value is DateTime ||
          value is byte[] ||
          ParseClient.ConvertTo<IDictionary<string, object>>(value) is IDictionary<string, object> ||
          ParseClient.ConvertTo<IList<object>>(value) is IList<object>;
    }

    public object Encode(object value) {
      // If this object has a special encoding, encode it and return the
      // encoded object. Otherwise, just return the original object.
      if (value is DateTime) {
        return new Dictionary<string, object> {
          {"iso", ((DateTime)value).ToString(ParseClient.DateFormatString)},
          {"__type", "Date"}
        };
      }

      var bytes = value as byte[];
      if (bytes != null) {
        return new Dictionary<string, object> {
          {"__type", "Bytes"},
          {"base64", Convert.ToBase64String(bytes)}
        };
      }

      var obj = value as ParseObject;
      if (obj != null) {
        return EncodeParseObject(obj);
      }

      var jsonConvertible = value as IJsonConvertible;
      if (jsonConvertible != null) {
        return jsonConvertible.ToJSON();
      }

      var dict = ParseClient.ConvertTo<IDictionary<string, object>>(value) as IDictionary<string, object>;
      if (dict != null) {
        var json = new Dictionary<string, object>();
        foreach (var pair in dict) {
          json[pair.Key] = Encode(pair.Value);
        }
        return json;
      }

      var list = ParseClient.ConvertTo<IList<object>>(value) as IList<object>;
      if (list != null) {
        return EncodeList(list);
      }

      // TODO (hallucinogen): convert IParseFieldOperation to IJsonConvertible
      var operation = value as IParseFieldOperation;
      if (operation != null) {
        return operation.Encode();
      }

      return value;
    }

    protected abstract IDictionary<string, object> EncodeParseObject(ParseObject value);

    private object EncodeList(IList<object> list) {
      var newArray = new List<object>();
#if UNITY
      // We need to explicitly cast `list` to `List<object>` rather than
      // `IList<object>` because IL2CPP is stricter than the usual Unity AOT compiler pipeline.
      if (PlatformHooks.IsCompiledByIL2CPP && list.GetType().IsArray) {
        list = new List<object>(list);
      }
#endif
      foreach (var item in list) {
        if (!IsValidType(item)) {
          throw new ArgumentException("Invalid type for value in an array");
        }
        newArray.Add(Encode(item));
      }
      return newArray;
    }
  }
}
