using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace VSIXDemoZip123.Shared
{

    internal static class VsHelpers
    {
        public static DTE2 DTE { get; } = GetService<DTE, DTE2>();

        public static TReturnType GetService<TServiceType, TReturnType>()
        {
            return (TReturnType)ServiceProvider.GlobalProvider.GetService(typeof(TServiceType));
        }

        public static string GetFileInVsix(string relativePath)
        {
            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(folder, relativePath);
        }

        public static void ExecuteCommand(string command)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                Command cmd = DTE.Commands.Item(command);

                if (cmd != null && cmd.IsAvailable)
                {
                    DTE.Commands.Raise(cmd.Guid, cmd.ID, null, null);
                }
            }
            catch (Exception)
            {


            }

            // DTE.ExecuteCommand(command);
        }

        public static void OpenFileAndRefresh(string path)
        {
            VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, path);
            ExecuteCommand("SolutionExplorer.Refresh");

            ThreadHelper.Generic.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
            {
                ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
            });
        }

        public static bool IsTemplateFolder(out ProjectItem templateFolderItem)
        {
            templateFolderItem = DTE.SelectedItems.Item(1)?.ProjectItem;

            if (templateFolderItem != null && templateFolderItem.Name.Equals("Constants.Folder"))
                return true;

            return false;
        }

        public static string GetRootFolder(this Project project)
        {
            if (project == null)
                return null;

            if (project.IsKind("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}"))//ProjectKinds.vsProjectKindSolutionFolder))
                return Path.GetDirectoryName(DTE.Solution.FullName);

            if (string.IsNullOrEmpty(project.FullName))
                return null;

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;

            if (Directory.Exists(fullPath))
                return fullPath;

            if (File.Exists(fullPath))
                return Path.GetDirectoryName(fullPath);

            return null;
        }
        public static string GetWebRootFolder(this Project project)
        {
            string rootPath = GetRootFolder(project);
            if (IsKind(project, ProjectTypes.CSHARP, ProjectTypes.DOTNET_Core))
            {
                return System.IO.Path.Combine(rootPath, "wwwroot");
            }
            else
            {
                return rootPath;
            }
        }

        public static bool IsKind(this Project project, params string[] kindGuids)
        {
            foreach (string guid in kindGuids)
            {
                if (project.Kind.Contains(guid))
                    return true;
            }

            return false;
        }

        public static bool IsWebProject(this Project project)
        {
            if (IsKind(project, ProjectTypes.CSHARP, ProjectTypes.DOTNET_Core))
            {
                string root = project.GetRootFolder();
                return System.IO.Directory.Exists(System.IO.Path.Combine(root, "wwwroot"));
            }
            else
            {
                return IsKind(project, ProjectTypes.ASPNET5, ProjectTypes.ASPNETMVC1, ProjectTypes.ASPNETMVC2, ProjectTypes.ASPNETMVC3, ProjectTypes.ASPNETMVC4, ProjectTypes.ASPNETMVC5, ProjectTypes.WEBSITE_PROJECT);
            }
        }
    }

    public static class ProjectTypes
    {
        public const string CSHARP = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        public const string DOTNET_Core = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";
        public const string MISC = "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}";
        public const string NODE_JS = "{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}";
        public const string SOLUTION_FOLDER = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
        public const string SSDT = "{00d1a9c2-b5f0-4af3-8072-f6c62b433612}";
        public const string UNIVERSAL_APP = "{262852C6-CD72-467D-83FE-5EEB1973A190}";
        public const string WAP = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        public const string WEBSITE_PROJECT = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
        public const string ASPNET5 = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";
        public const string ASPNETMVC1 = "{603C0E0B-DB56-11DC-BE95-000D561079B0}";
        public const string ASPNETMVC2 = "{F85E285D-A4E0-4152-9332-AB1D724D3325}";
        public const string ASPNETMVC3 = "{E53F8FEA-EAE0-44A6-8774-FFD645390401}";
        public const string ASPNETMVC4 = "{E3E379DF-F4C6-4180-9B81-6769533ABE47}";
        public const string ASPNETMVC5 = "{349C5851-65DF-11DA-9384-00065B846F21}";
    }
}
