using System;
using System.Numerics;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Globalization;

namespace lab1_var4
{
    /*___________________DataItem___________________*/

    public struct DataItem
    {
        public Vector2 Point { get; set; }
        public Vector2 Field { get; set; }

        public DataItem(Vector2 _point, Vector2 _field)
        {
            Point = _point;
            Field = _field;
        }

        public string ToLongString(string format)
        {
            string PointStr = "Point (" + Point.X.ToString(format) + ", " + Point.Y.ToString(format) + ") ";
            string FieldStr = "Field vector = {" + Field.X.ToString(format) + ", " + Field.Y.ToString(format) + "} ";
            double FieldMagnitude = Math.Sqrt((Field.X * Field.X) + (Field.Y * Field.Y));
            string FieldMagnStr = "Field vector magnitude = " + FieldMagnitude.ToString(format);
            return PointStr + FieldStr + FieldMagnStr;
        }

        public override string ToString()
        {
            string PointStr = "Point (" + Point.X.ToString() + ", " + Point.Y.ToString() + ")";
            string FieldStr = "Field vector = {" + Field.X.ToString() + ", " + Field.Y.ToString() + "}";
            return base.ToString() + " " + PointStr + " " + FieldStr;
        }
    }

    /*_____________________Delegate_____________________*/

    public delegate Vector2 Fv2Vector2(Vector2 v2);

    /*______________IEnumerable<DataItem>_______________*/

    public interface IEnumerable<DataItem> : IEnumerable
    {
        new IEnumerator<DataItem> GetEnumerator();
    }

    public interface IEnumerator<DataItem> : IEnumerator
    {
        new DataItem Current
        {
            get;
        }
    }

    /*______________________V4Data______________________*/

    public abstract class V4Data : IEnumerable<DataItem>
    {
        public string ID
        {
            get;
            protected set;
        }
        public DateTime MeasureTime
        {
            get;
            protected set;
        }

        public V4Data(string _ID, DateTime _MeasureTime)
        {
            ID = _ID;
            MeasureTime = _MeasureTime;
        }

        public abstract int Count { get; }
        public abstract float MaxFromOrigin { get; }

        public abstract string ToLongString(string format);

        public override string ToString()
        {
            return base.ToString() + string.Format(" ID = {0}, MeasureTime = {1}", ID, MeasureTime);
        }

        public abstract IEnumerator<DataItem> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /*____________________V4DataList____________________*/

    public class V4DataList : V4Data
    {
        public List<DataItem> DataList { get; }

        public V4DataList(string _ID, DateTime _MeasureTime)
            : base(_ID, _MeasureTime)
        {
            DataList = new List<DataItem>();
        }

        public bool Add(DataItem newItem)
        {
            foreach (DataItem item in DataList)
            {
                if (item.Point == newItem.Point)
                {
                    return false;
                }
            }
            DataList.Add(newItem);
            return true;
        }

        public int AddDefaults(int nItems, Fv2Vector2 F)
        {
            int counter = 0;
            for (int i = 0; i < nItems; i++)
            {
                Vector2 newPoint = new Vector2(i, nItems - i);
                Vector2 newField = F(newPoint);
                DataItem newItem = new DataItem(newPoint, newField);
                if (Add(newItem)) counter++;
            }
            return counter;
        }

        public override int Count => DataList.Count;

        public override float MaxFromOrigin
        {
            get
            {
                float maxDist = 0;
                foreach (DataItem item in DataList)
                {
                    float currDist = (float)Math.Sqrt(item.Point.X * item.Point.X + item.Point.Y * item.Point.Y);
                    if (currDist > maxDist) maxDist = currDist;
                }
                return maxDist;
            }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(", ListCount = {0}", Count);
        }

        public override string ToLongString(string format)
        {
            string finalString = ToString();
            int counter = 1;
            foreach (DataItem item in DataList)
            {
                finalString += "\n" + counter++.ToString() + ") ";
                finalString += item.ToLongString(format);
            }
            return finalString;
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            return new V4ListEnum(DataList);
        }

