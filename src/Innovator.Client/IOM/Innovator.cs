namespace Innovator.Client.IOM
{
  /// <summary>
  /// Wraps a connection with a API which is compatible with 
  /// <a href="http://www.aras.com/support/documentation/DocumentView.aspx?file=./11.0%20SP9/Other%20Documentation/On-Line%20.NET%20API%20Guide.html">Aras's IOM</a>
  /// In particular, the Innovator class provides methods for connecting to Aras 
  /// Innovator Server and for performing miscellaneous mostly non-Item related 
  /// operations.
  /// </summary>
  public class Innovator : ElementFactory
  {
    private readonly IConnection _conn;

    /// <summary>
    /// Initializes a new instance of the <see cref="Innovator"/> class.
    /// </summary>
    /// <param name="conn">The server connection to use</param>
    public Innovator(IConnection conn) : base(conn.AmlContext.LocalizationContext, conn.AmlContext.ItemFactory)
    {
      _conn = conn;
    }

    /// <summary>
    /// Sends request to Innovator server and returns server response as an item.
    /// </summary>
    /// <param name="AML">AML script to be sent to Innovator server.</param>
    /// <param name="args">Parameters to be injected into the query</param>
    /// <returns>Server response.</returns>
    /// <remarks>It's assumed that passed AML script has a root tag <c>&lt;AML&gt;</c>
    /// that contains one or more <c>&lt;Item&gt;</c> children elements. The method 
    /// send the AML script to the server and returns an <see cref="Item"/> object 
    /// containing the XML response returned from the server.</remarks>
    public Item applyAML(string AML, params object[] args)
    {
      return new Item(_conn, _conn.Apply(new Command(AML, args).WithAction(CommandAction.ApplyAML)));
    }

    /// <summary>
    /// Apply a method by name, passing a string value as its body.
    /// </summary>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="body">Context for the method (method item).</param>
    /// <returns>The <see cref="Item"/> response</returns>
    /// <remarks>The context for the method will be the method item, in the form: 
    /// <c>&lt;Item type="Method" action="{methodName}"&gt;{body}&lt;/Item&gt;</c> 
    /// Note, that only methods that use IOM namespace from .NET (C#, VBScript) can 
    /// be applied, and methods written in JavaScript cannot be applied.</remarks>
    public Item applyMethod(string methodName, string body)
    {
      return new Item(_conn, _conn.Apply(new Command("<Item type='Method' action='@0'>@1!</Item>", methodName, body)
        .WithAction(CommandAction.ApplyMethod)));
    }

    /// <summary>
    /// Sends SQL request to Innovator server and returns server response as an 
    /// <see cref="Item"/>.
    /// </summary>
    /// <param name="sql">SQL to be sent to Innovator server.</param>
    /// <returns>Server response.</returns>
    /// <remarks>Uses <see cref="CommandAction.ApplySQL"/> type of server request. 
    /// The returned <see cref="Item"/> object contains the XML returned from the 
    /// server.</remarks>
    public Item applySQL(string sql)
    {
      return new Item(_conn, _conn.ApplySql(sql));
    }

    /// <summary>
    /// Returns the server connection set on the instance.
    /// </summary>
    /// <returns>Server connection object that implements <see cref="IConnection"/> 
    /// interface.</returns>
    public IConnection getConnection()
    {
      return _conn;
    }

    /// <summary>
    /// Returns an <see cref="Item"/> object that matches the 
    /// <paramref name="itemTypeName"/> and <paramref name="id"/> for the Item.
    /// </summary>
    /// <param name="itemTypeName">Name of the ItemType.</param>
    /// <param name="id">Item's ID.</param>
    /// <returns>If request to the server failed the method returns an "error" 
    /// item; if no item with specified type and id found in the database the 
    /// method returns 'null'; otherwise the method returns the found item.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="itemTypeName"/> is not specified
    /// - or -
    /// <paramref name="id"/> is not specified
    /// </exception>
    public Item getItemById(string itemTypeName, string id)
    {
      return new Item(_conn, _conn.ItemById(itemTypeName, id));
    }

    /// <summary>
    /// Returns an <see cref="Item"/> object that matches the 
    /// <paramref name="itemTypeName"/> and <paramref name="keyedName"/> for the Item.
    /// </summary>
    /// <param name="itemTypeName">Name of the ItemType.</param>
    /// <param name="keyedName">Keyed name for the searched item.</param>
    /// <returns>If request to the server failed the method returns an "error" 
    /// item; if no item with specified type and id found in the database the 
    /// method returns 'null'; otherwise the method returns the found item.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="itemTypeName"/> is not specified
    /// - or -
    /// <paramref name="keyedName"/> is not specified
    /// </exception>
    public Item getItemByKeyedName(string itemTypeName, string keyedName)
    {
      return new Item(_conn, _conn.ItemByKeyedName(itemTypeName, keyedName));
    }

    /// <summary>
    /// Gets a new 32 character hex string based on a GUID.
    /// </summary>
    /// <returns>New GUID.</returns>
    public string getNewID()
    {
      return _conn.AmlContext.NewId();
    }

    /// <summary>
    /// Returns the next sequence value by sequence name.
    /// </summary>
    /// <param name="sequenceName">Sequence name.</param>
    /// <returns>New sequence value if request to the server succeeded, 
    /// <c>null</c> otherwise.</returns>
    /// <remarks>Method makes a request to Innovator server in order to 
    /// get new sequence value.</remarks>
    public string getNextSequence(string sequenceName)
    {
      return _conn.NextSequence(sequenceName);
    }

    /// <summary>
    /// Returns id of the user assosiated with a given instance of 
    /// Innovator if this user is currently logged in
    /// </summary>
    /// <returns>User id.</returns>
    public string getUserID()
    {
      return _conn.UserId;
    }

    /// <summary>
    /// Returns a new "error" item.
    /// </summary>
    /// <param name="explanation">Is used to populate the <c>&lt;faultstring&gt;</c> 
    /// tag inside <c>&lt;Fault&gt;</c> node. According to the SOAP 1.1 specification 
    /// the <c>faultstring</c> element is intended to provide a human readable 
    /// explanation of the fault and is not intended for algorithmic processing. Note, 
    /// that passed <c>message</c> is set as <c>InnerText</c> on the <c>&lt;faultstring&gt;</c>
    /// element without any pre-processing, so symbols like '&lt;', '&gt;', etc. will 
    /// be escaped.</param>
    /// <returns></returns>
    public Item newError(string explanation)
    {
      return new Item(_conn, _conn.AmlContext.ServerException(explanation));
    }

    /// <summary>
    /// Returns a new <see cref="Item"/>.
    /// </summary>
    /// <returns>Newly created instance of <see cref="Item"/>.</returns>
    /// <remarks>The new <see cref="Item"/> will have no properties.</remarks>
    public Item newItem()
    {
      return new Item(_conn);
    }

    /// <summary>
    /// Returns a new <see cref="Item"/> with the specified type..
    /// </summary>
    /// <param name="itemTypeName">Name of the ItemType.</param>
    /// <returns>Newly created instance of <see cref="Item"/>.</returns>
    /// <remarks>The <paramref name="itemTypeName"/> is used to set the 'type' attribute. 
    /// The new <see cref="Item"/> will have no properties. In order to populate it with 
    /// default property values of its ItemType call <see cref="Item.fetchDefaultPropertyValues(bool)"/>
    /// on the item.</remarks>
    public Item newItem(string itemTypeName)
    {
      var aml = _conn.AmlContext;
      return new Item(_conn, aml.Type(itemTypeName));
    }

    /// <summary>
    /// Returns a new <see cref="Item"/> with the specified type and action.
    /// </summary>
    /// <param name="itemTypeName">Name of the item type.</param>
    /// <param name="action">Name of action (e.g. "get", "update", etc.)</param>
    /// <returns>Newly created instance of <see cref="Item"/>.</returns>
    /// <remarks>The <paramref name="itemTypeName"/> is used to set the 'type' attribute 
    /// and the <paramref name="action"/> name is used to set the 'action' attribute on the
    /// <see cref="Item"/>. The new <see cref="Item"/> will have no properties. In order 
    /// to populate it with default property values of its ItemType call 
    /// <see cref="Item.fetchDefaultPropertyValues(bool)"/> on the item.</remarks>
    public Item newItem(string itemTypeName, string action)
    {
      var aml = _conn.AmlContext;
      return new Item(_conn, aml.Type(itemTypeName), aml.Action(action));
    }

    /// <summary>
    /// Returns a new <see cref="Item"/> with the AML body.
    /// </summary>
    /// <param name="aml">AML to populate the item with</param>
    /// <param name="args">Parameters to be injected into the query</param>
    public Item newItemFromAml(string aml, params object[] args)
    {
      return new Item(_conn, _conn.AmlContext.FromXml(aml, args));
    }

    /// <summary>
    /// Returns an <see cref="Item"/> where the text passed in as the argument is the body for 
    /// the <c>&lt;Result&gt;</c> tag.
    /// </summary>
    /// <param name="text">Text to be set as the body for the <c>&lt;Result&gt;</c> tag.</param>
    /// <returns>Created item.</returns>
    public Item newResult(string text)
    {
      return new Item(_conn, text);
    }

    /// <summary>
    /// Computes the MD5 hash value for the specified string.
    /// </summary>
    /// <param name="val">String to be encoded.</param>
    /// <returns>MD5 hash value.</returns>
    public static string ScalcMD5(string val)
    {
      return MD5.ComputeHash(Utils.AsciiGetBytes(val)).ToLowerInvariant();
    }
  }
}
