using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using FastReport;
using FastReport.Data;
using FastReport.RichTextParser;

namespace DataFromDataSet
{
    public partial class Form1 : Form
    {
        private DataSet FDataSet;
        private TableDataSource tbs;

        public Form1()
        {
            InitializeComponent();
            RunReport();

        }

        private void CreateDataSet()
        {
            // create simple dataset with one table
            FDataSet = new DataSet();

            var table = new MyDataTable();
            table.TableName = "Employees";
            FDataSet.Tables.Add(table);

            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));

            table.Rows.Add(1, "Andrew Fuller");
            table.Rows.Add(2, "Nancy Davolio");
            table.Rows.Add(3, "Margaret Peacock");
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            CreateDataSet();

            // create report instance
            Report report = new Report();
                        

            // register the dataset
            report.RegisterData(FDataSet);

            // enable the "Employees" datasource programmatically. 
            // You can also do this in the "Report|Choose Report Data..." menu.
            report.GetDataSource("Employees").Enabled = true;

            // design the report
            report.Design();

            // free resources used by report
            report.Dispose();
        }

        private void btnRunExisting_Click(object sender, EventArgs e)
        {
            CreateDataSet();

            // create report instance
            Report report = new Report();

            // load the existing report
            report.Load(@"..\..\report.frx");

            // register the dataset
            report.RegisterData(FDataSet);

            // run the report
            report.Show();

            // free resources used by report
            report.Dispose();
        }

        //public class DataClassBase {
        //    private Dictionary<string, object> _values = new Dictionary<string, object>();
        //    public void Assign(Dictionary<string, object> values)
        //    {
        //        _values.Clear();
        //        foreach (var kvp in values)
        //            _values.Add(kvp.Key, kvp.Value);
        //    }
        //    protected T GetValue<T>(string name, T defaultValue) {
        //        if (!_values.TryGetValue(name, out var result))
        //        {
        //            result = null;
        //        }

        //        return typeof(T).IsInstanceOfType(result) ? (T)result : defaultValue;
        //        //return result as T ?? defaultValue;
        //    }
        //}

        //public class DataClass: DataClassBase
        //{
        //    public long ID => GetValue<long>("ID", 0);
        //    public string Title => GetValue<long>("Title", "");
        //    public int Tag => GetValue<int>("Tag", 0);
        //}

        public class DataClass
        {
            public long ID { get; set; }
            public string Title { get; set; }
            public int Tag { get; set; }
        }


        private void RunReport()
        {
            // create report instance
            Report report = new Report();

            //// register the dataset
            //report.RegisterData(FDataSet);

            //BusinessObjectDataSource bs = new BusinessObjectDataSource();
            //report.AllObjects.Add(bs);
            var bslist = new List<DataClass>();
            report.RegisterData(bslist, "TEST");

            var ds = report.GetDataSource("TEST") as BusinessObjectDataSource;
            if (ds != null)
            {
                //ds.Columns.Add(new Column() { DataType = typeof(long), Name = "ID" });
                //ds.Columns.Add(new Column() { DataType = typeof(string), Name = "Title" });
                ds.Enabled = true;
                ds.LoadBusinessObject += Bs_LoadBusinessObject;
            }

            var bslist2 = new List<DataClass>();
            report.RegisterData(bslist2, "TEST2");
            var oldBS = report.Dictionary.DataSources.FindByAlias("TEST2");
            report.Dictionary.DataSources.Remove(oldBS);
            report.RemoveChild(oldBS);
            var newBS = new MyBusinessObjectDataSource();
            newBS.Assign(oldBS);
            newBS.CreateColumns(new Dictionary<string, Type>() { ["ID"] = typeof(long), ["Title"] = typeof(string) });
            report.Dictionary.DataSources.Add(newBS);

            var ds2 = report.GetDataSource("TEST2") as BusinessObjectDataSource;
            if (ds2 != null)
            {
                //ds2.Columns.Add(new Column() { DataType = typeof(long), Name = "ID" });
                //ds2.Columns.Add(new Column() { DataType = typeof(string), Name = "Title" });
                ds2.LoadBusinessObject += Bs_LoadBusinessObject2;
                ds2.Enabled = true;
            }


            var table = new MyDataTable();
            table.TableName = "XXX";
            
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));

            report.RegisterData(table, "XXX");
            TryToLoadData(table);

            // enable the "Employees" datasource programmatically. 
            // You can also do this in the "Report|Choose Report Data..." menu.
            //report.GetDataSource("Employees").Enabled = true;

            // design the report
            report.Design();

            // free resources used by report
            report.Dispose();
        }

        private void TryToLoadData( MyDataTable table)
        {            

            string text = table.GetType().Name;
            if (text.EndsWith("DataTable"))
            {
                text = text.Substring(0, text.Length - 9);
            }

            text += "TableAdapter";
            Type[] types = table.GetType().Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!(type.Name == text))
                {
                    continue;
                }

                object obj = Activator.CreateInstance(type);
                try
                {
                    MethodInfo method = obj.GetType().GetMethod("Fill");
                    if (method != null)
                    {
                        method.Invoke(obj, new object[1] { table });
                    }
                }
                catch
                {
                }
                finally
                {
                    if (obj is IDisposable)
                    {
                        (obj as IDisposable).Dispose();
                    }
                }

                break;
            }
        }

        private void Bs_LoadBusinessObject(object sender, LoadBusinessObjectEventArgs e)
        {
            Debug.WriteLine($"Bs_LoadBusinessObject: {e.parent}", "INFO");
            var bs = sender as BusinessObjectDataSource;
            (bs.Reference as List<DataClass>).Add(new DataClass() { ID = 123, Title = "asdfasdf" });
            (bs.Reference as List<DataClass>).Add(new DataClass() { ID = 456, Title = "dfstgdf" });
            (bs.Reference as List<DataClass>).Add(new DataClass() { ID = 789, Title = "xxx" });
        }
        private void Bs_LoadBusinessObject2(object sender, LoadBusinessObjectEventArgs e)
        {
            Debug.WriteLine($"Bs_LoadBusinessObject2", "INFO");
            var bs = sender as BusinessObjectDataSource;
            var ps = bs.Report.GetDataSource("TEST") as BusinessObjectDataSource;
            var parentrowno = ps.CurrentRowNo;
            Debug.WriteLine($"Bs_LoadBusinessObject2: parentrow: {parentrowno}", "INFO");

            (bs.Reference as List<DataClass>).Clear();
            if (parentrowno == 0)
            {
                (bs.Reference as List<DataClass>).Add(new DataClass() { ID = 55656, Title = "asaa" });
                (bs.Reference as List<DataClass>).Add(new DataClass() { ID = 54552, Title = "bb" });
                (bs.Reference as List<DataClass>).Add(new DataClass() { ID = 578787, Title = "ccc" });
            }
            else if (parentrowno == 1)
            {
                (bs.Reference as List<DataClass>).Add(new DataClass() { ID = 5656, Title = "fsdafgasdfsd" });
                (bs.Reference as List<DataClass>).Add(new DataClass() { ID = 45549, Title = "dfsagsdfgsdf" });
            }
            else if (parentrowno == 2)
            {
                (bs.Reference as List<DataClass>).Add(new DataClass() { ID = 4, Title = "sdfkljghjksdfjkghsdkfjhgkjsdhfkghsdkfghk" });
            }
        }
    }
}