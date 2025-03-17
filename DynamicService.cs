using System.Text.Json;

using Microsoft.EntityFrameworkCore;

namespace AltiumSQL
{
    public class DynamicService
    {
        private readonly DynamicDbContext _context;
        private readonly Dictionary<string, Dictionary<string, string>> _config;

        public DynamicService(DynamicDbContext context)
        {
            _context = context;
            var json = File.ReadAllText("config.json");
            _config = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
        }
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public object GetDynamicDbSet(string tableName)
        {
            if (!_config.ContainsKey(tableName))
                throw new Exception($"Таблица '{tableName}' не найдена в конфиге");

            var properties = _config[tableName]
                .ToDictionary(k => k.Key, v => typeof(string));

            return _context.GetDynamicDbSet(tableName.Replace(" ", "").Replace(":", ""), properties);
        }
        public void PrintTableData(string tableName)
        {
            try
            {
                Console.WriteLine($"Попытка получить данные из таблицы '{tableName}'...");
                var dbSet = GetDynamicDbSet(tableName);
                var data = ((IQueryable<object>)dbSet).ToList();

                Console.WriteLine($"Данные из таблицы '{tableName}':");

                if (data.Count == 0)
                {
                    Console.WriteLine("Нет данных.");
                    return;
                }

                // Получаем свойства динамического объекта для вывода заголовков
                var properties = data.First().GetType().GetProperties();

                // Выводим заголовки столбцов
                Console.WriteLine(string.Join(" | ", properties.Select(p => p.Name)));
                Console.WriteLine(new string('-', properties.Length * 20));

                // Выводим значения по каждой строке
                foreach (var item in data)
                {
                    var values = properties.Select(p => p.GetValue(item)?.ToString() ?? "NULL");
                    Console.WriteLine(string.Join(" | ", values));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выводе данных из таблицы '{tableName}': {ex.Message}");
            }
        }


        //public DynamicService(DynamicDbContext context)
        //{
        //    _context = context;
        //    var json = File.ReadAllText("config.json");
        //    _config = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
        //}

        //public DbSet<object> GetDynamicDbSet(string tableName)
        //{
        //    if (!_config.ContainsKey(tableName))
        //        throw new Exception($"Таблица '{tableName}' не найдена в конфиге");

        //    var properties = _config[tableName]
        //        .ToDictionary(k => k.Key, v => typeof(string));

        //    return _context.GetDynamicDbSet(tableName.Replace(" ", "").Replace(":", ""), properties);
        //}
    }
}

