#if !SECURESTRING
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  /// <summary>
  /// A class for storing sensitive information (e.g. passwords)
  /// </summary>
  /// <remarks>
  /// The portable version of this class merely obfuscates the password in memory, it does not
  /// encrypt it or pin it like the version present in the full .Net framework compilation
  /// of the library
  /// </remarks>
  public sealed class SecureToken : IDisposable
  {
    private char[] _encoded;

    private const int Cipher = 35863;

    /// <summary>
    /// An anonymous function for processing a value by reference
    /// </summary>
    public delegate TR FuncRef<T1, TR>(ref T1 value);

    /// <summary>
    /// Gets the length of the token (in characters)
    /// </summary>
    public int Length { get { return _encoded.Length; } }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureToken"/> class
    /// </summary>
    public SecureToken(ArraySegment<byte> unencrypted)
    {
      var array = unencrypted.Array;
      FromBytes(ref array, unencrypted.Offset, unencrypted.Count);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureToken"/> class
    /// </summary>
    public SecureToken(ref byte[] unencrypted, int start, int length)
    {
      FromBytes(ref unencrypted, start, length);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureToken"/> class
    /// </summary>
    public SecureToken(ref string unencrypted)
    {
      var chars = unencrypted.ToCharArray();
      FromChars(ref chars);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureToken"/> class
    /// </summary>
    public SecureToken(ref char[] unencrypted)
    {
      FromChars(ref unencrypted);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureToken"/> class
    /// </summary>
    public SecureToken(Stream data)
    {
      try
      {
        using (var reader = new StreamReader(data))
        {
          var list = new List<char>();
          var value = reader.Read();
          while (value > 0)
          {
            list.Add((char)(value ^ Cipher));
            value = reader.Read();
          }
          _encoded = list.ToArray();
        }
      }
      finally
      {
      }
    }

    private void FromBytes(ref byte[] unencrypted, int start, int length)
    {
      var chars = new char[length];
      for (var i = start; i < start + length; i++)
      {
        chars[i] = (char)unencrypted[i];
      }
      FromChars(ref chars);
    }

    private void FromChars(ref char[] chars)
    {
      _encoded = new char[chars.Length];
      for (var i = 0; i < chars.Length; i++)
      {
        _encoded[i] = (char)(chars[i] ^ Cipher);
        chars[i] = '\0';
      }
    }

    /// <summary>
    /// Use the password as a string in a secure fashion
    /// </summary>
    public T UseString<T>(FuncRef<string, T> callback)
    {
      var chars = new char[_encoded.Length];
      string str;
      try
      {
        for (var i = 0; i < chars.Length; i++)
        {
          chars[i] = (char)(_encoded[i] ^ Cipher);
        }
        str = new string(chars);
        return callback(ref str);
      }
      finally
      {
        str = null;
        for (var i = 0; i < chars.Length; i++)
        {
          chars[i] = '\0';
        }
      }
    }

    /// <summary>
    /// Use the password as a string in a secure fashion
    /// </summary>
    public T UseBytes<T>(FuncRef<byte[], T> callback)
    {
      var chars = new char[_encoded.Length];
      byte[] data = null;
      try
      {
        for (var i = 0; i < chars.Length; i++)
        {
          chars[i] = (char)(_encoded[i] ^ Cipher);
        }
        data = Utils.AsciiGetBytes(chars);
        return callback(ref data);
      }
      finally
      {
        for (var i = 0; i < chars.Length; i++)
        {
          chars[i] = '\0';
        }
        if (data != null)
        {
          for (var i = 0; i < data.Length; i++)
          {
            data[i] = 0;
          }
        }
      }
    }

    /// <summary>
    /// Releases unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      for (var i = 0; i < _encoded.Length; i++)
      {
        _encoded[i] = '\0';
      }
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="String"/> to <see cref="SecureToken"/>.
    /// </summary>
    public static implicit operator SecureToken(string val)
    {
      return new SecureToken(ref val);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="ArraySegment{Byte}"/> to <see cref="SecureToken"/>.
    /// </summary>
    public static implicit operator SecureToken(ArraySegment<byte> val)
    {
      return new SecureToken(val);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Byte"/>[] to <see cref="SecureToken"/>.
    /// </summary>
    public static implicit operator SecureToken(byte[] val)
    {
      return new SecureToken(ref val, 0, val.Length);
    }
  }
}
#endif
