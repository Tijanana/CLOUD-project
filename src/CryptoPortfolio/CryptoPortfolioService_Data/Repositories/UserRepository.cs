using CryptoPortfolioService_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoPortfolioService_Data.Repositories
{
    public class UserRepository
    {
        public static List<User> users = new List<User> 
        { 
            new User 
            { 
                Id = Guid.NewGuid().ToString(),
                Name = "Name1",
                Surname = "Surname1",
                Address = "Address1",
                City = "City1",
                Country = "Country1",
                Phone = "Phone1",
                Email = "Email1",
                Password = "Password1",
                PhotoUrl = "PhotoUrl1"
            },
            new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Name2",
                Surname = "Surname2",
                Address = "Address2",
                City = "City2",
                Country = "Country2",
                Phone = "Phone2",
                Email = "Email2",
                Password = "Password2",
                PhotoUrl = "PhotoUrl2"
            }
        };
        //private CloudStorageAccount _storageAccount;
        //private CloudTable _table;
        public UserRepository()
        {
            //_storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            //CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            //_table = tableClient.GetTableReference("StudentTable"); _table.CreateIfNotExists();
        }
        public IQueryable<User> RetrieveAllUsers()
        {
            return users.AsQueryable();
        }
        public void AddUsear(User newUser)
        { // Samostalni rad: izmestiti tableName u konfiguraciju servisa. 
            users.Add(newUser);
        }

        public bool Exists(string id)
        {
            return users.Any(user => user.Id == id);
        }

        public bool IsEmailUnique(string email)
        {
            return users.Any(user => user.Email == email);
        }

        public void RemoveUser(string id)
        {
            users.Remove(GetUser(id));
        }

        public User GetUser(string id)
        {
            return users.FirstOrDefault(user => user.Id == id);
        }

        public User GetUserByCredentials(string email, string password)
        {
            return users.FirstOrDefault(user => user.Email == email && user.Password == password);
        }

        public void UpdateUser(User user)
        {
            RemoveUser(user.Id);
            AddUsear(user);
        }
    }
}
