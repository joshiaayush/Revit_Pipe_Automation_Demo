/*using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PipeCreation
{
    public class PipeDrawingHandler
    {
        private UIApplication _uiApp;
        private UIDocument _uidoc;
        private Document _doc;
        private XYZ _startPoint;
        private XYZ _endPoint;
        private ElementId _systemTypeId;
        private ElementId _pipeTypeId;
        private ElementId _levelId;
        private double _maxSegmentLength = 5.6; // Maximum length before adding a placeholder (in feet or your units)

        public PipeDrawingHandler(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uidoc = _uiApp.ActiveUIDocument;
            _doc = _uidoc.Document;

            InitializePipeTypes();
        }

        private void InitializePipeTypes()
         {
             // Define the level
             Level level = new FilteredElementCollector(_doc)
                 .OfClass(typeof(Level))
                 .WhereElementIsNotElementType()
                 .FirstOrDefault(e => e.Name.Equals("Level 1")) as Level;
             _levelId = level?.Id;

             // Define the pipe type
             PipeType pipeType = new FilteredElementCollector(_doc)
                 .OfClass(typeof(PipeType))
                 .WhereElementIsElementType()
                 .FirstOrDefault() as PipeType;
             _pipeTypeId = pipeType?.Id;

            // Define the pipe system type
            MEPSystemType systemType = new FilteredElementCollector(_doc)
                .OfClass(typeof(MEPSystemType))
                .OfCategory(BuiltInCategory.OST_PipingSystem)
                .WhereElementIsElementType()
                .FirstOrDefault() as MEPSystemType;
            _systemTypeId = systemType?.Id;

            

        }



        public void StartPipeDrawing()
        {
            try
            {
                // Pick the start point
                _startPoint = _uidoc.Selection.PickPoint("Select start point for pipe.");

                while (true)
                {
                    
                    _endPoint = _uidoc.Selection.PickPoint("Select next point for pipe.");

                    // Create pipe segments and add placeholders if necessary
                    CreatePipeSegments(_startPoint, _endPoint);

                    // Set the end point as the new start point for the next segment
                    _startPoint = _endPoint;
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // User canceled the operation
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        private void CreatePipeSegments(XYZ startPoint, XYZ endPoint)
        {
            double distance = startPoint.DistanceTo(endPoint);

            if (distance <= _maxSegmentLength)
            {
                // Create a single pipe segment if within the maximum length
                CreatePipe(startPoint, endPoint);
            }
            else
            {
                // Create multiple pipe segments with placeholders
                int segmentCount = (int)Math.Ceiling(distance / _maxSegmentLength);
                XYZ direction = (endPoint - startPoint).Normalize();
                XYZ currentStart = startPoint;

                for (int i = 0; i < segmentCount; i++)
                {
                    XYZ currentEnd = (i == segmentCount - 1) ? endPoint : currentStart + direction * _maxSegmentLength;

                    CreatePipe(currentStart, currentEnd);

                    if (i < segmentCount - 1)
                    {
                        AddPlaceholder(currentEnd);
                    }

                    currentStart = currentEnd;
                }
            }
        }

        private void CreatePipe(XYZ startPoint, XYZ endPoint)
        {
            using (Transaction trans = new Transaction(_doc, "Create Pipe Segment"))
            {
                trans.Start();
                Pipe.Create(_doc, _systemTypeId, _pipeTypeId, _levelId, startPoint, endPoint);
                trans.Commit();
            }
        }

        private void AddPlaceholder(XYZ position)
        {
            using (Transaction trans = new Transaction(_doc, "Add Placeholder"))
            {
                trans.Start();

                // Create a small pipe segment as a placeholder
                XYZ placeholderEnd = new XYZ(position.X, position.Y, position.Z + 1.0); // Adjust this to create a visible marker
                Pipe.Create(_doc, _systemTypeId, _pipeTypeId, _levelId, position, placeholderEnd);

                
                trans.Commit();
            }
        }
        public void CreatePipeConnectors(UIDocument uiDocument, Extrusion extrusion)
        {
            // get the faces of the extrusion
            Options geoOptions = uiDocument.Document.Application.Create.NewGeometryOptions();
            geoOptions.View = uiDocument.Document.ActiveView;
            geoOptions.ComputeReferences = true;

            List<PlanarFace> planarFaces = new List<PlanarFace>();
            Autodesk.Revit.DB.GeometryElement geoElement = extrusion.get_Geometry(geoOptions);
            foreach (GeometryObject geoObject in geoElement)
            {
                Solid geoSolid = geoObject as Solid;
                if (null != geoSolid)
                {
                    foreach (Face geoFace in geoSolid.Faces)
                    {
                        if (geoFace is PlanarFace)
                        {
                            planarFaces.Add(geoFace as PlanarFace);
                        }
                    }
                }
            }

            if (planarFaces.Count > 1)
            {
                ConnectorElement connSupply =
                    ConnectorElement.CreatePipeConnector(uiDocument.Document, PipeSystemType.SupplyHydronic, planarFaces[0].Reference);
                Parameter param = connSupply.get_Parameter(BuiltInParameter.CONNECTOR_RADIUS);
                param.Set(1.0); // 1' radius
                param = connSupply.get_Parameter(BuiltInParameter.RBS_PIPE_FLOW_DIRECTION_PARAM);
                param.Set(2);

                ConnectorElement connReturn =
                    ConnectorElement.CreatePipeConnector(uiDocument.Document, PipeSystemType.ReturnHydronic, planarFaces[1].Reference);
                param = connReturn.get_Parameter(BuiltInParameter.CONNECTOR_RADIUS);
                param.Set(0.5); // 6" radius
                param = connReturn.get_Parameter(BuiltInParameter.RBS_PIPE_FLOW_DIRECTION_PARAM);
                param.Set(1);
            }
        }


    }
}
*/
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Linq;
using Autodesk.Revit.DB.Structure;
using System.IO;
using System.Collections.Generic;

