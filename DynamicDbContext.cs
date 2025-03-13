using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;


namespace AltiumSQL
{
    public class DynamicDbContext : DbContext
    {
        private readonly Dictionary<string, Type> _dynamicTypes = new();

        public DynamicDbContext(DbContextOptions options) : base(options) { }

        public DynamicDbContext() { }

        public Type CreateDynamicModel(string modelName, Dictionary<string, Type> properties)
        {
            var assemblyName = new AssemblyName("DynamicModels");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeBuilder = moduleBuilder.DefineType(modelName, TypeAttributes.Public);

            foreach (var prop in properties)
            {
                var fieldBuilder = typeBuilder.DefineField("_" + prop.Key, prop.Value, FieldAttributes.Private);
                var propertyBuilder = typeBuilder.DefineProperty(prop.Key, PropertyAttributes.HasDefault, prop.Value, null);

                var getMethodBuilder = typeBuilder.DefineMethod("get_" + prop.Key,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    prop.Value, Type.EmptyTypes);

                var ilGet = getMethodBuilder.GetILGenerator();
                ilGet.Emit(OpCodes.Ldarg_0);
                ilGet.Emit(OpCodes.Ldfld, fieldBuilder);
                ilGet.Emit(OpCodes.Ret);

                var setMethodBuilder = typeBuilder.DefineMethod("set_" + prop.Key,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    null, new[] { prop.Value });

                var ilSet = setMethodBuilder.GetILGenerator();
                ilSet.Emit(OpCodes.Ldarg_0);
                ilSet.Emit(OpCodes.Ldarg_1);
                ilSet.Emit(OpCodes.Stfld, fieldBuilder);
                ilSet.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethodBuilder);
                propertyBuilder.SetSetMethod(setMethodBuilder);
            }

            return typeBuilder.CreateType();
        }

        public object GetDynamicDbSet(string tableName, Dictionary<string, Type> properties)
        {
            if (!_dynamicTypes.ContainsKey(tableName))
            {
                var type = CreateDynamicModel(tableName, properties);
                _dynamicTypes[tableName] = type;
            }

            var modelBuilder = new ModelBuilder();
            modelBuilder.Entity(_dynamicTypes[tableName]);

            // Получаем универсальный метод Set<TEntity>()
            var setMethod = typeof(DbContext)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "Set" && m.IsGenericMethod);

            if (setMethod == null)
            {
                throw new InvalidOperationException("Method 'Set<TEntity>()' not found.");
            }

            // Создаем универсальный метод для конкретного типа
            var genericSetMethod = setMethod.MakeGenericMethod(_dynamicTypes[tableName]);

            // Вызываем метод и возвращаем DbSet<TEntity>
            return genericSetMethod.Invoke(this, null);
        }

        //public Type CreateDynamicModel(string modelName, Dictionary<string, Type> properties)
        //{
        //    var assemblyName = new AssemblyName("DynamicModels");
        //    var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        //    var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        //    var typeBuilder = moduleBuilder.DefineType(modelName, TypeAttributes.Public);

        //    foreach (var prop in properties)
        //    {
        //        var fieldBuilder = typeBuilder.DefineField("_" + prop.Key, prop.Value, FieldAttributes.Private);
        //        var propertyBuilder = typeBuilder.DefineProperty(prop.Key, PropertyAttributes.HasDefault, prop.Value, null);

        //        var getMethodBuilder = typeBuilder.DefineMethod("get_" + prop.Key,
        //            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
        //            prop.Value, Type.EmptyTypes);

        //        var ilGet = getMethodBuilder.GetILGenerator();
        //        ilGet.Emit(OpCodes.Ldarg_0);
        //        ilGet.Emit(OpCodes.Ldfld, fieldBuilder);
        //        ilGet.Emit(OpCodes.Ret);

        //        var setMethodBuilder = typeBuilder.DefineMethod("set_" + prop.Key,
        //            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
        //            null, new[] { prop.Value });

        //        var ilSet = setMethodBuilder.GetILGenerator();
        //        ilSet.Emit(OpCodes.Ldarg_0);
        //        ilSet.Emit(OpCodes.Ldarg_1);
        //        ilSet.Emit(OpCodes.Stfld, fieldBuilder);
        //        ilSet.Emit(OpCodes.Ret);

        //        propertyBuilder.SetGetMethod(getMethodBuilder);
        //        propertyBuilder.SetSetMethod(setMethodBuilder);
        //    }

        //    return typeBuilder.CreateType();
        //}

        //public DbSet<object> GetDynamicDbSet(string tableName, Dictionary<string, Type> properties)
        //{
        //    if (!_dynamicTypes.ContainsKey(tableName))
        //    {
        //        var type = CreateDynamicModel(tableName, properties);
        //        _dynamicTypes[tableName] = type;
        //    }

        //    var modelBuilder = new ModelBuilder();
        //    modelBuilder.Entity(_dynamicTypes[tableName]);

        //    // Получаем универсальный метод Set<TEntity>()
        //    var setMethod = typeof(DbContext)
        //        .GetMethods()
        //        .FirstOrDefault(m => m.Name == "Set" && m.IsGenericMethod);

        //    if (setMethod == null)
        //    {
        //        throw new InvalidOperationException("Method 'Set<TEntity>()' not found.");
        //    }

        //    // Создаем универсальный метод для конкретного типа
        //    var genericSetMethod = setMethod.MakeGenericMethod(_dynamicTypes[tableName]);

        //    // Вызываем метод
        //    var dbSet = genericSetMethod.Invoke(this, null);

        //    return (DbSet<object>)dbSet;
        //    //if (!_dynamicTypes.ContainsKey(tableName))
        //    //{
        //    //    var type = CreateDynamicModel(tableName, properties);
        //    //    _dynamicTypes[tableName] = type;
        //    //}

        //    //var modelBuilder = new ModelBuilder();
        //    //modelBuilder.Entity(_dynamicTypes[tableName]);

        //    //var dbSetType = typeof(DbSet<>).MakeGenericType(_dynamicTypes[tableName]);
        //    //var dbSet = this.GetType().GetMethod("Set")?.MakeGenericMethod(_dynamicTypes[tableName]).Invoke(this, null);

        //    //return (DbSet<object>)dbSet;
        //}
    }
}
