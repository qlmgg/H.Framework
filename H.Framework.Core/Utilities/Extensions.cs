﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace H.Framework.Core.Utilities
{
    public static class Extensions
    {
        public static IEnumerable<T> AddRangeNoRept<T>(this IEnumerable<T> list, IEnumerable<T> addList, IEqualityComparer<T> comparer = null)
        {
            var bothList = list.Intersect(addList, comparer);
            var notExsitList = addList.Except(bothList, comparer);
            return list.Concat(notExsitList);
        }

        public static string GetFileExt(this string fileName)
        {
            return fileName.Substring(fileName.LastIndexOf(".") + 1, (fileName.Length - fileName.LastIndexOf(".") - 1));
        }

        public static byte[] FileToBytes(this string path)
        {
            if (!File.Exists(path))
                return new byte[0];
            var fi = new FileInfo(path);
            var buff = new byte[fi.Length];
            using (FileStream fs = fi.OpenRead())
                fs.Read(buff, 0, Convert.ToInt32(fs.Length));
            return buff;
        }

        public static string FileToBase64String(this string path)
        {
            if (!File.Exists(path))
                return string.Empty;
            using (var filestream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var bt = new byte[filestream.Length];
                filestream.Read(bt, 0, bt.Length);
                return Convert.ToBase64String(bt);
            }
        }

        public static void WriteTxt(this string text, string file)
        {
            var fileStream = new FileStream(Environment.CurrentDirectory + "\\" + file, FileMode.Append);
            StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.Default);
            streamWriter.Write(text);
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }

        public static List<string> GetFile(this string path)
        {
            //传入一个路径
            var list = new List<string>();
            //如果路径是文件夹，继续遍历，把新遍历出来的文件夹和文件存到list
            if (Directory.Exists(path))
            {
                var dirs = Directory.GetDirectories(path);
                if (dirs != null)
                {
                    foreach (string dir in dirs)
                    {
                        list.AddRange(GetFile(dir));
                    }
                }

                var files = Directory.GetFiles(path);
                if (files != null)
                {
                    list.AddRange(files);
                }
            }
            //如果路径是文件，添加到list
            else if (File.Exists(path))
            {
                list.Add(path);
            }
            return list;
        }

        public static string GetRelativePath(this string path, string[] arr)
        {
            foreach (var item in arr)
                path = path.Replace(item, "");
            return path;
        }

        public static T Readjson<T>(this string json)
        {
            using (var file = File.OpenText(json))
            {
                using (var reader = new JsonTextReader(file))
                {
                    return JToken.ReadFrom(reader).ToString().ToJsonObj<T>();
                }
            }
        }

        public static bool NotNullAny<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }

        /// <summary>
        /// 由DataTable计算公式
        /// </summary>
        /// <param name="expression">表达式</param>
        public static double Calc(this string expression)
        {
            return double.Parse(new DataTable().Compute(expression, "").ToString());
        }

        public static bool IsPicture(this string filePath)
        {
            return filePath.IsMatchFileExt(new string[] { ".jpg", ".png", ".bmp", ".jpeg" });
        }

        public static bool IsMatchFileExt(this string filePath, params string[] exts)
        {
            return exts.Contains(Path.GetExtension(filePath));
        }

        public static DateTime ToDateTime(this long d, bool isMillisecond = false)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var resultTime = DateTime.MinValue;
            if (isMillisecond)
                resultTime = start.AddMilliseconds(d);
            else
                resultTime = start.AddSeconds(d);
            return resultTime.AddHours(8);
        }

        public static long ToLong(this DateTime dt, bool isMillisecond = false)
        {
            //var dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            //var toNow = dt.Subtract(dtStart);
            //var timeStamp = toNow.Ticks;
            //var len = isMillisecond ? 4 : 7;
            //timeStamp = long.Parse(timeStamp.ToString().Substring(0, timeStamp.ToString().Length - len));
            //return timeStamp;
            var jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var result = dt.AddHours(-8) - jan1st1970;
            if (isMillisecond)
                return (long)result.TotalMilliseconds;
            else
                return (long)result.TotalSeconds;
        }

        /// <summary>
        /// Json序列化
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToJson(this object source)
        {
            return JsonConvert.SerializeObject(source);
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T ToJsonObj<T>(this string source)
        {
            return JsonConvert.DeserializeObject<T>(source);
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T JsonToJsonObj<T>(this object source)
        {
            return source.ToString().ToJsonObj<T>();
        }

        /// <summary>
        /// 文件大小转换方法
        /// </summary>
        /// <param name="size">字节值</param>
        /// <returns></returns>
        public static string ReadableFilesize(this double size)
        {
            var units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
            double mod = 1024.0;
            int i = 0;
            while (size >= mod)
            {
                size /= mod;
                i++;
            }
            return Math.Round(size) + units[i];
        }

        public static ProjectionExpression<TSource> Project<TSource>(this IQueryable<TSource> source)
        {
            return new ProjectionExpression<TSource>(source);
        }

        public static void AllRound(this DataRow row, int i)
        {
            foreach (DataColumn _column in row.Table.Columns)
            {
                if (_column.DataType == typeof(decimal))
                {
                    row[_column.ColumnName] = Math.Round((decimal)row[_column.ColumnName], i);
                }
            }
        }

        public static void AllRound<T>(this T entity, int i)
        {
            T _t = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] propertys = _t.GetType().GetProperties();
            foreach (PropertyInfo property in propertys)
            {
                if (property.PropertyType == typeof(decimal))
                {
                    decimal round2 = (decimal)property.GetValue(entity, null);
                    property.SetValue(entity, Math.Round(round2, i), null);
                }
            }
        }

        public static DataRow Copy(this DataRow row)
        {
            DataRow newRow = row.Table.NewRow();
            newRow.ItemArray = row.ItemArray;
            return newRow;
        }

        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<IGrouping<int, T>> IGList) where T : class, new()
        {
            ObservableCollection<T> collection = new ObservableCollection<T>();
            foreach (IGrouping<int, T> group in IGList)
                foreach (T t in group)
                    collection.Add(t);
            return collection;
        }

        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<IGrouping<string, T>> IGList) where T : class, new()
        {
            ObservableCollection<T> collection = new ObservableCollection<T>();
            foreach (IGrouping<string, T> group in IGList)
                foreach (T t in group)
                    collection.Add(t);
            return collection;
        }

        public static List<T> List<T>(this IEnumerable<IGrouping<string, T>> IGList) where T : class, new()
        {
            List<T> collection = new List<T>();
            foreach (IGrouping<string, T> group in IGList)
                foreach (T t in group)
                    collection.Add(t);
            return collection;
        }

        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> Enum) where T : class, new()
        {
            ObservableCollection<T> obs = new ObservableCollection<T>(Enum);
            return obs;
        }

        public static ObservableCollection<T> ToObservable<T>(this Collection<T> Enum) where T : class, new()
        {
            ObservableCollection<T> obs = new ObservableCollection<T>(Enum);
            return obs;
        }

        public static ObservableCollection<T> ToObservable<T>(this IList<T> Enum) where T : class, new()
        {
            ObservableCollection<T> obs = new ObservableCollection<T>(Enum);
            return obs;
        }

        public static ObservableCollection<T> ToObservable<T>(this IList Enum) where T : class, new()
        {
            ObservableCollection<T> obs = new ObservableCollection<T>();
            foreach (T t in Enum)
                obs.Add(t);
            return obs;
        }

        public static ObservableCollection<T> ToObservable<T>(this IQueryable<T> Enum) where T : class, new()
        {
            ObservableCollection<T> obs = new ObservableCollection<T>(Enum);
            return obs;
        }

        public static ObservableCollection<T> ToObservable<T>(this IQueryable Enum) where T : class, new()
        {
            ObservableCollection<T> obs = new ObservableCollection<T>();
            foreach (T t in Enum)
                obs.Add(t);
            return obs;
        }

        public static void DefultAssignment<T>(this T t) where T : class, new()
        {
            PropertyInfo[] propertys = t.GetType().GetProperties();
            foreach (PropertyInfo property in propertys)
            {
                if (property.GetValue(t, null) == null)
                {
                    if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(t, 0, null);
                    }
                    if (property.PropertyType == typeof(Decimal))
                    {
                        property.SetValue(t, decimal.Parse("0"), null);
                    }
                    if (property.PropertyType == typeof(Double))
                    {
                        property.SetValue(t, 0.0, null);
                    }
                    else if (property.PropertyType == typeof(String))
                    {
                        property.SetValue(t, "", null);
                    }
                    else if (property.PropertyType == typeof(DateTime))
                    {
                        property.SetValue(t, DateTime.MinValue, null);
                    }
                    else if (property.PropertyType == typeof(Boolean))
                    {
                        property.SetValue(t, false, null);
                    }
                }
            }
        }

        public static string ToRound(this string parm, int decimals)
        {
            decimal outParm = 0;
            string result = "";
            if (decimal.TryParse(parm, out outParm))
            {
                result = Math.Round(decimal.Parse(parm), decimals).ToString();
            }
            else
                result = "0.00";
            return result;
        }

        public static bool TryParseDateYYMM(this string parm, ref decimal result)
        {
            string _dateText = parm + ".01";
            DateTime dateresult;
            bool is_date = DateTime.TryParse(_dateText, out dateresult);
            if (is_date)
            {
                result = Math.Abs(decimal.Parse(parm));
                return true;
            }
            else
            {
                return false;
            }
        }

        public static ObservableCollection<T> Copy<T>(this ObservableCollection<T> Enum) where T : class, new()
        {
            ObservableCollection<T> CopyedList = new ObservableCollection<T>();
            foreach (T SelfItem in Enum)
            {
                PropertyInfo[] propertys = SelfItem.GetType().GetProperties();
                T CopyItem = new T();
                foreach (var property in propertys)
                {
                    property.SetValue(CopyItem, property.GetValue(SelfItem, null), null);
                }
                CopyedList.Add(CopyItem);
            }
            return CopyedList;
        }

        public static ObservableCollection<T> CopyListWithoutItem<T>(this ObservableCollection<T> Enum) where T : class, new()
        {
            ObservableCollection<T> CopyedList = new ObservableCollection<T>();
            foreach (T SelfItem in Enum)
            {
                CopyedList.Add(SelfItem);
            }
            return CopyedList;
        }

        public static ObservableCollection<T> CopyListWithoutItemToObservable<T>(this IEnumerable<T> Enum) where T : class, new()
        {
            ObservableCollection<T> CopyedList = new ObservableCollection<T>();
            foreach (T SelfItem in Enum)
            {
                CopyedList.Add(SelfItem);
            }
            return CopyedList;
        }

        public static List<TChild> CopyToChild<T, TChild>(this List<T> Enum) where TChild : class, new()
        {
            T _t = (T)Activator.CreateInstance(typeof(T));
            TChild _tChild = (TChild)Activator.CreateInstance(typeof(TChild));
            PropertyInfo[] selfPropertys = _t.GetType().GetProperties();
            PropertyInfo[] childPropertys = _tChild.GetType().GetProperties();
            List<TChild> ChildEnum = new List<TChild>();
            foreach (T selfItem in Enum)
            {
                TChild childItem = new TChild();
                foreach (var property in selfPropertys)
                {
                    foreach (var childProperty in childPropertys)
                    {
                        if (property.Name == childProperty.Name)
                            property.SetValue(childItem, property.GetValue(selfItem, null), null);
                    }
                }
                ChildEnum.Add(childItem);
            }
            return ChildEnum;
        }

        public static ObservableCollection<TChild> CopyToChild<T, TChild>(this ObservableCollection<T> Enum) where TChild : class, new()
        {
            T _t = (T)Activator.CreateInstance(typeof(T));
            TChild _tChild = (TChild)Activator.CreateInstance(typeof(TChild));
            PropertyInfo[] selfPropertys = _t.GetType().GetProperties();
            PropertyInfo[] childPropertys = _tChild.GetType().GetProperties();
            ObservableCollection<TChild> ChildEnum = new ObservableCollection<TChild>();
            foreach (T selfItem in Enum)
            {
                TChild childItem = new TChild();
                foreach (var property in selfPropertys)
                {
                    foreach (var childProperty in childPropertys)
                    {
                        if (property.Name == childProperty.Name)
                            property.SetValue(childItem, property.GetValue(selfItem, null), null);
                    }
                }
                ChildEnum.Add(childItem);
            }
            return ChildEnum;
        }

        public static TChild CopyToChild<T, TChild>(this T entity) where TChild : class, new()
        {
            T _t = (T)Activator.CreateInstance(typeof(T));
            TChild _tChild = (TChild)Activator.CreateInstance(typeof(TChild));
            PropertyInfo[] selfPropertys = _t.GetType().GetProperties();
            PropertyInfo[] childPropertys = _tChild.GetType().GetProperties();
            TChild childEntity = new TChild();
            foreach (var property in selfPropertys)
            {
                foreach (var childProperty in childPropertys)
                {
                    if (property.Name == childProperty.Name)
                        property.SetValue(childEntity, property.GetValue(entity, null), null);
                }
            }
            return childEntity;
        }

        public static bool CompareTo<T>(this ObservableCollection<T> Enum, ObservableCollection<T> CompareList, ref string Different) where T : class, new()
        {
            T _t = (T)Activator.CreateInstance(typeof(T));

            PropertyInfo[] propertys = _t.GetType().GetProperties();

            bool isEquals = true;
            foreach (T t1 in Enum)
            {
                foreach (T t2 in CompareList)
                {
                    foreach (PropertyInfo p_info in propertys)
                    {
                        if (Enum.IndexOf(t1).Equals(CompareList.IndexOf(t2)))
                        {
                            if (!p_info.Name.Equals("ExtensionData"))
                            {
                                if (p_info.GetValue(t1, null) == null || p_info.GetValue(t2, null) == null)
                                {
                                    if (p_info.GetValue(t1, null) == null && p_info.GetValue(t2, null) == null)
                                    {
                                        isEquals = true;
                                    }
                                    else
                                    {
                                        Different = "下标：" + CompareList.IndexOf(t2) + "--字段：" + p_info.Name + "--[原值：" + p_info.GetValue(t1, null) + "]-[比较值：" + p_info.GetValue(t2, null) + "]";
                                        isEquals = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (p_info.PropertyType == typeof(decimal) || p_info.PropertyType == typeof(double) || p_info.PropertyType == typeof(int))
                                    {
                                        if (!p_info.GetValue(t1, null).Equals(p_info.GetValue(t2, null)))
                                        {
                                            Different = "字段：" + p_info.Name + "--[原值：" + p_info.GetValue(t1, null) + "][比较值：" + p_info.GetValue(t2, null) + "]";
                                            isEquals = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (!p_info.GetValue(t1, null).ToString().Trim().Equals(p_info.GetValue(t2, null).ToString().Trim()))
                                        {
                                            Different = "字段：" + p_info.Name + "--[原值：" + p_info.GetValue(t1, null).ToString() + "][比较值：" + p_info.GetValue(t2, null).ToString() + "]";
                                            isEquals = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (!isEquals)
                {
                    break;
                }
            }
            return isEquals;
        }

        public static bool CompareTo<T>(this T Entity, T CompareEntity, ref string Different) where T : class, new()
        {
            T _t = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] propertys = _t.GetType().GetProperties();
            bool isEquals = true;
            foreach (PropertyInfo p_info in propertys)
            {
                if (!p_info.Name.Equals("ExtensionData"))
                {
                    if (p_info.GetValue(Entity, null) == null || p_info.GetValue(CompareEntity, null) == null)
                    {
                        if (p_info.GetValue(Entity, null) == null && p_info.GetValue(CompareEntity, null) == null)
                        {
                            isEquals = true;
                        }
                        else
                        {
                            Different = "字段：" + p_info.Name + "--[原值：" + p_info.GetValue(Entity, null) + "][比较值：" + p_info.GetValue(CompareEntity, null) + "]";
                            isEquals = false;
                            break;
                        }
                    }
                    else
                    {
                        if (p_info.PropertyType == typeof(decimal) || p_info.PropertyType == typeof(double) || p_info.PropertyType == typeof(int))
                        {
                            if (!p_info.GetValue(Entity, null).Equals(p_info.GetValue(CompareEntity, null)))
                            {
                                Different = "字段：" + p_info.Name + "--[原值：" + p_info.GetValue(Entity, null) + "][比较值：" + p_info.GetValue(CompareEntity, null) + "]";
                                isEquals = false;
                                break;
                            }
                        }
                        else
                        {
                            if (!p_info.GetValue(Entity, null).ToString().Trim().Equals(p_info.GetValue(CompareEntity, null).ToString().Trim()))
                            {
                                Different = "字段：" + p_info.Name + "--[原值：" + p_info.GetValue(Entity, null).ToString() + "][比较值：" + p_info.GetValue(CompareEntity, null).ToString() + "]";
                                isEquals = false;
                                break;
                            }
                        }
                    }
                }
                if (!isEquals)
                {
                    break;
                }
            }
            return isEquals;
        }

        public static T Copy<T>(this T Entity) where T : class, new()
        {
            T CopyedEntity = new T();
            var propertys = CopyedEntity.GetType().GetProperties();
            foreach (var property in propertys)
            {
                if (property.CanWrite)
                    property.SetValue(CopyedEntity, property.GetValue(Entity, null), null);
            }
            return CopyedEntity;
        }

        public static DataTable ToDataTable<T>(this ObservableCollection<T> collection, string ColumnNames) where T : class, new()
        {
            if (collection == null)
                return null;
            DataTable result = new DataTable("table1");
            T _t = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] propertys = _t.GetType().GetProperties();
            string[] ColumnArray = ColumnNames.Split(',');
            foreach (var columnname in ColumnArray)
            {
                foreach (PropertyInfo p_info in propertys)
                {
                    if (p_info.Name.Equals(columnname))
                    {
                        result.Columns.Add(p_info.Name, p_info.PropertyType);
                    }
                }
            }
            if (result.Columns.Contains("ExtensionData"))
                result.Columns.Remove("ExtensionData");
            foreach (T entity in collection)
            {
                PropertyInfo[] entityPropertys = entity.GetType().GetProperties();
                DataRow row = result.NewRow();
                foreach (PropertyInfo p_info in entityPropertys)
                {
                    if (result.Columns.Contains(p_info.Name))
                        row[p_info.Name] = p_info.GetValue(entity, null);
                }
                result.Rows.Add(row);
            }
            return result;
        }

        public static DataTable ToSchemeTable<T>(this ObservableCollection<T> collection, DataTable SchemeTable) where T : class, new()
        {
            if (collection == null)
                return null;
            T _t = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] propertys = _t.GetType().GetProperties();
            foreach (T entity in collection)
            {
                PropertyInfo[] entityPropertys = entity.GetType().GetProperties();
                DataRow row = SchemeTable.NewRow();
                foreach (PropertyInfo p_info in entityPropertys)
                {
                    if (SchemeTable.Columns.Contains(p_info.Name))
                        row[p_info.Name] = p_info.GetValue(entity, null);
                }
                SchemeTable.Rows.Add(row);
            }
            return SchemeTable;
        }

        /// <summary>
        /// 未完成（或许不需要）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="PropertyName"></param>
        public static void RemoveProperty<T>(this ObservableCollection<T> collection, string PropertyName) where T : class, new()
        {
            if (collection == null)
                return;
            T _t = (T)Activator.CreateInstance(typeof(T));
            List<PropertyInfo> propertys = _t.GetType().GetProperties().ToList();
            foreach (PropertyInfo p_info in propertys)
            {
                if (p_info.Name.Contains(PropertyName))
                {
                    propertys.Remove(p_info);
                    break;
                }
            }
        }

        public static T Field<T>(this IDataRecord record, string fieldName)
        {
            T fieldValue = default(T);
            for (int i = 0; i < record.FieldCount; i++)
            {
                if (string.Equals(record.GetName(i), fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    if (record[i] != DBNull.Value)
                    {
                        fieldValue = (T)record[fieldName];
                    }
                }
            }
            return fieldValue;
        }

        public static List<T> GetEnumerator<T>(this IDataReader reader)
        {
            List<T> newList = new List<T>();
            DataTable schema = reader.GetSchemaTable();
            DataColumn[] columns = { schema.Columns["ColumnName"] };
            schema.PrimaryKey = columns;
            while (reader.Read())
            {
                T _t = (T)Activator.CreateInstance(typeof(T));
                PropertyInfo[] propertys = typeof(T).GetProperties();
                foreach (PropertyInfo property in propertys)
                {
                    if (schema.Rows.Find(property.Name) != null)
                        property.SetValue(_t, reader[property.Name], null);
                }
                newList.Add(_t);
            }
            return newList;
        }

        public static IEnumerable<T> GetEnumerator<T>(this IDataReader reader, Func<IDataRecord, T> generator)
        {
            while (reader.Read())
                yield return generator(reader);
        }

        public static Func<IDataRecord, T> DynamicCreateEntity<T>()
        {
            ParameterExpression r = Expression.Parameter(typeof(IDataRecord), "r");
            List<MemberBinding> bindings = new List<MemberBinding>();
            foreach (PropertyInfo property in (typeof(T).GetProperties()))
            {
                MethodCallExpression propertyValue = Expression.Call(typeof(Extensions).GetMethod("Field").MakeGenericMethod(property.PropertyType), r, Expression.Constant(property.Name));
                MemberBinding binding = Expression.Bind(property, propertyValue); bindings.Add(binding);
            }
            Expression initializer = Expression.MemberInit(Expression.New(typeof(T)), bindings);
            Expression<Func<IDataRecord, T>> lambda = Expression.Lambda<Func<IDataRecord, T>>(initializer, r);
            return lambda.Compile();
        }

        //public static IList<T> ExecuteStoredProc<T>(this DataBaseHelper dataBase, string storedProcedureName, params SqlParameter[] parameterValues)
        //{
        //    IList<T> result = null;
        //    using (IDataReader dr = dataBase.ExecuteReaderStoredProcForReader(storedProcedureName, parameterValues))
        //    {
        //        result = dr.GetEnumerator(DynamicCreateEntity<T>()).ToList();
        //    }
        //    return result;
        //}

        public static DataSet GetEnumeratorDataSet(this IDataReader reader, int TableCount)
        {
            DataSet newListCollection = new DataSet();
            string[] tables = new string[TableCount];
            newListCollection.Load(reader, LoadOption.OverwriteChanges, tables);
            return newListCollection;
        }

        public static List<List<T>> GetEnumeratorCollection<T>(this IDataReader reader)
        {
            List<List<T>> newListCollection = new List<List<T>>();
            if (reader.Read())
            {
                List<T> newList1 = new List<T>();
                DataTable schemaFirst = reader.GetSchemaTable();
                DataColumn[] columnsFirst = { schemaFirst.Columns["ColumnName"] };
                schemaFirst.PrimaryKey = columnsFirst;
                T _tFirst = (T)Activator.CreateInstance(typeof(T));
                PropertyInfo[] propertysFirst = typeof(T).GetProperties();
                foreach (PropertyInfo property in propertysFirst)
                {
                    if (schemaFirst.Rows.Find(property.Name) != null)
                        property.SetValue(_tFirst, reader[property.Name], null);
                }
                newList1.Add(_tFirst);
                while (reader.Read())
                {
                    T _t = (T)Activator.CreateInstance(typeof(T));
                    PropertyInfo[] propertys = typeof(T).GetProperties();
                    foreach (PropertyInfo property in propertys)
                    {
                        if (schemaFirst.Rows.Find(property.Name) != null)
                            property.SetValue(_t, reader[property.Name], null);
                    }
                    newList1.Add(_t);
                }
                newListCollection.Add(newList1);
                while (reader.NextResult())
                {
                    List<T> newList2 = new List<T>();
                    DataTable schema2 = reader.GetSchemaTable();
                    DataColumn[] columns2 = { schema2.Columns["ColumnName"] };
                    schema2.PrimaryKey = columns2;
                    while (reader.Read())
                    {
                        T _t = (T)Activator.CreateInstance(typeof(T));
                        PropertyInfo[] propertys = typeof(T).GetProperties();
                        foreach (PropertyInfo property in propertys)
                        {
                            if (schema2.Rows.Find(property.Name) != null)
                                property.SetValue(_t, reader[property.Name], null);
                        }
                        newList2.Add(_t);
                    }
                    newListCollection.Add(newList2);
                }
            }
            return newListCollection;
        }

        public static string ValidateHasField(this DataTable dataTable, string ColumnNames)
        {
            StringBuilder builder = new StringBuilder();
            bool hasError = false;
            string[] ColumnsName = ColumnNames.Split(',');
            foreach (var name in ColumnsName)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (name.Trim().Equals(column.ColumnName))
                    {
                        hasError = false;
                        break;
                    }
                    else
                        hasError = true;
                }
                if (hasError)
                    builder.Append(name + "不存在;");
            }
            return builder.ToString();
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this DataTable dt) where T : class, new()
        {
            if (dt == null)
            {
                throw new Exception("DT为null");
            }
            DataTable _DataTable = dt;
            // 返回值初始化
            ObservableCollection<T> result = new ObservableCollection<T>();
            for (int j = 0; j < _DataTable.Rows.Count; j++)
            {
                T _t = (T)Activator.CreateInstance(typeof(T));
                PropertyInfo[] propertys = _t.GetType().GetProperties();
                foreach (PropertyInfo p_info in propertys)
                {
                    for (int i = 0; i < _DataTable.Columns.Count; i++)
                    {
                        // 属性与字段名称一致的进行赋值
                        if (p_info.Name.Equals(_DataTable.Columns[i].ColumnName))
                        {
                            // 数据库NULL值单独处理
                            if (_DataTable.Rows[j][i] != DBNull.Value)
                                p_info.SetValue(_t, _DataTable.Rows[j][i], null);
                            else
                                p_info.SetValue(_t, null, null);
                            break;
                        }
                    }
                }
                result.Add(_t);
            }
            return result;
        }
    }
}