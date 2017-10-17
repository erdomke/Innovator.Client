#if REFLECTION
// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;

namespace Innovator.Client.Queryable
{
  /// <summary>
  /// Type related helper methods
  /// </summary>
  internal static class TypeHelper
  {
    public static Type FindIEnumerable(Type seqType)
    {
      if (seqType == null || seqType == typeof(string))
        return null;
      if (seqType.IsArray)
        return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
      if (seqType.IsGenericType)
      {
        foreach (Type arg in seqType.GetGenericArguments())
        {
          Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
          if (ienum.IsAssignableFrom(seqType))
          {
            return ienum;
          }
        }
      }
      Type[] ifaces = seqType.GetInterfaces();
      if (ifaces != null && ifaces.Length > 0)
      {
        foreach (Type iface in ifaces)
        {
          Type ienum = FindIEnumerable(iface);
          if (ienum != null) return ienum;
        }
      }
      if (seqType.BaseType != null && seqType.BaseType != typeof(object))
      {
        return FindIEnumerable(seqType.BaseType);
      }
      return null;
    }

    public static Type GetElementType(Type seqType)
    {
      Type ienum = FindIEnumerable(seqType);
      if (ienum == null) return seqType;
      return ienum.GetGenericArguments()[0];
    }
  }
}
#endif