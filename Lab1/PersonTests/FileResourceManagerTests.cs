using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonApp;
using System;
using System.IO;

namespace PersonTests
{
    [TestClass]
    public class FileResourceManagerTests
    {
        private string _testFilePath = "test_file.txt";
        
        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }
        
        [TestMethod]
        public void WriteAndRead_WorksCorrectly()
        {
            using var fileManager = new FileResourceManager(_testFilePath, FileMode.Create);

            fileManager.OpenForWriting();
            fileManager.WriteLine("Первая строка");
            fileManager.WriteLine("Вторая строка");
            
            fileManager.OpenForReading();
            string content = fileManager.ReadAllText();

            StringAssert.Contains(content, "Первая строка");
            StringAssert.Contains(content, "Вторая строка");
        }
        
        [TestMethod]
        public void AppendText_AddsToEnd()
        {
            File.WriteAllText(_testFilePath, "Начальный текст\n");
            using var fileManager = new FileResourceManager(_testFilePath, FileMode.Open);

            fileManager.AppendText("Добавленный текст");
            fileManager.OpenForReading();
            string content = fileManager.ReadAllText();

            StringAssert.Contains(content, "Добавленный текст");
        }
        
        [TestMethod]
        public void GetFileInfo_ReturnsCorrectInfo()
        {
            File.WriteAllText(_testFilePath, "Тестовое содержимое");
            using var fileManager = new FileResourceManager(_testFilePath, FileMode.Open);

            string info = fileManager.GetFileInfo();

            StringAssert.Contains(info, "test_file.txt");
            StringAssert.Contains(info, "bytes");
        }
        
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Dispose_MakesObjectUnusable()
        {
            var fileManager = new FileResourceManager(_testFilePath, FileMode.Create);
 
            fileManager.Dispose();

            fileManager.WriteLine("Это должно вызвать исключение");
        }
        
        [TestMethod]
        public void UsingStatement_AutomaticallyDisposes()
        {
            using (var fileManager = new FileResourceManager(_testFilePath, FileMode.Create))
            {
                fileManager.WriteLine("Тест");
            }

            Assert.IsTrue(File.Exists(_testFilePath));
        }
    }
}