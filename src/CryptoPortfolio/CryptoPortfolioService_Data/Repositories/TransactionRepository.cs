﻿using CryptoPortfolioService_Data.Entities;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoPortfolioService_Data.Repositories
{
    public class TransactionRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public TransactionRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("TransactionTable");
            _table.CreateIfNotExists();
        }

        public IQueryable<Transaction> RetrieveAllTransactions()
        {
            var results = from g in _table.CreateQuery<Transaction>()
                          where g.PartitionKey == "Transaction" 
                          select g;
            return results;
        }

        public List<Transaction> RetrieveAllTransactionsForUser(string userId)
        {
            return RetrieveAllTransactions().Where(x => x.UserId == userId).ToList();
        }

        public void AddTransaction(Transaction newTransaction)
        {
            TableOperation insertOperation = TableOperation.Insert(newTransaction);
            _table.Execute(insertOperation);
        }

        public void RemoveTransaction(string id)
        {
            Transaction transaction = RetrieveAllTransactions().Where(s => s.RowKey == id).FirstOrDefault();
            if (transaction != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(transaction);
                _table.Execute(deleteOperation);
            }
        }
    }
}