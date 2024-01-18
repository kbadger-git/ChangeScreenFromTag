#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using FTOptix.OPCUAServer;
using System.Collections.Generic;
using System.Reflection.Metadata;
using FTOptix.System;
using FTOptix.EventLogger;
using FTOptix.Store;
#endregion

public class ChangePageFromTag : BaseNetLogic {
    public override void Start() {
        // Insert code to be executed when the user-defined logic is started
        myLongRunningTask = new LongRunningTask(CreatePagesList, LogicObject);
        myLongRunningTask.Start();
    }

    public override void Stop() {
        // Insert code to be executed when the user-defined logic is stopped
        myLongRunningTask.Dispose();
    }

    [ExportMethod]
    public void CreatePagesList() {
        pagesDictionary.Clear();
        panelLoader = InformationModel.Get<PanelLoader>(LogicObject.GetVariable("PanelLoader").Value);
        pageChangeTag = InformationModel.GetVariable(LogicObject.GetVariable("PageChangeTag").Value);
        //Code below allows this to work without needing to reference the pageChangeTag with a button/label in order for it to monitor tag value changes.
        var variableSynchronizer = new RemoteVariableSynchronizer();
        variableSynchronizer.Add(pageChangeTag);
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        NodeId screensFolder = LogicObject.GetVariable("ScreensFolder").Value;
        String basePageTypeName = InformationModel.Get(LogicObject.GetVariable("BasePageType").Value).BrowseName;
        RecursiveSearch(InformationModel.Get(screensFolder), basePageTypeName);
        pageChangeTag.VariableChange += PageChangeTag_VariableChange;
    }

    private void PageChangeTag_VariableChange(object sender, VariableChangeEventArgs e) {
        if (pageChangeTag.Value == 0) {
            return;
        } else {
            NodeId destinationPage = NodeId.Empty;
            if (pagesDictionary.TryGetValue(pageChangeTag.Value, out destinationPage)) {
                panelLoader.ChangePanel(InformationModel.Get(destinationPage));
            }
        }
    }

    private void RecursiveSearch(IUANode inputObject, String screenType) {
        foreach (IUANode childrenObject in inputObject.Children) {
            try {
                if (childrenObject is FTOptix.Core.Folder) {
                    Log.Verbose1("FindPages.Folder", "Found folder with name [" + childrenObject.BrowseName + "] and Type: [" + childrenObject.GetType().ToString() + "]");
                    RecursiveSearch(childrenObject, screenType);
                } else if (((UAManagedCore.UAObjectType)childrenObject).SuperType.BrowseName == screenType) {
                    UInt32 pageId = childrenObject.GetVariable("ScreenId").Value;
                    if (pageId > 0) {
                        Log.Info("FindPages", "Found page with name [" + childrenObject.BrowseName + "] and ID: [" + pageId.ToString() + "]");
                        pagesDictionary.Add(pageId, childrenObject.NodeId);
                    } else {
                        Log.Error("FindPages.Page", "Found page with name [" + childrenObject.BrowseName + "] and invalid ID: [" + pageId.ToString() + "]");
                    }
                } else {
                    Log.Verbose1("FindPages.Else", "Found unknown with name [" + childrenObject.BrowseName + "] and Type: [" + childrenObject.GetType().ToString() + "]");
                }
            } catch (Exception ex) {
                Log.Error("FindPages.Catch", "Exception thrown: " + ex.Message);
            }
        }
    }

    private PanelLoader panelLoader;
    private IUAVariable pageChangeTag;
    private Dictionary<UInt32, NodeId> pagesDictionary = new Dictionary<UInt32, NodeId>();
    private LongRunningTask myLongRunningTask;
}
