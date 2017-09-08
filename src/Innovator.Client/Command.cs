using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Innovator.Client
{
  /// <summary>
  /// An AML request to be submitted to the server
  /// </summary>
  /// <remarks>
  /// <para>In most situations, you should never need to interact with the <see cref="Command"/> class
  /// directly since a <see cref="string"/> is implicitly promoted to a <see cref="Command"/>.  For
  /// example, with the <see cref="ConnectionExtensions.Apply(IConnection, Command, object[])"/>, 
  /// you can use code similar to</para>
  /// <code lang="C#">
  /// // Get preliminary parts which have existed for a little bit of time
  /// var components = conn.Apply(@"&lt;Item type='Part' action='get'&gt;
  ///   &lt;classification&gt;@0&lt;/classification&gt;
  ///   &lt;created_on condition='lt'&gt;@1&lt;/created_on&gt;
  ///   &lt;state&gt;Preliminary&lt;/state&gt;
  /// &lt;/Item&gt;", classification, DateTime.Now.AddMinutes(-20)).Items();
  /// </code>
  /// <para>Behind the scenes, this is equivalent to the code </para>
  /// <code lang="C#">
  /// // Get preliminary parts which have existed for a little bit of time
  /// var components = conn.Apply(new Command(@"&lt;Item type='Part' action='get'&gt;
  ///   &lt;classification&gt;@0&lt;/classification&gt;
  ///   &lt;created_on condition='lt'&gt;@1&lt;/created_on&gt;
  ///   &lt;state&gt;Preliminary&lt;/state&gt;
  /// &lt;/Item&gt;", classification, DateTime.Now.AddMinutes(-20))).Items();
  /// </code>
  /// <para>So why would you need to create a <see cref="Command"/>?  Perhaps, you need to specify
  /// the SOAP action to use</para>
  /// <code lang="C#">
  /// var cmd = new Command(@"&lt;AML&gt;
  ///   &lt;Item type = 'Part' action='get' id='30F66F086BDE4032B29034FC7D84A99D' /&gt;
  ///   &lt;Item type = 'Part' action='get' id='3F292E347CC64FB698C58E010364537E' /&gt;
  /// &lt;/AML&gt;").WithAction(CommandAction.ApplyAML);
  /// var parts = conn.Apply(cmd).Items();
  /// </code>
  /// <para>Or, you would rather use named parameters instead of numbered parameters</para>
  /// <code lang="C#">
  /// // Get preliminary parts which have existed for a little bit of time
  /// var components = conn.Apply(new Command(@"&lt;Item type='Part' action='get'&gt;
  ///   &lt;classification&gt;@class&lt;/classification&gt;
  ///   &lt;created_on condition='lt'&gt;@created&lt;/created_on&gt;
  ///   &lt;state&gt;Preliminary&lt;/state&gt;
  /// &lt;/Item&gt;")
  ///   .WithParam("class", classification)
  ///   .WithParam("created", DateTime.Now.AddMinutes(-20))).Items();
  /// </code>
  /// <para>For more information on how parameter substitution works see
  /// <see cref="ParameterSubstitution"/>.  For other ways to create AML, see 
  /// <see cref="ElementFactory"/></para>
  /// </remarks>
  /// <see cref="ConnectionExtensions.Apply(IConnection, Command, object[])"/>
  /// <see cref="ConnectionExtensions.ApplyAsync(IAsyncConnection, Command, System.Threading.CancellationToken, object[])"/>
  /// <seealso cref="ParameterSubstitution"/>
  public class Command
  {
    private readonly List<string> _queries = new List<string>(1);
    private readonly ParameterSubstitution _sub = new ParameterSubstitution();
    private CommandAction _action;
    private string _actionString;

    /// <summary>
    /// SOAP action to use with the AML
    /// </summary>
    public CommandAction Action
    {
      get { return _action; }
      set { _action = value; _actionString = null; }
    }

    /// <summary>
    /// SOAP action to use with the AML (represented as a string)
    /// </summary>
    public string ActionString
    {
      get { return _actionString ?? Action.ToString(); }
      set { _actionString = value; }
    }

    /// <summary>
    /// The AML query
    /// </summary>
    public virtual string Aml
    {
      get
      {
        if (_queries.Count < 1) return null;
        if (_queries.Count == 1) return _queries[0];
        return "<AML>" + _queries.GroupConcat("") + "</AML>";
      }
      set
      {
        if (value == null)
        {
          _queries.Clear();
        }
        else
        {
          if (_queries.Count == 1)
          {
            _queries[0] = value;
          }
          else
          {
            _queries.Clear();
            _queries.Add(value);
          }
        }
      }
    }
    /// <summary>
    /// Gets or sets the delegate which will be called when a log item is written pertaining to this request.
    /// </summary>
    /// <value>
    /// The delegate which will be called when a log item is written pertaining to this request.
    /// </value>
    /// <remarks>
    /// The arguments to the delegate are
    /// <list type="table">
    ///   <listheader><term>Value</term><description>Description</description></listheader>
    ///   <item><term><see cref="int"/>: Log Level</term><description>Indicates the severity of the message.  Always <c>4</c> to indiate an informational message</description></item>
    ///   <item><term><see cref="string"/>: Message</term><description>The text of the message</description></item>
    ///   <item><term><see cref="IEnumerable{KeyValuePair}"/>: Parameters</term><description>Structured parameters which contain useful log data</description></item>
    /// </list>
    /// </remarks>
    public Action<int, string, IEnumerable<KeyValuePair<string, object>>> LogListener { get; set; }

    /// <summary>
    /// Instantiate a new command
    /// </summary>
    public Command()
    {
      this.Action = CommandAction.ApplyItem;
    }

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    /// <value>
    /// The parameters.
    /// </value>
    public IEnumerable<KeyValuePair<string, object>> Parameters
    {
      get { return _sub; }
    }

    internal string Substitute(string query, IServerContext context)
    {
      return _sub.RenderParameter(query, context);
    }

    /// <summary>
    /// Creates an AML command that will be sent to the server.  Numbered AML parameters (e.g. @0, used as attribute 
    /// and element values) will be replaced with the corresponding arguments.
    /// </summary>
    /// <param name="query">Query string containing the parameters</param>
    /// <param name="args">Replacement values for the numbered parameters</param>
    /// <returns>A valid AML string</returns>
    /// <remarks>Property type conversion and XML formatting is performed</remarks>
    public Command(string query, params object[] args) : this()
    {
      this.WithAml(query, args);
    }

#if DBDATA
    /// <summary>
    /// Creates an AML command that will be sent to the server.  Named AML parameters (e.g. @qty, used as attribute 
    /// and element values) will be replaced with the corresponding arguments.
    /// </summary>
    /// <param name="format">Format string containing the parameters</param>
    /// <param name="paramaters">Replacement values for the named parameters</param>
    /// <returns>A valid AML string</returns>
    /// /// <remarks>Property type conversion and XML formatting is performed</remarks>
    public Command(string query, Connection.DbParams parameters) : this()
    {
      this.Aml = query;
      foreach (var param in parameters)
      {
        _sub.AddParameter(param.ParameterName, param.Value);
      }
    }
#endif
    /// <summary>
    /// Creates an AML command that will be sent to the server from an <see cref="IAmlNode"/>
    /// </summary>
    /// <param name="aml">The AML object (e.g. <see cref="IReadOnlyItem"/>) that you want to create 
    /// the command with</param>
    public Command(IAmlNode aml) : this(aml.ToAml())
    {
      var elem = aml as IReadOnlyElement;
      if (elem != null && elem.Name == "AML" && elem.Elements().Count() > 1)
        this.Action = CommandAction.ApplyAML;
    }
    /// <summary>
    /// Creates an AML command that will be sent to the server from an <see cref="IEnumerable{IAmlNode}"/>
    /// </summary>
    /// <param name="aml"><see cref="IEnumerable{IAmlNode}"/> that you want to create the
    /// command with</param>
    public Command(IEnumerable<IAmlNode> aml) : this()
    {
      this.Aml = "<AML>" + aml.GroupConcat("", i => i.ToAml()) + "</AML>";
      this.Action = CommandAction.ApplyAML;
    }

    /// <summary>
    /// <a href="https://en.wikipedia.org/wiki/Fluent_interface">Fluent</a> interface used to 
    /// specify the SOAP action to use with the AML
    /// </summary>
    /// <param name="action">The SOAP action to send with the AML</param>
    /// <returns>The current command for chaining additional method calls</returns>
    public Command WithAction(CommandAction action)
    {
      this.Action = action;
      return this;
    }
    /// <summary>
    /// <a href="https://en.wikipedia.org/wiki/Fluent_interface">Fluent</a> interface used to 
    /// specify the SOAP action to use with the AML (as a string)
    /// </summary>
    /// <param name="action">The SOAP action to send with the AML</param>
    /// <returns>The current command for chaining additional method calls</returns>
    public Command WithAction(string action)
    {
      CommandAction parsed;
      if (Utils.EnumTryParse<CommandAction>(action, true, out parsed))
      {
        this.Action = parsed;
      }
      else
      {
        _actionString = action;
      }
      return this;
    }
    /// <summary>
    /// <a href="https://en.wikipedia.org/wiki/Fluent_interface">Fluent</a> interface used to 
    /// specify the AML of the request.  SQL Server style numbered AML parameters (used as 
    /// attribute and element values) are replaced with the corresponding arguments. Property type 
    /// conversion and XML formatting is performed
    /// </summary>
    /// <param name="query">Format string containing the parameters</param>
    /// <param name="args">Replacement values for the numbered parameters</param>
    /// <returns>The current command for chaining additional method calls</returns>
    public Command WithAml(string query, params object[] args)
    {
      this.Aml = query;
      _sub.AddIndexedParameters(args);
      return this;
    }

    /// <summary>
    /// <a href="https://en.wikipedia.org/wiki/Fluent_interface">Fluent</a> interface used to 
    /// specify a log listener to be called when logging data is recorded during the execution of 
    /// this command
    /// </summary>
    /// <param name="listener">Callback to handle the logging of information.  The parameters 
    /// include the message level, message, and any relevant structured data</param>
    /// <returns>The current command for chaining additional method calls</returns>
    public Command WithLogListener(Action<int, string, IEnumerable<KeyValuePair<string, object>>> listener)
    {
      this.LogListener = listener;
      return this;
    }

    /// <summary>
    /// Specify a named parameter and its value
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    public Command WithParam(string name, object value)
    {
      _sub.AddParameter(name, value);
      return this;
    }

    /// <summary>
    /// Append a query to this request
    /// </summary>
    /// <param name="query">Query to append</param>
    protected virtual void AddAml(string query)
    {
      _queries.Add(query);
    }

    /// <summary>
    /// Specify a method to configure each outgoing HTTP request associated specifically with this AML request
    /// </summary>
    public Action<IHttpRequest> Settings { get; set; }

    /// <summary>
    /// Implicitly convert strings to commands as needed
    /// </summary>
    public static implicit operator Command(string aml)
    {
      return new Command() { Aml = aml };
    }

    /// <summary>
    /// Implicitly convert XML elements to commands as needed
    /// </summary>
    public static implicit operator Command(XElement aml)
    {
      return new Command() { Aml = aml.ToString() };
    }

#if XMLLEGACY
    /// <summary>
    /// Implicitly convert XML elements to commands as needed
    /// </summary>
    public static implicit operator Command(XmlNode aml)
    {
      return new Command() { Aml = aml.OuterXml };
    }
#endif


#if INTERPOLATEDSTR
    /// <summary>
    /// Create a command from an interpolated string
    /// </summary>
    /// <param name="formatted">Interpolated string to convert to a command</param>
    public Command(FormattableString formatted)
    {
      this.WithAml(formatted.Format, formatted.GetArguments());
      _sub.Style = ParameterStyle.CSharp;
    }

    /// <summary>
    /// Create a command from an interpolated string
    /// </summary>
    /// <param name="formatted">Interpolated string to convert to a command</param>
    public static implicit operator Command(FormattableString formatted)
    {
      return new Command(formatted);
    }
#endif

    /// <summary>
    /// Perform parameter substitutions and return the resulting AML
    /// </summary>
    /// <param name="context">Localization context (e.g. from 
    /// <see cref="ElementFactory.LocalizationContext"/>)</param>
    /// <returns>AML string</returns>
    public string ToNormalizedAml(IServerContext context)
    {
      var aml = this.Aml;
      if (_sub.ParamCount > 0 || aml.IndexOf("origDateRange") > 0)
        return _sub.Substitute(aml, context);
      return aml;
    }

    /// <summary>
    /// Perform parameter substitutions and return the resulting AML
    /// </summary>
    /// <param name="context">Localization context (e.g. from 
    /// <see cref="ElementFactory.LocalizationContext"/>)</param>
    /// <param name="writer">Writer to which AML is written</param>
    public void ToNormalizedAml(IServerContext context, TextWriter writer)
    {
      var aml = this.Aml;
      if (_sub.ParamCount > 0 || aml.IndexOf("origDateRange") > 0)
        _sub.Substitute(aml, context, writer);
      else
        writer.Write(aml);
    }

    /// <summary>
    /// Perform parameter substitutions and return the resulting AML
    /// </summary>
    /// <param name="context">Localization context (e.g. from 
    /// <see cref="ElementFactory.LocalizationContext"/>)</param>
    /// <param name="writer">Writer to which AML is written</param>
    public void ToNormalizedAml(IServerContext context, XmlWriter writer)
    {
      var aml = this.Aml;
      if (_sub.ParamCount > 0 || aml.IndexOf("origDateRange") > 0)
      {
        _sub.Substitute(aml, context, writer);
      }
      else
      {
        using (var reader = new StringReader(aml))
        using (var xml = XmlReader.Create(reader))
        {
          xml.CopyTo(writer);
        }
      }
    }

    /// <summary>
    /// Return the AML string
    /// </summary>
    public override string ToString()
    {
      return this.Aml;
    }
  }
}
