using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonApp
{
    public class PersonSerializer
    {
        private readonly JsonSerializerOptions _options;
        private static readonly object _fileLock = new object();
        
        public PersonSerializer()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null,
                IncludeFields = true 
            };
        }
        
        // 1. Сериализация в строку
        public string SerializeToJson(Person person)
        {
            try
            {
                return JsonSerializer.Serialize(person, _options);
            }
            catch (Exception ex)
            {
                LogError("SerializeToJson", ex);
                throw;
            }
        }
        
        // 2. Десериализация из строки
        public Person DeserializeFromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<Person>(json, _options);
            }
            catch (Exception ex)
            {
                LogError("DeserializeFromJson", ex);
                throw;
            }
        }
        
        // 3. Сохранение в файл (синхронно)
        public void SaveToFile(Person person, string filePath)
        {
            lock (_fileLock)
            {
                try
                {
                    string json = SerializeToJson(person);
                    File.WriteAllText(filePath, json);
                }
                catch (Exception ex)
                {
                    LogError($"SaveToFile: {filePath}", ex);
                    throw;
                }
            }
        }
        
        // 4. Загрузка из файла (синхронно)
        public Person LoadFromFile(string filePath)
        {
            lock (_fileLock)
            {
                try
                {
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException($"File not found: {filePath}");
                    
                    string json = File.ReadAllText(filePath);
                    return DeserializeFromJson(json);
                }
                catch (Exception ex)
                {
                    LogError($"LoadFromFile: {filePath}", ex);
                    throw;
                }
            }
        }
        
        // 5. Сохранение в файл (асинхронно)
        public async Task SaveToFileAsync(Person person, string filePath)
        {
            try
            {
                string json = SerializeToJson(person);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                LogError($"SaveToFileAsync: {filePath}", ex);
                throw;
            }
        }
        
        // 6. Загрузка из файла (асинхронно)
        public async Task<Person> LoadFromFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}");
                
                string json = await File.ReadAllTextAsync(filePath);
                return DeserializeFromJson(json);
            }
            catch (Exception ex)
            {
                LogError($"LoadFromFileAsync: {filePath}", ex);
                throw;
            }
        }
        
        // 7. Экспорт нескольких объектов в файл
        public void SaveListToFile(List<Person> people, string filePath)
        {
            lock (_fileLock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(people, _options);
                    File.WriteAllText(filePath, json);
                }
                catch (Exception ex)
                {
                    LogError($"SaveListToFile: {filePath}", ex);
                    throw;
                }
            }
        }
        
        // 8. Импорт из файла
        public List<Person> LoadListFromFile(string filePath)
        {
            lock (_fileLock)
            {
                try
                {
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException($"File not found: {filePath}");
                    
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<Person>>(json, _options);
                }
                catch (Exception ex)
                {
                    LogError($"LoadListFromFile: {filePath}", ex);
                    throw;
                }
            }
        }

        private void LogError(string methodName, Exception ex)
        {
            string logMessage = $"[{DateTime.Now}] Error in {methodName}: {ex.Message}\nStackTrace: {ex.StackTrace}\n\n";
            string logPath = "error_log.txt";
            
            try
            {
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Не можем записать в лог
            }
        }
    }
}