        public bool SaveBinary(string filename)
        {
            try
            {
                File.Create(filename).Close();
            }
            catch (ArgumentException)
            {
                return false;
            }
            using (BinaryWriter output_file = new BinaryWriter(File.OpenWrite(filename)))
            {
                try
                {
                    output_file.Write(string.Format("ID ={0}", ID));
                    output_file.Write(string.Format("Measure_time = {0}", MeasureTime));
                }
                catch (IOException)
                { return false; }
                foreach (DataItem item in DataList)
                {
                    string PointStr = "Point = " + item.Point.X.ToString() + ", " + item.Point.Y.ToString() + " ;";
                    string FieldStr = "Field vector = " + item.Field.X.ToString() + ", " + item.Field.Y.ToString();
                    try { output_file.Write(PointStr + FieldStr); }
                    catch (IOException)
                    { return false; }
                }
            }
            return true;
        }

        public bool LoadBinary(string filename)
        {
            try
            {
                if (!File.Exists(filename)) return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            try
            {
                using BinaryReader input_file = new BinaryReader(File.OpenRead(filename));
                string line;
                try { line = input_file.ReadString().Split('=')[1]; }
                catch (EndOfStreamException)
                { return false; }
                ID = line;
                try { line = input_file.ReadString().Split('=')[1]; }
                catch (EndOfStreamException)
                { return false; }
                _ = DateTime.TryParse(line, out DateTime date);
                MeasureTime = date;
                try { line = input_file.ReadString(); }
                catch (EndOfStreamException)
                { return false; }
                while (true)
                {
                    string pointline = line.Split(';')[0].Split('=')[1];
                    string fieldline = line.Split(';')[1].Split('=')[1];
                    _ = int.TryParse(pointline.Split(',')[0], out int pointX);
                    _ = int.TryParse(pointline.Split(',')[1], out int pointY);
                    _ = int.TryParse(fieldline.Split(',')[0], out int fieldX);
                    _ = int.TryParse(fieldline.Split(',')[1], out int fieldY);
                    DataList.Add(new DataItem(new Vector2(pointX, pointY), new Vector2(fieldX, fieldY)));
                    try { line = input_file.ReadString(); }
                    catch (EndOfStreamException)
                    { return true; }
                }
            }
            catch(IOException)
            {
                return false;
            }
           
        }
    }

    public class V4ListEnum : IEnumerator<DataItem>
    {
        public List<DataItem> DataList;

        private int position = -1;

        public V4ListEnum(List<DataItem> _DataList)
        {
            DataList = _DataList;
        }

