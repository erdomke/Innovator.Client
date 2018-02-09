namespace Innovator.Client
{
  /// <summary>Allowable SOAP actions</summary>
  public enum CommandAction
  {
    ActivateActivity,
    AddItem,
    /// <summary>
    /// Executes every <c>&lt;Item/&gt;</c> tag found.  If the method called returns
    /// a result string, the result will not be shown.  If any query returns an exception,
    /// the whole transaction will be rolled back
    /// </summary>
    ApplyAML,
    /// <summary>
    /// Executes the first <c>&lt;Item/&gt;</c> tag found.
    /// </summary>
    ApplyItem,
    ApplyMethod,
    /// <summary>
    /// Execute the SQL statement.  A result will only be returned if the first word of the
    /// SQL is "select".  This action can only be performed by users in the Administrators
    /// identity
    /// </summary>
    ApplySQL,
    ApplyUpdate,
    BuildProcessReport,
    CacheDiag,
    CancelWorkflow,
    ChangeUserPassword,
    CheckImportedItemType,
    /// <summary>Clears all server caches</summary>
    ClearCache,
    /// <summary>Performs DELETE FROM innovator.HISTORY;</summary>
    ClearHistory,
    CloneForm,
    CloseWorkflow,
    CompileMethod,
    CopyItem,
    CopyItem2,
    CreateFileExchangeTxn,
    CreateItem,
    DeleteItem,
    /// <summary>Removes a user session. Input should be similar to the output of <see cref="GetUsersList"/></summary>
    DeleteUsers,
    DeleteVersionFile,
    /// <summary>Command action specific to this library used for downloading files</summary>
    DownloadFile,
    EditItem,
    EvaluateActivity,
    /// <summary>Execute workflow escalations (generally called on a schedule)</summary>
    ExecuteEscalations,
    /// <summary>Execute workflow reminders (generally called on a schedule)</summary>
    ExecuteReminders,
    ExportItemType,
    GenerateNewGUID,
    GenerateNewGUIDEx,
    GenerateParametersGrid,
    GenerateRelationshipsTabbar,
    GenerateRelationshipsTable,
    GetAffectedItems,
    GetAssignedActivities,
    GetAssignedTasks,
    GetCheckUpdateInfo,
    GetConfigurableGridMetadata,
    /// <summary>Get the user ID of the currently logged in user</summary>
    GetCurrentUserID,
    GetFormForDisplay,
    GetHistoryItems,
    GetIdentityList,
    GetItem,
    /// <summary>Get all versions (i.e. generations) of an item. It requires an id attribute and
    /// does not respect the select attribute. Another way to do this is to search for generations
    /// &gt; 0.</summary>
    GetItemAllVersions,
    GetItemLastVersion,
    GetItemNextStates,
    GetItemRelationships,
    GetItemTypeByFormID,
    GetItemTypeForClient,
    GetItemWhereUsed,
    GetMainTreeItems,
    GetNextSequence,
    GetPermissionsForClient,
    GetUsersList,
    GetUserWorkingDirectory,
    InstantiateWorkflow,
    LoadCache,
    LoadProcessInstance,
    LoadVersionFile,
    LockItem,
    LogMessage,
    /// <summary>Log off the current user</summary>
    LogOff,
    MergeItem,
    /// <summary>Returns the AML for adding a new item including specifying all of the default
    /// properties</summary>
    NewItem,
    NewRelationship,
    PopulateRelationshipsGrid,
    PopulateRelationshipsTables,
    ProcessFileTransferResult,
    ProcessReplicationQueue,
    PromoteItem,
    PurgeItem,
    ReassignActivity,
    RebuildKeyedName,
    RebuildView,
    ReplicationExecutionResult,
    ResetAllItemsAccess,
    ResetItemAccess,
    ResetLifeCycle,
    ResetServerCache,
    SaveCache,
    ServerErrorTest,
    SetDefaultLifeCycle,
    SetNullBooleanTo0,
    SetUserWorkingDirectory,
    SkipItem,
    StartDefaultWorkflow,
    StartFileExchangeTxn,
    StartNamedWorkflow,
    StartWorkflow,
    StoreVersionFile,
    TransformVaultServerURL,
    UnlockAll,
    UnlockItem,
    UpdateItem,
    ValidateUser,
    ValidateVote,
    ValidateWorkflowMap,
    VaultApplyAml,
    VaultApplyItem
  }
}
