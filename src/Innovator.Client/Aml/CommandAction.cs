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
    ClearCache,
    ClearHistory,
    CloneForm,
    CloseWorkflow,
    CompileMethod,
    CopyItem,
    CopyItem2,
    CreateItem,
    DeleteItem,
    DeleteUsers,
    DeleteVersionFile,
    /// <summary>Command action specific to this library used for downloading files</summary>
    DownloadFile
    ,EditItem
    ,EvaluateActivity
    ,ExecuteEscalations
    ,ExecuteReminders
    ,ExportItemType
    ,GenerateNewGUID
    ,GenerateNewGUIDEx
    ,GenerateParametersGrid
    ,GenerateRelationshipsTabbar
    ,GenerateRelationshipsTable
    ,GetAffectedItems
    ,GetAssignedActivities
    ,GetAssignedTasks
    ,GetConfigurableGridMetadata
    ,GetCurrentUserID
    ,GetFormForDisplay
    ,GetHistoryItems
    ,GetIdentityList
    ,GetItem
    ,GetItemAllVersions
    ,GetItemLastVersion
    ,GetItemNextStates
    ,GetItemRelationships
    ,GetItemTypeByFormID
    ,GetItemTypeForClient
    ,GetItemWhereUsed
    ,GetMainTreeItems
    ,GetNextSequence
    ,GetPermissionsForClient
    ,GetUsersList
    ,GetUserWorkingDirectory
    ,InstantiateWorkflow
    ,LoadCache
    ,LoadProcessInstance
    ,LoadVersionFile
    ,LockItem
    ,LogMessage
    ,LogOff
    ,MergeItem
    ,NewItem
    ,NewRelationship
    ,PopulateRelationshipsGrid
    ,PopulateRelationshipsTables
    ,ProcessReplicationQueue
    ,PromoteItem
    ,PurgeItem
    ,ReassignActivity
    ,RebuildKeyedName
    ,RebuildView
    ,ReplicationExecutionResult
    ,ResetAllItemsAccess
    ,ResetItemAccess
    ,ResetLifeCycle
    ,ResetServerCache
    ,SaveCache
    ,ServerErrorTest
    ,SetDefaultLifeCycle
    ,SetNullBooleanTo0
    ,SetUserWorkingDirectory
    ,SkipItem
    ,StartDefaultWorkflow
    ,StartNamedWorkflow
    ,StartWorkflow
    ,StoreVersionFile
    ,TransformVaultServerURL
    ,UnlockAll
    ,UnlockItem
    ,UpdateItem
    ,ValidateUser
    ,ValidateVote
    ,ValidateWorkflowMap
  }
}
