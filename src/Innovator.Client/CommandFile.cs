using System;
using System.Data.HashFunction;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml;

namespace Innovator.Client
{
  internal class CommandFile
  {
    private string _aml;
    private string _checksum;
    private Stream _data;
    private string _id;
    private long _length;
    private string _path;
#if FILEIO
    private string _filePath;
#endif

    public string Aml { get { return _aml; } }
    public string Id { get { return _id; } }
    public long Length { get { return _length; } }
    public string Path { get { return _path; } }
    public IPromise<Stream> UploadPromise { get; set; }

    public CommandFile(string id, string path, Stream data, string vaultId, bool isNew = true)
    {
      _id = id;

#if FILEIO
      var fileStream = data as FileStream;
      if (data == null || fileStream != null)
        _filePath = fileStream?.Name ?? path;
      _path = NormalizePath(path);
#else
      _path = path;
#endif

#if FILEIO
      if (!string.IsNullOrEmpty(_filePath))
      {
        _id = id;
        if (!File.Exists(_filePath)) throw new IOException("File " + _filePath + " does not exist");

        using (var file = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var mD = System.Security.Cryptography.MD5.Create())
        {
          _checksum = mD.ComputeHash(file).HexString().ToUpperInvariant();
        }

        _length = new FileInfo(_filePath).Length;
      }
      else
#endif
      if (data == null)
      {
        throw new NotSupportedException();
      }
      else
      {
        _data = data.Seekable();
        _checksum = MD5.ComputeHash(_data).ToUpperInvariant();
        _data.Position = 0;
        _length = _data.Length;
      }
      _aml = GetFileItem(id, path, vaultId, isNew);
    }

    private string GetFileItem(string id, string path, string vaultId, bool isNew)
    {
      var settings = new XmlWriterSettings
      {
        OmitXmlDeclaration = true,
        Indent = false
      };

      using (var writer = new StringWriter())
      using (var xml = XmlWriter.Create(writer, settings))
      {
        xml.WriteStartElement("Item");
        xml.WriteAttributeString("type", "File");
        xml.WriteAttributeString("action", isNew ? "add" : "edit");
        xml.WriteAttributeString("id", id);
        xml.WriteElementString("actual_filename", path);
        xml.WriteElementString("checkedout_path", GetDirectoryName(path));
        xml.WriteElementString("filename", GetFileName(path));
        xml.WriteElementString("checksum", _checksum);
        xml.WriteElementString("file_size", _length.ToString());

        xml.WriteStartElement("Relationships");
        xml.WriteStartElement("Item");
        xml.WriteAttributeString("type", "Located");
        if (isNew)
        {
          xml.WriteAttributeString("action", "add");
          xml.WriteAttributeString("id", Guid.NewGuid().ToString("N").ToUpperInvariant());
          xml.WriteElementString("file_version", "1");
          xml.WriteElementString("related_id", vaultId);
          xml.WriteElementString("source_id", id);
        }
        else
        {
          xml.WriteAttributeString("action", "merge");
          xml.WriteAttributeString("where", string.Format("[Located].[related_id]='{0}'", vaultId));
          xml.WriteElementString("related_id", vaultId);
        }
        xml.WriteEndElement();
        xml.WriteEndElement();

        xml.WriteEndElement();
        xml.Flush();
        writer.Flush();
        return writer.ToString();
      }
    }

    public HttpContent AsContent(Command cmd, IServerContext context, bool multipart)
    {
      HttpContent result;
#if FILEIO
      if (_data == null)
      {
        result = new SimpleContent(new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096), true);
      }
      else
      {
        _data.Position = 0;
        result = new SimpleContent(_data, false);
      }
#else
      _data.Position = 0;
      result = new SimpleContent(_data, false);
#endif

      var id = _id[0] == '@' ? cmd.Substitute(_id, context) : _id;
      var path = GetFileName(_path[0] == '@' ? cmd.Substitute(_path, context) : _path);
      if (multipart)
      {
        result.Headers.Add("Content-Disposition", string.Format("form-data; name=\"{0}\"; filename=\"{1}\"", id, path));
      }
      else
      {
        result.Headers.Add("Content-Disposition", "attachment; filename*=" + Encode5987(path));
        result.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", 0, _length - 1, _length));

        var hash = default(byte[]);

#if FILEIO
        if (_data == null)
        {
          hash = new xxHash(32).ComputeHash(new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096));
        }
        else
        {
          _data.Position = 0;
          hash = new xxHash(32).ComputeHash(_data);
          _data.Position = 0;
        }
#else
        _data.Position = 0;
        hash = new xxHash(32).ComputeHash(_data);
        _data.Position = 0;
#endif
        var hashStr = BitConverter.ToUInt32(hash, 0).ToString(CultureInfo.InvariantCulture);
        result.Headers.Add("Aras-Content-Range-Checksum", hashStr);
        result.Headers.Add("Aras-Content-Range-Checksum-Type", "xxHashAsUInt32AsDecimalString");
      }
      return result;
    }

    private static string GetDirectoryName(string path)
    {
      var idx = path.LastIndexOfAny(new char[] { '/', '\\' });
      if (idx >= 0)
        return path.Substring(0, idx);
      return string.Empty;
    }

    private static string GetFileName(string path)
    {
      var idx = path.LastIndexOfAny(new char[] { '/', '\\' });
      if (idx >= 0)
        return path.Substring(idx + 1);
      return path;
    }

#if FILEIO
    public static string NormalizePath(string path)
    {
      return System.IO.Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
    }
#endif

    private static string Encode5987(string input)
    {
      var stringBuilder = new StringBuilder("utf-8''");
      foreach (char c in input)
      {
        if (c > '\u007f')
        {
          foreach (var character in Encoding.UTF8.GetBytes(c.ToString()))
          {
            stringBuilder.Append('%').Append(character.ToString("x2"));
          }
        }
        else if (!_tokenChars[(int)c] || c == '*' || c == '\'' || c == '%')
        {
          stringBuilder.Append('%').Append(((int)c).ToString("x2"));
        }
        else
        {
          stringBuilder.Append(c);
        }
      }
      return stringBuilder.ToString();
    }

    private static readonly bool[] _tokenChars = new bool[128];

    static CommandFile()
    {
      for (int i = 33; i < 127; i++)
      {
        _tokenChars[i] = true;
      }
      _tokenChars[40] = false;
      _tokenChars[41] = false;
      _tokenChars[60] = false;
      _tokenChars[62] = false;
      _tokenChars[64] = false;
      _tokenChars[44] = false;
      _tokenChars[59] = false;
      _tokenChars[58] = false;
      _tokenChars[92] = false;
      _tokenChars[34] = false;
      _tokenChars[47] = false;
      _tokenChars[91] = false;
      _tokenChars[93] = false;
      _tokenChars[63] = false;
      _tokenChars[61] = false;
      _tokenChars[123] = false;
      _tokenChars[125] = false;
    }
  }
}