namespace PipeCreation
{
    public class PipeDrawingHandler
    {
        private UIApplication _uiApp;
        private UIDocument _uidoc;
        private Document _doc;
        private XYZ _startPoint;
        private XYZ _endPoint;
        private ElementId _systemTypeId;
        private ElementId _pipeTypeId;
        private ElementId _levelId;
        private ElementId _pipeCouplerTypeId;
        private double _maxSegmentLength = 10.0; // Maximum length before adding a coupler (in feet or your units)
        private string _filePath = @"C:\ProgramData\Autodesk\RVT 2024\Libraries\English-Imperial\US\Pipe\Fittings\Generic\Coupling - Generic.rfa";
        private string _couplerFamilyName = "Coupling - Generic";

        public PipeDrawingHandler(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uidoc = _uiApp.ActiveUIDocument;
            _doc = _uidoc.Document;

            InitializePipeTypes();
        }

        private void InitializePipeTypes()
        {
            // Define the level
            Level level = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .FirstOrDefault(e => e.Name.Equals("Level 1")) as Level;
            _levelId = level?.Id;

            // Define the pipe type
            PipeType pipeType = new FilteredElementCollector(_doc)
                .OfClass(typeof(PipeType))
                .WhereElementIsElementType()
                .FirstOrDefault() as PipeType;
            _pipeTypeId = pipeType?.Id;

            // Define the pipe system type
            MEPSystemType systemType = new FilteredElementCollector(_doc)
                .OfClass(typeof(MEPSystemType))
                .OfCategory(BuiltInCategory.OST_PipingSystem)
                .WhereElementIsElementType()
                .FirstOrDefault() as MEPSystemType;
            _systemTypeId = systemType?.Id;

            // Load the family and get the coupler type
            EnsureFamilyLoaded(_doc, _filePath, _couplerFamilyName);
            FamilySymbol couplerSymbol = GetCouplingType(_doc, _couplerFamilyName);
            if (couplerSymbol != null)
            {
                _pipeCouplerTypeId = couplerSymbol.Id;
            }
        }

        private void EnsureFamilyLoaded(Document doc, string familyPath, string familyName)
        {
            // Check if the family is already loaded
            if (new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Any(fam => fam.Name.Equals(familyName)))
            {
                return;
            }

            // Load the family if not already loaded
            if (!File.Exists(familyPath))
            {
                TaskDialog.Show("Error", "File not found: " + familyPath);
                return;
            }

            using (Transaction trans = new Transaction(doc, "Load Family"))
            {
                trans.Start();
                Family loadedFamily;

                if (doc.LoadFamily(familyPath, out loadedFamily))
                {
                    trans.Commit();
                }
                else
                {
                    trans.RollBack();
                    throw new InvalidOperationException($"Failed to load family {familyName} from path: {familyPath}");
                }
            }
        }

