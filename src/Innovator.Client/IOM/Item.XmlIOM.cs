#if XMLLEGACY
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Innovator.Client.IOM
{
  public partial class Item : IEnumerable<Item>
  {
    /// <summary>
    /// A reference to the instance of <see cref="XmlDocument"/> that holds the data for the Item in the AML format.
    /// </summary>
    public XmlDocument dom { get; set; }

    /// <summary>
    /// A reference to the <c>&lt;Item&gt;</c> node in the <see cref="dom"/> for this Item instance.
    /// </summary>
    public XmlElement node { get { return Xml; } set { Xml = value; } }

    /// <summary>
    /// List of <c>&lt;Item&gt;</c> nodes from the <see cref="dom"/> for this Item instance.
    /// </summary>
    public XmlNodeList nodeList { get; set; }

    /// <summary>
    /// Add specified item as a relationship item to the instance.
    /// </summary>
    /// <param name="item">Item(s) to be added to item's relationships.</param>
    public void addRelationship(Item item)
    {
      Relationships().Add(item.AssertItems());
    }

    /// <summary>
    /// Appends the item(s).
    /// </summary>
    /// <param name="item">Item(s) to append.</param>
    public void appendItem(Item item)
    {
      if (!Exists)
        throw new InvalidOperationException("Cannot append an item to an item that doesn't exist");

      var itemsToImport = item.AssertItems().OfType<Item>().Select(i => i.Xml).ToList();
      if (itemsToImport.Count < 1)
        throw new ArgumentException("Cannot append an item which doesn't exist", "item");

      var first = dom.SelectSingleNode("//Item") as XmlElement;
      var parent = first?.ParentNode as XmlElement;

      if (parent == null && first != null)
      {
        parent = dom.CreateElement("AML");
        dom.RemoveChild(first);
        dom.AppendChild(parent);
        parent.AppendChild(first);
      }

      if (parent == null)
      {
        var result = dom.SelectSingleNode(XPathResult) as XmlElement;
        if (result != null)
        {
          result.RemoveAll();
          parent = result;
        }
      }

      if (parent == null)
      {
        var body = dom.SelectSingleNode("/*[local-name()='Envelope' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]/*[local-name()='Body' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]") as XmlElement;
        if (body != null)
        {
          parent = body.ChildNodes.OfType<XmlElement>().FirstOrDefault() ?? body;
          parent.RemoveAll();
        }
      }

      if (parent == null)
        throw new InvalidOperationException("Invalid item structure");

      foreach (var toImport in itemsToImport)
        parent.AppendChild(dom.ImportNode(toImport, true));

      node = null;
      nodeList = parent.SelectNodes("./Item");

      if (nodeList.Count == 1)
      {
        node = (XmlElement)nodeList[0];
        nodeList = null;
      }
    }

    /// <summary>
    /// Applies the AML.
    /// </summary>
    /// <returns>An <see cref="Item"/> containing result of the AML call</returns>
    public Item apply()
    {
      return apply(null);
    }

    /// <summary>
    /// Sets the action property and applies the AML.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>An <see cref="Item"/> containing result of the AML call</returns>
    public Item apply(string action)
    {
      if (!string.IsNullOrEmpty(action))
        setAction(action);

      var aml = this.ToAml();
      return getInnovator().Apply(aml);
    }

    //public string applyStylesheet(string xslStylesheet, string type)
    //{
    //
    //}

    //public void attachPhysicalFile(string filePath)
    //{
    //
    //}

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <param name="cloneRelationships">If <c>true</c>, all relationships of
    /// the item are cloned as well.</param>
    /// <returns>Cloned item.</returns>
    /// <remarks>Cloned item as well as cloned relationships have new IDs and 
    /// action attributes set to 'add'.</remarks>
    public Item clone(bool cloneRelationships)
    {
      AssertXml();
      var item = getInnovator().newItemFromAml(this);
      item.setNewID();
      item.setAction("add");
      if (!cloneRelationships)
      {
        var relElem = (XmlElement)item.node.SelectSingleNode("Relationships");
        relElem?.ParentNode.RemoveChild(relElem);
      }
      else
      {
        var rels = item.node.SelectNodes(".//Relationships/Item");
        for (int i = 0; i < rels.Count; i++)
        {
          ((XmlElement)rels[i]).SetAttribute("id", getNewID());
          ((XmlElement)rels[i]).SetAttribute("action", "add");
        }
      }
      return item;
    }

    /// <summary>
    /// Creates an item with the specified type and action and sets 
    /// it as the specified property of this item.
    /// </summary>
    /// <param name="propName">Name of the property.</param>
    /// <param name="type">Type of the item property.</param>
    /// <param name="action">Name of the action that will be set on the item 
    /// property.</param>
    /// <returns>Created item with the specified type and action.</returns>
    /// <remarks>The method is similar to 
    /// <see cref="setPropertyItem(string, Item)"/> except that
    /// <see cref="setPropertyItem(string, Item)"/> gets an item as the method 
    /// argument and doesn't create it internally.</remarks>
    public Item createPropertyItem(string propName, string type, string action)
    {
      var elem = GetPropertyForLanguage(propName, null);
      if (elem == null)
      {
        elem = (XmlElement)Xml.AppendChild(dom.CreateElement(propName));
      }
      else
      {
        // remove existing
        foreach (var child in elem.ChildNodes.OfType<XmlNode>().ToList())
          elem.RemoveChild(child);
      }
      var item = Innovator.AppendNewItem(elem, type, action);
      return new Item(getInnovator(), item);
    }

    /// <summary>
    /// Creates an item with the specified type and action and sets it as the 
    /// related item on this item.
    /// </summary>
    /// <param name="type">Type of the related item.</param>
    /// <param name="action">Name of the action that will be set on created 
    /// related item.</param>
    /// <returns>Created related item with specified type and action.</returns>
    /// <remarks>If property <c>related_id</c> doesn't exist on this item, 
    /// it's created. The method is similar to <see cref="setRelatedItem(Item)"/>
    /// except that <see cref="setRelatedItem(Item)"/> gets the related item as a
    /// method argument and doesn't create it internally.</remarks>
    public Item createRelatedItem(string type, string action)
    {
      return createPropertyItem("related_id", type, action);
    }

    /// <summary>
    /// Creates new <c>&lt;Item&gt;</c> node with specified 'type' and 'action' 
    /// under <c>&lt;Relationships&gt;</c> node.
    /// </summary>
    /// <param name="type">Value of attribute 'type' that will be set on the 
    /// created <c>&lt;Item&gt;</c> node.</param>
    /// <param name="action">Value of attribute 'action' that will be set on 
    /// the created <c>&lt;Item&gt;</c> node.</param>
    /// <returns>Item that reference newly created <c>&lt;Item&gt;</c> node.</returns>
    /// <remarks>If a <c>&lt;Relationships&gt;</c> node doesn't exist, it's created.</remarks>
    public Item createRelationship(string type, string action)
    {
      var elem = ItemElement("Relationships")
        ?? (XmlElement)Xml.AppendChild(dom.CreateElement("Relationships"));
      var item = Innovator.AppendNewItem(elem, type, action);
      return new Item(getInnovator(), item);
    }

    //public bool email(Item emailItem, Item identityItem) { }

    /// <summary>
    /// Fetches from server default values for all properties of the item's 
    /// ItemType and sets them on the item.
    /// </summary>
    /// <param name="overwrite_current">If <c>true</c>, overwrite existing property 
    /// values.</param>
    /// <returns>If fetching failed, returns "error" item that contains information 
    /// about the failure. Otherwise, returns this with set property values.</returns>
    /// <remarks>If a property doesn't have a default value, the property is not set.</remarks>
    public Item fetchDefaultPropertyValues(bool overwrite_current)
    {
      var props = getInnovator().getConnection().Apply(@"<Item type='ItemType' action='get' select='id'>
  <name>@0</name>
  <Relationships>
    <Item type='Property' action='get' select='name,default_value' />
  </Relationships>
</Item>", TypeName()).AssertItem().Relationships().OfType<Model.Property>();

      foreach (var prop in props.Where(p => p.DefaultValue().HasValue()))
      {
        var existing = Property(prop.NameProp().Value);
        if (overwrite_current || !existing.HasValue())
          existing.Set(prop.DefaultValue().Value);
      }

      return this;
    }

    // public Item fetchFileProperty(string propertyName, string targetPath, FetchFileMode mode)

    /// <summary>
    /// Fetches from the server the locked status of the item using the item's ID.
    /// </summary>
    /// <returns>
    /// <list type="table">
    ///   <listheader><term>Value</term><description>Meaning</description></listheader>  
    ///   <item><term>-1</term><description>failed to fetch item's lock status from server</description></item>
    ///   <item><term>0</term><description>0 not locked</description></item>
    ///   <item><term>1</term><description>1 locked by user</description></item>
    ///   <item><term>2</term><description>2 locked by other</description></item>
    /// </list>
    /// </returns>
    /// <remarks>The method sends a request to the server in order to fetch the 
    /// latest lock status of the item on server and updates the item's property
    /// <c>locked_by_id</c>.</remarks>
    public int fetchLockStatus()
    {
      var id = AssertId();
      var type = AssertTypeName();
      var result = getInnovator().getConnection().Apply("<Item isNew='1' isTemp='1' type='@0' action='get' select='locked_by_id' id='@1' />", type, id);

      if (result.Exception != null)
        return -1;

      var lockedById = result.AssertItem().LockedById().Value ?? "";
      this.LockedById().Set(lockedById);
      if (string.IsNullOrEmpty(lockedById))
        return 0;
      if (lockedById == getInnovator().getConnection().UserId)
        return 1;
      return 2;
    }

    /// <summary>
    /// Fetches relationships of specified type from the server and sets them on the item.
    /// </summary>
    /// <param name="relationshipTypeName">Name of the relationship type.</param>
    /// <returns>It returns this.</returns>
    /// <exception cref="ServerException">If an error was returned from the server</exception>
    public Item fetchRelationships(string relationshipTypeName)
    {
      return fetchRelationships(relationshipTypeName, null, null);
    }

    /// <summary>
    /// Fetches relationships of specified type from the server and sets them on the item.
    /// </summary>
    /// <param name="relationshipTypeName">Name of the relationship type.</param>
    /// <param name="selectList">Select list that is set on attribute <c>select</c> of the request</param>
    /// <returns>It returns this.</returns>
    /// <exception cref="ServerException">If an error was returned from the server</exception>
    public Item fetchRelationships(string relationshipTypeName, string selectList)
    {
      return fetchRelationships(relationshipTypeName, selectList, null);
    }

    /// <summary>
    /// Fetches relationships of specified type from the server and sets them on the item.
    /// </summary>
    /// <param name="relationshipTypeName">Name of the relationship type.</param>
    /// <param name="selectList">Select list that is set on attribute <c>select</c> of the request</param>
    /// <param name="orderBy">The value is set on attribute <c>orderBy</c> of the request</param>
    /// <returns>It returns this.</returns>
    /// <exception cref="ServerException">If an error was returned from the server</exception>
    public Item fetchRelationships(string relationshipTypeName, string selectList, string orderBy)
    {
      var id = AssertId();

      if (relationshipTypeName?.Trim().Length < 1)
        throw new ArgumentException("Relationship type is not specified");

      var result = getInnovator().getConnection().Apply("<Item type='@0' action='get' select='@1' orderBy='@2'><source_id>@3</source_id></Item>"
        , relationshipTypeName, selectList, orderBy, id).Items();

      var existing = Relationships(relationshipTypeName).ToList();
      foreach (var rel in existing)
      {
        rel.Remove();
      }
      var relTag = Relationships();
      foreach (var rel in result)
      {
        relTag.Add(rel);
      }

      return this;
    }

    /// <summary>
    /// Returns the <c>action</c> attribute from the Item node.
    /// </summary>
    /// <returns>Value of the <c>action</c> attribute if the attribute exists, <c>null</c> 
    /// otherwise</returns>
    public string getAction()
    {
      return this.Action().Value;
    }

    /// <summary>
    /// Returns value of the attribute with the specified name on the item's node.
    /// </summary>
    /// <param name="attributeName">The qualified name of the attribute.</param>
    /// <returns>Attribute value or <c>null</c> if the attribute doesn't exist</returns>
    public string getAttribute(string attributeName)
    {
      return this.getAttribute(attributeName, null);
    }

    /// <summary>
    /// Returns value of the attribute with the specified name on the item's node.
    /// </summary>
    /// <param name="attributeName">The qualified name of the attribute.</param>
    /// <param name="defaultValue">Default value of the attribute.</param>
    /// <returns>Attribute value or <paramref name="defaultValue"/> if the attribute doesn't 
    /// exist</returns>
    public string getAttribute(string attributeName, string defaultValue)
    {
      return ((IReadOnlyItem)this).Attribute(attributeName).AsString(defaultValue);
    }

    /// <summary>
    /// Gets the error code of the "error" item.
    /// </summary>
    /// <returns>Value of <c>&lt;faultcode&gt;</c>. If the item is not an "error" item, <c>null</c>
    /// is returned.</returns>
    public string getErrorCode()
    {
      return GetErrorDetail("faultcode");
    }

    /// <summary>
    /// Returns details of the error item.
    /// </summary>
    /// <returns>If the instance is not an error item, <c>null</c> is returned.</returns>
    /// <remarks>In most cases error details contains a low level error details (e.g. actual SQL 
    /// error message) obtained from server.</remarks>
    public string getErrorDetail()
    {
      return GetErrorDetail("detail");
    }

    /// <summary>
    /// Returns the content of the <c>&lt;faultactor&gt;</c> element of SOAP Fault element.
    /// </summary>
    /// <returns>If the instance is not an error item, <c>null</c> is returned.</returns>
    public string getErrorSource()
    {
      return GetErrorDetail("faultactor");
    }

    /// <summary>
    /// Returns the error message.
    /// </summary>
    /// <returns>The returned value is obtained from the <c>&lt;faultstring&gt;</c> tag of 
    /// <c>&lt;Fault&gt;</c>. If the instance is not an error item, <c>null</c> is returned.</returns>
    public string getErrorString()
    {
      return GetErrorDetail("faultstring");
    }

    private string GetErrorDetail(string tagName)
    {
      if (dom == null)
        return string.Empty;

      var node = dom.SelectSingleNode(XPathFault + "/" + tagName);
      if (node == null)
        return string.Empty;

      return node.InnerText;
    }

    /// <summary>
    /// Returns ID of the Item node. According to AML standard ID could be set on 
    /// <c>&lt;Item&gt;</c> either as the attribute with name 'id' or as a sub-tag 
    /// <c>&lt;id&gt;</c> (i.e. item property) or both.
    /// </summary>
    /// <returns>ID of the item or <c>null</c> if ID was not found.</returns>
    public string getID()
    {
      return Id();
    }

    /// <summary>
    /// Returns instance of Innovator this Item "belongs" to.
    /// </summary>
    /// <returns>An <see cref="Innovator"/> for creating AML</returns>
    public Innovator getInnovator()
    {
      return (Innovator)AmlContext;
    }

    /// <summary>
    /// Gets an item by index.
    /// </summary>
    /// <param name="index">The 0-based index.</param>
    /// <returns>Found item</returns>
    public Item getItemByIndex(int index)
    {
      return AssertItems().OfType<Item>().ElementAt(index);
    }

    /// <summary>
    /// Returns the number of items that the instance represents.
    /// </summary>
    /// <returns>
    /// <list type="table">
    ///   <listheader><term>Value</term><description>Condition</description></listheader>
    ///   <item><term>0</term><description>The item contains an exception of type <see cref="NoItemsFoundException"/></description></item>
    ///   <item><term>-1</term><description>The item is not <see cref="NoItemsFoundException"/> and does not contain any items</description></item>
    ///   <item><term>N</term><description>Where N >= 1 and N is the number of items contained in the instance</description></item>
    /// </list>
    /// </returns>
    public int getItemCount()
    {
      if (dom == null)
        return -1;
      if (nodeList != null)
        return nodeList.Count;
      if (isError())
        return getErrorCode() == "0" ? 0 : -1;
      if (node == null)
        return -1;
      return 1;
    }

    /// <summary>
    /// Gets an item (single or collection) resolving XPath specified.
    /// </summary>
    /// <param name="xpath">XPath for selecting <c>&lt;Item&gt;</c> nodes</param>
    /// <returns>Instance of the class that represents either a single item or a collection of items.
    /// The collection could be empty. The returned item shares its <see cref="dom"/> with this item,
    /// i.e. returned item dom and <see cref="dom"/> reference the same instance </returns>
    /// <remarks>The method applies the specified XPath on <see cref="node"/> if it's set or on
    /// <see cref="dom"/> otherwise. If a single <c>&lt;Item&gt;</c> node was found, it's assigned
    /// to the <see cref="node"/>. If 0 or more than 1 <c>&lt;Item&gt;</c> nodes were found, it's
    /// referenced by the <see cref="nodeList"/> of the returned item.</remarks>
    public Item getItemsByXPath(string xpath)
    {
      if (dom == null)
        throw new Exception("Wrong internal structure of the this item; e.g. item's \"dom\" is not set; or item's \"node\" doesn't belong to the item's \"dom\"; or both \"node\" and \"nodeList\" are null; etc.");
      var xmlNodeList = node == null ? dom.SelectNodes(xpath) : node.SelectNodes(xpath);
      for (var i = 0; i < xmlNodeList.Count; i++)
      {
        if (xmlNodeList[i].LocalName != "Item")
          throw new ArgumentException($"Specified XPath '{xpath}' doesn't resolve to <Item> nodes");
      }

      return new Item(getInnovator(), this)
      {
        Parent = null,
        node = xmlNodeList.Count == 1 ? xmlNodeList[0] as XmlElement : null,
        nodeList = xmlNodeList.Count > 1 ? xmlNodeList : null,
        dom = dom
      };
    }

    /// <summary>
    /// Gets item's lock status based on the property <c>locked_by_id</c>.
    /// </summary>
    /// <returns>Like with the <see cref="LockStatusType"/> enumeration, the values are
    /// <list type="table">
    ///   <listheader><term>Value</term><description>Description</description></listheader>
    ///   <item><term>0</term><description>The item is not locked</description></item>
    ///   <item><term>1</term><description>The item is locked by the user</description></item>
    ///   <item><term>2</term><description>The item is locked by the someone else</description></item>
    /// </list> 
    /// </returns>
    public int getLockStatus()
    {
      return (int)this.LockStatus(getInnovator().getConnection());
    }

    /// <summary>
    /// Returns <see cref="IOM.Item"/> object with the <see cref="nodeList"/> containing logical
    /// nodes that are children of the this.node.
    /// </summary>
    /// <returns>Item object with the <see cref="nodeList"/> containing logical nodes that are
    /// direct children of the <see cref="node"/>. The returned item shares its <see cref="dom"/>
    /// with this item.</returns>
    /// <remarks>The method allows you to traverse logical nodes of an item</remarks>
    /// <example>
    /// <code lang="C#"><![CDATA[var lchildren = item.getLogicalChildren();
    /// for (var i = 0; i < lchildren.getItemCount(); i++)
    /// {
    ///   var lchild = lchildren.getItemByIndex(i);
    ///   // Do something
    /// }]]>
    /// </code>
    /// </example>
    public Item getLogicalChildren()
    {
      var logical = AssertXml().SelectNodes("./*[local-name()='and' or local-name()='or' or local-name()='not']");
      return new Item(getInnovator(), this)
      {
        dom = dom,
        node = null,
        nodeList = logical
      };
    }

    /// <summary>
    /// Generate new 32 character hex string globally unique identifier.
    /// </summary>
    /// <returns>GUID as a string</returns>
    public string getNewID()
    {
      return getInnovator().NewId();
    }

    /// <summary>
    /// Returns a parent item of the instance.
    /// </summary>
    /// <returns>If there is no parent, <c>null</c> is returned</returns>
    public Item getParentItem()
    {
      var elem = AssertXml();
      var parent = elem.SelectSingleNode("ancestor::Item") as XmlElement;
      if (parent == null)
        return null;

      return new Item(getInnovator(), parent);
    }

    /// <summary>
    /// Gets value of the property with the specified name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>If the property is an item-property, ID of the item-property is returned. If the 
    /// property doesn't exist or it's an item-property without ID, <c>null</c> is returned; 
    /// otherwise the method returns value of the specified property.  Note, that if the property 
    /// has attribute <c>is_null</c> set to 1 and the property value is empty string (e.g. 
    /// <c>&lt;p1 is_null='1'/&gt;</c> or <c>&lt;p1 is_null='1'&gt;&lt;/p1&gt;</c>) then the 
    /// property value is interpreted as <c>null</c>.</returns>
    public string getProperty(string propertyName)
    {
      return this.getProperty(propertyName, null, null);
    }

    /// <summary>
    /// Gets value of the property with the specified name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="defaultValue">Default value of the property.</param>
    /// <returns>If the property is an item-property, ID of the item-property is returned. If the 
    /// property doesn't exist or it's an item-property without ID, <paramref name="defaultValue"/> 
    /// is returned; otherwise the method returns value of the specified property.  Note, that if 
    /// the property has attribute <c>is_null</c> set to 1 and the property value is empty string 
    /// (e.g. <c>&lt;p1 is_null='1'/&gt;</c> or <c>&lt;p1 is_null='1'&gt;&lt;/p1&gt;</c>) then the 
    /// property value is interpreted as <c>null</c>.</returns>
    public string getProperty(string propertyName, string defaultValue)
    {
      return this.getProperty(propertyName, defaultValue, null);
    }

    /// <summary>
    /// Gets value of the property with the specified name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="defaultValue">Default value of the property.</param>
    /// <param name="lang">Language for which the property value has to be returned. If <c>null</c> 
    /// value is passed, the language of the current session is assumed.</param>
    /// <returns>If the property is an item-property, ID of the item-property is returned. If the 
    /// property doesn't exist or it's an item-property without ID, <paramref name="defaultValue"/> 
    /// is returned; otherwise the method returns value of the specified property.  Note, that if 
    /// the property has attribute <c>is_null</c> set to 1 and the property value is empty string 
    /// (e.g. <c>&lt;p1 is_null='1'/&gt;</c> or <c>&lt;p1 is_null='1'&gt;&lt;/p1&gt;</c>) then the 
    /// property value is interpreted as <c>null</c>.</returns>
    public string getProperty(string propertyName, string defaultValue, string lang)
    {
      return ((IReadOnlyItem)this).Property(propertyName, lang).AsString(defaultValue);
    }

    /// <summary>
    /// Gets the specified attribute of the property with the specified name
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    /// <param name="attributeName">Attribute name.</param>
    /// <returns>If either property with the <paramref name="propertyName"/> or the
    /// <paramref name="attributeName"/> on the property doesn't exist, returns the <c>null</c>;
    /// otherwise returns value of the property attribute.</returns>
    public string getPropertyAttribute(string propertyName, string attributeName)
    {
      return this.getPropertyAttribute(propertyName, attributeName, null, null);
    }

    /// <summary>
    /// Gets the specified attribute of the property with the specified name
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    /// <param name="attributeName">Attribute name.</param>
    /// <param name="defaultValue">Attribute default value.</param>
    /// <returns>If either property with the <paramref name="propertyName"/> or the
    /// <paramref name="attributeName"/> on the property doesn't exist, returns the <paramref name="defaultValue"/>;
    /// otherwise returns value of the property attribute.</returns>
    public string getPropertyAttribute(string propertyName, string attributeName, string defaultValue)
    {
      return this.getPropertyAttribute(propertyName, attributeName, defaultValue, null);
    }

    /// <summary>
    /// Gets the specified attribute of the property with the specified name and language.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    /// <param name="attributeName">Attribute name.</param>
    /// <param name="defaultValue">Attribute default value.</param>
    /// <param name="lang">Language for which the property attribute has to be returned. If
    /// <c>null</c> value is passed, the language of the current session is assumed.</param>
    /// <returns>If either property with the <paramref name="propertyName"/> or the
    /// <paramref name="attributeName"/> on the property doesn't exist, returns the <paramref name="defaultValue"/>;
    /// otherwise returns value of the property attribute for the specified <paramref name="lang"/>.</returns>
    public string getPropertyAttribute(string propertyName, string attributeName, string defaultValue, string lang)
    {
      return ((IReadOnlyItem)this).Property(propertyName, lang).Attribute(attributeName).AsString(defaultValue);
    }

    /// <summary>
    /// Gets the <c>condition</c> attribute of the property with the specified name.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    /// <returns>If either property with the specified name or attribute <c>condition</c> on the
    /// property doesn't exist, returns <c>null</c>.</returns>
    public string getPropertyCondition(string propertyName)
    {
      return this.getPropertyCondition(propertyName, null);
    }

    /// <summary>
    /// Gets the <c>condition</c> attribute of the property with the specified name and language.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    /// <param name="lang">Language for which the property condition has to be returned. If
    /// <c>null</c> value is passed, the language of the current session is assumed.</param>
    /// <returns>If either property with the specified name or attribute <c>condition</c> on the
    /// property doesn't exist, returns <c>null</c>.</returns>
    public string getPropertyCondition(string propertyName, string lang)
    {
      return this.getPropertyAttribute(propertyName, "condition", lang, null);
    }

    /// <summary>
    /// Gets item property by name.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    /// <returns>Found item property. If by some reason an item can't be returned (e.g. <see cref="dom"/>
    /// doesn't have a property with the specified name or the property contains just id but the
    /// type of the item property is unknown), the method returns a "null item" (where
    /// <see cref="IReadOnlyElement.Exists"/> = <c>false</c>). NOTE: If specified property name is
    /// <c>id</c> then the method returns this item.</returns>
    /// <remarks>The method tries to find the specified property on the item. If found, the propery
    /// may contain either
    /// <list type="bullet">
    ///   <item><description>A child <c>&lt;Item&gt;</c> node. In this case the child <c>&lt;Item&gt;</c>
    ///     node is used for constructing the returned Item.  Note that in this case, the returned
    ///     item shares it's <see cref="dom"/> with this item.</description></item>
    ///   <item><description>Just an ID. In this case, an attempt is made to construct an Item
    ///     using the id, type, and keyed_name information is available. In this case, the returned item
    ///     doesn't share it's <see cref="dom"/> with this</description></item>
    ///   <item><description>A reference to a vaulted image. In this case, an attempt is made to
    ///     construct an Item with the information that is available. In this case, the returned item
    ///     doesn't share it's <see cref="dom"/> with this</description></item>
    /// </list>
    /// </remarks>
    public Item getPropertyItem(string propertyName)
    {
      if (propertyName == "id")
        return this;

      return this.Property(propertyName, null).AsItem() as Item;
    }

    /// <summary>
    /// Gets related item of the relationship.
    /// </summary>
    /// <returns>Related item. If by some reason the related item can't be returned (e.g.
    /// <see cref="dom"/> doesn't have a <c>&lt;related_id&gt;</c> property or type of the related
    /// item is unknown), the method returns a "null item" (where <see cref="IReadOnlyElement.Exists"/>
    /// = <c>false</c>).</returns>
    /// <remarks>Tries to find <c>related_id</c> property on the item.</remarks>
    /// <seealso cref="getPropertyItem(string)"/>
    public Item getRelatedItem()
    {
      return getPropertyItem("related_id");
    }

    /// <summary>
    /// Tries to find related item (xpath: <c>./related_id</c>) on item's node and returns its ID.
    /// </summary>
    /// <returns>ID of the related item. If related item not found, return empty string.</returns>
    public string getRelatedItemID()
    {
      return getProperty("related_id");
    }

    /// <summary>
    /// Returns an item that is a collection of relationships item with specified type available on the instance.
    /// </summary>
    /// <returns>Item that is a collection of relationships items available on
    /// the instance. If this doesn't have any relationships, the returned item
    /// <see cref="nodeList"/> will be empty.</returns>
    /// <remarks>The method does not make a request to the server but rather returns relationship
    /// Items available in memory in the <see cref="dom"/>. If order to get relationships from server,
    /// use <see cref="fetchRelationships()"/>.</remarks>
    public Item getRelationships()
    {
      return (Item)Relationships();
    }

    /// <summary>
    /// Returns an item that is a collection of relationships item with specified type available on the instance.
    /// </summary>
    /// <param name="itemTypeName">Type of relationship.</param>
    /// <returns>Item that is a collection of relationships items with specified name available on
    /// the instance. If this doesn't have any relationships of this type, the returned item
    /// <see cref="nodeList"/> will be empty.</returns>
    /// <remarks>The method does not make a request to the server but rather returns relationship
    /// Items available in memory in the <see cref="dom"/>. If order to get relationships from server,
    /// use <see cref="fetchRelationships(string)"/>.</remarks>
    public Item getRelationships(string itemTypeName)
    {
      return (Item)Relationships(itemTypeName);
    }

    /// <summary>
    /// Returns <see cref="XmlNode.InnerText"/> property of the node with XPath = <see cref="XPathResult"/>
    /// from the item's <see cref="dom"/>.
    /// </summary>
    /// <returns>If <see cref="dom"/> is <c>null</c> or node with XPath = <see cref="XPathResult"/>
    /// was not found, the method returns <c>null</c>.</returns>
    public string getResult()
    {
      return Value;
    }

    /// <summary>
    /// Returns value of the <c>type</c> attribute of the Item node.
    /// </summary>
    /// <returns>Value of <c>type</c> attribute if the attribute exists, empty string otherwise.</returns>
    public string getType()
    {
      return TypeName();
    }

    /// <summary>
    /// Instantiates workflow for the item using specified workflow map.
    /// </summary>
    /// <param name="workflowMapID">ID of a Workflow Map.</param>
    /// <returns>Returns a Workflow Process item created by this method or throws an exception.</returns>
    /// <exception cref="ArgumentException">WorkflowMap ID is either 'null' or empty string</exception>
    /// <exception cref="Exception">
    ///   <list type="bullet">
    ///     <item><description>The instance doesn't represent a single item.</description></item>
    ///     <item><description>Either 'id' or 'typeId' is not set on the item.</description></item>
    ///     <item><description>The item is a new item not saved in the database yet.</description></item>
    ///   </list>
    /// </exception>
    /// <remarks>
    /// The method does the following:
    /// <list type="number">
    ///   <item><description>Sends a request to the server to instantiate the specified Workflow Map
    ///   to a Workflow Process</description></item>
    ///   <item><description>Using the ID of the Workflow Process item obtained from the server,
    ///   sends another request to attach the Workflow Process to the controlled item via the
    ///   "Workflow" relationship. Note, that the item itself is not modified by the method.</description></item>
    /// </list>
    /// </remarks>
    public Item instantiateWorkflow(string workflowMapID)
    {
      if (workflowMapID?.Trim().Length < 1)
        throw new ArgumentException("WorkflowMap ID is either 'null' or empty string");
      AssertXml();
      if (isNew())
        throw new Exception("The item is a new item");
      var id = AssertId();
      var type = AssertTypeName();

      var conn = getInnovator().getConnection();
      var process = conn.Apply("<Item isNew='1' isTemp='1' type='@0' action='instantiateWorkflow' id='@1'><WorkflowMap>@2</WorkflowMap></Item>"
        , type, id, workflowMapID).AssertItem();
      conn.Apply(@"<Item isNew='1' isTemp='1' type='Workflow' action='add'>
  <locked_by_id>@0</locked_by_id>
  <source_id>@1</source_id>
  <related_id>@2</related_id>
  <source_type>@3</source_type>
</Item>", conn.UserId, id, process.Id(), getAttribute("typeId")).AssertNoError();
      return this;
    }

    /// <summary>
    /// Checks if the instance represents a set of items and not a single item.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if <c>this.nodeList != null && this.node == null</c>, <c>false</c> otherwise
    /// </returns>
    public bool isCollection()
    {
      if (isError())
        return false;
      return nodeList != null && node == null;
    }

    /// <summary>
    /// Determines whether this instance represents a "No items found" exception
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this instance is an error with a fault code of <c>0</c>; otherwise, <c>false</c>.
    /// </returns>
    public bool isEmpty()
    {
      if (isError())
        return getErrorCode() == "0";
      return false;
    }

    /// <summary>
    /// Checks if the item is an "error item".
    /// </summary>
    /// <returns>
    ///   <c>true</c> if the item is an "error item" (i.e. <see cref="dom"/> has a top-level tag
    ///   <see cref="XPathFault"/>); otherwise, <c>false</c>.
    /// </returns>
    public bool isError()
    {
      if (dom == null)
        return false;
      return dom.SelectSingleNode(XPathFault) != null;
    }

    /// <summary>
    /// Find out if the item's node is a "logical" node (<c>and</c>, <c>or</c>, <c>not</c>).
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this instance is logical; otherwise, <c>false</c>.
    /// </returns>
    public bool isLogical()
    {
      return this is ILogical;
    }

    public bool isNew()
    {
      return getItemCount() == 1 && Attribute("isNew").AsBoolean(false);
    }

    public bool isRoot()
    {
      return node != null
        && node.LocalName == "Item"
        && node.SelectNodes("ancestor::node()[local-name()='Item']").Count <= 0
        && node.ParentNode.SelectNodes("Item").Count == 1;
    }

    public void loadAML(string AML)
    {
      dom.XmlResolver = null;
      dom.LoadXml(AML);
      InitNodes();
    }

    public Item lockItem()
    {
      var id = AssertId();
      var type = AssertTypeName();
      var result = getInnovator().Apply(new Command("<Item isNew='1' isTemp='1' type='@0' action='lock' id='@1' />", type, id));
      if (result.Exception == null)
      {
        this.LockedById().Set(result.AssertItem().LockedById().Value);
      }
      return result;
    }

    public Item newAND()
    {
      var andElem = AssertXml().OwnerDocument.CreateElement("and");
      node.AppendChild(andElem);
      return new Logical(getInnovator(), this, andElem);
    }

    public Item newItem()
    {
      return getInnovator().newItem();
    }

    public Item newItem(string itemTypeName)
    {
      return getInnovator().newItem(itemTypeName);
    }

    public Item newItem(string itemTypeName, string action)
    {
      return getInnovator().newItem(itemTypeName, action);
    }

    public Item newNOT()
    {
      var andElem = AssertXml().OwnerDocument.CreateElement("not");
      node.AppendChild(andElem);
      return new Logical(getInnovator(), this, andElem);
    }

    public Item newOR()
    {
      var andElem = AssertXml().OwnerDocument.CreateElement("or");
      node.AppendChild(andElem);
      return new Logical(getInnovator(), this, andElem);
    }

    public XmlDocument newXMLDocument()
    {
      return Innovator.NewXmlDocument();
    }

    public Item promote(string state, string comments)
    {
      if (state == null || state.Trim().Length == 0)
        throw new ArgumentException("'state' is either 'null' or an empty string");

      var aml = AmlContext;
      var promoteItem = aml.FromXml(AssertXml()).AssertItem();
      promoteItem.Action().Set("promoteItem");
      promoteItem.State().Set(state);
      if (!string.IsNullOrEmpty(comments))
        promoteItem.Property("comments").Set(comments);

      return getInnovator().Apply(new Command(promoteItem.ToAml()));
    }

    public void removeAttribute(string attributeName)
    {
      Attribute(attributeName).Remove();
    }

    public void removeItem(Item item)
    {
      if (item.dom != dom)
        throw new ArgumentException("Itemmust be from the same document");
      if (nodeList == null)
        throw new Exception("Not a collection of items");

      var toRemove = item.Items().Cast<Item>();
      var parent = nodeList[0].ParentNode as XmlElement;
      foreach (var remove in toRemove)
        parent.RemoveChild(remove.node);

      nodeList = parent.SelectNodes("./Item");
      if (nodeList.Count == 1)
      {
        node = (XmlElement)nodeList[0];
        nodeList = null;
      }
    }

    public void removeLogical(Item logicalItem)
    {
      var logical = logicalItem.AssertXml();
      AssertXml().RemoveChild(logical);
    }

    public void removeProperty(string propertyName)
    {
      this.removeProperty(propertyName, null);
    }

    public void removeProperty(string propertyName, string lang)
    {
      Property(propertyName, lang).Remove();
    }

    public void removePropertyAttribute(string propertyName, string attributeName)
    {
      this.removePropertyAttribute(propertyName, attributeName, null);
    }

    public void removePropertyAttribute(string propertyName, string attributeName, string lang)
    {
      Property(propertyName, lang).Attribute(attributeName).Remove();
    }

    public void removeRelationship(Item item)
    {
      Relationships().FirstOrNullItem(i => i == item.AssertItem() || i.Id() == item.getID()).Remove();
    }

    public void setAction(string action)
    {
      this.Action().Set(action);
    }

    public void setAttribute(string attributeName, string attributeValue)
    {
      Attribute(attributeName).Set(attributeValue);
    }

    public void setErrorCode(string errcode)
    {
      SetErrorDetail("faultcode", errcode);
    }

    public void setErrorDetail(string detail)
    {
      SetErrorDetail("detail", detail);
    }

    public void setErrorSource(string source)
    {
      SetErrorDetail("faultactor", source);
    }

    public void setErrorString(string errorMessage)
    {
      SetErrorDetail("faultstring", errorMessage);
    }

    private void SetErrorDetail(string errorDetailTagName, string errorDetailValue)
    {
      if (dom == null)
        return;

      var fault = dom.SelectSingleNode(XPathFault);
      if (fault == null)
        return;

      var tag = fault.SelectSingleNode(errorDetailTagName);
      if (tag == null)
        tag = fault.AppendChild(dom.CreateElement(errorDetailTagName));

      tag.InnerText = errorDetailValue;
    }

    // public Item setFileProperty(string propertyName, string pathToFile)

    public void setID(string id)
    {
      Attribute("id").Set(id);
      var prop = this.IdProp();
      if (prop.Exists)
        prop.Set(id);
    }

    public void setNewID()
    {
      setID(getNewID());
    }

    public void setProperty(string propertyName, object propertyValue)
    {
      this.setProperty(propertyName, propertyValue, null);
    }

    public void setProperty(string propertyName, object propertyValue, string lang)
    {
      Property(propertyName, lang).Set(propertyValue);
    }

    public void setPropertyAttribute(string propertyName, string attributeName, string attributeValue)
    {
      this.setPropertyAttribute(propertyName, attributeName, attributeValue, null);
    }

    public void setPropertyAttribute(string propertyName, string attributeName, string attributeValue, string lang)
    {
      Property(propertyName, lang).Attribute(attributeName).Set(attributeValue);
    }

    public void setPropertyCondition(string propertyName, string condition)
    {
      this.setPropertyCondition(propertyName, condition, null);
    }

    public void setPropertyCondition(string propertyName, string condition, string lang)
    {
      this.setPropertyAttribute(propertyName, "condition", condition, lang);
    }

    public Item setPropertyItem(string propertyName, Item item)
    {
      Property(propertyName).Set(item.AssertItem());
      return item;
    }

    public void setRelatedItem(Item ritem)
    {
      this.setPropertyItem("related_id", ritem);
    }

    public void setType(string itemTypeName)
    {
      this.Type().Set(itemTypeName);
    }

    public Item unlockItem()
    {
      var id = AssertId();
      var type = AssertTypeName();
      var result = getInnovator().Apply(new Command("<Item isNew='1' isTemp='1' type='@0' action='unlock' id='@1' />", type, id));
      if (result.Exception == null)
        this.LockedById().Remove();
      return result;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the items in the collection.
    /// </summary>
    /// <returns>
    /// An enumerator that can be used to iterate through the items in the collection.
    /// </returns>
    public IEnumerator<Item> GetEnumerator()
    {
      return Items().Cast<Item>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private string AssertId()
    {
      var id = Id();
      if (string.IsNullOrEmpty(id))
        throw new Exception("Item ID is not set");
      return id;
    }

    private string AssertTypeName()
    {
      var type = TypeName();
      if (string.IsNullOrEmpty(type))
        throw new Exception("Item type is not set");
      return type;
    }
  }
}
#endif
