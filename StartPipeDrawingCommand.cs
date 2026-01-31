using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace PipeCreation
{
    [Transaction(TransactionMode.Manual)]
    public class StartPipeDrawingCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            PipeDrawingHandler handler = new PipeDrawingHandler(commandData.Application);
            handler.StartPipeDrawing();

            return Result.Succeeded;
        }


    }
}
