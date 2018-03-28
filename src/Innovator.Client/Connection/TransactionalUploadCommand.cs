using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal class TransactionalUploadCommand : UploadCommand
  {
    private IPromise<string> _transactionId;
    private IPromise<Stream> _lastPromise;

    public override string TransactionId
    {
      get
      {
        if (_transactionId?.IsResolved == true)
          return _transactionId.Value;
        return null;
      }
      set
      {
        _transactionId = Promises.Resolved(value);
      }
    }

    public TransactionalUploadCommand(Connection.IArasConnection conn, Vault vault) : base(conn, vault)
    {

    }

    public override IPromise<Stream> Commit(bool async)
    {
      if (Status != UploadStatus.Pending)
        return _lastPromise;

      Status = UploadStatus.Committed;
      // No transaction has been started
      if (_transactionId == null && !Files.Any(f => f.UploadPromise != null))
      {
        _lastPromise = UploadAndApply(async);
      }
      else
      {
        var transactionId = default(string);
        _lastPromise = BeginTransaction(async)
          .Continue(t =>
          {
            transactionId = t;
            return Promises.All(Files
              .Select(f => f.UploadPromise ?? UploadFile(f, async))
              .ToArray());
          })
          .Continue(l =>
          {
            var aml = this.ToNormalizedAml(_conn.AmlContext.LocalizationContext);
            var content = new FormContent
            {
              {
                "XMLData",
                "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:i18n=\"http://www.aras.com/I18N\"><SOAP-ENV:Body><ApplyItem>"
                  + aml
                  + "</ApplyItem></SOAP-ENV:Body></SOAP-ENV:Envelope>"
              }
            };

            var req = new HttpRequest() { Content = content };
            _conn.SetDefaultHeaders(req.SetHeader);
            foreach (var ac in _conn.DefaultSettings)
            {
              ac.Invoke(req);
            }
            Settings?.Invoke(req);
            req.SetHeader("SOAPAction", "CommitTransaction");
            req.SetHeader("VAULTID", Vault.Id);
            req.SetHeader("transactionid", transactionId);

            var trace = new LogData(4
              , "Innovator: Execute vault query"
              , LogListener ?? Factory.LogListener
              , Parameters)
            {
              { "aras_url", _conn.MapClientUrl("../../Server") },
              { "database", _conn.Database },
              { "query", aml },
              { "soap_action", "CommitTransaction" },
              { "url", Vault.Url },
              { "user_id", _conn.UserId },
              { "vault_id", Vault.Id },
              { "version", _conn.Version }
            };
            return Vault.HttpClient.PostPromise(new Uri(Vault.Url), async, req, trace).Always(trace.Dispose);
          })
          .Convert(r => r.AsStream);
      }
      return _lastPromise;
    }

    public override IPromise<Stream> Rollback(bool async)
    {
      if (Status == UploadStatus.RolledBack)
        return _lastPromise;

      Status = UploadStatus.RolledBack;
      _lastPromise = BeginTransaction(async)
        .Continue(t => VaultApplyAction("RollbackTransaction", async))
        .Convert(r => r.AsStream);
      return _lastPromise;
    }

    public override IPromise<string> UploadFile(string id, string path, Stream data, bool async)
    {
      var file = new CommandFile(id, path, data, Vault.Id);
      Files.Add(file);
      file.UploadPromise = UploadFile(file, async);
      return file.UploadPromise.Convert(s => file.Aml);
    }

    private IPromise<Stream> UploadFile(CommandFile file, bool async)
    {
      return BeginTransaction(async)
        .Continue(t =>
        {
          var req = new HttpRequest()
          {
            Content = file.AsContent(this, _conn.AmlContext.LocalizationContext, false)
          };
          req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
          _conn.SetDefaultHeaders(req.SetHeader);

          foreach (var a in _conn.DefaultSettings)
          {
            a.Invoke(req);
          }
          Settings?.Invoke(req);
          req.SetHeader("SOAPAction", "UploadFile");
          req.SetHeader("VAULTID", Vault.Id);
          req.SetHeader("transactionid", t);

          var trace = new LogData(4
            , "Innovator: Upload File"
            , LogListener ?? Factory.LogListener)
          {
            { "aras_url", _conn.MapClientUrl("../../Server") },
            { "database", _conn.Database },
            { "soap_action", "UploadFile" },
            { "url", Vault.Url },
            { "user_id", _conn.UserId },
            { "vault_id", Vault.Id },
            { "version", _conn.Version }
          };
          var uri = new Uri(Vault.Url + "?fileId=" + file.Id);
          return Vault.HttpClient.PostPromise(uri, async, req, trace).Always(trace.Dispose);
        })
        .Convert(r => r.AsStream);
    }

    private IPromise<string> BeginTransaction(bool async)
    {
      if (_transactionId == null)
      {
        _transactionId = Vault.TransformUrl(_conn, async)
          .Continue(u => VaultApplyAction("BeginTransaction", async)
          .Convert(r => _conn.AmlContext.FromXml(r.AsStream).Value));
      }
      return _transactionId;
    }

    private IPromise<IHttpResponse> VaultApplyAction(string action, bool async)
    {
      var query = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" ><SOAP-ENV:Body><" + action + "></" + action + "></SOAP-ENV:Body></SOAP-ENV:Envelope>";
      var req = new HttpRequest()
      {
        Content = new SimpleContent(query, "text/xml")
      };
      _conn.SetDefaultHeaders(req.SetHeader);
      req.SetHeader("VAULTID", Vault.Id);
      if (_transactionId?.IsResolved == true)
        req.SetHeader("transactionid", _transactionId.Value);

      foreach (var a in _conn.DefaultSettings)
      {
        a.Invoke(req);
      }
      Settings?.Invoke(req);

      if (!string.IsNullOrEmpty(action))
        req.SetHeader("SOAPAction", action);

      var trace = new LogData(4
        , "Innovator: Execute vault query"
        , LogListener ?? Factory.LogListener)
      {
        { "aras_url", _conn.MapClientUrl("../../Server") },
        { "database", _conn.Database },
        { "query", query },
        { "soap_action", action },
        { "url", Vault.Url },
        { "user_id", _conn.UserId },
        { "vault_id", Vault.Id },
        { "version", _conn.Version }
      };
      return Vault.HttpClient.PostPromise(new Uri(Vault.Url), async, req, trace).Always(trace.Dispose);
    }
  }
}
