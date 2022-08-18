using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace m_sort_server.Utility
{
    public static class ExtensionMethods
    {
        public static void RemoveIfExists<TEntity>(this DbSet<TEntity> dbSet, params object[] keyValues)
            where TEntity : class
        {
            TEntity entity = dbSet.Find(keyValues);
            if (entity == null)
                return;
                
            dbSet.Remove(entity);
        } 
    }
}