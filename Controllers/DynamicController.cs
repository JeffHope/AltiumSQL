using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;

namespace AltiumSQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DynamicController : ControllerBase
    {
        private readonly DynamicService _service;

        public DynamicController(DynamicService service)
        {
            _service = service;
        }

        // ✅ Получить все записи
        [HttpGet("{tableName}")]
        public IActionResult GetTableData(string tableName)
        {
            try
            {
                var dbSet = _service.GetDynamicDbSet(tableName);

                var entityType = dbSet.GetType().GetGenericArguments()[0];
                var toListMethod = typeof(Enumerable).GetMethod("ToList")?
                    .MakeGenericMethod(entityType);

                var data = toListMethod?.Invoke(null, new[] { dbSet });

                var result = ((IEnumerable<object>)data).Select(item =>
                {
                    var properties = entityType.GetProperties();
                    return properties.ToDictionary(
                        prop => prop.Name,
                        prop => prop.GetValue(item)?.ToString() ?? "NULL"
                    );
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении данных: {ex.Message}");
            }
        }

        // ✅ Получить запись по ID
        [HttpGet("{tableName}/{id}")]
        public IActionResult GetById(string tableName, string id)
        {
            try
            {
                var dbSet = _service.GetDynamicDbSet(tableName);
                var entityType = dbSet.GetType().GetGenericArguments()[0];
                var findMethod = dbSet.GetType().GetMethod("Find");

                var entity = findMethod?.Invoke(dbSet, new object[] { id });

                if (entity == null)
                    return NotFound($"Запись с ID '{id}' не найдена");

                var properties = entityType.GetProperties();
                var result = properties.ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(entity)?.ToString() ?? "NULL"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении данных по ID: {ex.Message}");
            }
        }

        // ✅ Добавить запись
        [HttpPost("{tableName}")]
        public IActionResult AddData(string tableName, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                var dbSet = _service.GetDynamicDbSet(tableName);
                var entityType = dbSet.GetType().GetGenericArguments()[0];
                var entity = Activator.CreateInstance(entityType);

                foreach (var prop in data)
                {
                    var property = entityType.GetProperty(prop.Key);
                    if (property != null)
                    {
                        var value = prop.Value;
                        if (value is JsonElement jsonElement)
                        {
                            // Преобразуем JsonElement в соответствующий тип свойства
                            switch (jsonElement.ValueKind)
                            {
                                case JsonValueKind.String:
                                    value = jsonElement.GetString();
                                    break;
                                case JsonValueKind.Number:
                                    if (property.PropertyType == typeof(int))
                                        value = jsonElement.GetInt32();
                                    else if (property.PropertyType == typeof(long))
                                        value = jsonElement.GetInt64();
                                    else if (property.PropertyType == typeof(double))
                                        value = jsonElement.GetDouble();
                                    else if (property.PropertyType == typeof(decimal))
                                        value = jsonElement.GetDecimal();
                                    break;
                                case JsonValueKind.True:
                                case JsonValueKind.False:
                                    value = jsonElement.GetBoolean();
                                    break;
                                case JsonValueKind.Null:
                                    value = null;
                                    break;
                                default:
                                    throw new NotSupportedException($"Тип {jsonElement.ValueKind} не поддерживается.");
                            }
                        }

                        property.SetValue(entity, value);
                    }
                }

                var addMethod = dbSet.GetType().GetMethod("Add");
                addMethod?.Invoke(dbSet, new[] { entity });

                _service.SaveChanges();

                return Ok("Данные добавлены успешно");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при добавлении данных: {ex.Message}");
            }
        }

        // ✅ Обновить запись по ID
        [HttpPut("{tableName}/{id}")]
        public IActionResult UpdateData(string tableName, string id, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                var dbSet = _service.GetDynamicDbSet(tableName);
                var entityType = dbSet.GetType().GetGenericArguments()[0];
                var findMethod = dbSet.GetType().GetMethod("Find");

                var entity = findMethod?.Invoke(dbSet, new object[] { id });
                if (entity == null)
                    return NotFound($"Запись с ID '{id}' не найдена");

                foreach (var prop in data)
                {
                    var property = entityType.GetProperty(prop.Key);
                    if (property != null)
                    {
                        property.SetValue(entity, prop.Value);
                    }
                }

                _service.SaveChanges();

                return Ok("Данные обновлены успешно");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обновлении данных: {ex.Message}");
            }
        }

        // ✅ Удалить запись по ID
        [HttpDelete("{tableName}/{id}")]
        public IActionResult DeleteData(string tableName, string id)
        {
            try
            {
                var dbSet = _service.GetDynamicDbSet(tableName);
                var entityType = dbSet.GetType().GetGenericArguments()[0];
                var findMethod = dbSet.GetType().GetMethod("Find");

                var entity = findMethod?.Invoke(dbSet, new object[] { id });
                if (entity == null)
                    return NotFound($"Запись с ID '{id}' не найдена");

                var removeMethod = dbSet.GetType().GetMethod("Remove");
                removeMethod?.Invoke(dbSet, new[] { entity });

                _service.SaveChanges();

                return Ok("Данные удалены успешно");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при удалении данных: {ex.Message}");
            }
        }
    }
}
