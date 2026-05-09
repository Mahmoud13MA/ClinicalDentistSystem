using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Shared.Utilities;

public static class ConnectivityErrorDetector
{
    public static bool IsConnectivityError(Exception ex)
    {
        var rootEx = ex.GetBaseException();

        return rootEx switch
        {
            SqlException sqlEx => sqlEx.Number is 53 or -2 or -1 or 20 or 64 or 233 or 4060,
            TimeoutException => true,
            DbUpdateException dbEx => IsDbUpdateConnectivityError(dbEx),
            _ => false
        };
    }

    private static bool IsDbUpdateConnectivityError(DbUpdateException dbEx)
    {
        var innerEx = dbEx.InnerException;

        if (innerEx is SqlException sqlEx)
            return sqlEx.Number is 53 or -2 or -1 or 20 or 64 or 233 or 4060;

        return innerEx is TimeoutException;
    }
}