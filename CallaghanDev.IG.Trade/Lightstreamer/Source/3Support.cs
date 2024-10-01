// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.CollectionsSupport
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Lightstreamer.DotNet.Client
{
  internal class CollectionsSupport
  {
    private static MethodInfo GetMethod(ICollection c, string method) => c.GetType().GetTypeInfo().GetDeclaredMethod(method);

    public static bool Add(ICollection c, object obj)
    {
      bool flag = false;
      try
      {
        if ((int) (CollectionsSupport.GetMethod(c, nameof (Add)) ?? CollectionsSupport.GetMethod(c, "add")).Invoke((object) c, new object[1]
        {
          obj
        }) >= 0)
          flag = true;
      }
      catch (Exception ex)
      {
        throw ex;
      }
      return flag;
    }

    public static void Clear(ICollection c)
    {
      try
      {
        (CollectionsSupport.GetMethod(c, nameof (Clear)) ?? CollectionsSupport.GetMethod(c, "clear")).Invoke((object) c, new object[0]);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static bool Contains(ICollection c, object obj)
    {
      try
      {
        return (bool) (CollectionsSupport.GetMethod(c, nameof (Contains)) ?? CollectionsSupport.GetMethod(c, "contains")).Invoke((object) c, new object[1]
        {
          obj
        });
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static bool ContainsAll(ICollection target, ICollection c)
    {
      IEnumerator enumerator = c.GetEnumerator();
      bool flag = false;
      try
      {
        MethodInfo method1 = CollectionsSupport.GetMethod(target, "containsAll");
        if (method1 != null)
        {
          flag = (bool) method1.Invoke((object) target, new object[1]
          {
            (object) c
          });
        }
        else
        {
          MethodInfo method2 = CollectionsSupport.GetMethod(target, "Contains");
          while (enumerator.MoveNext())
          {
            MethodInfo methodInfo = method2;
            ICollection collection = target;
            object[] parameters = new object[1]
            {
              enumerator.Current
            };
            if (!(flag = (bool) methodInfo.Invoke((object) collection, parameters)))
              break;
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
      return flag;
    }

    public static bool Remove(ICollection c, object obj)
    {
      bool flag = false;
      try
      {
        MethodInfo method = CollectionsSupport.GetMethod(c, "remove");
        if (method != null)
        {
          method.Invoke((object) c, new object[1]{ obj });
        }
        else
        {
          flag = (bool) CollectionsSupport.GetMethod(c, "Contains").Invoke((object) c, new object[1]
          {
            obj
          });
          CollectionsSupport.GetMethod(c, nameof (Remove)).Invoke((object) c, new object[1]
          {
            obj
          });
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
      return flag;
    }

    public static object[] ToArray(ICollection c)
    {
      int num = 0;
      object[] array = new object[c.Count];
      foreach (object obj in (IEnumerable) c)
        array[num++] = obj;
      return array;
    }

    public static object[] ToArray(ICollection c, object[] objects)
    {
      int num = 0;
      object[] instance = (object[]) Array.CreateInstance(objects.GetType().GetElementType(), c.Count);
      foreach (object obj in (IEnumerable) c)
        instance[num++] = obj;
      if (objects.Length >= c.Count)
        instance.CopyTo((Array) objects, 0);
      return instance;
    }

    public static void Copy(IList SourceList, IList TargetList)
    {
      for (int index = 0; index < SourceList.Count; ++index)
        TargetList[index] = SourceList[index];
    }

    public static void Fill(IList List, object Element)
    {
      for (int index = 0; index < List.Count; ++index)
        List[index] = Element;
    }

    public static void Shuffle(IList List)
    {
      Random RandomList = new Random((int) DateTime.Now.Ticks);
      CollectionsSupport.Shuffle(List, RandomList);
    }

    public static void Shuffle(IList List, Random RandomList)
    {
      for (int index1 = 0; index1 < List.Count; ++index1)
      {
        int index2 = RandomList.Next(List.Count);
        object obj = List[index1];
        List[index1] = List[index2];
        List[index2] = obj;
      }
    }

    public static string ToString(ICollection c)
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (c != null)
      {
        int num = 0;
        object[] objArray = new object[c.Count];
        foreach (object obj in (IEnumerable) c)
          objArray[num++] = obj;
        bool flag = c is BitArray || c is IDictionary || objArray.Length != 0 && objArray[0] is DictionaryEntry;
        for (int index = 0; index < objArray.Length; ++index)
        {
          object obj = objArray[index];
          if (obj == null)
            stringBuilder.Append("null");
          else if (obj is DictionaryEntry)
          {
            stringBuilder.Append(((DictionaryEntry) obj).Key);
            stringBuilder.Append("=");
            stringBuilder.Append(((DictionaryEntry) obj).Value);
          }
          else
            stringBuilder.Append(obj);
          if (index < objArray.Length - 1)
            stringBuilder.Append(", ");
        }
        if (flag)
        {
          stringBuilder.Insert(0, "{");
          stringBuilder.Append("}");
        }
        else
        {
          stringBuilder.Insert(0, "[");
          stringBuilder.Append("]");
        }
      }
      else
        stringBuilder.Insert(0, "null");
      return stringBuilder.ToString();
    }

    private class CompareCharValues : IComparer
    {
      public int Compare(object x, object y) => string.CompareOrdinal((string) x, (string) y);
    }
  }
}
