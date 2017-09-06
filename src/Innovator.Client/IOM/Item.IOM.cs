using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.IOM
{
  public partial class Item : IEnumerable<Item>
  {
    #region IOM
    /// <summary>
    /// Add specified item as a relationship item to the instance.
    /// </summary>
    /// <param name="item">Item(s) to be added to item's relationships.</param>
    public void addRelationship(Item item)
    {
      Relationships().Add(item.AssertItems());
    }

    /// <summary>
    /// Appends the item.
    /// </summary>
    /// <param name="item">Item to append.</param>
    public void appendItem(Item item)
    {
      ((IList<IReadOnlyItem>)_content).Add(item.AssertItem());
      if (_parent != null)
        _parent.Add(item.AssertItem());
    }

    /// <summary>
    /// Applies the AML.
    /// </summary>
    public Item apply()
    {
      return apply(null);
    }

    /// <summary>
    /// Sets the action property and applies the AML.
    /// </summary>
    /// <param name="action">The action.</param>
    public Item apply(string action)
    {
      string aml;
      if (string.IsNullOrEmpty(action))
      {
        aml = this.ToAml();
      }
      else
      {
        var item = Clone();
        item.Action().Set(action);
        aml = item.ToAml();
      }

      return new Item(_conn, _conn.Apply(aml));
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
      var result = Clone();
      var newId = _conn.AmlContext.NewId();
      result.Attribute("id").Set(newId);
      var prop = result.IdProp();
      if (prop.Exists)
        prop.Set(newId);
      result.Action().Set("add");

      if (!cloneRelationships)
      {
        result.Relationships().Remove();
      }
      else
      {
        foreach (var rel in result.Relationships())
        {
          newId = _conn.AmlContext.NewId();
          rel.Attribute("id").Set(newId);
          prop = rel.IdProp();
          if (prop.Exists)
            prop.Set(newId);
          rel.Action().Set("add");
        }
      }
      return new Item(_conn, result);
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
      // remove existing
      Property(propName).AsItem().Remove();
      var aml = _conn.AmlContext;
      var newItem = aml.Item(aml.Attribute("isNew", true), aml.Attribute("isTemp", true), aml.Type(type), aml.Action(action));
      Property(propName).Set(newItem);
      return new Item(_conn, newItem);
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
      var aml = _conn.AmlContext;
      var newItem = aml.Item(aml.Attribute("isNew", true), aml.Attribute("isTemp", true), aml.Type(type), aml.Action(action));
      Relationships().Add(newItem);
      return new Item(_conn, newItem);
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
      var props = _conn.Apply(@"<Item type='ItemType' action='get' select='id'>
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
      return (int)this.FetchLockStatus(_conn);
    }

    public Item fetchRelationships(string relationshipTypeName)
    {
      return fetchRelationships(relationshipTypeName, null, null);
    }

    public Item fetchRelationships(string relationshipTypeName, string selectList)
    {
      return fetchRelationships(relationshipTypeName, selectList, null);
    }

    public Item fetchRelationships(string relationshipTypeName, string selectList, string orderBy)
    {
      var id = Id();

      if (string.IsNullOrEmpty(id))
        throw new Exception("ID is not set");
      if (relationshipTypeName == null || relationshipTypeName.Trim().Length == 0)
        throw new ArgumentException("Relationship type is not specified");

      var rels = _conn.Apply("<Item type='@0' action='get' select='@1' orderBy='@2'><source_id>@3</source_id></Item>"
        , relationshipTypeName, selectList, orderBy, id).Items();

      var existing = Relationships(relationshipTypeName).ToArray();
      foreach (var rel in existing)
      {
        rel.Remove();
      }
      foreach (var rel in rels)
      {
        Relationships().Add(rel);
      }

      return this;
    }

    public string getAction()
    {
      return this.Action().Value;
    }

    public string getAttribute(string attributeName)
    {
      return this.getAttribute(attributeName, null);
    }

    public string getAttribute(string attributeName, string defaultValue)
    {
      return ((IReadOnlyItem)this).Attribute(attributeName).AsString(defaultValue);
    }

    public string getErrorCode()
    {
      var ex = Exception as ServerException;
      return ex?.FaultCode;
    }

    public string getErrorDetail()
    {
      var ex = Exception as ServerException;
      if (ex == null)
        return null;
      return ex.Fault.Element("detail").InnerText();
    }

    public string getErrorSource()
    {
      var ex = Exception as ServerException;
      if (ex == null)
        return null;
      return ex.Fault.Element("faultactor").Value;
    }

    public string getErrorString()
    {
      return Exception?.Message;
    }

    public string getID()
    {
      return Id();
    }

    public Innovator getInnovator()
    {
      return new Innovator(_conn);
    }

    public Item getItemByIndex(int index)
    {
      return new Item(_conn, Items().ElementAt(index));
    }

    public int getItemCount()
    {
      if (Exception != null)
        return Exception is NoItemsFoundException ? 0 : -1;

      var items = _content as IEnumerable<IReadOnlyItem>;
      if (items == null)
        return -1;

      return items.Count();
    }

    // public Item getItemsByXPath(string xpath)

    public int getLockStatus()
    {
      return (int)this.LockStatus(_conn);
    }

    // public Item getLogicalChildren()
    // public Item getLogicalItems()

    public string getNewID()
    {
      return _conn.AmlContext.NewId();
    }

    public Item getParentItem()
    {
      var itemParent = this.Parents().OfType<IReadOnlyItem>().FirstOrDefault();
      if (itemParent == null)
        return null;

      return new Item(_conn, itemParent);
    }

    public string getProperty(string propertyName)
    {
      return this.getProperty(propertyName, null, null);
    }

    public string getProperty(string propertyName, string defaultValue)
    {
      return this.getProperty(propertyName, defaultValue, null);
    }

    public string getProperty(string propertyName, string defaultValue, string lang)
    {
      return ((IReadOnlyItem)this).Property(propertyName, lang).AsString(defaultValue);
    }

    public string getPropertyAttribute(string propertyName, string attributeName)
    {
      return this.getPropertyAttribute(propertyName, attributeName, null, null);
    }

    public string getPropertyAttribute(string propertyName, string attributeName, string defaultValue)
    {
      return this.getPropertyAttribute(propertyName, attributeName, defaultValue, null);
    }

    public string getPropertyAttribute(string propertyName, string attributeName, string defaultValue, string lang)
    {
      return ((IReadOnlyItem)this).Property(propertyName, lang).Attribute(attributeName).AsString(defaultValue);
    }

    public string getPropertyCondition(string propertyName)
    {
      return this.getPropertyCondition(propertyName, null);
    }

    public string getPropertyCondition(string propertyName, string lang)
    {
      return this.getPropertyAttribute(propertyName, "condition", lang, null);
    }

    public Item getPropertyItem(string propertyName)
    {
      if (propertyName == "id")
        return this;

      var result = ((IReadOnlyItem)this).Property(propertyName, null).AsItem();
      if (result.Exists)
        return new Item(_conn, result);
      return null;
    }

    public Item getRelatedItem()
    {
      return getPropertyItem("related_id");
    }

    public string getRelatedItemID()
    {
      return getProperty("related_id");
    }

    public Item getRelationships()
    {
      return getRelationships(null);
    }

    public Item getRelationships(string itemTypeName)
    {
      var rels = Relationships();
      return new Item(_conn, rels.Where(r => string.IsNullOrEmpty(itemTypeName) || r.TypeName() == itemTypeName).OfType<IReadOnlyItem>().ToList(), rels);
    }

    public string getResult()
    {
      return Value;
    }

    public string getType()
    {
      return TypeName();
    }

    // public Item instantiateWorkflow(string workflowMapID)

    public bool isCollection()
    {
      return getItemCount() > 1;
    }

    public bool isEmpty()
    {
      return this.Exception is NoItemsFoundException;
    }

    public bool isError()
    {
      return this.Exception != null;
    }

    public bool isLogical()
    {
      return (_content as IEnumerable)?.OfType<ILogical>()?.Count() == 1;
    }

    public bool isNew()
    {
      return getItemCount() == 1 && Attribute("isNew").AsBoolean(false);
    }

    public bool isRoot()
    {
      return !this.Parent.Exists;
    }

    public void loadAML(string AML)
    {
      loadAML(_conn.AmlContext.FromXml(AML));
    }

    private void loadAML(IReadOnlyResult result)
    {
      _content = (object)result.Exception ?? (object)result.Value ?? result.Items().ToList();
    }

    //public Item lockItem()
    //{
    //  var item = AssertSingle<IItem>();
    //  var result = _conn.Lock(item.TypeName(), item.Id());
    //}

    // public Item newAND()

    public Item newItem()
    {
      return new Item(_conn);
    }

    public Item newItem(string itemTypeName)
    {
      var aml = _conn.AmlContext;
      return new Item(_conn, aml.Type(itemTypeName));
    }

    public Item newItem(string itemTypeName, string action)
    {
      var aml = _conn.AmlContext;
      return new Item(_conn, aml.Type(itemTypeName), aml.Action(action));
    }

    // public Item newNOT()

    // public Item newOR()

    // public XmlDocument newXMLDocument()

    public Item promote(string state, string comments)
    {
      if (state == null || state.Trim().Length == 0)
        throw new ArgumentException("'state' is either 'null' or an empty string");
      return new Item(_conn, this.Promote(_conn, state, comments));
    }

    public void removeAttribute(string attributeName)
    {
      Attribute(attributeName).Remove();
    }

    public void removeItem(Item item)
    {
      var list = _content as IList<IReadOnlyItem>;
      if (list == null)
        throw new Exception("Not a collection of items");
      if (item == null)
        throw new ArgumentNullException("item");

      list.Remove(item.AssertItem());
    }

    // public void removeLogical(Item logicalItem)

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

    // public void setErrorCode(string errcode)
    // public void setErrorDetail(string detail)
    // public void setErrorSource(string source)
    // public void setErrorString(string errorMessage)

    // public Item setFileProperty(string propertyName, string pathToFile)

    public void setID(string id)
    {
      Attribute("id").Set(id);
      var prop = this.IdProp();
      if (prop.Exists)
        prop.Set(id);
    }

    public void setNewId()
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

    // public Item unlockItem()
    #endregion

    /// <summary>
    /// Returns an enumerator that iterates through the items in the collection.
    /// </summary>
    /// <returns>
    /// An enumerator that can be used to iterate through the items in the collection.
    /// </returns>
    public IEnumerator<Item> GetEnumerator()
    {
      return Items().Select(i => new Item(_conn, i)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
