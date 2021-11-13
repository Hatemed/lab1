using System;
using System.Numerics;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;

namespace lab1
{
    public struct DataItem
    {

        public Vector2 coor { get; set; }
        public Complex field { get; set; }
        public DataItem(Vector2 coor, Complex field)
        {
            this.coor = coor;
            this.field = field;
        }
        public override string ToString()
        {
            return $"x = {coor.X}, y = {coor.Y}, field = {field.Real}+{field.Imaginary}i, |field| = {Complex.Abs(field)}";
        }
        public string ToLongString(string format)
        {

            return $"x = {String.Format(format, coor.X)}, y = {String.Format(format, coor.Y)}," +
                        $" field = {String.Format(format, field.Real)}+({String.Format(format, field.Imaginary)})i, " +
                        $"|field| = {String.Format(format, Complex.Abs(field))}";
        }
    }

    public delegate Complex Fv2Comlex(Vector2 v2);

    public abstract class V2Data: IEnumerable<DataItem>
    {
        public string Name { get; protected set; }
        public DateTime Date { get; protected set; }
        public V2Data(string Name, DateTime Date)
        {
            this.Name = Name;
            this.Date = Date;
        }
        public abstract int Count { get; }
        public abstract float MinDistance { get; }
        public abstract string ToLongString(string format);
        public override string ToString()
        {
            return $"Name = {Name}, Date = {Date}";
        }
        protected abstract IEnumerator GetEnumerator_();
        protected abstract IEnumerator<DataItem> GetEnumerator_DataItem();
        IEnumerator<DataItem> IEnumerable<DataItem>.GetEnumerator()
        {
            return GetEnumerator_DataItem();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator_();
        }
    }

