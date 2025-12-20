using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("1. Тестирование класса Person:");
                Person person = new Person
                {
                    FirstName = "Арина",
                    LastName = "Яматина",
                    Age = 17,
                    Email = "example@example.com",
                    PhoneNumber = "+79870452057",
                    Password = "12345678",
                    BirthDate = new DateTime(2008, 07, 22)
                };
                
                Console.WriteLine($"FullName: {person.FullName}");
                Console.WriteLine($"IsAdult: {person.IsAdult}");
                Console.WriteLine($"Email: {person.Email}");
                
                Console.WriteLine("2. Тестирование PersonSerializer:");
                PersonSerializer serializer = new PersonSerializer();

                string json = serializer.SerializeToJson(person);
                Console.WriteLine($"Сериализованный JSON: {json}");

                string filePath = "person.json";
                serializer.SaveToFile(person, filePath);
                Console.WriteLine($"Объект сохранен в {filePath}");

                Person loadedPerson = serializer.LoadFromFile(filePath);
                Console.WriteLine($"Загруженный объект: {loadedPerson.FullName}");

                Console.WriteLine("3. Тестирование асинхронных операций:");
                await serializer.SaveToFileAsync(person, "person_async.json");
                Person asyncPerson = await serializer.LoadFromFileAsync("person_async.json");
                Console.WriteLine($"Асинхронно загружен: {asyncPerson.FullName}");

                Console.WriteLine("4. Тестирование списка объектов:");
                List<Person> people = new List<Person>
                {
                    new Person("Анна", "Иванова", 30, "anna@example.com", "+79992345678"),
                    new Person("Яна", "Петрова", 17, "yana@example.com", "+79993456789"),
                    new Person("Мария", "Петрова", 22, "maria@example.com", "+79994567890")
                };
                
                serializer.SaveListToFile(people, "people.json");
                List<Person> loadedPeople = serializer.LoadListFromFile("people.json");
                Console.WriteLine($"Загружено {loadedPeople.Count} объектов");

                Console.WriteLine("5. Тестирование FileResourceManager:");
                using (var fileManager = new FileResourceManager("test.txt", FileMode.Create))
                {
                    fileManager.OpenForWriting();
                    fileManager.WriteLine("Первая строка");
                    fileManager.WriteLine("Вторая строка");
                    
                    fileManager.OpenForReading();
                    string content = fileManager.ReadAllText();
                    Console.WriteLine($"Содержимое файла:{content}");
                    
                    Console.WriteLine($"Информация о файле:{fileManager.GetFileInfo()}");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine($"Детали: {ex}");
            }
            Console.ReadKey();
        }
    }
}