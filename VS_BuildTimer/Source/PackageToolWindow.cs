/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using Task = System.Threading.Tasks.Task;

namespace VSBuildTimer
{
    /// <summary>
    /// The Package class is responsible for the following:
    ///		- Attributes to enable registration of the components
    ///		- Enable the creation of our tool windows
    ///		- Respond to our commands
    /// 
    /// The following attributes are covered in other samples:
    ///		PackageRegistration:   Reference.Package
    ///		ProvideMenuResource:   Reference.MenuAndCommands
    /// 
    /// Our initialize method defines the command handlers for the commands that
    /// we provide under View|Other Windows to show our tool windows
    /// 
    /// The first new attribute we are using is ProvideToolWindow. That attribute
    /// is used to advertise that our package provides a tool window. In addition
    /// it can specify optional parameters to describe the default start location
    /// of the tool window. For example, the PersistedWindowPane will start tabbed
    /// with Solution Explorer. The default position is only used the very first
    /// time a tool window with a specific Guid is shown for a user. After that,
    /// the position is persisted based on the last known position of the window.
    /// When trying different default start positions, you may find it useful to
    /// delete *.prf from:
    ///		"%USERPROFILE%\Application Data\Microsoft\VisualStudio\10.0Exp\"
    /// as this is where the positions of the tool windows are persisted.
    /// 
    /// To get the Guid corresponding to the Solution Explorer window, we ran this
    /// sample, made sure the Solution Explorer was visible, selected it in the
    /// Persisted Tool Window and looked at the properties in the Properties
    /// window. You can do the same for any window.
    /// 
    /// The DynamicWindowPane makes use of a different set of optional properties.
    /// First it specifies a default position and size (again note that this only
    /// affects the very first time the window is displayed). Then it specifies the
    /// Transient flag which means it will not be persisted when Visual Studio is
    /// closed and reopened.
    /// 
    /// The second new attribute is ProvideToolWindowVisibility. This attribute
    /// is used to specify that a tool window visibility should be controled
    /// by a UI Context. For a list of predefined UI Context, look in vsshell.idl
    /// and search for "UICONTEXT_". Since we are using the UICONTEXT_SolutionExists,
    /// this means that it is possible to cause the window to be displayed simply by
    /// creating a solution/project.
    /// </summary>
    [ProvideToolWindow(typeof(BuildTimerWindowPane), PositionX = 250, PositionY = 250, Width = 160, Height = 180, Transient = true)]
    [ProvideMenuResource(1000, 1)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid("01069CDD-95CE-4620-AC21-DDFF6C57F012")]
    [ProvideBindingPath]
    public class VSBuildTimerPackage : AsyncPackage, ILogger
    {
        public EventRouter EvtRouter { get { return evtRouter; } }

        public IBuildInfoExtractionStrategy BuildInfoExtractor { get { return buildInfoExtractor; } }

        public void LogMessage(string message, LogLevel level)
        {
            if (this.wndPane != null)
            {
                #if DEBUG
                var minLevel = LogLevel.DebugInfo;
                #else
                var minLevel = LogLevel.UserInfo;
                #endif

                if (level >= minLevel)
                {
                    var time = System.DateTime.Now;
                    // Write message to both windows.
                    if (this.wndPane.OutputWindowPane != null)
                        this.wndPane.OutputWindowPane.OutputString(time + " - " + message + "\n");
                    if (this.wndPane.BuildTimerUICtrl != null)
                        this.wndPane.BuildTimerUICtrl.OutputString(time + " - " + message + "\n");
                }
            }
        }

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress
        )
		{
			await base.InitializeAsync(cancellationToken, progress);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Before anything else, create the event router. Other objects are going to need it.
            IServiceContainer serviceContainer = this as IServiceContainer;
            var dte = serviceContainer.GetService(typeof(SDTE)) as EnvDTE.DTE;

            //var dte = GetServiceAsync(typeof(SDTE));
            this.evtRouter = new EventRouter(dte);

            IVsSolutionBuildManager2 buildManager = await GetServiceAsync(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            this.buildInfoExtractor = new SDKBasedInfoExtractor(this, buildManager, this);

            CommandID id = new CommandID(GuidsList.guidClientCmdSet, PkgCmdId.cmdidBuildTimerWindow);
            DefineCommandHandler(new EventHandler(ShowBuildTimerWindow), id);
        }

        internal OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
		{
			// if the package is zombied, we don't want to add commands
			if (Zombied)
				return null;

			// Make sure we have the service
			if (menuService == null)
			{
				// Get the OleCommandService object provided by the MPF; this object is the one
				// responsible for handling the collection of commands implemented by the package.
				menuService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			}
			OleMenuCommand command = null;
			if (null != menuService)
			{
				// Add the command handler
				command = new OleMenuCommand(handler, id);
				menuService.AddCommand(command);
			}
			return command;
		}

		internal string GetResourceString(string resourceName)
		{
			string resourceValue;
			IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
			if (resourceManager == null)
			{
				throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
			}
			Guid packageGuid = GetType().GUID;
			int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
			ErrorHandler.ThrowOnFailure(hr);
			return resourceValue;
		}

        private void ShowBuildTimerWindow(object sender, EventArgs arguments)
        {
            // Get the one (index 0) and only instance of our tool window (if it does not already exist it will get created)
            this.wndPane = FindToolWindow(typeof(BuildTimerWindowPane), 0, true) as BuildTimerWindowPane;
            if (this.wndPane == null)
            {
                throw new COMException(GetResourceString("@101"));
            }

            IVsWindowFrame frame = this.wndPane.Frame as IVsWindowFrame;
            if (frame == null)
            {
                throw new COMException(GetResourceString("@102"));
            }
            // Bring the tool window to the front and give it focus
            ErrorHandler.ThrowOnFailure(frame.Show());
        }

        private OleMenuCommandService menuService;
        private EventRouter evtRouter;
        private IBuildInfoExtractionStrategy buildInfoExtractor;
        private BuildTimerWindowPane wndPane;
    }


    abstract class PackageUtils
    {
        public static string PackageVersionString()
        {
            var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return string.Format("Visual Studio Build Timer {0}.{1}.{2} - Build {3}\n",
                v.Major,
                v.Minor,
                v.Build,        // Microsfot versioning follows the scheme: major.minor.build.revision.
                v.Revision      // I prefer it in the form:                 major.minor.revision.build.
                                // Therefore I use the "Build" field as my revision number and vice-versa.
            );
        }
    }
}
