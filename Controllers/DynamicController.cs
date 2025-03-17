using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{tableName}")]
        public IActionResult GetTableData(string tableName)
        {
            try
            {
                // Получаем DbSet для динамического типа
                var dbSet = _service.GetDynamicDbSet(tableName);

                // Получаем тип динамической сущности
                var entityType = dbSet.GetType().GetGenericArguments()[0];

                // Получаем данные через рефлексию
                var toListMethod = typeof(Enumerable).GetMethod("ToList")?
                    .MakeGenericMethod(entityType);

                if (toListMethod == null)
                {
                    return StatusCode(500, "Метод 'ToList' не найден.");
                }

                var data = toListMethod.Invoke(null, new[] { dbSet });

                // Конвертируем данные в JSON (Dictionary<string, object>)
                var result = ((IEnumerable<object>)data).Select(item =>
                {
                    var properties = entityType.GetProperties();
                    var dict = properties.ToDictionary(
                        prop => prop.Name,
                        prop => prop.GetValue(item)?.ToString() ?? "NULL"
                    );

                    return dict;
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении данных: {ex.Message}");
            }
        }

        //[HttpGet("{tableName}")]
        //public IActionResult GetTableData(string tableName)
        //{
        //    try
        //    {
        //        // Получаем DbSet для динамического типа
        //        var dbSet = _service.GetDynamicDbSet(tableName);

        //        // Используем рефлексию для вызова метода ToList()
        //        var toListMethod = typeof(Enumerable).GetMethod("ToList")?
        //            .MakeGenericMethod(dbSet.GetType().GetGenericArguments()[0]);

        //        if (toListMethod == null)
        //        {
        //            return StatusCode(500, "Метод 'ToList' не найден.");
        //        }

        //        var data = toListMethod.Invoke(null, new[] { dbSet });

        //        return Ok(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Ошибка при получении данных: {ex.Message}");
        //    }
        //}

        [HttpPost("{tableName}")]
        public IActionResult AddData(string tableName, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                // Получаем DbSet для динамического типа
                var dbSet = _service.GetDynamicDbSet(tableName);

                // Создаем экземпляр динамического типа
                var entityType = dbSet.GetType().GetGenericArguments()[0];
                var entity = Activator.CreateInstance(entityType);

                // Заполняем свойства сущности
                foreach (var prop in data)
                {
                    var property = entityType.GetProperty(prop.Key);
                    if (property != null)
                    {
                        property.SetValue(entity, prop.Value);
                    }
                }

                // Добавляем сущность в DbSet
                var addMethod = dbSet.GetType().GetMethod("Add");
                addMethod?.Invoke(dbSet, new[] { entity });

                // Сохраняем изменения
                var saveChangesMethod = _service.GetType().GetMethod("SaveChanges");
                saveChangesMethod?.Invoke(_service, null);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при добавлении данных: {ex.Message}");
            }
        }
        //public IActionResult GetTableData(string tableName)
        //{
        //    var dbSet = _service.GetDynamicDbSet(tableName);
        //    var data = dbSet.ToList();
        //    return Ok(data);
        //}

        //[HttpPost("{tableName}")]
        //public IActionResult AddData(string tableName, [FromBody] Dictionary<string, object> data)
        //{
        //    var dbSet = _service.GetDynamicDbSet(tableName);
        //    var entity = Activator.CreateInstance(dbSet.EntityType.ClrType);

        //    foreach (var prop in data)
        //    {
        //        var property = entity.GetType().GetProperty(prop.Key);
        //        if (property != null)
        //        {
        //            property.SetValue(entity, prop.Value);
        //        }
        //    }

        //    dbSet.Add(entity);
        //    _service.GetType().GetMethod("SaveChanges").Invoke(_service, null);

        //    return Ok();
        //}
    }
}
