using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonApp;
using System;

namespace PersonTests
{
    [TestClass]
    public class PersonTests
    {
        [TestMethod]
        public void Person_Creation_Success()
        {
            Person person = new Person("Иван", "Иванов", 25, "ivan@example.com", "+79991234567");

            Assert.AreEqual("Иван", person.FirstName);
            Assert.AreEqual("Иванов", person.LastName);
            Assert.AreEqual(25, person.Age);
            Assert.AreEqual("ivan@example.com", person.Email);
            Assert.IsTrue(person.IsAdult);
        }
        
        [TestMethod]
        public void FullName_ReturnsCorrectFormat()
        {
            Person person = new Person
            {
                FirstName = "Анна",
                LastName = "Смирнова"
            };

            Assert.AreEqual("Анна Смирнова", person.FullName);
        }
        
        [TestMethod]
        public void IsAdult_WhenAge18OrMore_ReturnsTrue()
        {
            Person adult = new Person { Age = 18 };
            Person child = new Person { Age = 17 };

            Assert.IsTrue(adult.IsAdult);
            Assert.IsFalse(child.IsAdult);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Email_SetInvalidEmail_ThrowsException()
        {
            Person person = new Person();

            person.Email = "invalid-email"; 
            
        }
        
        [TestMethod]
        public void Email_SetValidEmail_Success()
        {
            Person person = new Person();

            person.Email = "test@example.com";

            Assert.AreEqual("test@example.com", person.Email);
        }
    }
}