using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PersonApp
{
    public class Person : IDisposable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        
        [JsonIgnore]
        public string Password { get; set; }
        
        [JsonPropertyName("personId")]
        public string Id { get; set; }
        
        [JsonInclude]
        private DateTime _birthDate;
        
        public DateTime BirthDate
        {
            get => _birthDate;
            set => _birthDate = value;
        }
        
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (!IsValidEmail(value))
                    throw new ArgumentException("Invalid email format");
                _email = value;
            }
        }
        
        [JsonPropertyName("phone")]
        public string PhoneNumber { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        
        public bool IsAdult => Age >= 18;

        public Person() { }
        
        public Person(string firstName, string lastName, int age, string email, string phone)
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Email = email;
            PhoneNumber = phone;
            Id = Guid.NewGuid().ToString();
        }
 
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            
            try
            {
                return email.Contains("@");
            }
            catch
            {
                return false;
            }
        }

        private bool _disposed = false;
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Освобождаем управляемые ресурсы
                    // В данном классе нет явных управляемых ресурсов
                }
                
                // Освобождаем неуправляемые ресурсы
                // В данном классе нет неуправляемых ресурсов
                
                _disposed = true;
            }
        }
        
        ~Person()
        {
            Dispose(false);
        }
    }
}