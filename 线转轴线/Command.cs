using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using OperationCanceledException = Autodesk.Revit.Exceptions.OperationCanceledException;

namespace 线转轴线
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //ui应用程序
            UIApplication uiapp = commandData.Application;
            //应用程序
            Application app = uiapp.Application;
            //ui文档
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //文档
            Document doc = uidoc.Document;
            try
            {
                var references = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element).ToList();

                using (Transaction trans = new Transaction(doc, "transaction"))
                {
                    trans.Start();
                    List<Curve> curves = new List<Curve>();
                    references.ForEach(
                        (i)
                        =>
                        {
                            Curve c = (doc.GetElement(i).Location as LocationCurve).Curve;
                            curves.Add(c);
                        });
                    curves.ForEach(i =>
                    {
                        if (i as Line != null)
                        {
                            Grid.Create(doc, i as Line);
                        }
                        if (i as Arc != null)
                        {
                            Grid.Create(doc, i as Arc);
                        }
                    });
                    trans.Commit();
                }
            }
            catch (OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Tip", ex.Message);
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }

    internal class CurveFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Name == "线")
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}