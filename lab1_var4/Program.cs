using System;
using System.Numerics;
using System.Collections.Generic;

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

    /*______________________V4Data______________________*/

    public abstract class V4Data
    {
        public string ID { get; }
        public DateTime MeasureTime { get; }

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
    }

    /*____________________V4DataArray___________________*/

    public class V4DataArray : V4Data
    {
        public int XGrid { get; }
        public int YGrid { get; }
        public Vector2 GridStep { get; }

        public Vector2[,] FieldVector { get; }

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
        static void Main()
        {
            Console.WriteLine("/*_____________________1______________________*/\n");

            V4DataArray ArrayEmpty = new V4DataArray("ArrayEmpty", new DateTime(2021, 7, 10, 8, 30, 52),
                2, 0, new Vector2(1, 2), FieldVectorCalculations.Random);
            Console.WriteLine(ArrayEmpty.ToLongString("F2") + "\n");

            V4DataArray Array1 = new V4DataArray("Array1", new DateTime(2021, 10, 10, 8, 30, 52),
                3, 2, new Vector2(1, 1), FieldVectorCalculations.Summation);
            Console.WriteLine(Array1.ToLongString("F2") + "\n");

            V4DataList ListFromArray1 = (V4DataList)Array1;
            Console.WriteLine(ListFromArray1.ToLongString("F2") + "\n");

            V4DataList ListEmpty = new V4DataList("ListEmpty", new DateTime(2021, 9, 10, 8, 30, 52));
            Console.WriteLine(ListEmpty.ToLongString("F2") + "\n");

            Console.WriteLine("Count for Array1 = " + Array1.Count.ToString());
            Console.WriteLine("Count for ArrayEmpty = " + ArrayEmpty.Count.ToString());
            Console.WriteLine("Count for ListFromArray1 = " + ListFromArray1.Count.ToString());
            Console.WriteLine("Count for ListEmpty = " + ListEmpty.Count.ToString() + "\n");

            Console.WriteLine("MaxFromOrigin for Array1 = " + Array1.MaxFromOrigin.ToString());
            Console.WriteLine("MaxFromOrigin for ArrayEmpty = " + ArrayEmpty.MaxFromOrigin.ToString());
            Console.WriteLine("MaxFromOrigin for ListFromArray1 = " + ListFromArray1.MaxFromOrigin.ToString());
            Console.WriteLine("MaxFromOrigin for ListEmpty = " + ListEmpty.MaxFromOrigin.ToString() + "\n\n");



            Console.WriteLine("/*_____________________2______________________*/\n");

            V4DataArray Array2 = new V4DataArray("Array2", new DateTime(2021, 7, 10, 8, 30, 52),
                2, 5, new Vector2(1, 2), FieldVectorCalculations.Random);

            V4DataList List2 = new V4DataList("List2", new DateTime(2021, 9, 10, 8, 30, 52));
            List2.AddDefaults(5, FieldVectorCalculations.Multiply);

            

            V4MainCollection Collection1 = new V4MainCollection();

             
            Collection1.Add(Array1);
            Collection1.Add(Array2);
            Collection1.Add(ArrayEmpty);
            Collection1.Add(ListFromArray1); // this list won't be added to the collection, because it has the same ID as Array1
            Collection1.Add(List2);

            Console.WriteLine(Collection1.ToLongString("F2"));



            Console.WriteLine("/*_____________________3______________________*/\n");

            for (int i = 0; i < Collection1.Count; i++)
            {
                Console.WriteLine("{3}) Count for {0} = {1},  MaxFromOrigin for {0} = {2}\n",
                    Collection1[i].ID, Collection1[i].Count, Collection1[i].MaxFromOrigin,i+1);
            }
        }
    }
}
