using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonApp;
using System.IO;
using System.Threading.Tasks;

namespace PersonTests
{
    [TestClass]
    public class PersonSerializerTests
    {
        private PersonSerializer _serializer;
        private Person _testPerson;
        
        [TestInitialize]
        public void Initialize()
        {
            _serializer = new PersonSerializer();
            _testPerson = new Person
            {
                FirstName = "Диана",
                LastName = "Хотеева",
                Age = 18,
                Email = "diana@example.com",
                PhoneNumber = "+79990000000",
                Password = "secret",
                BirthDate = new System.DateTime(1993, 1, 1)
            };
        }
        
        [TestMethod]
        public void SerializeToJson_ReturnsValidJson()
        {
            // Act
            string json = _serializer.SerializeToJson(_testPerson);
            
            // Assert
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"FirstName\":"));
            Assert.IsTrue(json.Contains("\"Тест\""));
            Assert.IsFalse(json.Contains("Password")); 
            Assert.IsTrue(json.Contains("\"phone\"")); 
        }
        
        [TestMethod]
        public void DeserializeFromJson_ReturnsValidPerson()
        {
            string json = @"{
                ""FirstName"": ""Соня"",
                ""LastName"": ""Мурина"",
                ""Age"": 18,
                ""Email"": ""sonya@example.com"",
                ""personId"": ""test-id"",
                ""_birthDate"": ""2007-09-06T00:00:00"",
                ""phone"": ""+79991234567""
            }";

            Person person = _serializer.DeserializeFromJson(json);

            Assert.AreEqual("Соня", person.FirstName);
            Assert.AreEqual("Мурина", person.LastName);
            Assert.AreEqual("sonya@example.com", person.Email);
            Assert.AreEqual("+79991234567", person.PhoneNumber);
        }
        
        [TestMethod]
        public void SaveAndLoadFromFile_WorksCorrectly()
        {
            string filePath = "test_person.json";

            _serializer.SaveToFile(_testPerson, filePath);
            Person loadedPerson = _serializer.LoadFromFile(filePath);

            Assert.AreEqual(_testPerson.FirstName, loadedPerson.FirstName);
            Assert.AreEqual(_testPerson.LastName, loadedPerson.LastName);
            Assert.AreEqual(_testPerson.Email, loadedPerson.Email);

            File.Delete(filePath);
        }
        
        [TestMethod]
        public async Task SaveAndLoadFromFileAsync_WorksCorrectly()
        {
            string filePath = "test_person_async.json";

            await _serializer.SaveToFileAsync(_testPerson, filePath);
            Person loadedPerson = await _serializer.LoadFromFileAsync(filePath);

            Assert.AreEqual(_testPerson.FirstName, loadedPerson.FirstName);
            Assert.AreEqual(_testPerson.LastName, loadedPerson.LastName);

            File.Delete(filePath);
        }
        
        [TestMethod]
        public void SaveAndLoadListFromFile_WorksCorrectly()
        {

            string filePath = "test_people.json";
            var people = new System.Collections.Generic.List<Person>
            {
                new Person("Анна", "Петрова", 30, "anna@example.com", "+79992345678"),
                new Person("Яна", "Иванова", 17, "yana@example.com", "+79993456789")
            };
            
            _serializer.SaveListToFile(people, filePath);
            var loadedPeople = _serializer.LoadListFromFile(filePath);
            
            Assert.AreEqual(2, loadedPeople.Count);
            Assert.AreEqual("Анна", loadedPeople[0].FirstName);
            Assert.AreEqual("Яна", loadedPeople[1].FirstName);
            
            File.Delete(filePath);
        }
        
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadFromFile_FileNotFound_ThrowsException()
        {
            _serializer.LoadFromFile("nonexistent.json");
        }
    }
}