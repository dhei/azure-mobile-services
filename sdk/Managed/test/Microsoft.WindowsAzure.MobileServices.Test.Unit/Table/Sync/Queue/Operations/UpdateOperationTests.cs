﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync.Queue.Operations
{
    [TestClass]
    public class UpdateOperationTests
    {
        private UpdateOperation operation;

        [TestInitialize]
        public void Initialize()
        {
            this.operation = new UpdateOperation("test", "abc");
        }

        [TestMethod]
        public async Task ExecuteAsync_UpdateGoesToTable()
        {
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);

            var table = new Mock<MobileServiceTable>("test", client.Object);
            this.operation.Table = table.Object;

            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");
            this.operation.Item = item;

            table.Setup(t => t.UpdateAsync(item)).Returns(Task.FromResult<JToken>(item));

            await this.operation.ExecuteAsync();
        }

        [TestMethod]
        public async Task ExecuteAsync_Throws_WhenItemIsNull()
        {
            var ex = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => this.operation.ExecuteAsync());
            Assert.AreEqual("Operation must have an item associated with it.", ex.Message);
        }

        [TestMethod]
        public async Task ExecuteLocalAsync_UpsertsOnStore()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");

            await this.operation.ExecuteLocalAsync(store.Object, item);
            store.Verify(s => s.UpsertAsync("test", item), Times.Once());
        }

        [TestMethod]
        public async Task ExecuteLocalAsync_Throws_WhenStoreThrows()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            var storeError = new InvalidOperationException();
            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");

            store.Setup(s => s.UpsertAsync("test", item)).Throws(storeError);
            var ex = await AssertEx.Throws<InvalidOperationException>(() => this.operation.ExecuteLocalAsync(store.Object, item));
            Assert.AreSame(storeError, ex);
        }

        [TestMethod]
        public void Validate_Throws_WithInsertOperation()
        {
            var tableOperation = new InsertOperation("test", "abc");
            var ex = AssertEx.Throws<InvalidOperationException>(() => this.operation.Validate(tableOperation));
            Assert.AreEqual("Update operation on the item is already in the queue.", ex.Message);
        }

        [TestMethod]
        public void Validate_Succeeds_WithUpdateOperation()
        {
            var tableOperation = new UpdateOperation("test", "abc");
            this.operation.Validate(tableOperation);
        }

        [TestMethod]
        public void Validate_Succeeds_WithDeleteOperation()
        {
            var tableOperation = new DeleteOperation("test", "abc");
            this.operation.Validate(tableOperation);
        }

        [TestMethod]
        public void Collapse_Succeeds_WithUpdateOperation()
        {
            var tableOperation = new UpdateOperation("test", "abc");
            this.operation.Collapse(tableOperation);
            Assert.IsTrue(tableOperation.IsCancelled);
            Assert.IsFalse(this.operation.IsCancelled);
        }

        [TestMethod]
        public void Collapse_Succeeds_WithDeleteOperation()
        {
            var tableOperation = new DeleteOperation("test", "abc");
            this.operation.Collapse(tableOperation);
            Assert.IsFalse(tableOperation.IsCancelled);
            Assert.IsTrue(this.operation.IsCancelled);
        }
    }
}