using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AltiumSQL
{
    public class DynamicDbContext : DbContext
    {
        private static Dictionary<string, Type> _dynamicTypes = new Dictionary<string, Type>();

        public DynamicDbContext(DbContextOptions<DynamicDbContext> options) : base(options) { }

        public object GetDynamicDbSet(string tableName, Dictionary<string, Type> properties)
        {
            try
            {
                if (!_dynamicTypes.ContainsKey(tableName))
                {
                    var dynamicType = CreateDynamicType(tableName, properties);
                    _dynamicTypes[tableName] = dynamicType;
                }

                var dynamicTypeInstance = _dynamicTypes[tableName];
                Console.WriteLine($"Получение DbSet для типа '{dynamicTypeInstance.Name}'...");

                // Фильтруем метод Set, чтобы исключить конфликт перегрузок
                var setMethod = this.GetType()
                                    .GetMethods()
                                    .Where(m => m.Name == nameof(Set) && m.IsGenericMethod && m.GetParameters().Length == 0)
                                    .FirstOrDefault();

                if (setMethod == null)
                    throw new InvalidOperationException($"Не удалось найти метод 'Set' без аргументов для типа '{dynamicTypeInstance.Name}'.");

                var dbSet = setMethod.MakeGenericMethod(dynamicTypeInstance).Invoke(this, null);
                if (dbSet == null)
                    throw new InvalidOperationException($"DbSet для типа '{dynamicTypeInstance.Name}' вернул null.");

                return dbSet;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании DbSet для таблицы '{tableName}': {ex}");
                throw;
            }
        }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var dynamicType in _dynamicTypes.Values)
            {
                Console.WriteLine($"Регистрация типа '{dynamicType.Name}' в модели...");

                var entityType = modelBuilder.Entity(dynamicType);

                // Указываем PartNumber как первичный ключ
                if (dynamicType.GetProperty("Part Number") != null)
                {
                    entityType.HasKey("Part Number");
                    Console.WriteLine($"Поле 'Part Number' установлено как первичный ключ для '{dynamicType.Name}'.");
                }
                else
                {
                    entityType.HasNoKey();
                    Console.WriteLine($"Тип '{dynamicType.Name}' зарегистрирован как keyless.");
                }
            }
        }



        private Type CreateDynamicType(string tableName, Dictionary<string, Type> properties)
        {
            try
            {
                string sanitizedTableName = new string(tableName
                    .Where(c => char.IsLetterOrDigit(c) || c == '_')
                    .ToArray());

                Console.WriteLine($"Создание динамического типа для таблицы '{sanitizedTableName}'...");

                var assemblyName = new AssemblyName("DynamicAssembly");
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
                var typeBuilder = moduleBuilder.DefineType(
                    sanitizedTableName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null
                );

                foreach (var prop in properties)
                {
                    Console.WriteLine($"Добавление свойства: {prop.Key} ({prop.Value})");

                    var fieldBuilder = typeBuilder.DefineField($"_{prop.Key}", prop.Value, FieldAttributes.Private);

                    var propertyBuilder = typeBuilder.DefineProperty(
                        prop.Key,
                        PropertyAttributes.HasDefault,
                        prop.Value,
                        null
                    );

                    var getter = typeBuilder.DefineMethod(
                        $"get_{prop.Key}",
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        prop.Value,
                        Type.EmptyTypes
                    );

                    var getterIL = getter.GetILGenerator();
                    getterIL.Emit(OpCodes.Ldarg_0);
                    getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
                    getterIL.Emit(OpCodes.Ret);

                    var setter = typeBuilder.DefineMethod(
                        $"set_{prop.Key}",
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        null,
                        new[] { prop.Value }
                    );

                    var setterIL = setter.GetILGenerator();
                    setterIL.Emit(OpCodes.Ldarg_0);
                    setterIL.Emit(OpCodes.Ldarg_1);
                    setterIL.Emit(OpCodes.Stfld, fieldBuilder);
                    setterIL.Emit(OpCodes.Ret);

                    propertyBuilder.SetGetMethod(getter);
                    propertyBuilder.SetSetMethod(setter);

                    // Добавляем атрибут [Key] к PartNumber
                    if (prop.Key == "Part Number")
                    {
                        var keyAttribute = typeof(System.ComponentModel.DataAnnotations.KeyAttribute);
                        var attributeBuilder = new CustomAttributeBuilder(
                            keyAttribute.GetConstructor(Type.EmptyTypes),
                            new object[] { }
                        );
                        propertyBuilder.SetCustomAttribute(attributeBuilder);
                        Console.WriteLine($"Атрибут [Key] добавлен к полю '{prop.Key}'.");
                    }
                }

                var dynamicType = typeBuilder.CreateType();
                Console.WriteLine($"Динамический тип '{sanitizedTableName}' создан успешно.");
                return dynamicType;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании типа '{tableName}': {ex.Message}");
                throw;
            }
        }



    }
}