    class V2DataList : V2Data
    {
        public List<DataItem> DataItems { get;}
        public V2DataList(string Name, DateTime Date) : base(Name, Date)
        {
            DataItems = new List<DataItem>();
        }
        public bool Add(DataItem newItem)
        {
            if (DataItems.Exists(x => x.coor == newItem.coor))
                return false;
            else
            {
                DataItems.Add(newItem);
                return true;
            }
        }
        public int AddDefaults(int nItems, Fv2Comlex F)
        {
            if (nItems < 0)
                throw new ArgumentException("отрицательное количество точек", "nItems");
            int result = 0;
            var rand = new Random();
            for (int i = 0; i < nItems; i++)
            {
                Vector2 coor = new Vector2(((float)rand.Next(-nItems, nItems)) / nItems,
                    ((float)rand.Next(-nItems, nItems)) / nItems); //случайная точка вблизи (0,0)
                DataItem newItem = new DataItem(coor, F(coor));
                if (this.Add(newItem))
                    result += 1;
            }
            return result;
        }
        public override int Count
        {
            get { return DataItems.Count; }
        }
        private float CountSqrDist(int l, int r)
        {
            return (DataItems[l].coor.X - DataItems[r].coor.X) * (DataItems[l].coor.X - DataItems[r].coor.X)
                + (DataItems[l].coor.Y - DataItems[r].coor.Y) * (DataItems[l].coor.Y - DataItems[r].coor.Y);
        }
        public override float MinDistance
        {
            get
            {
                if (Count > 1)
                {
                    float Min = float.MaxValue;
                    for (int i = 0; i < Count; i++)
                        for (int j = i + 1; j < Count; j++)
                            Min = Math.Min(Min, this.CountSqrDist(i, j));
                    return (float)Math.Sqrt(Min);
                }

                else
                    return 0;//меньше 2 точек
            }
        }
        public override string ToString()
        {
            return $"V2DataList, {base.ToString()}, Count = {Count} \n";
        }
        public override string ToLongString(string format)
        {
            string result = this.ToString();
            for (int i = 0; i < Count; i++)
                result += $"{DataItems[i].ToLongString(format)} \n";
            return result;
        }
        protected override IEnumerator GetEnumerator_()
        {
            return DataItems.GetEnumerator();
        }
        protected override IEnumerator<DataItem> GetEnumerator_DataItem()
        {
            return DataItems.GetEnumerator();
        }
        public bool SaveBinary(string filename)
        {
            try
            {
                using (var bw = new BinaryWriter(File.Open(filename, FileMode.Create)))
                {
                    bw.Write(Name);
                    bw.Write(Date.ToBinary());
                    foreach (DataItem i in DataItems)
                    {
                        bw.Write(i.coor.X);
                        bw.Write(i.coor.Y);
                        bw.Write(i.field.Real);
                        bw.Write(i.field.Imaginary);

                    };
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool LoadBinary(string filename)
        {
            try
            {
                using (var br = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    Name = br.ReadString();
                    Date = DateTime.FromBinary(br.ReadInt64());
                    DataItems.Clear();
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        var newitem = new DataItem(new Vector2(br.ReadSingle(), br.ReadSingle()), new Complex(br.ReadDouble(), br.ReadDouble()));
                        DataItems.Add(newitem);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }


    }

    class V2DataArray : V2Data
    {
        public int NumNodeX { get; private set; }
        public int NumNodeY { get; private set; }
        public Vector2 VectorNode { get; private set; }
        public Complex[,] ArrayField { get; private set; }
        public V2DataArray(string Name, DateTime Date) : base(Name, Date)
        {
            this.NumNodeX = 0;
            this.NumNodeY = 0;
            ArrayField = new Complex[0, 0];
        }
        public V2DataArray(string Name, DateTime Date, int NumNodeX, int NumNodeY,
                Vector2 VectorNode, Fv2Comlex Func) : base(Name, Date)
        {
            if ((NumNodeX < 0) || (NumNodeY < 0))
                throw new ArgumentException("отрицательное количество точек");
            this.NumNodeX = NumNodeX;
            this.NumNodeY = NumNodeY;
            this.VectorNode = VectorNode;
            ArrayField = new Complex[NumNodeX, NumNodeY];
            for (int i = 0; i < NumNodeX; i++)
                for (int j = 0; j < NumNodeY; j++)
                    ArrayField[i, j] = Func(new Vector2(i * VectorNode.X, j * VectorNode.Y));
        }

        public override int Count
        {
            get { return NumNodeX * NumNodeY; }
        }
        public override float MinDistance
        {
            get
            {
                if ((NumNodeX == 0) | (NumNodeY == 0))
                    return 0; // нет узлов
                else if ((NumNodeX == 1) & (NumNodeY == 1))
                    return 0; // 1 узел
                else if (NumNodeX == 1)
                    return VectorNode.Y;
                else if (NumNodeY == 1)
                    return VectorNode.X;
                else return Math.Min(VectorNode.X, VectorNode.Y);
            }
        }

        public override string ToString()
        {
            return $"V2DataArray, {base.ToString()}, NumNodeX = {NumNodeX}, " +
                    $"NumNodeY = {NumNodeY}, VectorX = {VectorNode.X}, VectorY = {VectorNode.Y} \n";
        }
        public override string ToLongString(string format)
        {
            string result = this.ToString();
            for (int i = 0; i < NumNodeX; i++)
                for (int j = 0; j < NumNodeY; j++)
                    result += $"x = {String.Format(format, i * VectorNode.X)}, y = {String.Format(format, j * VectorNode.Y)}," +
                        $" field = {String.Format(format, ArrayField[i, j].Real)}+({String.Format(format, ArrayField[i, j].Imaginary)})i, " +
                        $"|field| = {String.Format(format, Complex.Abs(ArrayField[i, j]))} \n";
            return result; ;
        }
        public static explicit operator V2DataList(V2DataArray Array)
        {
            V2DataList result = new V2DataList(Array.Name, Array.Date);
            for (int i = 0; i < Array.NumNodeX; i++)
                for (int j = 0; j < Array.NumNodeY; j++)
                {
                    DataItem NewItem = new DataItem(new Vector2(i * Array.VectorNode.X,
                            j * Array.VectorNode.Y), Array.ArrayField[i, j]);
                    result.Add(NewItem);
                }
            return result;
        }
        protected override IEnumerator GetEnumerator_()
        {
            return ((V2DataList)this).DataItems.GetEnumerator();
        }
        protected override IEnumerator<DataItem> GetEnumerator_DataItem()
        {
            return ((V2DataList)this).DataItems.GetEnumerator();
        }
        public bool SaveAsText(string filename)
        {
            try
            {
                using (var sw = new StreamWriter(filename))
                {
                    sw.WriteLine(Name);
                    sw.WriteLine(Date);
                    sw.WriteLine(NumNodeX);
                    sw.WriteLine(NumNodeY);
                    sw.WriteLine(VectorNode.X);
                    sw.WriteLine(VectorNode.Y);
                    foreach (var i in ArrayField)
                    {
                        sw.WriteLine(i.Real);
                        sw.WriteLine(i.Imaginary);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool LoadAsText(string filename)
        {
            try
            {
                using (var sr = new StreamReader(filename))
                {
                    Name = sr.ReadLine();
                    Date = DateTime.Parse(sr.ReadLine());
                    NumNodeX = int.Parse(sr.ReadLine());
                    NumNodeY = int.Parse(sr.ReadLine());
                    VectorNode = new Vector2(float.Parse(sr.ReadLine()), float.Parse(sr.ReadLine()));
                    ArrayField = new Complex[NumNodeX, NumNodeY];
                    for (var i = 0; i < NumNodeX; i++)
                        for (var j = 0; j < NumNodeY; j++)
                            ArrayField[i, j] = new Complex(float.Parse(sr.ReadLine()), float.Parse(sr.ReadLine()));
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
    class V2MainCollection
    {
        private List<V2Data> v2Datas;
        public V2MainCollection()
        {
            v2Datas = new List<V2Data>();
        }
        public int Count
        {
            get { return v2Datas.Count; }
        }
        public V2Data this[int num]
        {
            get { return v2Datas[num]; }
        }
        public bool Contains(string ID)

        {
            for (int i = 0; i < Count; i++)
            {
                if (v2Datas[i].Name == ID)
                {
                    return true;
                }
            }
            return false;
        }
        public bool Add(V2Data v2Data)
        {
            if (Contains(v2Data.Name))
                return false;
            else
            {
                v2Datas.Add(v2Data);
                return false;
            }
        }
        public string ToLongString(string format)
        {
            string result = "";
            for (int i = 0; i < Count; i++)
            {
                result += v2Datas[i].ToLongString(format) + "\n";
            }
            return result;
        }
        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Count; i++)
            {
                result += v2Datas[i].ToString() + "\n";
            }
            return result;
        }
        public DataItem? MaxField
        {
            get
            {
                var numDataItem = from i in v2Datas from j in i select j;
                if (numDataItem != null && numDataItem.Any())
                    return numDataItem.Aggregate((i1,i2) => Complex.Abs(i1.field) > Complex.Abs(i2.field)? i1: i2);
                else 
                    return null;
            }
        }
        public IEnumerable<Vector2> OnlyList
        {
            get
            {
                var numVector2List = from i in v2Datas where i is V2DataList from j in i select j.coor;
                var numVector2Array = from i in v2Datas where i is V2DataArray from j in i select j.coor;
                var result = numVector2List.Except<Vector2>(numVector2Array);
                if (result != null && result.Any())
                    return result;
                else
                    return null;
            }
        }
        public IEnumerable<IGrouping<int, V2Data>> Group
        {
            get
            {
                var numData = from i in v2Datas select i;
                var result = numData.GroupBy(X => X.Count);
                if (result != null && result.Any())
                    return result;
                else
                    return null;
            }
        }
 
    }
    static class TestFunc
    {
        public static Complex SimpleFunc(Vector2 v2)
        {
            return new Complex(v2.X, v2.Y);
        }
        public static Complex TrigFunc(Vector2 v2)
        {
            return new Complex(v2.X * (Math.Sin(v2.X) + Math.Sin(v2.Y)), v2.Y * (Math.Cos(v2.X) * Math.Cos(v2.Y)));
        }
    }
}