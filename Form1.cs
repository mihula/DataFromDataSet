using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using FastReport;
using FastReport.Data;
using FastReport.RichTextParser;
using static DataFromDataSet.Form1;

namespace DataFromDataSet
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            RunReport();
        }

        private void btnRunExisting_Click(object sender, EventArgs e)
        {
            RunReport();
        }

        public class ReportModel
        {
            public string Name { get; set; }

            public ReportSelectModel MasterSelect { get; set; }
        }

        public class ReportSelectModel
        {
            public string Name { get; set; }
            public string SQL { get; set; }
            public List<ReportSelectModel> SubSelects { get; } = new List<ReportSelectModel>();
        }

        public ReportModel CreateSampleModel()
        {
            ReportModel model = new ReportModel();
            model.Name = "TEST_REPORT";
            model.MasterSelect = new ReportSelectModel() { Name = "SERIES", SQL = "SELECT SERIES_ID, TITLE FROM PRG_SERIES_VW WHERE X_UPTITLE LIKE 'A%'" };
            model.MasterSelect.SubSelects.Add(new ReportSelectModel() { Name = "PROG", SQL = "SELECT PROG_ID, TITLE FROM PRG_PROG_VW WHERE SERIES_ID=:SERIES_ID" });
            return model;
        }

        public class ReportDataTable : DataTable
        {
            public ReportSelectModel SelectModel { get; set; }
            public ReportDataTable MasterDataTable { get; set; }
            public List<ReportDataTable> SubDataTables { get; } = new List<ReportDataTable>();
            public DataSourceBase ReportDataSource { get; set; }

            public void PopulateColumns(ReportSelectModel selectModel)
            {
                Columns.Clear();
                Columns.Add("currentrowid", typeof(int));
                Columns.Add("masterrowid", typeof(int));
                Columns.Add($"{selectModel.Name}_ID", typeof(decimal));
                Columns.Add("TITLE", typeof(string));
            }

            public void DataLoad(TableDataSource tableDataSource)
            {
                this.Rows.Clear();

                var masterDataSource = this.MasterDataTable?.ReportDataSource;
                var masterRownNo = masterDataSource?.CurrentRowNo ?? 0;
                int masterrowid = 0;
                if (masterDataSource != null)
                {
                    masterrowid = ((DataRow)masterDataSource.CurrentRow)[0] as int? ?? 0;
                }

                int rowid = 0;
                if (this.SelectModel.Name == "SERIES")
                {
                    this.Rows.Add(new object[] { rowid++, masterrowid, (decimal)123456, "SERIES 123456" });
                    this.Rows.Add(new object[] { rowid++, masterrowid, (decimal)888, "SERIES 888" });
                    this.Rows.Add(new object[] { rowid++, masterrowid, (decimal)98989, "SERIES 98989" });
                }
                else if (this.SelectModel.Name == "PROG")
                {
                    for (int i = 0; i < 5 + (2 * masterRownNo); i++)
                    {
                        this.Rows.Add(new object[] { rowid++, masterrowid, (decimal)((100 * (1 + masterRownNo)) + i), $"Master {masterRownNo}, Ep {i + 1}" });
                    }
                }
            }
        }

        //public class ReportTableAdapter : DataTable
        //{
        //    public int Fill(DataTable dataTable)
        //    {
        //        var reportDataTable = dataTable as ReportDataTable;
        //        if (reportDataTable == null) return 0;
        //        //reportDataTable.Rows.Clear();

        //        var masterDataSource = reportDataTable.MasterDataTable?.ReportDataSource;
        //        var masterRownNo = masterDataSource?.CurrentRowNo ?? 0;
        //        int masterrowid = 0;
        //        if (masterDataSource != null)
        //        {
        //            masterrowid = ((DataRow)masterDataSource.CurrentRow)[0] as int? ?? 0;
        //        }

        //        int rowid = 0;
        //        if (reportDataTable.SelectModel.Name == "SERIES")
        //        {
        //            reportDataTable.Rows.Add(new object[] { rowid++, masterrowid, (decimal)123456, "SERIES 123456" });
        //            reportDataTable.Rows.Add(new object[] { rowid++, masterrowid, (decimal)888, "SERIES 888" });
        //            reportDataTable.Rows.Add(new object[] { rowid++, masterrowid, (decimal)98989, "SERIES 98989" });
        //        }
        //        else if (reportDataTable.SelectModel.Name == "PROG")
        //        {
        //            for (int i = 1; i < 10 + (5 * masterRownNo); i++)
        //            {
        //                reportDataTable.Rows.Add(new object[] { rowid++, masterrowid, (decimal)((100 * (1 + masterRownNo)) + i), $"Master {masterRownNo}, Ep {i + 1}" });
        //            }
        //        }

        //        return rowid;
        //    }
        //}

        //public class QueryDataSource : DataSourceBase
        //{
        //    public DataView View => base.Reference as DataView;

        //    private FastReport.Data.Column CreateColumn(DataColumn column)
        //    {
        //        var column2 = new FastReport.Data.Column();
        //        column2.Name = column.ColumnName;
        //        column2.Alias = column.Caption;
        //        column2.DataType = column.DataType;
        //        if (column2.DataType == typeof(byte[]))
        //        {
        //            column2.BindableControl = ColumnBindableControl.Picture;
        //        }
        //        else if (column2.DataType == typeof(bool))
        //        {
        //            column2.BindableControl = ColumnBindableControl.CheckBox;
        //        }

        //        return column2;
        //    }

        //    private void CreateColumns()
        //    {
        //        base.Columns.Clear();
        //        if (View == null)
        //        {
        //            return;
        //        }

        //        foreach (DataColumn column in View.Table.Columns)
        //        {
        //            var value = CreateColumn(column);
        //            base.Columns.Add(value);
        //        }
        //    }

        //    protected override object GetValue(FastReport.Data.Column column)
        //    {
        //        if (column == null)
        //        {
        //            return null;
        //        }

        //        if (column.Tag == null)
        //        {
        //            column.Tag = View.Table.Columns.IndexOf(column.Name);
        //        }

        //        if (base.CurrentRow != null)
        //        {
        //            return ((DataRowView)base.CurrentRow)[(int)column.Tag];
        //        }

        //        return null;
        //    }

        //    public override void InitSchema()
        //    {
        //        if (base.Columns.Count == 0)
        //        {
        //            CreateColumns();
        //        }

        //        foreach (FastReport.Data.Column column in base.Columns)
        //        {
        //            column.Tag = null;
        //        }
        //    }

        //    public override void LoadData(ArrayList rows)
        //    {
        //        OnLoad();
        //        if (base.ForceLoadData || rows.Count == 0)
        //        {
        //            rows.Clear();
        //            for (int i = 0; i < View.Count; i++)
        //            {
        //                rows.Add(View[i]);
        //            }
        //        }
        //    }

        //    internal void RefreshColumns()
        //    {
        //        if (View == null)
        //        {
        //            return;
        //        }

        //        foreach (DataColumn column3 in View.Table.Columns)
        //        {
        //            if (base.Columns.FindByName(column3.ColumnName) == null)
        //            {
        //                var column = CreateColumn(column3);
        //                column.Enabled = true;
        //                base.Columns.Add(column);
        //            }
        //        }

        //        int num = 0;
        //        while (num < base.Columns.Count)
        //        {
        //            var column2 = base.Columns[num];
        //            if (!column2.Calculated && !View.Table.Columns.Contains(column2.Name))
        //            {
        //                column2.Dispose();
        //            }
        //            else
        //            {
        //                num++;
        //            }
        //        }
        //    }
        //}

        private ViewDataSource vds;
        private TableDataSource tbs;
        public ReportDataTable CreateDataTableFromModel(ReportSelectModel selectModel)
        {
            ReportDataTable dataTable = new ReportDataTable() { SelectModel = selectModel };
            dataTable.TableName = selectModel.Name;
            dataTable.PopulateColumns(selectModel);

            foreach (var subModel in selectModel.SubSelects)
            {
                var subDataTable = CreateDataTableFromModel(subModel);
                subDataTable.MasterDataTable = dataTable;
                dataTable.SubDataTables.Add(subDataTable);
            }
            return dataTable;
        }

        private void RunReport()
        {
            var model = CreateSampleModel();

            // create report instance
            Report report = new Report();
            report.Load("series-prog.frx");
            //report.Dictionary.DataSources.Clear();

            var masterTable = CreateDataTableFromModel(model.MasterSelect);

            Stack<ReportDataTable> stack = new Stack<ReportDataTable>();
            stack.Push(masterTable);
            while (stack.Count > 0)
            {
                var item = stack.Pop();

                report.RegisterData(item, item.TableName);
                item.ReportDataSource = report.GetDataSource(item.TableName);
                item.ReportDataSource.Load += (s, e) => { item.DataLoad(s as TableDataSource); };
                item.ReportDataSource.Enabled = true;
                //item.ReportDataSource.ForceLoadData = true;
                var tableDts = item.ReportDataSource as TableDataSource;
                if (tableDts != null)
                {
                    tableDts.Parameters.Add(new CommandParameter() { Name = "SERIES_ID", DataType = 107 } );
                }

                //if (item.MasterDataTable != null)
                //{
                //    var relationName = item.MasterDataTable.TableName + "-" + item.TableName;
                //    Relation relation = new Relation();
                //    relation.ReferenceName = relationName;
                //    relation.Reference = null;
                //    relation.Name = relationName;
                //    relation.Enabled = true;
                //    relation.ParentDataSource = item.MasterDataTable.ReportDataSource;
                //    relation.ChildDataSource = item.ReportDataSource;
                //    relation.ParentColumns = new string[] { "currentrowid" };
                //    relation.ChildColumns = new string[] { "masterrowid" };
                //    report.Dictionary.Relations.Add(relation);
                //}

                foreach (var subitem in item.SubDataTables)
                    stack.Push(subitem);
            }

            // preperni a ukaz preview
            report.Prepare();

            report.ShowPrepared();

            //// design the report
            //report.Design();

            // free resources used by report
            report.Dispose();
        }
    }
}