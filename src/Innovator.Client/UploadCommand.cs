using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Innovator.Client
{
  /// <summary>
  /// A request used for uploading files to the database
  /// </summary>
  public abstract class UploadCommand : Command
  {
    private readonly List<CommandFile> _files = new List<CommandFile>();
    /// <summary>
    /// Aras connection
    /// </summary>
    protected readonly Connection.IArasConnection _conn;

    /// <summary>
    /// The AML query.  If not explicitly set, it is built based on the files which have been added to the requested.
    /// </summary>
    public override string Aml
    {
      get
      {
        if (string.IsNullOrEmpty(base.Aml))
        {
          if (_files.Count == 1)
          {
            return _files[0].Aml;
          }
          else if (_files.Count > 1)
          {
            return "<AML>" + _files.GroupConcat("", f => f.Aml) + "</AML>";
          }
        }
        return base.Aml;
      }
      set
      {
        base.Aml = value;
      }
    }
    internal IList<CommandFile> Files { get { return _files; } }
    /// <summary>
    /// Gets the status of the upload operation.
    /// </summary>
    public UploadStatus Status { get; protected set; }
    internal Vault Vault { get; }
    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    /// <value>
    /// The transaction identifier.
    /// </value>
    public abstract string TransactionId { get; set; }

    /// <summary>
    /// Create an upload command with the specified vault metadata
    /// </summary>
    /// <param name="vault">Vault metadata</param>
    protected UploadCommand(Connection.IArasConnection conn, Vault vault)
    {
      Vault = vault;
      _conn = conn;
    }

    /// <summary>
    /// Add a file to the upload request
    /// </summary>
    /// <param name="path">Physical path of the file</param>
    /// <param name="isNew">Is this a new file being added to the database for the first time?</param>
    /// <returns>AML file string useful for building a larger AML statement</returns>
    public string AddFile(string path, bool isNew = true)
    {
      return AddFile(_conn.AmlContext.NewId(), path, null, isNew);
    }

    /// <summary>
    /// Add a file to the upload request
    /// </summary>
    /// <param name="id">Aras ID of the file</param>
    /// <param name="path">Physical path of the file</param>
    /// <param name="isNew">Is this a new file being added to the database for the first time?</param>
    /// <returns>AML file string useful for building a larger AML statement</returns>
    public string AddFile(string id, string path, bool isNew = true)
    {
      return AddFile(id, path, null, isNew);
    }

    /// <summary>
    /// Add a file to the upload request
    /// </summary>
    /// <param name="id">Aras ID of the file</param>
    /// <param name="path">Path (or pseudo path) of the file</param>
    /// <param name="data">Stream of data representing the file</param>
    /// <param name="isNew">Is this a new file being added to the database for the first time?</param>
    /// <returns>AML file string useful for building a larger AML statement</returns>
    public string AddFile(string id, string path, Stream data, bool isNew = true)
    {
      var file = new CommandFile(id, path, data, Vault.Id, isNew);
      _files.Add(file);
      return file.Aml;
    }

    /// <summary>
    /// Add a file to the request without specifying an ID
    /// </summary>
    /// <param name="path">Path (or pseudo path) of the file</param>
    /// <param name="data">Stream of data representing the file</param>
    /// <param name="isNew">Is this a new file being added to the database for the first time?</param>
    /// <returns>AML file string useful for building a larger AML statement</returns>
    public string AddFile(string path, Stream data, bool isNew = true)
    {
      return AddFile(_conn.AmlContext.NewId(), path, data, isNew);
    }

    /// <summary>
    /// Adds a file Item query to the request where the path to the file is specified as the actual_filename property
    /// </summary>
    /// <param name="query">Query to add to the request</param>
    public void AddFileQuery(string query)
    {
      using (var reader = new StringReader(query))
      using (var xml = XmlReader.Create(reader))
      {
        AddFileQuery(xml);
      }
    }

    /// <summary>
    /// Adds a file <c>Item</c> query to the request where the path to the file is specified as the
    /// <c>actual_filename</c> property, and possibly the <c>actual_data</c> property.
    /// </summary>
    /// <param name="query">Query to add to the request</param>
    public void AddFileQuery(XmlReader query)
    {
      var elem = XElement.Load(query);
      var files = elem.DescendantsAndSelf("Item")
        .Where(e => e.Attributes("type").Any(a => a.Value == "File")
          && e.Elements("actual_filename").Any(p => !string.IsNullOrEmpty(p.Value))
          && e.Attributes("id").Any(p => !string.IsNullOrEmpty(p.Value))
          && e.Attributes("action").Any(p => !string.IsNullOrEmpty(p.Value)));
      XElement newElem = null;
      foreach (var file in files.ToList())
      {
        var dataElem = file.Element("actual_data");
        if (dataElem == null)
        {
          newElem = XElement.Parse(AddFile(
            file.Attribute("id").Value,
            file.Element("actual_filename").Value,
            file.Attribute("action").Value == "add" || file.Attribute("action").Value == "create"));
        }
        else
        {
          var encoding = dataElem.Attribute("encoding");
          var data = encoding != null && string.Equals(encoding.Value, "base64", StringComparison.OrdinalIgnoreCase)
            ? Convert.FromBase64String(dataElem.Value)
            : Encoding.UTF8.GetBytes(dataElem.Value);
          var stream = new MemoryStream(data);
          newElem = XElement.Parse(AddFile(
            file.Attribute("id").Value,
            file.Element("actual_filename").Value,
            stream,
            file.Attribute("action").Value == "add" || file.Attribute("action").Value == "create"));
        }
        if (file.Parent != null)
        {
          MergeIfMissing(newElem, file);
          newElem = null;
        }
      }
      base.AddAml(newElem != null ? newElem.ToString() : elem.ToString());
    }

    /// <summary>
    /// Add a file to the upload request
    /// </summary>
    /// <param name="id">Aras ID of the file</param>
    /// <param name="path">Path (or pseudo path) of the file</param>
    /// <param name="data">Stream of data representing the file</param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>AML file string useful for building a larger AML statement</returns>
    public abstract IPromise<string> UploadFile(string id, string path, Stream data, bool async);

    /// <summary>
    /// Add a file to the request without specifying an ID
    /// </summary>
    /// <param name="path">Path (or pseudo path) of the file</param>
    /// <param name="data">Stream of data representing the file</param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>AML file string useful for building a larger AML statement</returns>
    public IPromise<string> UploadFile(string path, Stream data, bool async)
    {
      return UploadFile(_conn.AmlContext.NewId(), path, data, async);
    }

    /// <summary>
    /// Adds a file Item query to the request where the path to the file is specified as the actual_filename property
    /// </summary>
    /// <param name="query">Query to add to the request</param>
    public IPromise UploadFileQuery(string query, bool async)
    {
      using (var reader = new StringReader(query))
      using (var xml = XmlReader.Create(reader))
      {
        return UploadFileQuery(xml, async);
      }
    }

    /// <summary>
    /// Adds a file <c>Item</c> query to the request where the path to the file is specified as the
    /// <c>actual_filename</c> property, and possibly the <c>actual_data</c> property.
    /// </summary>
    /// <param name="query">Query to add to the request</param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    public IPromise UploadFileQuery(XmlReader query, bool async)
    {
      var elem = XElement.Load(query);
      var files = elem.DescendantsAndSelf("Item")
        .Where(e => e.Attributes("type").Any(a => a.Value == "File")
          && e.Elements("actual_filename").Any(p => !string.IsNullOrEmpty(p.Value))
          && e.Attributes("id").Any(p => !string.IsNullOrEmpty(p.Value))
          && e.Attributes("action").Any(p => !string.IsNullOrEmpty(p.Value)));

      return Promises.All(files.Select(file =>
      {
        var dataElem = file.Element("actual_data");
        IPromise<string> promise;
        if (dataElem == null)
        {
          promise = UploadFile(file.Attribute("id").Value, file.Element("actual_filename").Value
            , null, async);
        }
        else
        {
          var encoding = dataElem.Attribute("encoding");
          var data = encoding != null && string.Equals(encoding.Value, "base64", StringComparison.OrdinalIgnoreCase)
            ? Convert.FromBase64String(dataElem.Value)
            : Encoding.UTF8.GetBytes(dataElem.Value);
          var stream = new MemoryStream(data);

          promise = UploadFile(file.Attribute("id").Value, file.Element("actual_filename").Value
            , stream, async);
        }

        return promise.Convert(xml =>
        {
          var newElem = XElement.Parse(xml);
          if (file.Parent != null)
          {
            MergeIfMissing(newElem, file);
            return null;
          }
          return newElem;
        });
      }).ToArray())
        .Done(l =>
        {
          var newElem = l.OfType<XElement>().FirstOrDefault(e => e != null);
          base.AddAml(newElem != null ? newElem.ToString() : elem.ToString());
        });
    }

    /// <summary>
    /// Commits the files to the vault and the AML to the database.
    /// </summary>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>A promise to return an XML SOAP response as a <see cref="System.IO.Stream"/></returns>
    public abstract IPromise<Stream> Commit(bool async);

    /// <summary>
    /// Rollbacks the specified transaction.
    /// </summary>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>A promise to return an XML SOAP response as a <see cref="System.IO.Stream"/></returns>
    public abstract IPromise<Stream> Rollback(bool async);

    /// <summary>
    /// Uploads the files and applies the AML in a single transaction
    /// </summary>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>A promise to return an XML SOAP response as a <see cref="System.IO.Stream"/></returns>
    protected IPromise<Stream> UploadAndApply(bool async)
    {
      return UploadAndApply(ActionString
        , ToNormalizedAml(_conn.AmlContext.LocalizationContext)
        , Files.Where(f => f.UploadPromise == null)
        , async);
    }

    /// <summary>
    /// Uploads the files and applies the AML in a single transaction
    /// </summary>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>A promise to return an XML SOAP response as a <see cref="System.IO.Stream"/></returns>
    internal IPromise<Stream> UploadAndApply(string soapAction, string aml, IEnumerable<CommandFile> files, bool async)
    {
      return Vault.TransformUrl(_conn, async).Continue(u =>
      {
        // Compile the headers and AML query into the appropriate content
        var content = new FormContent();
        _conn.SetDefaultHeaders(content.Add);
        content.Add("SOAPACTION", soapAction);
        content.Add("VAULTID", Vault.Id);
        content.Add("XMLdata", "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:i18n=\"http://www.aras.com/I18N\"><SOAP-ENV:Body><ApplyItem>" +
                                  aml +
                                  "</ApplyItem></SOAP-ENV:Body></SOAP-ENV:Envelope>");
        foreach (var file in files)
        {
          content.Add(file.AsContent(this, _conn.AmlContext.LocalizationContext, true));
        }
        content.Compression = _conn.Compression;

        var req = new HttpRequest() { Content = content };
        foreach (var ac in _conn.DefaultSettings)
        {
          ac.Invoke(req);
        }
        Settings?.Invoke(req);

        req.Headers.TransferEncodingChunked = true;

        var trace = new LogData(4
          , "Innovator: Execute query"
          , LogListener ?? Factory.LogListener
          , Parameters)
        {
          { "aras_url", _conn.MapClientUrl("../../Server") },
          { "database", _conn.Database },
          { "query", aml },
          { "soap_action", soapAction },
          { "url", Vault.Url },
          { "user_id", _conn.UserId },
          { "vault_id", Vault.Id },
          { "version", _conn.Version }
        };
        return Vault.HttpClient.PostPromise(new Uri(Vault.Url), async, req, trace).Always(trace.Dispose);
      }).Convert(r => r.AsStream);
    }

    private void MergeIfMissing(XElement source, XElement target)
    {
      XElement targetElem;
      foreach (var elem in source.Elements())
      {
        targetElem = target.Element(elem.Name);
        if (targetElem == null)
        {
          target.Add(elem);
        }
        else
        {
          MergeIfMissing(elem, targetElem);
        }
      }
    }
  }
}