        public DataItem Current
        {
            get
            {
                try
                {
                    return DataList[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            position++;
            return (position < DataList.Count);
        }

        public void Reset()
        {
            position = -1;
        }
    }

    /*____________________V4DataArray___________________*/

    public class V4DataArray : V4Data
    {
        public int XGrid
        {
            get;
            private set;
        }
        public int YGrid
        {
            get;
            private set;
        }
        public Vector2 GridStep
        {
            get;
            private set;
        }

        public Vector2[,] FieldVector
        {
            get;
            private set;
        }

        public V4DataArray(string _ID, DateTime _MeasureTime)
            : base(_ID, _MeasureTime)
        {
            FieldVector = new Vector2[0, 0];
        }
        public V4DataArray(string _ID, DateTime _MeasureTime, int _XGrid, int _YGrid, Vector2 _GridStep, Fv2Vector2 F)
            : base(_ID, _MeasureTime)
        {
            XGrid = _XGrid;
            YGrid = _YGrid;
            GridStep = _GridStep;
            FieldVector = new Vector2[XGrid, YGrid];
            for (int i = 0; i < XGrid; i++)
            {
                for (int j = 0; j < YGrid; j++)
                {
                    Vector2 currPoint = new Vector2(i * GridStep.X, j * GridStep.Y);
                    FieldVector[i, j] = F(currPoint);
                }
            }
        }

        public override int Count => XGrid * YGrid;

        public override float MaxFromOrigin
        {
            get
            {
                if (Count == 0) return 0;
                float XDist = (XGrid - 1) * GridStep.X;
                float YDist = (YGrid - 1) * GridStep.Y;
                return (float)Math.Sqrt(XDist * XDist + YDist * YDist);
            }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format("\nXGrid = {0}, " +
                "YGrid = {1}, GridStep = ({2}, {3})", XGrid, YGrid, GridStep.X, GridStep.Y);
        }

        public override string ToLongString(string format)
        {
            string finalString = ToString();
            for (int i = 0; i < XGrid; i++)
            {
                for (int j = 0; j < YGrid; j++)
                {
                    finalString += "\n Point: (" + (i * GridStep.X).ToString(format) + ", " +
                        (j * GridStep.Y).ToString(format) + ") ";
                    finalString += "FieldVector = {" + FieldVector[i, j].X.ToString(format) + "," +
                        FieldVector[i, j].Y.ToString(format) + "} ";
                    double magnitude = Math.Sqrt(Math.Pow(FieldVector[i, j].X, 2) + Math.Pow(FieldVector[i, j].Y, 2));
                    finalString += "FieldVector magnitude = " + magnitude.ToString(format);
                }
            }
            return finalString;
        }

        public static explicit operator V4DataList(V4DataArray array)
        {
            V4DataList newList = new V4DataList(array.ID, array.MeasureTime);
            
            for (int i = 0; i < array.XGrid; i++)
            {
                for (int j = 0; j < array.YGrid; j++)
                {
                    Vector2 newPoint = new Vector2(i * array.GridStep.X, j * array.GridStep.Y);
                    DataItem newData = new DataItem(newPoint, array.FieldVector[i, j]);
                    _ = newList.Add(newData);
                }
            }
            return newList;
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            return new V4ArrayEnum(FieldVector);
        }

        public bool SaveAsText(string filename)
        {
            try
            {
                File.Create(filename).Close();
            }
            catch (ArgumentException)
            {
                return false;
            }
            using (StreamWriter output_file = new StreamWriter(filename))
            {
                try
                {
                    output_file.WriteLine(string.Format("ID ={0}", ID));
                    output_file.WriteLine(string.Format("Measure_time = {0}", MeasureTime));
                    output_file.WriteLine(string.Format("XGrid = {0}", XGrid));
                    output_file.WriteLine(string.Format("YGrid = {0}", YGrid));
                    output_file.WriteLine(string.Format("GridStep = {0},{1}", GridStep.X, GridStep.Y));
                }
                catch (IOException)
                { return false; }
                for (int i = 0; i < XGrid; i++)
                {
                    for (int j = 0; j < YGrid; j++)
                    {
                        try
                        {
                            output_file.WriteLine(string.Format("Point: ({0}, {1}); Field Vector = {2}, {3}",
                                i * GridStep.X, j * GridStep.Y, FieldVector[i, j].X, FieldVector[i, j].Y));
                        }
                        catch (IOException)
                        { return false; }
                    }
                }
            }
            return true;
        }

        public bool LoadAsText(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                return false;
            }

            using (StreamReader input_file = File.OpenText(filename))
            {
                string line;
                try { line = input_file.ReadLine().Split('=')[1]; }
                catch (EndOfStreamException)
                { return false; }
                ID = line;
                try { line = input_file.ReadLine().Split('=')[1]; }
                catch (EndOfStreamException)
                { return false; }
                _ = DateTime.TryParse(line, out DateTime date);
                MeasureTime = date;
                try { line = input_file.ReadLine().Split('=')[1]; }
                catch (EndOfStreamException)
                { return false; }
                _ = int.TryParse(line, out  int number1);
                XGrid = number1;
                try { line = input_file.ReadLine().Split('=')[1]; }
                catch (EndOfStreamException)
                { return false; }
                _ = int.TryParse(line, out number1);
                YGrid = number1;
                try { line = input_file.ReadLine().Split('=')[1]; }
                catch (EndOfStreamException)
                { return false; }
                _ = int.TryParse(line.Split(',')[0], out number1);
                _ = int.TryParse(line.Split(',')[1], out int number2);
                GridStep = new Vector2(number1, number2);
                FieldVector = new Vector2[XGrid, YGrid];
                for (int i = 0; i < XGrid; i++)
                {
                    for (int j = 0; j < YGrid; j++)
                    {
                        try { line = input_file.ReadLine().Split(';')[1].Split('=')[1]; }
                        catch (EndOfStreamException)
                        { return false; }
                        _ = int.TryParse(line.Split(',')[0], out number1);
                        _ = int.TryParse(line.Split(',')[1], out number2);
                        FieldVector[i, j] = new Vector2(number1, number2);
                    }
                }
            }
            return true;
        }

    }

    public class V4ArrayEnum : IEnumerator<DataItem>
    {
        public Vector2 position = new Vector2(-1, -1);
        public Vector2[,] FieldVector;

        public V4ArrayEnum(Vector2[,] _FieldVector)
        {
            FieldVector = _FieldVector;
        }

        public DataItem Current
        {
            get
            {
                try
                {
                    int x = (int)position.X;
                    int y = (int)position.Y;
                    return new DataItem(position, FieldVector[x, y]);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if(position.Y < (FieldVector.GetLength(1) - 1))
            {
                position.Y++;
                if(position.X == -1)
                {
                    position.X++;
                }
                return true;
            }
            else if (position.X < (FieldVector.GetLength(0) - 1))
            {
                position.X++;
                position.Y = 0;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            position.X = -1;
            position.Y = -1;
        }
    }

    /*_________________V4DataMainCollection________________*/

    public class V4MainCollection
    {
        private readonly List<V4Data> DataList;

        public int Count => DataList.Count;

        public V4Data this[int index] => DataList[index];

        public V4MainCollection() => DataList = new List<V4Data>(); 

        public bool Contains(string ID)
        {
            foreach (V4Data item in DataList)
            {
                if (item.ID == ID) return true;
            }
            return false;
        }

        public bool Add(V4Data _V4Data)
        {
            foreach (V4Data item in DataList)
            {
                if (item.ID == _V4Data.ID) return false;
            }
            DataList.Add(_V4Data);
            return true;
        }

        public string ToLongString(string format)
        {
            string finalString = base.ToString() + "\n";
            foreach (V4Data item in DataList)
            {
                finalString += item.ToLongString(format) + "\n\n";
            }
            return finalString;
        }

        public override string ToString()
        {
            string finalString = base.ToString();
            foreach (V4Data item in DataList)
            {
                finalString += item.ToString() + "\n";
            }
            return finalString;
        }

        public int? ZeroFieldCount
        {
            get
            {
                if (DataList.Count == 0) return null;
                var Measurements = from data in DataList
                                   from DataItem item in data
                                   where item.Field.X == 0 || item.Field.Y == 0
                                   select item;
                if (Measurements.Count() == 0) return -1;
                return Measurements.Count();
            }
        }

        public System.Collections.Generic.IEnumerable<IGrouping<DateTime, V4DataList>> GroupTime
        {
            get
            {
                var ListMeasurements = from data in DataList
                                       where data.GetType() == typeof(V4DataList)
                                       select data as V4DataList;
                return ListMeasurements.GroupBy(data => data.MeasureTime);
            }
        }

       public System.Collections.Generic.IEnumerable<Vector2> IntersectMeasurePoints
        {
            get
            {
                var Points = from DataItem item in DataList[0]
                             select item.Point;
                var Intersection = from data in DataList
                                   select Points = Points.Intersect(from DataItem item in data
                                                                    select item.Point);
                Intersection = Intersection.TakeLast(1);
                return from data in Intersection
                       from item in data
                       select item; ;
            }
        }
    }

    /*_______________FieldVectorCalculations_______________*/

    public static class FieldVectorCalculations
    {
        public static Vector2 Summation(Vector2 v2) => new Vector2(v2.X + v2.Y, v2.X + v2.Y);
        public static Vector2 Multiply(Vector2 v2) => new Vector2(v2.X * v2.Y, v2.X * v2.Y);
        public static Vector2 Random(Vector2 v2) => new Vector2(new Random().Next(1,10), new Random().Next(1,10));
    }

    /*_____________________MainMethod______________________*/

    class Program
    {
        public static void SaveLoadObjects()
        {
            /* Array */
            Console.WriteLine("___________Array___________\n");

            V4DataArray Array1 = new V4DataArray("Array1", new DateTime(2021, 11, 13),
                3, 2, new Vector2(1, 1), FieldVectorCalculations.Summation);
            Console.WriteLine(Array1.ToLongString("F2") + "\n");
            if (Array1.SaveAsText("array1.txt"))
            {
                Console.WriteLine("CONSOLE: Array1 is saved in file 'array1.txt' as text\n");
            }
            else
            {
                Console.WriteLine("CONSOLE: ****ERROR! Array1 is not saved!****\n");
            }

            V4DataArray Array1FromFile = new V4DataArray("Array1FromFile", new DateTime(2021, 11, 13));
            if (Array1FromFile.LoadAsText("array1.txt"))
            {
                Console.WriteLine("CONSOLE: Array1FromFile is loaded from file 'array1.txt'\n");
            }
            else
            {
                Console.WriteLine("CONSOLE: ****ERROR! Array1FromFile is not loaded!****\n");
            }
            Console.WriteLine(Array1FromFile.ToLongString("F2") + "\n");

            /* List */
            Console.WriteLine("\n___________List___________\n");

            V4DataList List1 = new V4DataList("List1", new DateTime(2021, 11, 13));
            List1.AddDefaults(5, FieldVectorCalculations.Multiply);
            Console.WriteLine(List1.ToLongString("F2") + "\n");
            if(List1.SaveBinary("list1.txt"))
            {
                Console.WriteLine("CONSOLE: List1 is saved in file 'list1.txt' as binary\n");
            }
            else
            {
                Console.WriteLine("CONSOLE: ****ERROR! List1 is not saved!****\n");
            }

            V4DataList List1FromFile = new V4DataList("List1FromFile", new DateTime(2021, 11, 13));       
            if(List1FromFile.LoadBinary("list1.txt"))
            {
                Console.WriteLine("CONSOLE: List1FromFile is loaded from file 'list1.txt'\n");
            }
            else
            {
                Console.WriteLine("CONSOLE: ****ERROR! List1FromFile is not loaded!****\n");
            }
            Console.WriteLine(List1FromFile.ToLongString("F2") + "\n");
        }

        public static void UsageOfLINQ()
        {
            /* Create Collection */
            V4MainCollection collection = new V4MainCollection();
            V4DataArray Array1 = new V4DataArray("Array1", new DateTime(2021, 11, 13),
                2, 2, new Vector2(1, 1), FieldVectorCalculations.Summation);
            collection.Add(Array1);
            V4DataArray ArrayEmpty = new V4DataArray("ArrayEmpty", new DateTime(2021, 11, 13),
                0, 0, new Vector2(1, 1), FieldVectorCalculations.Summation);
            collection.Add(ArrayEmpty);
            V4DataList List1 = new V4DataList("List1", new DateTime(2021, 11, 13));
            List1.AddDefaults(2, FieldVectorCalculations.Multiply);
            collection.Add(List1);
            V4DataList List2 = new V4DataList("List2", new DateTime(2010, 11, 14));
            List2.AddDefaults(2, FieldVectorCalculations.Random);
            collection.Add(List2);
            V4DataList ListEmpty2 = new V4DataList("ListEmpty2", new DateTime(2010, 11, 14));
            collection.Add(ListEmpty2);
            V4DataList ListEmpty1 = new V4DataList("ListEmpty1", new DateTime(2021, 11, 13));
            collection.Add(ListEmpty1);

            Console.WriteLine("___________Collection___________\n");
            Console.WriteLine(collection.ToLongString("F2") + "\n");

            Console.WriteLine("___________ZeroFieldCount___________\n");
            Console.WriteLine("The amount of points with (Field.X == 0 or Field.Y ==0):   {0}\n", collection.ZeroFieldCount);

            Console.WriteLine("\n___________Lists from Collection Grouped By Datetime___________");
            foreach(IGrouping<DateTime, V4DataList> group in collection.GroupTime)
            {
                Console.WriteLine("\n-----------Date of measure: " + group.Key + "\n");
                foreach(V4DataList item in group)
                {
                    Console.WriteLine(item.ToLongString("F2") + "\n");
                }
            }

            Console.WriteLine("\n___________Intersection of Measure Points from collection___________\n");
            Console.WriteLine("-----Collection WITH empty lists and arrays:");
            int i = 0;
            Console.WriteLine("Intersection of measure points:");
            foreach(Vector2 point in collection.IntersectMeasurePoints)
            {
                Console.WriteLine("{1}) Point = {0}", point, i++);
            }
            if (i == 0)
            {
                Console.WriteLine("*****The intersection is empty!*****\n");
            }

            Console.WriteLine("\n-----Collection WITHOUT empty lists and arrays:");
            V4MainCollection collection1 = new V4MainCollection();
            collection1.Add(Array1);
            collection1.Add(List2);
            collection1.Add(List1);
            i = 0;
            Console.WriteLine("Intersection of measure points:");
            foreach (Vector2 point in collection1.IntersectMeasurePoints)
            {
                Console.WriteLine("{1}) Point = {0}", point, ++i);
            }
            if (i == 0)
            {
                Console.WriteLine("The intersection is empty!");
            }
        }

        static void Main()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);

            Console.WriteLine("--------------------------Saving and loading objects--------------------------\n");
            SaveLoadObjects();

            Console.WriteLine("--------------------------Usage of LINQ in Collection--------------------------\n");
            UsageOfLINQ();
        }
    }
}
