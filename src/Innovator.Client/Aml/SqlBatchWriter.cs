using System;
using System.IO;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// Class for building a large string of SQL which will be sent to the database in batches for
  /// execution using the ApplySQL action
  /// </summary>
  /// <example>
  /// For an example of inserting multiple records into the database, consider the following
  /// <code lang="C#">
  /// var sql = new SqlBatchWriter(conn);
  /// for (var i = 0; i &lt; 30000; i++)
  /// {
  ///   sql.Command("insert into innovator._numbers (num) values (@0)", i);
  /// }
  /// </code>
  /// </example>
  public class SqlBatchWriter : IDisposable
  {
    private readonly ElementFactory _aml;
    private readonly IConnection _conn;
    private readonly StringBuilder _builder;
    private readonly ParameterSubstitution _subs;
    private int _commands = 0;
    private string _lastQuery;
    private IPromise<Stream> _lastResult = null;

    /// <summary>
    /// Number of commands at which to send the query to the database
    /// </summary>
    public int Threshold { get; set; }

    /// <summary>Instantiate the writer</summary>
    public SqlBatchWriter() : this(96) { }

    /// <summary>Instantiate the writer with an initial capacity for the internal <see cref="StringBuilder"/></summary>
    /// <param name="capacity"><see cref="StringBuilder"/> initial capacity</param>
    public SqlBatchWriter(int capacity)
    {
      _aml = ElementFactory.Utc;
      _subs = new ParameterSubstitution()
      {
        Mode = ParameterSubstitutionMode.Sql
      };
      this.Threshold = 3000;
      _builder = new StringBuilder(capacity);
    }

    /// <summary>Instantiate the writer with a connection</summary>
    /// <param name="conn">Server connection</param>
    public SqlBatchWriter(IConnection conn) : this(conn, 96) { }

    /// <summary>Instantiate the writer with a connection and an initial capacity for the internal <see cref="StringBuilder"/></summary>
    /// <param name="conn">Server connection</param>
    /// <param name="capacity"><see cref="StringBuilder"/> initial capacity</param>
    public SqlBatchWriter(IConnection conn, int capacity) : this(capacity)
    {
      _conn = conn;
      _builder.Append("<sql>");
    }

    /// <summary>Append a new line (empty command) to the SQL</summary>
    /// <remarks>Depending on the number of commands written and the <see cref="Threshold"/>, the
    /// buffer of SQL commands might be sent to the server after this call</remarks>
    public SqlBatchWriter Command()
    {
      _builder.AppendLine();
      ProcessCommand(false);
      return this;
    }

    /// <summary>Append the specified command to the SQL</summary>
    /// <param name="value">SQL command to execute</param>
    /// <remarks>Depending on the number of commands written and the <see cref="Threshold"/>, the
    /// buffer of SQL commands might be sent to the server after this call</remarks>
    public SqlBatchWriter Command(string value)
    {
      if (_conn == null)
      {
        _builder.AppendLine(value);
      }
      else
      {
        _builder.AppendEscapedXml(value).AppendLine();
        ProcessCommand(false);
      }
      return this;
    }
    /// <summary>Append the specified command with parameters the SQL. @# (e.g. @0) style 
    /// parameters are replaced</summary>
    /// <remarks>Depending on the number of commands written and the <see cref="Threshold"/>, the
    /// buffer of SQL commands might be sent to the server after this call. See 
    /// <see cref="Innovator.Client.Command"/> and <see cref="ParameterSubstitution"/> for more 
    /// information on how parameters are substituted</remarks>
    public SqlBatchWriter Command(string format, params object[] args)
    {
      _subs.AddIndexedParameters(args);
      var value = _subs.Substitute(format, _aml.LocalizationContext);
      if (_conn == null)
      {
        _builder.AppendLine(value);
      }
      else
      {
        _builder.AppendEscapedXml(value).AppendLine();
        ProcessCommand(false);
      }
      _subs.ClearParameters();
      return this;
    }

    /// <summary>Append the specified command with parameters the SQL. @# (e.g. @0) style 
    /// parameters are replaced</summary>
    /// <remarks>Depending on the number of commands written and the <see cref="Threshold"/>, the
    /// buffer of SQL commands might be sent to the server after this call. See 
    /// <see cref="Innovator.Client.Command"/> and <see cref="ParameterSubstitution"/> for more 
    /// information on how parameters are substituted</remarks>
    public SqlBatchWriter Command(IFormattable formattable)
    {
      var format = formattable.ToString(null, _subs);
      var value = _subs.Substitute(format, _aml.LocalizationContext);
      if (_conn == null)
      {
        _builder.AppendLine(value);
      }
      else
      {
        _builder.AppendEscapedXml(value).AppendLine();
        ProcessCommand(false);
      }
      _subs.ClearParameters();
      return this;
    }

    /// <summary>Append a part of a command to the SQL</summary>
    /// <remarks>No SQL will be sent to the server until the SQL "part" has been finished with a 
    /// call to <see cref="SqlBatchWriter.Command()"/> (or one of the overloads)</remarks>
    public SqlBatchWriter Part(string value)
    {
      if (_conn == null)
        _builder.Append(value);
      else
        _builder.AppendEscapedXml(value);
      return this;
    }

    /// <summary>Append the specified command with parameters the SQL. @# (e.g. @0) style 
    /// parameters are replaced</summary>
    /// <remarks>No SQL will be sent to the server until the SQL "part" has been finished with a 
    /// call to <see cref="SqlBatchWriter.Command()"/> (or one of the overloads). See 
    /// <see cref="Innovator.Client.Command"/> and <see cref="ParameterSubstitution"/> for more 
    /// information on how parameters are substituted</remarks>
    public SqlBatchWriter Part(string format, params object[] args)
    {
      _subs.AddIndexedParameters(args);
      var value = _subs.Substitute(format, _aml.LocalizationContext);
      if (_conn == null)
        _builder.Append(value);
      else
        _builder.AppendEscapedXml(value);
      _subs.ClearParameters();
      return this;
    }

    /// <summary>Append the specified command with parameters the SQL. @# (e.g. @0) style 
    /// parameters are replaced</summary>
    /// <remarks>No SQL will be sent to the server until the SQL "part" has been finished with a 
    /// call to <see cref="SqlBatchWriter.Command()"/> (or one of the overloads). See 
    /// <see cref="Innovator.Client.Command"/> and <see cref="ParameterSubstitution"/> for more 
    /// information on how parameters are substituted</remarks>
    public SqlBatchWriter Part(IFormattable formattable)
    {
      var format = formattable.ToString(null, _subs);
      var value = _subs.Substitute(format, _aml.LocalizationContext);
      if (_conn == null)
        _builder.Append(value);
      else
        _builder.AppendEscapedXml(value);
      _subs.ClearParameters();
      return this;
    }

    private void ProcessCommand(bool force)
    {
      _commands++;
      if ((force || _commands > this.Threshold) && _builder.Length > 5)
      {
        // Execute the query
        _builder.Append("</sql>");
        WaitLastResult();
        _lastQuery = _builder.ToString();

        // Run either synchronously or asynchronously based on what's currently supported
        var asyncConn = _conn as IAsyncConnection;
        if (asyncConn == null)
        {
          _conn.Apply(new Command(_lastQuery).WithAction(CommandAction.ApplySQL)).AssertNoError();
        }
        else
        {
          _lastResult = asyncConn.Process(new Command(_lastQuery).WithAction(CommandAction.ApplySQL), true);
        }

        // Reset the state
        _builder.Length = 0;
        _builder.Append("<sql>");
        _commands = 0;
      }
    }

    private void WaitLastResult()
    {
      if (_lastResult != null)
      {
        _conn.AmlContext.FromXml(_lastResult.Wait(), _lastQuery, _conn).AssertNoError();
        _lastResult = null;
      }
    }

    /// <summary>
    /// Render the current buffer to a string
    /// </summary>
    public override string ToString()
    {
      return _builder + (_conn == null ? "" : "</sql>");
    }

    /// <summary>
    /// Send the current buffer to the database
    /// </summary>
    public void Flush()
    {
      if (_conn != null)
      {
        ProcessCommand(true);
        WaitLastResult();
      }
    }

    /// <summary>
    /// Send the current buffer to the database
    /// </summary>
    public void Dispose()
    {
      this.Flush();
    }
  }
}
