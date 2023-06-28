﻿namespace Apps.AmazonS3.Utils;

public static class AmazonClientHandler
{
    public static async Task<T> ExecuteS3Action<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
        }
        catch(Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}