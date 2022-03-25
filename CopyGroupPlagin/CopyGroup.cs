using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
            try
            {
                UIDocument uIDoc = commandData.Application.ActiveUIDocument;
                Document doc = uIDoc.Document;

                GroupPickfilter pickfilter = new GroupPickfilter();

                Reference reference = uIDoc.Selection.PickObject(ObjectType.Element, pickfilter, "Выберите группу объектов");
                Element element = doc.GetElement(reference);
                Group group = element as Group;
                XYZ centerGroup = GetElementCenter(group);
                Room room = GetRoomByPoint(doc, centerGroup);
                XYZ centerRoom = GetElementCenter(room);
                XYZ offset = centerGroup - centerRoom;

                XYZ point = uIDoc.Selection.PickPoint("Выберите точку");
                Room selectRoom = GetRoomByPoint(doc, point);
                XYZ pointCenter = GetElementCenter(selectRoom);
                XYZ offsetCenter = offset + pointCenter;

                Transaction transaction = new Transaction(doc);
                transaction.Start("Копирование группы");
                doc.Create.PlaceGroup(offsetCenter, group.GroupType);
                transaction.Commit();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
            return (boundingBox.Max + boundingBox.Min) / 2;
        }
        public Room GetRoomByPoint(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Element element in collector)
            {
                Room room = element as Room;
                if (room != null)
                {
                    if (room.IsPointInRoom(point))
                        return room;
                }
            }
            return null;
        }
        public class GroupPickfilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
                    return true;
                else
                    return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
