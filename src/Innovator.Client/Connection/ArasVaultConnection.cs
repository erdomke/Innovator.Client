using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Innovator.Client.Connection
{
  /// <summary>
  /// Class for uploading files to and downloading files from an Aras vault
  /// </summary>
  public class ArasVaultConnection
  {
    private const string DownloadFileAmlFormat = @"<Item type='File' action='get' select='id,filename' id='@0'>
                                                    <Relationships>
                                                      <Item type='Located' select='id,related_id,file_version' action='get'>
                                                        <related_id>
                                                          <Item type='Vault' select='id,vault_url' action='get'></Item>
                                                        </related_id>
                                                      </Item>
                                                    </Relationships>
                                                  </Item>";

    private readonly IArasConnection _conn;
    private IVaultStrategy _vaultStrategy = new DefaultVaultStrategy();
    private readonly IVaultFactory _factory = new DefaultVaultFactory();

    /// <summary>
    /// Gets or sets the vault strategy for determining which vaults to read from and write
    /// to for a given <see cref="IAsyncConnection"/>
    /// </summary>
    /// <value>
    /// The vault strategy.
    /// </value>
    public IVaultStrategy VaultStrategy
    {
      get { return _vaultStrategy; }
      set { _vaultStrategy = new CacheVaultStrategy(value); }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArasVaultConnection"/> class for 
    /// a given <see cref="IArasConnection"/>
    /// </summary>
    /// <param name="conn">The corresponding <see cref="IArasConnection"/>.</param>
    public ArasVaultConnection(IArasConnection conn)
    {
      _conn = conn;
    }

    /// <summary>
    /// Initializes the strategy.
    /// </summary>
    public void InitializeStrategy()
    {
      _vaultStrategy.Initialize(_conn, _factory);
    }

    /// <summary>
    /// Downloads the specified file.
    /// </summary>
    /// <param name="request">The file to download.</param>
    /// <param name="async">if set to <c>true</c>, download asynchronously.  Otherwise, 
    /// a resolved promise will be returned</param>
    /// <returns>A promise to return a stream of the file's bytes</returns>
    public IPromise<Stream> Download(Command request, bool async)
    {
      var parsedAml = _conn.AmlContext.FromXml(request);
      var file = (IReadOnlyItem)parsedAml.AssertItem("File");
      var hasVaults = file.Relationships("Located")
                          .Select(r => r.RelatedId().AsItem())
                          .Any(i => i.Exists);
      var filePromise = hasVaults ?
        Promises.Resolved(file) :
        _conn.ItemByQuery(new Command(DownloadFileAmlFormat, file.Id()), async);

      // Get the file data and user data as necessary
      return Promises
        .All(filePromise, _vaultStrategy.ReadPriority(async))
        .Continue(o =>
        {
          // Get the correct vault for the file
          var vault = GetReadVaultForFile(o.Result1, o.Result2);
          if (vault == null) throw new InvalidOperationException("Vault location of the file is unknown");

          // Download the file
          return DownloadFileFromVault(o.Result1, vault, async, request);
        })
        .Convert(r => r.AsStream);
    }

    private Vault GetReadVaultForFile(IReadOnlyItem file, IEnumerable<Vault> readPriority)
    {
      var located = file.Relationships("Located").ToList();
      if (located.Count < 1)
      {
        return null;
      }
      else if (located.Count == 1)
      {
        return _factory.GetVault(located[0].RelatedId().AsItem());
      }
      else
      {
        // The maximum file version
        var maxVersion = located.Select(l => l.Property("file_version").AsInt(-1)).Max();
        if (maxVersion < 0) return _factory.GetVault(located[0].RelatedId().AsItem());
        located = located.Where(l => l.Property("file_version").AsInt(-1) == maxVersion).ToList();
        if (located.Count == 1) return _factory.GetVault(located[0].RelatedId().AsItem());

        // The vault sortOrders
        var vaultSort = new Dictionary<string, int>();
        var vaultList = readPriority.ToList();
        // Looping in reverse order to ensure that the minimum number is stored
        for (int i = vaultList.Count - 1; i >= 0; i--)
        {
          vaultSort[vaultList[i].Id] = i;
        }

        // Sort based on the vault priorities
        located.Sort((x, y) =>
        {
          int xSort, ySort;
          if (!vaultSort.TryGetValue(x.RelatedId().Value, out xSort)) xSort = 999999;
          if (!vaultSort.TryGetValue(y.RelatedId().Value, out ySort)) ySort = 999999;

          int compare = xSort.CompareTo(ySort);
          if (compare == 0) compare = x.Id().CompareTo(y.Id());
          return compare;
        });

        return _factory.GetVault(located.First(l => l.Property("file_version").AsInt(-1) == maxVersion).RelatedId().AsItem());
      }
    }

    private IPromise<IHttpResponse> DownloadFileFromVault(IReadOnlyItem fileItem, Vault vault, bool async, Command request)
    {
      var url = vault.Url;
      if (string.IsNullOrEmpty(url)) return null;

      var urlPromise = url.IndexOf("$[") < 0 ?
        Promises.Resolved(url) :
        _conn.Process(new Command("<url>@0</url>", url)
                .WithAction(CommandAction.TransformVaultServerURL), async)
                .Convert(s => s.AsString());

      return urlPromise.Continue(u =>
      {
        if (u != vault.Url) vault.Url = u;
        var uri = new Uri(string.Format("{0}?dbName={1}&fileId={2}&fileName={3}&vaultId={4}",
          u, _conn.Database, fileItem.Id(),
          Uri.EscapeDataString(fileItem.Property("filename").Value),
          vault.Id));

        var client = new SyncHttpClient();
        var req = new HttpRequest();
        _conn.SetDefaultHeaders((k, v) => { req.SetHeader(k, v); });
        req.SetHeader("VAULTID", vault.Id);
        foreach (var a in _conn.DefaultSettings)
        {
          a.Invoke(req);
        }
        request.Settings?.Invoke(req);

        var trace = new LogData(4
          , "Innovator: Download file from vault"
          , request.LogListener ?? Factory.LogListener
          , request.Parameters)
        {
          { "aras_url", _conn.MapClientUrl("../../Server") },
          { "database", _conn.Database },
          { "file_id", fileItem.Id() },
          { "filename", fileItem.Property("filename").Value },
          { "query", request.Aml },
          { "url", uri },
          { "user_id", _conn.UserId },
          { "vault_id", vault.Id },
          { "version", _conn.Version }
        };
        return client.GetPromise(uri, async, trace, req).Always(trace.Dispose);
      });
    }

    /// <summary>
    /// Uploads the specified file to the vault.
    /// </summary>
    /// <param name="upload">The file to upload.</param>
    /// <param name="async">if set to <c>true</c>, download asynchronously.  Otherwise, 
    /// a resolved promise will be returned</param>
    /// <returns>A promise to return a stream of the resulting AML</returns>
    public IPromise<Stream> Upload(UploadCommand upload, bool async)
    {
      // Transform the vault URL (as necessary)
      var urlPromise = upload.Vault.Url.IndexOf("$[") < 0 ?
        Promises.Resolved(upload.Vault.Url) :
        _conn.Process(new Command("<url>@0</url>", upload.Vault.Url)
                .WithAction(CommandAction.TransformVaultServerURL), async)
                .Convert(s => s.AsString());
        //GetResult("TransformVaultServerURL", "<url>" + upload.Vault.Url + "</url>", async);

      return urlPromise.Continue(u =>
      {
        // Determine the authentication used by the vault
        if (u != upload.Vault.Url) upload.Vault.Url = u;

        // Compile the headers and AML query into the appropriate content
        var content = new FormContent();
        _conn.SetDefaultHeaders(content.Add);
        content.Add("SOAPACTION", upload.Action.ToString());
        content.Add("VAULTID", upload.Vault.Id);
        content.Add("XMLdata", "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:i18n=\"http://www.aras.com/I18N\"><SOAP-ENV:Body><ApplyItem>" +
                                  upload.ToNormalizedAml(_conn.AmlContext.LocalizationContext) +
                                  "</ApplyItem></SOAP-ENV:Body></SOAP-ENV:Envelope>");
        foreach (var file in upload.Files)
        {
          content.Add(file.AsContent(upload, _conn.AmlContext.LocalizationContext));
        }
        content.Compression = _conn.Compression;

        var req = new HttpRequest() { Content = content };
        foreach (var ac in _conn.DefaultSettings)
        {
          ac.Invoke(req);
        }
        if (upload.Settings != null) upload.Settings.Invoke(req);
        req.Headers.TransferEncodingChunked = true;

        var handler = new SyncClientHandler();
        handler.CookieContainer = upload.Vault.Cookies;
        var http = new SyncHttpClient(handler);

        var trace = new LogData(4
          , "Innovator: Execute query"
          , upload.LogListener ?? Factory.LogListener
          , upload.Parameters)
        {
          { "aras_url", _conn.MapClientUrl("../../Server") },
          { "database", _conn.Database },
          { "query", upload.Aml },
          { "soap_action", upload.ActionString },
          { "url", upload.Vault.Url },
          { "user_id", _conn.UserId },
          { "vault_id", upload.Vault.Id },
          { "version", _conn.Version }
        };
        return http.PostPromise(new Uri(upload.Vault.Url), async, req, trace).Always(trace.Dispose);
      }).Convert(r => r.AsStream);
    }
  }
}
