using System;
using System.Numerics;

namespace lab1
{
        class Program
    {
        private const string Filename = "C:\\Users\\Kozhe\\OneDrive\\Desktop\\Новый текстовый документ.txt";
        static void SaveLoad(string filename)
        {
            V2DataArray dataArray = new V2DataArray("test", DateTime.Today, 3, 3, new Vector2(0.2f, 0.5f), TestFunc.TrigFunc);
            dataArray.SaveAsText(filename);
            Console.WriteLine(dataArray.ToLongString("{0:f}"));
            V2DataArray copy_dataArray = new V2DataArray("test", DateTime.Today);
            copy_dataArray.LoadAsText(filename);
            Console.WriteLine(copy_dataArray.ToLongString("{0:f}"));

            V2DataList dataList = new V2DataList("test", DateTime.Today);
            dataList.AddDefaults(10, TestFunc.SimpleFunc);
            dataList.SaveBinary(filename);
            Console.WriteLine(dataList.ToLongString("{0:f}"));
            V2DataList copy_dataList = new V2DataList("test", DateTime.Today);
            copy_dataList.LoadBinary(filename);
            Console.WriteLine(copy_dataList.ToLongString("{0:f}"));
        }
        static void LINQ()
        {
            V2MainCollection mainCollection = new V2MainCollection();
            mainCollection.Add(new V2DataArray("test1", DateTime.Today, 5, 5, new Vector2(0.2f, 0.2f), TestFunc.TrigFunc));
            mainCollection.Add(new V2DataArray("test2", DateTime.Today, 0, 3, new Vector2(0.2f, 0.5f), TestFunc.SimpleFunc));
            V2DataList testList = new V2DataList("test3", DateTime.Today);
            testList.AddDefaults(0, TestFunc.TrigFunc);
            mainCollection.Add(testList);
            V2DataList testList2 = new V2DataList("test4", DateTime.Today);
            testList2.AddDefaults(5, TestFunc.SimpleFunc);
            mainCollection.Add(testList2);
            Console.WriteLine(mainCollection.ToLongString("{0:f}"));
            Console.WriteLine("DataItem c максимальным модулем поля: ");
            Console.WriteLine(mainCollection.MaxField);
            Console.WriteLine("Точки поля содержащиеся только в V2DataList: ");
            foreach (var i in mainCollection.OnlyList) // выведутся все векторы, кроме векторов 1 четверти,
                                                       // но из-за случайной генерации их может и не быть изначально
            {
                Console.WriteLine(i);
            }
            Console.WriteLine("Групировка V2Data по числу измерения поля: ");
            foreach (var i in mainCollection.Group) 
            {
                Console.Write("Key : ");
                Console.WriteLine(i.Key);
                foreach (var j in i)
                {
                    Console.WriteLine(j);
                }
            }
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Проверка записи/чтения");
            SaveLoad(Filename);
            Console.WriteLine("Проверка запросов");
            LINQ();
        }
    }
}
