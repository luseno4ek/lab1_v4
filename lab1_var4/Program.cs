using System;
using System.Numerics;
using System.Collections.Generic;

namespace lab1_var4
{
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
            string PointStr = "Point (" + Point.X.ToString(format) + "," + Point.Y.ToString(format) + ") ";
            string FieldStr = "Field vector (" + Field.X.ToString(format) + "," + Field.Y.ToString(format) + ") ";
            double FieldMagnitude = Math.Sqrt((Field.X * Field.X) + (Field.Y * Field.Y));
            string FieldMagnStr = "Field vector magnitude = " + FieldMagnitude.ToString(format);
            return PointStr + FieldStr + FieldMagnStr;
        }

        public override string ToString()
        {
            string PointStr = "Point " + Point.X.ToString() + "," + Point.Y.ToString();
            string FieldStr = "Field vector " + Field.X.ToString() + "," + Field.Y.ToString();
            return base.ToString() + " " + PointStr + " " + FieldStr;
        }
    }

    public delegate Vector2 Fv2Vector2(Vector2 v2);

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

    public class V4DataList : V4Data
    {
        public List<DataItem> DataList { get; }

        public V4DataList(string _ID, DateTime _MeasureTime) : base(_ID, _MeasureTime)
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
                float max_dist = 0;
                foreach (DataItem item in DataList)
                {
                    float curr_dist = (float)Math.Sqrt(item.Point.X * item.Point.X + item.Point.Y * item.Point.Y);
                    if (curr_dist > max_dist) max_dist = curr_dist;
                }
                return max_dist;
            }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(", ListCount = {0}", Count);
        }

        public override string ToLongString(string format)
        {
            string final_string = ToString();
            int counter = 1;
            foreach (DataItem item in DataList)
            {
                final_string += "\n" + counter++.ToString() + ") ";
                final_string += item.ToLongString(format);
            }
            return final_string;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var date1 = new DateTime(2008, 5, 1, 8, 30, 52);
            V4DataList check = new V4DataList("check", date1);
            check.AddDefaults(7, (v2) => new Vector2(v2.X + v2.Y, v2.X - v2.Y));
            Console.WriteLine(check.ToLongString("F3"));
        }
    }
}
