using Microsoft.Data.SqlClient;

namespace DAL;

internal static class DataReaderExtensions
{
    public static int? GetOrdinalSafe(this SqlDataReader reader, string column)
    {
        try
        {
            return reader.GetOrdinal(column);
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }
}
