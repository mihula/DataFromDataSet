using System.Data;
using System.Data.Common;

namespace DataFromDataSet
{
    public class MyDataTable: DataTable
    {
        public override void Load(IDataReader reader, LoadOption loadOption, FillErrorEventHandler errorHandler)
        {
            base.Load(reader, loadOption, errorHandler);
        }
    }

    public class MyTableAdapter  {
        public int Fill(DataTable dataTable)
        {
            dataTable.Rows.Add(new object[] { 0, "ABC" });
            dataTable.Rows.Add(new object[] { 1, "Zzz" });
            return 2;
        }
    }

}