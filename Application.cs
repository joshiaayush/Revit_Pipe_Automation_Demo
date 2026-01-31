using System;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

namespace PipeCreation
{
    [Transaction(TransactionMode.Manual)]
    public class PipeCreator : Autodesk.Revit.UI.IExternalApplication
    {

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Create a custom ribbon tab
                string tabName = "Pipe Automation";
                application.CreateRibbonTab(tabName);

                // Create a custom ribbon panel
                string panelName = "Automate Piping";
                RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, panelName);

                // Create a push button in the custom panel
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
                PushButtonData buttonData = new PushButtonData(
                    "Pipe Creation",
                    "Create Pipe",
                    thisAssemblyPath,
                    "PipeCreation.StartPipeDrawingCommand"
                );

                PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;
                pushButton.ToolTip = "Pipe Automation";

                
                string iconFullPath = "D:\\Aayush_Joshi_Workspace\\Aayush_Joshi\\Revit_API\\PipeCreation\\PipeCreation\\Resources\\icon.ico";
                Uri iconUri = new Uri(iconFullPath);
                BitmapImage bitmapImage = new BitmapImage(iconUri);
                pushButton.Image = bitmapImage;
                pushButton.LargeImage = bitmapImage;
                return Result.Succeeded;

            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
                return Result.Failed;
            }

        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
