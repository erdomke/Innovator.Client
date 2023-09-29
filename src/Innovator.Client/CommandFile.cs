using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client
{
  internal class CommandFile
  {
    private readonly string _aml;
    private readonly string _checksum;
    private readonly Stream _data;
    private readonly string _id;
    private readonly long? _length;
    private readonly string _path;
#if FILEIO
    private readonly string _filePath;
#endif

    public string Aml { get { return _aml; } }
    public string Id { get { return _id; } }
    public long Length { get { return _length ?? -1; } }
    public string Path { get { return _path; } }
    public IPromise<Stream> UploadPromise { get; set; }

    public CommandFile(string id, string path, Stream data, string vaultId, bool isNew = true, bool calcChecksum = true)
    {
      _id = id ?? ElementFactory.Local.NewId();

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
        _data = calcChecksum ? data.Seekable() : data;
        if (calcChecksum)
        {
          _checksum = MD5.ComputeHash(_data).ToUpperInvariant();
          _data.Position = 0;
        }
        _length = _data.Length;
      }
      _aml = GetFileItem(_id, path, vaultId, isNew);
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
        if (!string.IsNullOrEmpty(_checksum))
          xml.WriteElementString("checksum", _checksum);
        if (_length.HasValue)
          xml.WriteElementString("file_size", _length.Value.ToString());

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

    public IEnumerable<HttpContent> AsContent(Command cmd, IServerContext context, bool multipart)
    {
      var stream = _data;
      if (stream?.CanSeek == true)
        stream.Position = 0;
      var disposeLast = false;
#if FILEIO
      if (stream == null)
      {
        stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096);
        disposeLast = true;
      }
#endif

      const long chunkSize = 16 * 1024 * 1024;
      var numChunks = _length.HasValue && !multipart ? (int)Math.Ceiling((double)_length / chunkSize) : 1;
      var results = new HttpContent[numChunks];

      for (var i = 0; i < numChunks; i++)
      {
        var chunkLength = _length;
        if (_length.HasValue && numChunks > 1)
        {
          chunkLength = Math.Min(chunkSize, _length.Value - i * chunkSize);
          results[i] = new FileStreamContent(stream, chunkLength.Value, disposeLast && i == numChunks - 1);
        }
        else
        {
          results[i] = new SimpleContent(stream, disposeLast);
        }

        var id = _id[0] == '@' ? cmd.Substitute(_id, context) : _id;
        var path = GetFileName(_path[0] == '@' ? cmd.Substitute(_path, context) : _path);
        if (multipart)
        {
          results[i].Headers.Add("Content-Disposition", string.Format("form-data; name=\"{0}\"; filename=\"{1}\"", id, path));
        }
        else
        {
          results[i].Headers.Add("Content-Disposition", "attachment; filename*=" + Encode5987(path));
          if (chunkLength.HasValue && chunkLength.Value > 0)
            results[i].Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", i * chunkSize, i * chunkSize + chunkLength - 1, _length));

          if (numChunks == 1)
          {
            var hash = default(byte[]);
#if FILEIO
            if (_data == null)
            {
              using (var s = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096))
                hash = new xxHash(32).ComputeHash(s);
            }
            else if (_data.CanSeek)
            {
              _data.Position = 0;
              hash = new xxHash(32).ComputeHash(_data);
              _data.Position = 0;
            }
#else
            if (_data.CanSeek)
            {
              _data.Position = 0;
              hash = new xxHash(32).ComputeHash(_data);
              _data.Position = 0;
            }
#endif
            if (hash != null)
            {
              var hashStr = BitConverter.ToUInt32(hash, 0).ToString(CultureInfo.InvariantCulture);
              results[i].Headers.Add("Aras-Content-Range-Checksum", hashStr);
              results[i].Headers.Add("Aras-Content-Range-Checksum-Type", "xxHashAsUInt32AsDecimalString");
            }
          }
        }
      }

      return results;
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
        else if (!_tokenChars[c] || c == '*' || c == '\'' || c == '%')
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

    private class FileStreamContent : StreamContent, ISyncContent
    {
      private readonly Stream _stream;
      private readonly long _length;
      private readonly bool _doDispose;
      private const int bufferSize = 81920;

      public FileStreamContent(Stream stream, long length, bool doDispose) : base(stream)
      {
        _stream = stream;
        _length = length;
        _doDispose = doDispose;
      }

      protected override void Dispose(bool disposing)
      {
        try
        {
          if (disposing && _doDispose)
          {
            // The base call will dispose the stream
            base.Dispose(disposing);
          }
        }
        catch (Exception) { }
      }

      protected override bool TryComputeLength(out long length)
      {
        length = _length;
        return true;
      }

#if TASKS
      protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
      {
        if (_length <= 0)
          return;

        var buffer = new byte[bufferSize];
        var bytesRead = 0;
        var totalRead = 0;
        while ((bytesRead = await _stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, (int)(_length - totalRead))).ConfigureAwait(false)) != 0)
        {
          await stream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
          totalRead += bytesRead;
        }
      }
#else
      protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
      {
        var factory = new TaskCompletionSource<bool>();
        factory.SetResult(true);
        if (_length <= 0)
          return factory.Task;

        var buffer = new byte[bufferSize];
        var bytesRead = 0;
        var totalRead = 0;
        while ((bytesRead = _stream.Read(buffer, 0, Math.Min(buffer.Length, (int)(_length - totalRead)))) != 0)
        {
          stream.Write(buffer, 0, bytesRead);
          totalRead += bytesRead;
        }
        return factory.Task;
      }
#endif

      public void SerializeToStream(Stream stream)
      {
        if (_length <= 0)
          return;

        var buffer = new byte[bufferSize];
        var bytesRead = 0;
        var totalRead = 0;
        while ((bytesRead = _stream.Read(buffer, 0, Math.Min(buffer.Length, (int)(_length - totalRead)))) != 0)
        {
          stream.Write(buffer, 0, bytesRead);
          totalRead += bytesRead;
        }
      }
    }
  }
}
