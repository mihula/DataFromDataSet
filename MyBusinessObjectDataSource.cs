using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FastReport;
using FastReport.Data;
using FastReport.Utils;

namespace DataFromDataSet
{
    public class MyBusinessObjectDataSource : BusinessObjectDataSource
    {
        public void Assign(DataSourceBase originalDataSource)
        {
            this.ReferenceName = originalDataSource.ReferenceName;
            this.Reference = originalDataSource.Reference;
            this.DataType = originalDataSource.DataType;
            this.Name = originalDataSource.Name;
            this.Alias = originalDataSource.Alias;
            this.Enabled = originalDataSource.Enabled;
        }

        public void CreateColumns(Dictionary<string, Type> columnTypes)
        {
            foreach (var columnType in columnTypes)
            {
                Column column = new Column();
                column.Name = columnType.Key;
                column.DataType = columnType.Value;
                this.Columns.Add(column);
            }
        }
        protected override object GetValue(string alias)
        {
            Debug.WriteLine($"GetValue(alias={alias})", "INFO");
            return base.GetValue(alias);
        }

        protected override object GetValue(Column column)
        {
            Debug.WriteLine($"GetValue", "INFO");
            return base.GetValue(column);
        }

        public override void InitSchema()
        {
            Debug.WriteLine($"InitSchema", "INFO");
            base.InitSchema();
        }

        public override void LoadData(ArrayList rows)
        {
            Debug.WriteLine($"LoadData", "INFO");
            base.LoadData(rows);
        }

        public override void Deserialize(FRReader reader)
        {
            Debug.WriteLine($"Deserialize", "INFO");
            base.Deserialize(reader);
        }
    }
}