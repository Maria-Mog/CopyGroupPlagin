using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyGroupPlagin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CopyGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
          UIDocument uIDoc =  commandData.Application.ActiveUIDocument;
            Document doc = uIDoc.Document;

          Reference reference=  uIDoc.Selection.PickObject(ObjectType.Element, "Выберите группу объектов");
           Element element= doc.GetElement(reference);
            Group group = element as Group;

            XYZ point = uIDoc.Selection.PickPoint("Выберите точку");

            Transaction transaction = new Transaction(doc);
            transaction.Start("Копирование группы");
            doc.Create.PlaceGroup(point, group.GroupType);
            transaction.Commit();

            return Result.Succeeded;
        }        
    } 
}
