﻿using Innovator.Client;
using Innovator.Client.Connection;
using InnovatorAdmin.Connections;
using InnovatorAdmin.Controls;
using InnovatorAdmin.Editor;
using Nancy;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace InnovatorAdmin
{
  public partial class EditorWindow : Form, IUpdateListener
  {
    private const string GeneratedPage = "__GeneratedPage_";

    private Timer _clock = new Timer();
    private UiCommandManager _commands;
    private IPromise _currentQuery;
    private bool _disposeProxy = true;
    private Editor.AmlLinkElementGenerator _linkGenerator;
    private bool _loadingConnection = false;
    private string _locale;
    private DataSet _outputSet;
    private bool _outputTextSet = false;
    private bool _panelCollapsed;
    private Dictionary<string, QueryParameter> _paramCache = new Dictionary<string, QueryParameter>();
    private IEditorProxy _proxy;
    private IResultObject _result;
    private string _soapAction;
    private DateTime _start = DateTime.UtcNow;
    private int _timeout = Innovator.Client.Connection.DefaultHttpService.DefaultTimeout;
    private string _timeZone;
    private string _uid;
    private bool _updateCheckComplete = false;
    private IHttpService _webService = new DefaultHttpService();

    public bool AllowRun
    {
      get { return !splitEditors.Panel2Collapsed; }
      set { splitEditors.Panel2Collapsed  = !value; }
    }
    public OutputType PreferredMode { get; set; }
    public IEditorProxy Proxy
    {
      get { return _proxy; }
      set { SetProxy(value); }
    }

    public string Script
    {
      get { return inputEditor.Text; }
      set { inputEditor.Text = value; }
    }
    public string SoapAction
    {
      get { return _soapAction; }
      set
      {
        _soapAction = value;
        if (_proxy != null) _proxy.Action = value;
        btnSoapAction.Text = value + " ▼";
      }
    }
    public string Uid { get { return _uid; } }

    public EditorWindow()
    {
      InitializeComponent();

      this.KeyPreview = true;
      this.PreferredMode = OutputType.Any;

      _uid = GetUid();
      var assy = Assembly.GetExecutingAssembly().GetName().Version;
      this.lblVersion.Text = "v" + assy.ToString();

      var lastQuery = SnippetManager.Instance.LastQuery;
      inputEditor.Text = lastQuery.Text;
      inputEditor.CleanUndoStack();
      this.SoapAction = lastQuery.Action;
      menuStrip.Renderer = new SimpleToolstripRenderer();

      btnSoapAction.Visible = false;
      lblSoapAction.Visible = false;
      exploreButton.Visible = false;
      btnSubmit.Visible = false;

      treeItems.CanExpandGetter = m => ((IEditorTreeNode)m).HasChildren;
      treeItems.ChildrenGetter = m => ((IEditorTreeNode)m).GetChildren();
      colName.ImageGetter = m => ((IEditorTreeNode)m).ImageKey;

      treeItems.SmallImageList.Images.Add("class-16", InnovatorAdmin.Ide.TreeImages.class_16);
      treeItems.SmallImageList.Images.Add("folder-16", InnovatorAdmin.Ide.TreeImages.folder_16);
      treeItems.SmallImageList.Images.Add("folder-special-16", InnovatorAdmin.Ide.TreeImages.folder_special_16);
      treeItems.SmallImageList.Images.Add("property-16", InnovatorAdmin.Ide.TreeImages.property_16);
      treeItems.SmallImageList.Images.Add("xml-tag-16", InnovatorAdmin.Ide.TreeImages.xml_tag_16);

      _clock.Interval = 250;
      _clock.Tick += _clock_Tick;
      _panelCollapsed = Properties.Settings.Default.EditorWindowPanelCollapsed;
      UpdatePanelCollapsed();

      _linkGenerator = new Editor.AmlLinkElementGenerator();
      _linkGenerator.AmlLinkClicked += _linkGenerator_AmlLinkClicked;

      tbcOutputView.SelectedTab = pgTools;

      inputEditor.SelectionChanged += inputEditor_SelectionChanged;
      inputEditor.FindAllAction = res => SetResult(res, 0);
      inputEditor.BindToolStripItem(mniCut, System.Windows.Input.ApplicationCommands.Cut);
      inputEditor.BindToolStripItem(mniCopy, System.Windows.Input.ApplicationCommands.Copy);
      inputEditor.BindToolStripItem(mniPaste, System.Windows.Input.ApplicationCommands.Paste);
      inputEditor.BindToolStripItem(mniUndo, System.Windows.Input.ApplicationCommands.Undo);
      inputEditor.BindToolStripItem(mniRedo, System.Windows.Input.ApplicationCommands.Redo);

      // Wire up the commands
      _commands = new UiCommandManager(this);
      inputEditor.KeyDown += _commands.OnKeyDown;
      outputEditor.KeyDown += _commands.OnKeyDown;
      _commands.Add<Control>(btnEditConnections, e => e.KeyCode == Keys.O && e.Control, ChangeConnection);
      _commands.Add<Control>(btnSoapAction, e => e.KeyCode == Keys.M && e.Control, ChangeSoapAction);
      _commands.Add<Control>(mniNewWindow, e => e.KeyCode == Keys.N && e.Control, c => NewWindow().Show());
      _commands.Add<Editor.FullEditor>(mniFind, null, c => c.Find());
      _commands.Add<Editor.FullEditor>(mniFindNext, null, c => c.FindNext());
      _commands.Add<Editor.FullEditor>(mniFindPrevious, null, c => c.FindPrevious());
      _commands.Add<Editor.FullEditor>(mniReplace, null, c => c.Replace());
      _commands.Add<Editor.FullEditor>(mniGoTo, e => e.KeyCode == Keys.G && e.Control, c =>
      {
        using (var dialog = new InputBox())
        {
          dialog.Caption = "Go To Line";
          dialog.Message = string.Format("Line Number (1 - {0}):", c.Editor.LineCount);
          int line;
          if (dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK
            && int.TryParse(dialog.Value, out line)
            && line >= 1
            && line <= c.Editor.LineCount)
          {
            var targetLine = c.Editor.TextArea.TextView.GetOrConstructVisualLine(c.Editor.Document.GetLineByNumber(line));
            var docHeight = c.Editor.TextArea.TextView.DocumentHeight;
            var winHeight = c.Editor.TextArea.TextView.ActualHeight;
            var target = Math.Min(docHeight, Math.Max(0, (int)(targetLine.VisualTop - (winHeight - targetLine.Height) / 2.0)));
            c.Editor.ScrollToVerticalOffset(target);
            c.Editor.TextArea.Caret.Line = line;
          }
        }
      });
      _commands.Add<Editor.FullEditor>(mniMd5Encode, null, c => c.ReplaceSelectionSegments(t => ConnectionDataExtensions.CalcMD5(t)));
      _commands.Add<Editor.FullEditor>(mniDoubleToSingleQuotes, null, c => c.ReplaceSelectionSegments(t => t.Replace('"', '\'')));
      _commands.Add<Editor.FullEditor>(mniSingleToDoubleQuotes, null, c => c.ReplaceSelectionSegments(t => t.Replace('\'', '"')));
      _commands.Add<Editor.FullEditor>(mniUppercase, null, c => c.TransformUppercase());
      _commands.Add<Editor.FullEditor>(mniLowercase, null, c => c.TransformLowercase());
      _commands.Add<Editor.FullEditor>(mniMoveUpCurrentLine, null, c => c.MoveLineUp());
      _commands.Add<Editor.FullEditor>(mniMoveDownCurrentLine, null, c => c.MoveLineDown());
      _commands.Add<Editor.FullEditor>(mniInsertNewGuid, null, c => c.ReplaceSelectionSegments(t => Guid.NewGuid().ToString("N").ToUpperInvariant()));
      _commands.Add<Editor.FullEditor>(mniXmlToEntity, null, c => c.ReplaceSelectionSegments(t => {
          try
          {
            var sb = new System.Text.StringBuilder();
            var settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            using (var strWriter = new StringWriter(sb))
            using (var writer = XmlWriter.Create(strWriter, settings))
            {
              writer.WriteStartElement("a");
              writer.WriteValue(t);
              writer.WriteEndElement();
            }
            return sb.ToString(3, sb.Length - 7);
          }
          catch (XmlException)
          {
            return t.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;");
          }
        }));
      _commands.Add<Editor.FullEditor>(mniEntityToXml, null, c => c.ReplaceSelectionSegments(t => {
          try
          {
            var xml = "<a>" + t + "</a>";
            using (var strReader = new StringReader(xml))
            using (var reader = XmlReader.Create(strReader))
            {
              while (reader.Read())
              {
                if (reader.NodeType == XmlNodeType.Text)
                {
                  return reader.Value;
                }
              }
            }
          }
          catch (XmlException)
          {
            return t.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
          }
          return t;
        }));
    }

    public void SetConnection(IAsyncConnection conn, string name = null)
    {
      if (conn == null) throw new ArgumentNullException("conn");
      exploreButton.Visible = true;
      mniLocale.Visible = true;
      mniTimeZone.Visible = true;
      SetProxy(new ArasEditorProxy(conn, name ?? conn.Database));
      _disposeProxy = false;
    }

    public void SetConnection(ConnectionData conn)
    {
      if (!_loadingConnection && !string.IsNullOrEmpty(conn.Url)
        && !string.IsNullOrEmpty(conn.Database))
      {
        _loadingConnection = true;
        btnSoapAction.Visible = false;
        lblSoapAction.Visible = false;
        exploreButton.Visible = false;
        btnSubmit.Visible = false;

        try
        {
          btnEditConnections.Text = "Connecting... ▼";
          ProxyFactory.FromConn(conn)
            .UiPromise(this)
            .Done(proxy =>
            {
              btnSubmit.Visible = true;
              SetProxy(proxy);
              _disposeProxy = true;
            })
            .Fail(ex =>
            {
              lblItems.Text = ex.Message;
              btnEditConnections.Text = "Not Connected ▼";
              lblConnection.Visible = true;
              btnEditConnections.Visible = lblConnection.Visible;
            }).Always(() => _loadingConnection = false);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    public void SetProxy(IEditorProxy proxy)
    {
      DisposeProxy();

      _proxy = proxy;

      if (proxy == null)
        return;

      if (_proxy.GetActions().Any())
      {
        _proxy.Action = _soapAction;
        btnSoapAction.Visible = true;
      }
      else
      {
        btnSoapAction.Visible = false;
      }
      lblSoapAction.Visible = btnSoapAction.Visible;

      inputEditor.Helper = _proxy.GetHelper();
      outputEditor.Helper = _proxy.GetHelper();
      btnEditConnections.Text = string.Format("{0} ▼", _proxy.Name);

      if (proxy.ConnData != null)
      {
        lblConnection.Visible = true;
        btnEditConnections.Visible = lblConnection.Visible;
        lblConnColor.BackColor = proxy.ConnData.Color;
      }
      InitializeUi(_proxy as ArasEditorProxy);
      treeItems.Roots = null;
      treeItems.RebuildAll(false);
      _proxy.GetNodes()
        .UiPromise(this)
        .Done(r =>
        {
          treeItems.Roots = r;
        });
    }

    public static IEnumerable<ItemReference> GetItems(Connections.ConnectionData conn)
    {
      using (var dialog = new EditorWindow())
      {
        dialog.dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dialog.tbcOutputView.Appearance = TabAppearance.FlatButtons;
        dialog.tbcOutputView.ItemSize = new Size(0, 1);
        dialog.tbcOutputView.SelectedTab = dialog.pgTableOutput;
        dialog.tbcOutputView.SizeMode = TabSizeMode.Fixed;
        dialog.SetConnection(conn);
        dialog.PreferredMode = OutputType.Table;
        if (dialog.ShowDialog() == DialogResult.OK
          && dialog._outputSet.Tables[0].Columns.Contains(Extensions.AmlTable_TypeName)
          && dialog._outputSet.Tables[0].Columns.Contains("id"))
        {
          return dialog.dgvItems.SelectedRows
                       .OfType<DataGridViewRow>()
                       .Where(r => r.Index != r.DataGridView.NewRowIndex)
                       .Select(r => ((DataRowView)r.DataBoundItem).Row)
                       .Select(r => new ItemReference((string)r[Extensions.AmlTable_TypeName], (string)r["id"])
                       {
                         KeyedName = dialog._outputSet.Tables[0].Columns.Contains("keyed_name") ? (string)r["keyed_name"] : null
                       }).ToList();
        }
        return Enumerable.Empty<ItemReference>();
      }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
      base.OnFormClosed(e);
      SaveFormBounds();
      DisposeProxy();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);
      _commands.OnKeyDown(this, e);
    }

    protected override void OnLoad(EventArgs e)
    {
      try
      {
        base.OnLoad(e);
        btnOk.Visible = this.Modal;
        btnCancel.Visible = this.Modal;

        //lblConnection.Visible = _proxy != null && _proxy.ConnData != null;
        lblConnection.Visible = true;
        btnEditConnections.Visible = lblConnection.Visible;

        if (_proxy == null)
        {
          var conn = ConnectionManager.Current.Library.Connections
            .FirstOrDefault(c => c.ConnectionName == Properties.Settings.Default.LastConnection)
            ?? ConnectionManager.Current.Library.Connections.FirstOrDefault();
          if (conn != null)
            SetConnection(conn);
        }

        var bounds = Properties.Settings.Default.EditorWindow_Bounds;
        if (bounds.Width < 100 || bounds.Height < 100)
        {
          // Do nothing
        }
        else if (bounds != Rectangle.Empty && bounds.IntersectsWith(SystemInformation.VirtualScreen))
        {
          this.DesktopBounds = bounds;
        }
        else
        {
          this.Size = bounds.Size;
        }

        inputEditor.Focus();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }
    protected override void OnMove(EventArgs e)
    {
      base.OnMove(e);
      SaveFormBounds();
    }

    protected override void OnResizeEnd(EventArgs e)
    {
      base.OnResizeEnd(e);
      SaveFormBounds();
    }

    void inputEditor_SelectionChanged(object sender, Editor.SelectionChangedEventArgs e)
    {
      try
      {
        if (e.SelectionLength < 1)
        {
          lblSelection.Text = string.Format("Ln: {0}  Col: {1}  Sel: {2} ['' = 0x0 = 0]"
            , e.CaretLine, e.CaretColumn, e.SelectionLength);
        }
        else
        {
          var text = e.GetText(1);
          lblSelection.Text = string.Format("Ln: {0}  Col: {1}  Sel: {2} ['{3}' = 0x{4:x} = {4}]"
            , e.CaretLine, e.CaretColumn, e.SelectionLength, text, (int)text[0]);
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }


    void _linkGenerator_AmlLinkClicked(object sender, Editor.AmlLinkClickedEventArgs e)
    {
      try
      {
        var window = NewWindow();
        var query = "<Item type='" + e.Type + "' action='get' id='" + e.Id + "' levels='1' />";
        window.inputEditor.Text = query;
        window.SoapAction = "ApplyItem";
        window.Submit(query);
        window.Show();
        Task.Delay(200).ContinueWith(_ => window.Activate(), TaskScheduler.FromCurrentSynchronizationContext());
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    void _clock_Tick(object sender, EventArgs e)
    {
      lblItems.Text = string.Format(@"Processing... {0:hh\:mm\:ss}", DateTime.UtcNow - _start);
    }
    private void ConfigureRequest(IHttpRequest req)
    {
      req.SetHeader("LOCALE", _locale);
      req.SetHeader("TIMEZONE_NAME", _timeZone);
      req.Timeout = _timeout;
    }

    private void DisposeProxy()
    {
      if (_proxy != null)
      {
        var arasProxy = _proxy as ArasEditorProxy;
        if (arasProxy != null)
        {
          var remote = arasProxy.Connection as IRemoteConnection;
          if (remote != null)
            remote.DefaultSettings(r => { });
        }
        if (_disposeProxy) _proxy.Dispose();
      }
    }
    private void EnsureDataTable()
    {
      if (_outputSet == null && _result != null && tbcOutputView.SelectedTab == pgTableOutput)
      {
        _outputSet = _result.GetDataSet();
        if (_outputSet.Tables.Count > 0)
        {
          dgvItems.DataSource = _outputSet.Tables[0];
          FormatDataGrid(dgvItems);
          pgTableOutput.Text = _outputSet.Tables[0].TableName;

          var i = 1;
          foreach (var tbl in _outputSet.Tables.OfType<DataTable>().Skip(1))
          {
            var pg = new TabPage(tbl.TableName);
            pg.Name = GeneratedPage + i.ToString();
            var grid = new DataGridView();
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.ContextMenuStrip = this.conTable;
            grid.DataSource = tbl;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Location = new System.Drawing.Point(0, 0);
            grid.Margin = new System.Windows.Forms.Padding(0);
            grid.TabIndex = 0;
            grid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvItems_CellFormatting);
            pg.Controls.Add(grid);
            tbcOutputView.TabPages.Add(pg);
            FormatDataGrid(grid);
            i++;
          }
        }
      }
    }

    private void EnsureTextResult()
    {
      if (!_outputTextSet && _result != null && tbcOutputView.SelectedTab == pgTableOutput)
      {
        _outputTextSet = true;

        var text = _result.GetTextSource();
        outputEditor.Document.Replace(0, outputEditor.Document.TextLength, text);

        if (_result.ItemCount > 1 && outputEditor.Editor.LineCount > 100)
        {
          outputEditor.CollapseAll();
        }
      }
    }

    private void FormatDataGrid(DataGridView grid)
    {
      grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader);
      var minWidths = grid.Columns.OfType<DataGridViewColumn>().Select(c => c.Width).ToArray();
      grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
      var maxWidths = grid.Columns.OfType<DataGridViewColumn>().Select(c => c.Width).ToArray();
      var maxWidth = (int)(grid.Width * 0.8);

      DataColumn boundColumn;
      for (var i = 0; i < grid.Columns.Count; i++)
      {
        grid.Columns[i].Width = Math.Min(maxWidths[i] < 100 ? maxWidths[i] :
            (maxWidths[i] < minWidths[i] + 60 ? maxWidths[i] : minWidths[i] + 60)
          , maxWidth);
        grid.Columns[i].DefaultCellStyle.Alignment =
          (IsNumericType(grid.Columns[i].ValueType)
            ? DataGridViewContentAlignment.TopRight
            : DataGridViewContentAlignment.TopLeft);
        boundColumn = ((DataTable)grid.DataSource).Columns[grid.Columns[i].DataPropertyName];
        grid.Columns[i].HeaderText = boundColumn.Caption;
        grid.Columns[i].Visible = boundColumn.IsUiVisible();
      }

      if (!grid.Columns.OfType<DataGridViewColumn>().Any(c => c.Visible))
      {
        foreach (var col in grid.Columns.OfType<DataGridViewColumn>())
        {
          col.Visible = true;
        }
      }

      var orderedColumns = grid.Columns.OfType<DataGridViewColumn>()
        .Select(c => new
        {
          Column = c,
          SortOrder = ((DataTable)grid.DataSource).Columns[c.DataPropertyName].SortOrder()
        })
        .OrderBy(c => c.SortOrder)
        .ThenBy(c => c.Column.HeaderText)
        .Select((c, i) => new { Column = c.Column, Index = i })
        .ToArray();
      foreach (var col in orderedColumns)
      {
        col.Column.DisplayIndex = col.Index;
      }

      grid.AllowUserToAddRows = _outputSet != null
        && ((DataTable)grid.DataSource).Columns.Contains("id")
        && ((DataTable)grid.DataSource).Columns.Contains(Extensions.AmlTable_TypeName);
      grid.AllowUserToDeleteRows = grid.AllowUserToAddRows;
      grid.ReadOnly = !grid.AllowUserToAddRows;
    }

    private string GetUid()
    {
      return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace('+', '-').Replace('/', '_');
    }
    private void InitializeUi(ArasEditorProxy proxy)
    {
      if (proxy == null || proxy.Connection.AmlContext == null)
      {
        outputEditor.ElementGenerators.Remove(_linkGenerator);
        return;
      }

      exploreButton.Visible = true;
      mniLocale.Visible = true;
      mniTimeZone.Visible = true;

      if (!outputEditor.ElementGenerators.Contains(_linkGenerator))
        outputEditor.ElementGenerators.Add(_linkGenerator);

      var local = proxy.Connection.AmlContext.LocalizationContext;
      var remote = proxy.Connection as IRemoteConnection;
      _locale = local.Locale;
      _timeZone = local.TimeZone;
      mniLocale.ShortcutKeyDisplayString = "(" + _locale + ")";
      mniTimeZone.ShortcutKeyDisplayString = "(" + _timeZone + ")";
      mniTimeout.ShortcutKeyDisplayString = "(" + (_timeout / 1000) + "s)";

      mniLocale.Enabled = remote != null;
      mniTimeZone.Enabled = mniLocale.Enabled;
      mniTimeout.Visible = mniLocale.Enabled;

      if (remote != null)
      {
        remote.DefaultSettings(ConfigureRequest);
      }
    }

    private bool IsNumericType(Type type)
    {
      return type == typeof(byte)
        || type == typeof(short) || type == typeof(ushort)
        || type == typeof(int) || type == typeof(uint)
        || type == typeof(long) || type == typeof(ulong)
        || type == typeof(float) || type == typeof(double)
        || type == typeof(decimal);
    }

    private void SaveFormBounds()
    {
      if (this.WindowState == FormWindowState.Normal)
      {
        Properties.Settings.Default.EditorWindow_Bounds = this.DesktopBounds;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Reload();
      }
    }

    private void UpdatePanelCollapsed()
    {
      splitMain.IsSplitterFixed = _panelCollapsed;
      treeItems.Visible = !_panelCollapsed;
      if (_panelCollapsed)
      {
        btnPanelToggle.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        btnPanelToggle.Orientation = Orientation.Vertical;
        splitMain.SplitterDistance = btnPanelToggle.Width;
      }
      else
      {
        btnPanelToggle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnPanelToggle.Orientation = Orientation.Horizontal;
        btnPanelToggle.Height = 25;
        splitMain.SplitterDistance = 220;
      }
    }

    #region Run Query
    private void btnSubmit_Click(object sender, System.EventArgs e)
    {
      try
      {
        Submit(inputEditor.Text);
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }


    private void mniRunAll_Click(object sender, EventArgs e)
    {
      try
      {
        Submit(inputEditor.Editor.Text);
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniRunCurrent_Click(object sender, EventArgs e)
    {
      try
      {
        Submit(inputEditor.Helper.GetCurrentQuery(inputEditor.Text, inputEditor.Editor.CaretOffset)
          ?? inputEditor.Text);
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }


    private void mniRunCurrentNewWindow_Click(object sender, EventArgs e)
    {
      try
      {
        var window = NewWindow();
        var query = inputEditor.Helper.GetCurrentQuery(inputEditor.Text, inputEditor.Editor.CaretOffset)
          ?? inputEditor.Text;
        window.inputEditor.Text = query;
        window.SoapAction = this.SoapAction;
        window.Submit(query);
        window.Show();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private EditorWindow NewWindow()
    {
      var window = new EditorWindow();
      window.SetProxy(_proxy.Clone());
      window._disposeProxy = false;
      return window;
    }

    private void Submit(string query)
    {
      if (_currentQuery != null)
      {
        _currentQuery.Cancel();
        _clock.Enabled = false;
        lblItems.Text = "Canceled";
        _currentQuery = null;
        outputEditor.Text = "";
        lblItems.Text = "";
        _clock.Enabled = false;
        btnSubmit.Text = "► Run";
        return;
      }
      if (_proxy.ConnData.Confirm)
      {
        if (MessageBox.Show("Do you want to run this query on " + _proxy.ConnData.ConnectionName +"?", "Confirm Execution", MessageBoxButtons.YesNo) == DialogResult.No)
        {
          return;
        }
      }
      try
      {
        outputEditor.Text = "Processing...";
        lblItems.Text = "Processing...";
        _start = DateTime.UtcNow;
        _clock.Enabled = true;
        _outputTextSet = false;
        btnSubmit.Text = "Cancel";

        var cmd = _proxy.NewCommand().WithQuery(query).WithAction(this.SoapAction);
        var queryParams = _proxy.GetHelper().GetParameterNames(query)
          .Select(p => GetCreateParameter(p)).ToList();
        if (queryParams.Any())
        {
          using (var dialog = new ParameterWindow(queryParams))
          {
            switch (dialog.ShowDialog(this))
            {
              case System.Windows.Forms.DialogResult.OK:
                foreach (var param in queryParams)
                {
                  cmd.WithParam(param.Name, param.GetValue());
                }
                break;
              case System.Windows.Forms.DialogResult.Ignore:
                break;
              default:
                return;
            }
          }
        }

        _result = null;
        _outputSet = null;
        var pagesToRemove = tbcOutputView.TabPages.OfType<TabPage>()
                .Where(p => p.Name.StartsWith(GeneratedPage)).ToArray();
        foreach (var page in pagesToRemove)
        {
          tbcOutputView.TabPages.Remove(page);
        }
        pgTableOutput.Text = "Table";
        dgvItems.DataSource = null;

        SnippetManager.Instance.LastQuery = new Snippet()
        {
          Action = this.SoapAction,
          Text = inputEditor.Text
        };
        Properties.Settings.Default.LastConnection = _proxy.ConnData.ConnectionName;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Reload();

        var st = Stopwatch.StartNew();
        _currentQuery = _proxy.Process(cmd, true)
          .UiPromise(this)
          .Done(result =>
          {
            try
            {
              var milliseconds = st.ElapsedMilliseconds;
              _clock.Enabled = false;

              SetResult(result, milliseconds);
            }
            catch (Exception ex)
            {
              Utils.HandleError(ex);
            }
          }).Fail(ex =>
          {
            outputEditor.Text = ex.Message;
            tbcOutputView.SelectedTab = pgTextOutput;
            lblItems.Text = "Error";
          })
          .Always(() =>
          {
            _clock.Enabled = false;
            _currentQuery = null;
            btnSubmit.Text = "► Run";
          });
      }
      catch (Exception err)
      {
        outputEditor.Text = err.Message;
        tbcOutputView.SelectedTab = pgTextOutput;
        _clock.Enabled = false;
        lblItems.Text = "Error";
        _currentQuery = null;
        btnSubmit.Text = "► Run";
      }
    }

    private void SetResult(IResultObject result, long milliseconds)
    {
      _outputTextSet = false;
      dgvItems.DataSource = null;
      _outputSet = null;

      var mode = this.PreferredMode;
      if (mode == OutputType.Any)
        mode = result.PreferredMode;

      _result = result;
      if (result.ItemCount > 0)
      {
        lblItems.Text = string.Format("{0} item(s) found in {1} ms.", result.ItemCount, milliseconds);
      }
      else
      {
        lblItems.Text = string.Format("No items found in {0} ms.", milliseconds);
      }

      if (result.PreferredMode == OutputType.Table && result.ItemCount > 0)
      {
        tbcOutputView.SelectedTab = pgTableOutput;
        EnsureDataTable();
      }
      else if (result.PreferredMode == OutputType.Html)
      {
        browser.Navigate(GetReportUri().ToString());
        tbcOutputView.SelectedTab = pgHtml;
      }
      else
      {
        tbcOutputView.SelectedTab = pgTextOutput;
        EnsureTextResult();
      }

      inputEditor.Editor.Focus();
      this.Text = result.Title + " [AmlStudio]";
    }
    #endregion

    private QueryParameter GetCreateParameter(string name)
    {
      QueryParameter result;
      if (_paramCache.TryGetValue(name, out result))
        return result;
      result = new QueryParameter() { Name = name };
      _paramCache[name] = result;
      return result;
    }

    private void ChangeConnection(Control active)
    {
      using (var dialog = new ConnectionEditorForm())
      {
        dialog.Multiselect = false;
        if (_proxy != null && _proxy.ConnData != null)
          dialog.SetSelected(_proxy.ConnData);
        if (dialog.ShowDialog(this, menuStrip.RectangleToScreen(btnEditConnections.Bounds)) ==
          System.Windows.Forms.DialogResult.OK)
        {
          SetConnection(dialog.SelectedConnections.First());
        }
      }
    }

    private void tbcOutputView_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        EnsureDataTable();
        EnsureTextResult();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private class AmlAction
    {
      private Stopwatch _stopwatch = Stopwatch.StartNew();

      public string SoapAction { get; set; }
      public string Aml { get; set; }
      public string Output { get; set; }
      public Stopwatch Stopwatch
      {
        get { return _stopwatch; }
      }
    }

    private void inputEditor_RunRequested(object sender, Editor.RunRequestedEventArgs e)
    {
      try
      {
        Submit(e.Query);
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void ChangeSoapAction(Control active)
    {
      if (_proxy == null) return;

      using (var dialog = new FilterSelect<string>())
      {
        dialog.DataSource = _proxy.GetActions();
        dialog.Message = "Select an action to perform";
        if (dialog.ShowDialog(this, menuStrip.RectangleToScreen(btnSoapAction.Bounds)) ==
          DialogResult.OK && dialog.SelectedItem != null)
        {
          this.SoapAction = dialog.SelectedItem;
        }
      }
    }

    #region Table Handling

    private void mniAcceptChanges_Click(object sender, EventArgs e)
    {
      try
      {
        _outputSet.AcceptChanges();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniResetChanges_Click(object sender, EventArgs e)
    {
      try
      {
        _outputSet.RejectChanges();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniTableEditsToClipboard_Click(object sender, EventArgs e)
    {
      try
      {
        var aml = GetTableChangeAml((DataTable)tbcOutputView.SelectedTab.Controls.OfType<DataGridView>().Single().DataSource);
        if (!string.IsNullOrEmpty(aml))
          Clipboard.SetText(aml);
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniTableEditsToFile_Click(object sender, EventArgs e)
    {
      try
      {
        using (var dialog = new SaveFileDialog())
        {
          if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          {
            System.IO.File.WriteAllText(dialog.FileName, GetTableChangeAml((DataTable)tbcOutputView.SelectedTab.Controls.OfType<DataGridView>().Single().DataSource));
          }
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniTableEditsToQueryEditor_Click(object sender, EventArgs e)
    {
      try
      {
        var window = NewWindow();
        window.inputEditor.Text = GetTableChangeAml((DataTable)tbcOutputView.SelectedTab.Controls.OfType<DataGridView>().Single().DataSource);
        window.SoapAction = this.SoapAction;
        window.Show();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private string GetTableChangeAml(DataTable table)
    {
      var arasProxy = _proxy as ArasEditorProxy;
      var context = arasProxy == null
        ? ElementFactory.Local.LocalizationContext
        : arasProxy.Connection.AmlContext.LocalizationContext;

      var changes = table.GetChanges(DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
      if (changes == null)
        return string.Empty;

      var settings = new System.Xml.XmlWriterSettings();
      settings.OmitXmlDeclaration = true;
      settings.Indent = true;
      settings.IndentChars = "  ";

      var types = table.AsEnumerable()
        .Select(r => r.CellValue(Extensions.AmlTable_TypeName).ToString())
        .Where(t => !string.IsNullOrEmpty(t))
        .Distinct().ToList();
      var singleType = types.Count == 1 ? types[0] : null;
      object newValue;

      using (var writer = new System.IO.StringWriter())
      using (var xml = XmlWriter.Create(writer, settings))
      {
        xml.WriteStartElement("AML");
        foreach (var row in changes.AsEnumerable())
        {
          xml.WriteStartElement("Item");
          xml.WriteAttributeString("type", singleType ?? row.CellValue(Extensions.AmlTable_TypeName).ToString());
          xml.WriteAttributeString("id", row.CellIsNull("id")
            ? Guid.NewGuid().ToString("N").ToUpperInvariant()
            : row.CellValue("id").ToString());

          switch (row.RowState)
          {
            case DataRowState.Added:
              xml.WriteAttributeString("action", "add");
              foreach (var column in changes.Columns.OfType<DataColumn>())
              {
                if (!row.IsNull(column) && !column.ColumnName.Contains('/'))
                {
                  xml.WriteElementString(column.ColumnName, context.Format(row[column]));
                }
              }
              break;
            case DataRowState.Deleted:
              xml.WriteAttributeString("action", "delete");
              break;
            case DataRowState.Modified:
              xml.WriteAttributeString("action", "edit");
              foreach (var column in changes.Columns.OfType<DataColumn>())
              {
                if (!column.ColumnName.Contains('/')
                  && IsChanged(row, column, out newValue))
                {
                  xml.WriteElementString(column.ColumnName, context.Format(newValue));
                }
              }
              break;
          }
          xml.WriteEndElement();
        }
        xml.WriteEndElement();
        xml.Flush();
        return writer.ToString() ?? string.Empty;
      }
    }


    private bool IsChanged(DataRow row, DataColumn col, out object newValue)
    {
      newValue = null;
      if (!row.HasVersion(DataRowVersion.Original) || !row.HasVersion(DataRowVersion.Current))
        return false;
      var orig = row[col, DataRowVersion.Original];
      var curr = row[col, DataRowVersion.Current];

      if (orig == DBNull.Value && curr == DBNull.Value)
        return false;

      newValue = curr;
      if (orig == DBNull.Value && curr == DBNull.Value)
        return true;

      return !orig.Equals(curr);
    }

    #endregion


    private void mniTidyXml_Click(object sender, EventArgs e)
    {
      try
      {
        if (outputEditor.ContainsFocus)
        {
          outputEditor.TidyXml();
        }
        else
        {
          inputEditor.TidyXml();
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniClose_Click(object sender, EventArgs e)
    {
      try
      {
        this.Close();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniTimeZone_Click(object sender, EventArgs e)
    {
      try
      {
        using (var dialog = new FilterSelect<string>())
        {
          dialog.DataSource = TimeZoneInfo.GetSystemTimeZones().Select(t => t.Id).ToList();
          dialog.Message = "Select a time zone";
          if (dialog.ShowDialog(this) ==
            DialogResult.OK && dialog.SelectedItem != null)
          {
            _timeZone = dialog.SelectedItem;
            mniTimeZone.ShortcutKeyDisplayString = _timeZone;
          }
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniLocale_Click(object sender, EventArgs e)
    {
      try
      {
        using (var dialog = new FilterSelect<string>())
        {
          dialog.DataSource = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c => c.Name).ToList();
          dialog.Message = "Select a locale";
          if (dialog.ShowDialog(this) ==
            DialogResult.OK && dialog.SelectedItem != null)
          {
            _locale = dialog.SelectedItem;
            mniLocale.ShortcutKeyDisplayString = _locale;
          }
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniTimeout_Click(object sender, EventArgs e)
    {
      try
      {
        using (var dialog = new InputBox())
        {
          dialog.Caption = "Select Timeout";
          dialog.Message = "Specify the desired timeout (in seconds)";
          dialog.Value = (_timeout < 0 ? -1 : _timeout / 1000).ToString();
          int newTimeout;
          if (dialog.ShowDialog(this) == DialogResult.OK && int.TryParse(dialog.Value, out newTimeout))
          {
            if (newTimeout < 0)
            {
              _timeout = -1;
              mniTimeout.ShortcutKeyDisplayString = "(none)";
            }
            else
            {
              _timeout = newTimeout * 1000;
              mniTimeout.ShortcutKeyDisplayString = "(" + newTimeout.ToString() + "s)";
            }
          }
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }
    private void exploreButton_Click(object sender, EventArgs e)
    {
      try
      {
        if (_proxy != null && _proxy.ConnData != null)
        {
          var connData = _proxy.ConnData;
          connData.Explore();
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void dgvItems_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      try
      {
        if (e.Value == e.CellStyle.DataSourceNullValue)
        {
          e.CellStyle.BackColor = Color.AntiqueWhite;
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }


    private void treeItems_CellRightClick(object sender, BrightIdeasSoftware.CellRightClickEventArgs e)
    {
      try
      {
        var node = e.Model as IEditorTreeNode;
        var scripts = node.GetScripts();
        if (node != null && scripts.Any())
        {
          var con = new ContextMenuStrip();
          foreach (var script in scripts)
          {
            con.Items.Add(new ToolStripMenuItem(script.Name, null, (s, ev) =>
            {
              this.SoapAction = script.Action;
              this.Script = script.Script;
            }));
          }
          con.Show(treeItems.PointToScreen(e.Location));
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void treeItems_CellToolTipShowing(object sender, BrightIdeasSoftware.ToolTipShowingEventArgs e)
    {
      try
      {
        var node = (IEditorTreeNode)e.Model;
        if (e.Column.AspectName == "Name" && !string.IsNullOrWhiteSpace(node.Description))
        {
          e.Text = node.Description;
        }
      }
      catch (Exception) { }
    }

    private void treeItems_Expanding(object sender, BrightIdeasSoftware.TreeBranchExpandingEventArgs e)
    {
      try
      {
        if (treeItems.CanExpand(e.Model) && !treeItems.GetChildren(e.Model).OfType<IEditorTreeNode>().Any())
          treeItems.RefreshObject(e.Model);
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void treeItems_ModelDoubleClick(object sender, ModelDoubleClickEventArgs e)
    {
      try
      {
        var node = e.Model as IEditorTreeNode;
        if (node != null && node.GetScripts().Any())
        {
          var script = node.GetScripts().First();
          this.SoapAction = script.Action;
          this.Script = script.Script;
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void btnPanelToggle_Click(object sender, EventArgs e)
    {
      try
      {
        _panelCollapsed = !_panelCollapsed;
        UpdatePanelCollapsed();
        Properties.Settings.Default.EditorWindowPanelCollapsed = _panelCollapsed;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Reload();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void btnInstall_Click(object sender, EventArgs e)
    {
      try
      {
        var main = new Main();
        main.GoToStep(new InstallSource());
        main.Show();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void btnCreate_Click(object sender, EventArgs e)
    {
      try
      {
        var main = new Main();
        var connSelect = new ConnectionSelection();
        connSelect.MultiSelect = false;
        connSelect.GoNextAction = () => main.GoToStep(new ExportSelect());
        main.GoToStep(connSelect);
        main.Show();
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    private void mniColumns_Click(object sender, EventArgs e)
    {
      try
      {
        using (var dialog = new ColumnSelect())
        {
          dialog.DataSource = dgvItems;
          dialog.ShowDialog(this);
        }
      }
      catch (Exception ex)
      {
        Utils.HandleError(ex);
      }
    }

    public Response GetResponse(NancyContext context, string rootPath)
    {
      if (context.Request.Method != "GET" || _result == null)
        return new Response().WithStatusCode(HttpStatusCode.InternalServerError);

      if (string.Equals(context.Request.Url.Path, GetReportUri().LocalPath))
      {
        var resp = new Response().WithStatusCode(HttpStatusCode.OK);
        resp.ContentType = "text/html";
        resp.Contents = s =>
        {
          using (var writer = new StreamWriter(s))
          {
            writer.Write(_result.Html);
          }
        };
        return resp;
      }
      else
      {
        var arasProxy = _proxy as ArasEditorProxy;
        if (arasProxy != null && arasProxy.Connection != null)
        {
          var reportUrlBase = GetReportUri().LocalPath;
          var idx = reportUrlBase.IndexOf("/Client/") + 8;
          var relativeUrl = "../" + context.Request.Url.Path.Substring(idx);
          var absUrl = arasProxy.Connection.MapClientUrl(relativeUrl);
          var pResp = _webService.Execute("GET", absUrl, null, null, false, null).Wait();
          var resp = new Response().WithStatusCode((int)pResp.StatusCode);
          resp.ContentType = pResp.Headers["Content-Type"];
          resp.Contents = s => pResp.AsStream.CopyTo(s);
          return resp;
        }
      }

      return new Response().WithStatusCode(HttpStatusCode.NotFound);
    }

    private Uri GetReportUri()
    {
      return new Uri("http://localhost:" + Program.PortNumber + "/" + Uid + "/Client/Scripts/report.html");
    }

    public void UpdateCheckComplete(Version latestVersion)
    {
      try
      {
        _updateCheckComplete = true;
        var currVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        if (latestVersion == default(Version))
        {
          this.lblVersion.Text = string.Format("v{0} (No updates available)", currVer);
        }
        else
        {
          var newVer = latestVersion.ToString();

          if (newVer != currVer)
          {
            this.lblVersion.Text = string.Format("v{0} (Restart to install v{1}!)", currVer, newVer);
          }
          else
          {
            this.lblVersion.Text = string.Format("v{0} (No updates available)", currVer);
          }
        }
      }
      catch (Exception) { }
    }

    public void UpdateCheckProgress(int progress)
    {
      try
      {
        if (!_updateCheckComplete)
        {
          var currVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();
          this.lblVersion.Text = string.Format("v{0} (Checking updates: {1}%)", currVer, progress);
        }
      }
      catch (Exception) { }
    }
  }
}