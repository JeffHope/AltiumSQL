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

        public object GetDynamicDbSet(string tableName)
        {
            if (!_config.ContainsKey(tableName))
                throw new Exception($"Таблица '{tableName}' не найдена в конфиге");

            var properties = _config[tableName]
                .ToDictionary(k => k.Key, v => typeof(string));

            return _context.GetDynamicDbSet(tableName.Replace(" ", "").Replace(":", ""), properties);
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