        private FamilySymbol GetCouplingType(Document doc, string familyName)
        {
            string lowerFamilyName = familyName.ToLower();

            FamilySymbol couplingType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_PipeFitting)
                .Cast<FamilySymbol>()
                .FirstOrDefault(sym => sym.Name.ToLower().Equals(lowerFamilyName)
                                    || sym.Family.Name.ToLower().Contains("coupling - generic"));

            if (couplingType == null)
            {
                throw new InvalidOperationException($"Coupling type '{familyName}' not found in the project. Please load the family and try again.");
            }

            return couplingType;
        }

        public void StartPipeDrawing()
        {
            try
            {
                // Pick the start point
                _startPoint = _uidoc.Selection.PickPoint("Select start point for pipe.");

                while (true)
                {
                    _endPoint = _uidoc.Selection.PickPoint("Select next point for pipe.");

                    // Create pipe segments and add couplers if necessary
                    CreatePipeSegments(_startPoint, _endPoint);

                    // Set the end point as the new start point for the next segment
                    _startPoint = _endPoint;
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // User canceled the operation
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        private void CreatePipeSegments(XYZ startPoint, XYZ endPoint)
        {
            double distance = startPoint.DistanceTo(endPoint);

            if (distance <= _maxSegmentLength)
            {
                // Create a single pipe segment if within the maximum length
                Pipe pipe = CreatePipe(startPoint, endPoint);
                double pipeDiameter = GetPipeDiameter(pipe);
                AddPipeCoupler(endPoint, pipeDiameter);
            }
            else
            {
                // Create multiple pipe segments with couplers
                int segmentCount = (int)Math.Ceiling(distance / _maxSegmentLength);
                XYZ direction = (endPoint - startPoint).Normalize();
                XYZ currentStart = startPoint;

                for (int i = 0; i < segmentCount; i++)
                {
                    XYZ currentEnd = (i == segmentCount - 1) ? endPoint : currentStart + direction * _maxSegmentLength;

                    Pipe pipe = CreatePipe(currentStart, currentEnd);
                    double pipeDiameter = GetPipeDiameter(pipe);

                    if (i < segmentCount - 1)
                    {
                        AddPipeCoupler(currentEnd, pipeDiameter);
                    }

                    currentStart = currentEnd;
                }
            }
        }

        private Pipe CreatePipe(XYZ startPoint, XYZ endPoint)
        {
            using (Transaction trans = new Transaction(_doc, "Create Pipe Segment"))
            {
                trans.Start();
                Pipe pipe = Pipe.Create(_doc, _systemTypeId, _pipeTypeId, _levelId, startPoint, endPoint);
                trans.Commit();
                return pipe;
            }
        }

        private double GetPipeDiameter(Pipe pipe)
        {
            Parameter diameterParam = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
            double diameterInFeet = diameterParam.AsDouble();
            double diameterInMM = diameterInFeet * 304.8; // Convert from feet to millimeters
            return diameterInMM;
        }

        private void AddPipeCoupler(XYZ position, double pipeDiameter)
        {
            using (Transaction trans = new Transaction(_doc, "Add Pipe Coupler"))
            {
                trans.Start();

                if (_pipeCouplerTypeId != ElementId.InvalidElementId)
                {
                    // Create a pipe coupler at the specified position
                    FamilySymbol couplerSymbol = _doc.GetElement(_pipeCouplerTypeId) as FamilySymbol;

                    if (couplerSymbol != null)
                    {
                        if (!couplerSymbol.IsActive)
                        {
                            couplerSymbol.Activate();
                            _doc.Regenerate();
                        }

                        // Place the coupler at the position
                        FamilyInstance couplerInstance = _doc.Create.NewFamilyInstance(position, couplerSymbol, StructuralType.NonStructural);
                        
                        

                        // Set the coupler size to match the pipe diameter
                        Parameter diameterParam = couplerInstance.LookupParameter("Nominal Diameter");
                        if (diameterParam != null)
                        {
                            diameterParam.Set(pipeDiameter);
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Error", "Pipe coupler family symbol not found.");
                    }
                }
                else
                {
                    TaskDialog.Show("Error", "Pipe coupler type is not initialized.");
                }

                trans.Commit();
            }
        }
    }
}
