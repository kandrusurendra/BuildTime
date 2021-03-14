using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace VSBuildTimer
{
    [Guid("0520C451-DB04-4622-90EC-B7113574346F")]
    public class BuildTimerWindowPane : ToolWindowPane
    {
        /// <summary>
        /// Constructor for ToolWindowPane.
        /// 
        /// Initialization that depends on the package or that requires access
        /// to VS services should be done in OnToolWindowCreated.
        /// </summary>
        public BuildTimerWindowPane()
        {
            // Set the image that will appear on the tab of the window frame when docked with another window.
            // KnownMonikers is a set of image monkiers that are globablly recognized by VS. These images can be
            // used in any project without needing to include the source image.
            BitmapImageMoniker = Microsoft.VisualStudio.Imaging.KnownMonikers.Search;

            var package = Package as BuildTimerPackage;

            // Creating the user control that will be displayed in the window
            BuildTimerUICtrl = new BuildTimerCtrl(this);
            base.Content = BuildTimerUICtrl;
        }

        public override void OnToolWindowCreated()
        {
            // This is called after our control has been created and sited.
            // This is a good place to initialize the control with data gathered
            // from Visual Studio services.
             
            base.OnToolWindowCreated();

            BuildTimerPackage package = (BuildTimerPackage)Package;

            // Set the text that will appear in the title bar of the tool window.
            // Note that because we need access to the package for localization,
            // we have to wait to do this here. If we used a constant string,
            // we could do this in the constructor.
            this.Caption = package.GetResourceString("@120");

            // Register to the window events
            WindowStatus windowFrameEventsHandler = new WindowStatus(OutputWindowPane, Frame as IVsWindowFrame);
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)Frame).SetProperty((int)__VSFPROPID.VSFPROPID_ViewHelper, windowFrameEventsHandler));

            BuildTimerUICtrl.Initialize(package.BuildInfoExtractor, package.EvtRouter, windowFrameEventsHandler, package.SettingsManager);
        }

        public BuildTimerCtrl BuildTimerUICtrl { get; } = null;

        public IVsOutputWindowPane OutputWindowPane
        {
            get
            {
                if (outputWindowPane == null)
                {
                    // First make sure the output window is visible
                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                    // Get the frame of the output window
                    Guid outputWindowGuid = GuidsList.guidOutputWindowFrame;
                    IVsWindowFrame outputWindowFrame = null;
                    ErrorHandler.ThrowOnFailure(uiShell.FindToolWindow((uint)__VSCREATETOOLWIN.CTW_fForceCreate, ref outputWindowGuid, out outputWindowFrame));
                    // Show the output window
                    if (outputWindowFrame != null)
                        outputWindowFrame.Show();

                    // Get the output window service
                    IVsOutputWindow outputWindow = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
                    // The following GUID is a randomly generated one. This is to uniquely identify our output pane.
                    // It is best to change it to something else to avoid sharing it with someone else.
                    // If the goal is to share, then the same guid should be used, and the pane should only
                    // be created if it does not already exist.
                    Guid paneGuid = new Guid("{291A5129-ADA8-4FB7-A9C4-7557854E00F0}");
                    // Create the pane
                    BuildTimerPackage package = (BuildTimerPackage)Package;
                    string paneName = package.GetResourceString("@120");
                    ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref paneGuid, paneName, 1 /*visible=true*/, 0 /*clearWithSolution=false*/));
                    // Retrieve the pane
                    ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref paneGuid, out outputWindowPane));


                    if (outputWindowPane != null)
                    {
                        OutputWindowPane.OutputString(PackageUtils.PackageVersionString());
                    }
                }

                return outputWindowPane;
            }
        }

        // Caching our output window pane
        private IVsOutputWindowPane outputWindowPane = null;
    }
}
