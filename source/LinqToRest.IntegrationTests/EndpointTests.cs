﻿// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Changes;

using DataModel.Tests;

using LinqToRest.Client;
using LinqToRest.Client.Http;
using LinqToRest.Client.Http.Impl;
using LinqToRest.Client.Linq;
using LinqToRest.Client.Serialization.Impl;
using LinqToRest.OData.Impl;

using NUnit.Framework;

using TestWebApiService.Controllers;

using WebApi.TestHarness;
using WebApi.TestHarness.Configuration;
using WebApi.TestHarness.Hosting;

namespace LinqToRest.IntegrationTests
{
	[TestFixture]
	public class EndpointTests
	{
		private IWebServiceHost _host;
		private Endpoint _endpoint;

		[SetUp]
		public void TestSetUp()
		{
			var configuration = new HostConfiguration
			{
				RouteTable = new RouteConfigurationTable("http://localhost:6789/", new RouteConfiguration
				{
					Name = "DefaultRoute",
					Template = "api/{controller}/{id}",
					DefaultParameters = new List<RouteConfigurationParameter>
					{
						RouteConfigurationParameter.Create("id")
					}
				})
			};

			_host = WebServiceHostFactory.CreateFor<TestObjectController>(configuration);

			var httpClientFactory = new DefaultHttpClientFactory();
			var serializer = new JsonSerializer();
			var uriFactory = new UriFactory(new Uri("http://localhost:6789/api/"));
			var queryFactory = new DefaultODataQueryFactory();

			var httpService = new HttpService(httpClientFactory, serializer);

			var queryModelTranslator = new RestQueryModelVisitor(uriFactory, queryFactory);

			var restQueryableFactory = new RestQueryableFactory(httpService, queryModelTranslator);

			_endpoint = new Endpoint(restQueryableFactory, httpService, uriFactory);
		}

		[TearDown]
		public void TestTearDown()
		{
			_host.Dispose();

			_host = null;
		}

		[Test]
		public void Get_NoConstraints_ReturnsAllItems()
		{
			var result = _endpoint.Get<TestObject>().ToList();

			Assert.That(result.Count, Is.EqualTo(4));
		}

		[Test]
		public void Get_NoConstraintsSelectId_ReturnsAllIds()
		{
			var result = _endpoint.Get<TestObject>().Select(x => x.Id).ToList();

			Assert.That(result.Count, Is.EqualTo(4));
		}

		[Test]
		public void Get_WhereIdEqualsThree_ReturnsListWithSingleResult()
		{
			var result = _endpoint.Get<TestObject>().Where(x => x.Id == 3).ToList();

			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Single().Id, Is.EqualTo(3));
			Assert.That(result.Single().TestProperty, Is.EqualTo("3"));
		}

		[Test]
		public void Get_WhereIdModTwoEqualsZero_ReturnsListWithTwoResults()
		{
			var result = _endpoint.Get<TestObject>().Where(x => x.Id % 2 == 0).ToList();

			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result.ElementAt(0).Id, Is.EqualTo(2));
			Assert.That(result.ElementAt(0).TestProperty, Is.EqualTo("2"));
			Assert.That(result.ElementAt(1).Id, Is.EqualTo(4));
			Assert.That(result.ElementAt(1).TestProperty, Is.EqualTo("4"));
		}

		[Test]
		public void Put_SetPropertyTo6_SetsProperty()
		{
			var changeSet = new ChangeSet<TestObject>();

			changeSet.SetChangeFor(x => x.TestProperty, "6");

			var putStatus = _endpoint.Put(3, changeSet);

			var result = _endpoint.Get<TestObject>().Single(x => x.Id == 3);

			Assert.That(putStatus, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(result.Id, Is.EqualTo(3));
			Assert.That(result.TestProperty, Is.EqualTo("6"));
		}

		[Test]
		public void Post_NewItem_AddsItem()
		{
			var item = new TestObject
			{
				Id = 42,
				TestProperty = "Hello, World."
			};

			var postStatus = _endpoint.Post(item);

			var result = _endpoint.Get<TestObject>().ToList();

			Assert.That(postStatus, Is.EqualTo(HttpStatusCode.Created));
			Assert.That(result, Contains.Item(item));
		}

		[Test]
		public void Delete_ItemWithId3_RemovesItem()
		{
			var deleteStatus = _endpoint.Delete<TestObject>(3);

			var result = _endpoint.Get<TestObject>().Single(x => x.Id == 3);

			Assert.That(deleteStatus, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(result, Is.Null);
		}
	}
}
