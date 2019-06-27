using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client
{
  internal class NontransactionalUploadCommand : UploadCommand
  {
    private IPromise<Stream> _lastPromise;

    public override string TransactionId
    {
      get { return null; }
      set { /*Do Nothing*/ }
    }

    public NontransactionalUploadCommand(Connection.IArasConnection conn, Vault vault) : base(conn, vault)
    {
    }

    public override IPromise<Stream> Commit(bool async)
    {
      if (Status != UploadStatus.Pending)
        return _lastPromise;

      Status = UploadStatus.Committed;
      _lastPromise = Promises.All(Files
        .Where(f => f.UploadPromise != null)
        .Select(f => f.UploadPromise)
        .ToArray())
        .Continue(l =>
        {
          var errorResult = l
            .Select(s => _conn.AmlContext.FromXml((Stream)s))
            .FirstOrDefault(r => r.Exception != null);
          if (errorResult == null)
          {
            return UploadAndApply(async);
          }
          else
          {
            var memStream = new MemoryStream();
            var xml = XmlWriter.Create(memStream);
            errorResult.ToAml(xml);
            memStream.Position = 0;
            return Promises.Resolved((Stream)memStream);
          }
        });
      return _lastPromise;
    }

    public override IPromise<Stream> Rollback(bool async)
    {
      if (Status == UploadStatus.RolledBack)
        return _lastPromise;

      Status = UploadStatus.RolledBack;
      _lastPromise = Promises.All(Files
        .Where(f => f.UploadPromise != null)
        .Select(f => f.UploadPromise.Continue(s =>
        {
          var result = _conn.AmlContext.FromXml(s);
          if (result.Exception != null)
            return Promises.Resolved((Stream)new MemoryStream());

          return _conn.Process(new Command("<Item type='File' action='delete' id='@0' />", f.Id), async);
        })).ToArray())
        .Convert(l => l.OfType<Stream>().FirstOrDefault() ?? new MemoryStream());
      return _lastPromise;
    }

    public override IPromise<string> UploadFile(string id, string path, Stream data, bool async, bool calcChecksum = true)
    {
      var aml = AddFile(id, path, data, true, calcChecksum);
      var files = new[] { Files.Single(f => f.Id == id) };
      files[0].UploadPromise = this.UploadAndApply("ApplyItem", aml, files, async);
      return files[0].UploadPromise
        .Convert(s => "<Item type='File' action='get' id='" + id + "' />");
    }
  }
}
