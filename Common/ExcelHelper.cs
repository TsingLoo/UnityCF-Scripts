using MiniExcelLibs;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


//https://gitee.com/dotnetchina/MiniExcel?_from=gitee_search#https://gitee.com/link?target=https%3A%2F%2Fdotnetfiddle.net%2Fw5WD1J


public class ExcelHelper
{
    public static string Extension = ".xlsx";

    #region Read Excel
    [System.Obsolete]
    public static async Task<DataTable> GetDataTableAsync(string path, bool hasHeader = true)
    {
        var table = await MiniExcel.QueryAsDataTableAsync(path, hasHeader);
        return table;

        // 如果导入数据时用的是GetDataTable方法，dataTable转Class如果碰到数字转换的问题，可以用这个方法
        // 将数值统一换成string
        //Change value to string for SqlSugar or other tools to get a Entity list from DataTable
        //, since double to int could cause error
        //var newTable = ChangeValueToString(table);
        //return newTable;
    }

    public static async Task<List<T>> GetListAsync<T>(string path, string sheetName = null, ExcelType excelType = ExcelType.UNKNOWN, string startCell = "A1", IConfiguration configuration = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, new()
    {
        var items = await MiniExcel.QueryAsync<T>(path, sheetName, excelType, startCell, configuration, cancellationToken);
        
        return items.ToList();    
    }
    #endregion

    #region Write Excel
    // Save excel, overwriteFile = true
    public static async Task SaveAsReplaceAsync(string path, object value, bool printHeader = true, string sheetName = "Sheet1", ExcelType excelType = ExcelType.UNKNOWN, IConfiguration configuration = null, bool overwriteFile = false, CancellationToken cancellationToken = default(CancellationToken))
    {
        overwriteFile = true;

        var folderPath = FileHelper.GetDirectory(path);
        FileHelper.CreateDirectory(folderPath);

        await MiniExcel.SaveAsAsync(path, value, printHeader, sheetName, excelType, configuration, overwriteFile
            , cancellationToken);
    }
    #endregion

    #region Help Function
    public static DataTable ChangeValueToString(DataTable table)
    {
        DataTable newTable = table.Clone(); //just copy structure, no data
        for (int i = 0; i < newTable.Columns.Count; i++)
        {
            if (newTable.Columns[i].DataType != typeof(string))
                newTable.Columns[i].DataType = typeof(string);
        }

        foreach (DataRow row in table.Rows)
        {
            newTable.ImportRow(row);
        }

        return newTable;
    }
    #endregion


}
