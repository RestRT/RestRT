#region License
//   Original work Copyright 2010 John Sheehan
//   Modified work Copyright 2013 Devin Rader
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RestRT.Deserializers;
using RestRT.Tests.SampleClasses;
using Windows.ApplicationModel;
using Windows.Storage;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace RestRT.Tests
{
    [TestClass]
	public class JsonDeserializationTests
	{
		private const string AlternativeCulture = "pt-PT";

		private const string GuidString = "AC1FC4BC-087A-4242-B8EE-C53EBE9887A5";

		[TestMethod]
		public async void Can_Deserialize_4sq_Json_With_Root_Element_Specified()
		{
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(Path.Combine("SampleData", "4sq.txt"));
            var doc = await FileIO.ReadTextAsync(file);

			var json = new JsonDeserializer();
			json.RootElement = "response";

			var output = (VenuesResponse)json.Deserialize(new RestResponse { Content = doc }, typeof(VenuesResponse));

			Assert.IsTrue(output.Groups.Count>0);
		}

		[TestMethod]
		public async void Can_Deserialize_Lists_of_Simple_Types()
		{
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(Path.Combine("SampleData", "jsonlists.txt"));
            var doc = await FileIO.ReadTextAsync(file);
            
			var json = new JsonDeserializer ();

			var output = (JsonLists)json.Deserialize(new RestResponse { Content = doc }, typeof(JsonLists));

			Assert.IsTrue (output.Names.Count>0);
            Assert.IsTrue(output.Numbers.Count > 0);
		}

		[TestMethod]
		public void Can_Deserialize_Simple_Generic_List_of_Simple_Types()
		{
			const string content = "{\"users\":[\"johnsheehan\",\"jagregory\",\"drusellers\",\"structuremap\"]}";
			var json = new JsonDeserializer {RootElement = "users"};

			var output = (List<string>)json.Deserialize(new RestResponse {Content = content}, typeof(List<string>));

            Assert.IsTrue(output.Count > 0);
		}

		[TestMethod]
		public void Can_Deserialize_Simple_Generic_List_of_Simple_Types_With_Nulls ()
		{
			const string content = "{\"users\":[\"johnsheehan\",\"jagregory\",null,\"drusellers\",\"structuremap\"]}";
			var json = new JsonDeserializer { RootElement = "users" };

            var output = (List<string>)json.Deserialize(new RestResponse { Content = content }, typeof(List<string>));

            Assert.IsTrue(output.Count > 0);
			Assert.AreEqual (null, output[2]);
			Assert.AreEqual (5, output.Count);
		}

		[TestMethod]
		public void Can_Deserialize_Simple_Generic_List_Given_Item_Without_Array ()
		{
			const string content = "{\"users\":\"johnsheehan\"}";
			var json = new JsonDeserializer { RootElement = "users" };

            var output = (List<string>)json.Deserialize(new RestResponse { Content = content }, typeof(List<string>));

			Assert.IsTrue (output.SequenceEqual (new[] { "johnsheehan" }));
		}

		[TestMethod]
		public void Can_Deserialize_Simple_Generic_List_Given_Toplevel_Item_Without_Array ()
		{
			const string content = "\"johnsheehan\"";
			var json = new JsonDeserializer ();

            var output = (List<string>)json.Deserialize(new RestResponse { Content = content }, typeof(List<string>));

			Assert.IsTrue (output.SequenceEqual (new[] { "johnsheehan" }));
		}

		[TestMethod]
		public async void Can_Deserialize_From_Root_Element()
		{
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(Path.Combine("SampleData", "sojson.txt"));
            var doc = await FileIO.ReadTextAsync(file);

			var json = new JsonDeserializer();
			json.RootElement = "User";

            var output = (SOUser)json.Deserialize(new RestResponse { Content = doc }, typeof(SOUser));
			Assert.AreEqual("John Sheehan", output.DisplayName);
		}

		[TestMethod]
		public async void Can_Deserialize_Generic_Members()
		{
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(Path.Combine("SampleData", "GenericWithList.txt"));
            var doc = await FileIO.ReadTextAsync(file);

			var json = new JsonDeserializer();

            var output = (Generic<GenericWithList<Foe>>)json.Deserialize(new RestResponse { Content = doc }, typeof(Generic<GenericWithList<Foe>>));
			Assert.AreEqual("Foe sho", output.Data.Items[0].Nickname);
		}

		[TestMethod]
		public void Can_Deserialize_List_of_Guid()
		{
			Guid ID1 = new Guid("b0e5c11f-e944-478c-aadd-753b956d0c8c");
			Guid ID2 = new Guid("809399fa-21c4-4dca-8dcd-34cb697fbca0");
			var data = new JObject();
			data["Ids"] = new JArray(ID1, ID2);

			var d = new JsonDeserializer();
			var response = new RestResponse { Content = data.ToString() };
            var p = (GuidList)d.Deserialize(response, typeof(GuidList));

			Assert.AreEqual(2, p.Ids.Count);
			Assert.AreEqual(ID1, p.Ids[0]);
			Assert.AreEqual(ID2, p.Ids[1]);
		}
		
		[TestMethod]
		public void Can_Deserialize_Generic_List_of_DateTime()
		{
			DateTime Item1 = new DateTime(2010, 2, 8, 11, 11, 11);
			DateTime Item2 = Item1.AddSeconds(12345);
			var data = new JObject();
			data["Items"] = new JArray(Item1.ToString("u"), Item2.ToString("u"));

			var d = new JsonDeserializer();
			var response = new RestResponse { Content = data.ToString() };
            var p = (GenericWithList<DateTime>)d.Deserialize(response, typeof(GenericWithList<DateTime>));

			Assert.AreEqual(2, p.Items.Count);
			Assert.AreEqual(Item1, p.Items[0]);
			Assert.AreEqual(Item2, p.Items[1]);
		}

		[TestMethod]
		public void Can_Deserialize_Empty_Elements_to_Nullable_Values()
		{
			var doc = CreateJsonWithNullValues();

			var json = new JsonDeserializer();
            var output = (NullableValues)json.Deserialize(new RestResponse { Content = doc }, typeof(NullableValues));

			Assert.IsNull(output.Id);
			Assert.IsNull(output.StartDate);
			Assert.IsNull(output.UniqueId);
		}

		[TestMethod]
		public void Can_Deserialize_Elements_to_Nullable_Values()
		{
			var doc = CreateJsonWithoutEmptyValues();

			var json = new JsonDeserializer();
            var output = (NullableValues)json.Deserialize(new RestResponse { Content = doc }, typeof(NullableValues));

			Assert.IsNotNull(output.Id);
			Assert.IsNotNull(output.StartDate);
			Assert.IsNotNull(output.UniqueId);

			Assert.AreEqual(123, output.Id);
			Assert.IsNotNull(output.StartDate);
			Assert.AreEqual(
				new DateTime(2010, 2, 21, 9, 35, 00, DateTimeKind.Utc),
				output.StartDate.Value);
			Assert.AreEqual(new Guid(GuidString), output.UniqueId);
		}

		[TestMethod]
		public void Can_Deserialize_Custom_Formatted_Date()
		{
			 var culture = CultureInfo.InvariantCulture;
			var format = "dd yyyy MMM, hh:mm ss tt";
			var date = new DateTime(2010, 2, 8, 11, 11, 11);

			var formatted = new
			{
				StartDate = date.ToString(format, culture)
			};

			var data = JsonConvert.SerializeObject(formatted);
			var response = new RestResponse { Content = data };

			var json = new JsonDeserializer { DateFormat = format }; //, Culture = culture };

            var output = (PersonForJson)json.Deserialize(response, typeof(PersonForJson));

			Assert.AreEqual(date, output.StartDate);
		}

		[TestMethod]
		public async void Can_Deserialize_Root_Json_Array_To_List()
		{
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(Path.Combine("SampleData", "jsonarray.txt"));
            var data = await FileIO.ReadTextAsync(file);

			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer();
            var output = (List<status>)json.Deserialize(response, typeof(List<status>));
			Assert.AreEqual(4, output.Count);
		}

		[TestMethod]
		public async void Can_Deserialize_Root_Json_Array_To_Inherited_List()
		{
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(Path.Combine("SampleData", "jsonarray.txt"));
            var data = await FileIO.ReadTextAsync(file);

			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer();
            var output = (StatusList)json.Deserialize(response, typeof(StatusList));
			Assert.AreEqual(4, output.Count);
		}

		[TestMethod]
		public async void Can_Deserialize_Various_Enum_Values ()
		{
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(Path.Combine("SampleData", "jsonenums.txt"));
            var data = await FileIO.ReadTextAsync(file);

			//var data = File.ReadAllText ();
			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer ();
			var output = (JsonEnumsTestStructure)json.Deserialize(response, typeof(JsonEnumsTestStructure));

			Assert.AreEqual(Disposition.Friendly,output.Upper);
			Assert.AreEqual(Disposition.Friendly,output.Lower);
			Assert.AreEqual(Disposition.SoSo,output.CamelCased);
			Assert.AreEqual(Disposition.SoSo,output.Underscores);
			Assert.AreEqual(Disposition.SoSo,output.LowerUnderscores);
			Assert.AreEqual(Disposition.SoSo,output.Dashes);
			Assert.AreEqual(Disposition.SoSo,output.LowerDashes);
			Assert.AreEqual(Disposition.SoSo,output.Integer);
		}

		[TestMethod]
		public void Deserialization_Of_Undefined_Int_Value_Returns_Enum_Default()
		{
			const string data = @"{ ""Integer"" : 1024 }";
			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer ();
			var result = (JsonEnumsTestStructure)json.Deserialize(response, typeof(JsonEnumsTestStructure));
			Assert.AreEqual(Disposition.Friendly,result.Integer);
		}

		[TestMethod]
		public void Can_Deserialize_Guid_String_Fields()
		{
			var doc = new JObject();
			doc["Guid"] = GuidString;

			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc.ToString() };
			var p = (PersonForJson)d.Deserialize(response, typeof(PersonForJson));

			Assert.AreEqual(new Guid(GuidString), p.Guid);
		}

		[TestMethod]
		public void Can_Deserialize_Quoted_Primitive()
		{
			var doc = new JObject();
			doc["Age"] = "28";

			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc.ToString() };
			var p = (PersonForJson)d.Deserialize(response, typeof(PersonForJson));

			Assert.AreEqual(28, p.Age);
		}

		[TestMethod]
		public void Can_Deserialize_With_Default_Root()
		{
			var doc = CreateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var p = (PersonForJson)d.Deserialize(response, typeof(PersonForJson));

			Assert.AreEqual("John Sheehan", p.Name);
			Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc), p.StartDate);
			Assert.AreEqual(28, p.Age);
			Assert.AreEqual(long.MaxValue, p.BigNumber);
			Assert.AreEqual(99.9999m, p.Percent);
			Assert.AreEqual(false, p.IsCool);
			Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.AreEqual(Guid.Empty, p.EmptyGuid);
			Assert.AreEqual(new Guid(GuidString), p.Guid);

			Assert.AreEqual(Order.Third, p.Order);
			Assert.AreEqual(Disposition.SoSo, p.Disposition);

			Assert.IsNotNull(p.Friends);
			Assert.AreEqual(10, p.Friends.Count);

			Assert.IsNotNull(p.BestFriend);
			Assert.AreEqual("The Fonz", p.BestFriend.Name);
			Assert.AreEqual(1952, p.BestFriend.Since);

            Assert.IsTrue(p.Foes.Count > 0);
			Assert.AreEqual("Foe 1", p.Foes["dict1"].Nickname);
			Assert.AreEqual("Foe 2", p.Foes["dict2"].Nickname);
		}

		[TestMethod]
		public void Can_Deserialize_With_Default_Root_Alternative_Culture()
		{
			using (new CultureChange(AlternativeCulture))
			{
					Can_Deserialize_With_Default_Root();
			}
		}

		[TestMethod]
		public async void Can_Deserialize_Names_With_Underscore_Prefix()
		{
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(Path.Combine("SampleData", "underscore_prefix.txt"));
            var data = await FileIO.ReadTextAsync(file);

			var response = new RestResponse { Content = data };
			var json = new JsonDeserializer();
			json.RootElement = "User";

			var output = (SOUser)json.Deserialize(response, typeof(SOUser));

			Assert.AreEqual("John Sheehan", output.DisplayName);
			Assert.AreEqual(1786, output.Id);
		}

		[TestMethod]
		public void Can_Deserialize_Names_With_Underscores_With_Default_Root()
		{
			var doc = CreateJsonWithUnderscores();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var p = (PersonForJson)d.Deserialize(response, typeof(PersonForJson));

			Assert.AreEqual("John Sheehan", p.Name);
			Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.AreEqual(28, p.Age);
			Assert.AreEqual(long.MaxValue, p.BigNumber);
			Assert.AreEqual(99.9999m, p.Percent);
			Assert.AreEqual(false, p.IsCool);
			Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.IsNotNull(p.Friends);
			Assert.AreEqual(10, p.Friends.Count);

			Assert.IsNotNull(p.BestFriend);
			Assert.AreEqual("The Fonz", p.BestFriend.Name);
			Assert.AreEqual(1952, p.BestFriend.Since);

            Assert.IsTrue(p.Foes.Count > 0);
			Assert.AreEqual("Foe 1", p.Foes["dict1"].Nickname);
			Assert.AreEqual("Foe 2", p.Foes["dict2"].Nickname);
		}

		[TestMethod]
		public void Can_Deserialize_Names_With_Underscores_With_Default_Root_Alternative_Culture()
		{
			using (new CultureChange(AlternativeCulture))
			{
					Can_Deserialize_Names_With_Underscores_With_Default_Root();
			}
		}

		[TestMethod]
		public void Can_Deserialize_Names_With_Dashes_With_Default_Root()
		{
			var doc = CreateJsonWithDashes();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var p = (PersonForJson)d.Deserialize(response, typeof(PersonForJson));

			Assert.AreEqual("John Sheehan", p.Name);
			//Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc), p.StartDate);
			Assert.AreEqual(28, p.Age);
			Assert.AreEqual(long.MaxValue, p.BigNumber);
			Assert.AreEqual(99.9999m, p.Percent);
			Assert.AreEqual(false, p.IsCool);
			Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.IsNotNull(p.Friends);
			Assert.AreEqual(10, p.Friends.Count);

			Assert.IsNotNull(p.BestFriend);
			Assert.AreEqual("The Fonz", p.BestFriend.Name);
			Assert.AreEqual(1952, p.BestFriend.Since);

            Assert.IsTrue(p.Foes.Count > 0);
			Assert.AreEqual("Foe 1", p.Foes["dict1"].Nickname);
			Assert.AreEqual("Foe 2", p.Foes["dict2"].Nickname);
		}

		[TestMethod]
		public void Can_Deserialize_Names_With_Dashes_With_Default_Root_Alternative_Culture()
		{
			using (new CultureChange(AlternativeCulture))
			{
					Can_Deserialize_Names_With_Dashes_With_Default_Root();
			}
		}

		[TestMethod]
		public void Ignore_Protected_Property_That_Exists_In_Data()
		{
			var doc = CreateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var p = (PersonForJson)d.Deserialize(response, typeof(PersonForJson));

			Assert.IsNull(p.IgnoreProxy);
		}

		[TestMethod]
		public void Ignore_ReadOnly_Property_That_Exists_In_Data()
		{
			var doc = CreateJson();
			var response = new RestResponse { Content = doc };
			var d = new JsonDeserializer();
			var p = (PersonForJson)d.Deserialize(response, typeof(PersonForJson));

			Assert.IsNull(p.ReadOnlyProxy);
		}

		[TestMethod]
		public async void Can_Deserialize_TimeSpan()
		{
			var payload = await GetPayLoad<TimeSpanTestStructure>("timespans.txt");

			Assert.AreEqual(new TimeSpan(468006), payload.Tick);
			Assert.AreEqual(new TimeSpan(0, 0, 0, 0, 125), payload.Millisecond);
			Assert.AreEqual(new TimeSpan(0, 0, 8), payload.Second);
			Assert.AreEqual(new TimeSpan(0, 55, 2), payload.Minute);
			Assert.AreEqual(new TimeSpan(21, 30, 7), payload.Hour);
			Assert.IsNull(payload.NullableWithoutValue);
			Assert.IsNotNull(payload.NullableWithValue);
			Assert.AreEqual(new TimeSpan(21, 30, 7), payload.NullableWithValue.Value);
		}

		[TestMethod]
		public void Can_Deserialize_Iso_Json_Dates()
		{
			var doc = CreateIsoDateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var bd = (Birthdate)d.Deserialize(response, typeof(Birthdate));

			Assert.AreEqual(new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc), bd.Value);
		}

		[TestMethod]
		public void Can_Deserialize_Unix_Json_Dates()
		{
			var doc = CreateUnixDateJson();
			var d = new JsonDeserializer();
			var response = new RestResponse { Content = doc };
			var bd = (Birthdate)d.Deserialize(response, typeof(Birthdate));

			Assert.AreEqual(new DateTime(2011, 6, 30, 8, 15, 46, DateTimeKind.Utc), bd.Value);
		}

        [TestMethod]
        public void Can_Deserialize_To_Dictionary_String_String()
        {
            var doc = CreateJsonStringDictionary();
            var d = new JsonDeserializer();
            var response = new RestResponse { Content = doc };
            var bd = (Dictionary<string, string>)d.Deserialize(response, typeof(Dictionary<string, string>));

            Assert.AreEqual(bd["Thing1"], "Thing1");
            Assert.AreEqual(bd["Thing2"], "Thing2");
            Assert.AreEqual(bd["ThingRed"], "ThingRed");
            Assert.AreEqual(bd["ThingBlue"], "ThingBlue");
        }

        [TestMethod]
        public void Can_Deserialize_To_Dictionary_String_String_With_Dynamic_Values()
        {
            var doc = CreateDynamicJsonStringDictionary();
            var d = new JsonDeserializer();
            var response = new RestResponse { Content = doc };
            var bd = (Dictionary<string, string>)d.Deserialize(response, typeof(Dictionary<string, string>));

            Assert.AreEqual("[\"Value1\",\"Value2\"]", bd["Thing1"]);
            Assert.AreEqual("Thing2", bd["Thing2"]);
            Assert.AreEqual("{\"Name\":\"ThingRed\",\"Color\":\"Red\"}", bd["ThingRed"]);
            Assert.AreEqual("{\"Name\":\"ThingBlue\",\"Color\":\"Blue\"}", bd["ThingBlue"]);
        }


		[TestMethod]
		public async void Can_Deserialize_JsonNet_Dates()
		{
			var person = await GetPayLoad<PersonForJson>("person.json.txt");

			Assert.AreEqual(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				person.StartDate);
		}

		[TestMethod]
		public async void Can_Deserialize_DateTime()
		{
			var payload = await GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.AreEqual(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				payload.DateTime);
		}

		[TestMethod]
		public async void Can_Deserialize_Nullable_DateTime_With_Value()
		{
			var payload = await GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.IsNotNull(payload.NullableDateTimeWithValue);
			Assert.AreEqual(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				payload.NullableDateTimeWithValue.Value);
		}

		[TestMethod]
		public async void Can_Deserialize_Nullable_DateTime_With_Null()
		{
			var payload = await GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.IsNull(payload.NullableDateTimeWithNull);
		}

		[TestMethod]
		public async void Can_Deserialize_DateTimeOffset()
		{
			var payload = await GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.AreEqual(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				payload.DateTimeOffset);
		}

		[TestMethod]
		public async void Can_Deserialize_Iso8601DateTimeLocal()
		{
			var payload = await GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.txt");

			Assert.AreEqual(
				new DateTime(2012, 7, 19, 10, 23, 25, DateTimeKind.Utc),
				payload.DateTimeLocal);
		}

		[TestMethod]
		public async void Can_Deserialize_Iso8601DateTimeZulu()
		{
			var payload = await GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.txt");

			Assert.AreEqual(
				new DateTime(2012, 7, 19, 10, 23, 25, 544, DateTimeKind.Utc),
				payload.DateTimeUtc.ToUniversalTime());
		}

		[TestMethod]
		public async void Can_Deserialize_Iso8601DateTimeWithOffset()
		{
			var payload = await GetPayLoad<Iso8601DateTimeTestStructure>("iso8601datetimes.txt");

			Assert.AreEqual(
				new DateTime(2012, 7, 19, 10, 23, 25, 544, DateTimeKind.Utc),
				payload.DateTimeWithOffset.ToUniversalTime());
		}

		[TestMethod]
		public async void Can_Deserialize_Nullable_DateTimeOffset_With_Value()
		{
			var payload = await GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.IsNotNull(payload.NullableDateTimeOffsetWithValue);
			Assert.AreEqual(
				new DateTime(2011, 6, 30, 8, 15, 46, 929, DateTimeKind.Utc),
				payload.NullableDateTimeOffsetWithValue);
		}

		[TestMethod]
		public async void Can_Deserialize_Nullable_DateTimeOffset_With_Null()
		{
			var payload = await GetPayLoad<DateTimeTestStructure>("datetimes.txt");

			Assert.IsNull(payload.NullableDateTimeOffsetWithNull);
		}

        #region Private - JSON Creation Helpers

        private string CreateJsonWithUnderscores()
		{
			var doc = new JObject();
			doc["name"] = "John Sheehan";
			doc["start_date"] = new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc);
			doc["age"] = 28;
			doc["percent"] = 99.9999m;
			doc["big_number"] = long.MaxValue;
			doc["is_cool"] = false;
			doc["ignore"] = "dummy";
			doc["read_only"] = "dummy";
			doc["url"] = "http://example.com";
			doc["url_path"] = "/foo/bar";

			doc["best_friend"] = new JObject(
									new JProperty("name", "The Fonz"),
									new JProperty("since", 1952)
								);

			var friendsArray = new JArray();
			for (int i = 0; i < 10; i++)
			{
				friendsArray.Add(new JObject(
									new JProperty("name", "Friend" + i),
									new JProperty("since", DateTime.Now.Year - i)
								));
			}

			doc["friends"] = friendsArray;

			var foesArray = new JObject(
								new JProperty("dict1", new JObject(new JProperty("nickname", "Foe 1"))),
								new JProperty("dict2", new JObject(new JProperty("nickname", "Foe 2")))
							);

			doc["foes"] = foesArray;

			return doc.ToString();
		}

		private string CreateJsonWithDashes()
		{
			var doc = new JObject();
			doc["name"] = "John Sheehan";
			doc["start-date"] = new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc);
			doc["age"] = 28;
			doc["percent"] = 99.9999m;
			doc["big-number"] = long.MaxValue;
			doc["is-cool"] = false;
			doc["ignore"] = "dummy";
			doc["read-only"] = "dummy";
			doc["url"] = "http://example.com";
			doc["url-path"] = "/foo/bar";

			doc["best-friend"] = new JObject(
									new JProperty("name", "The Fonz"),
									new JProperty("since", 1952)
								);

			var friendsArray = new JArray();
			for (int i = 0; i < 10; i++)
			{
				friendsArray.Add(new JObject(
									new JProperty("name", "Friend" + i),
									new JProperty("since", DateTime.Now.Year - i)
								));
			}

			doc["friends"] = friendsArray;

			var foesArray = new JObject(
								new JProperty("dict1", new JObject(new JProperty("nickname", "Foe 1"))),
								new JProperty("dict2", new JObject(new JProperty("nickname", "Foe 2")))
							);

			doc["foes"] = foesArray;

			return doc.ToString();
		}

		private string CreateIsoDateJson()
		{
			var bd = new Birthdate();
			bd.Value = new DateTime(1910, 9, 25, 9, 30, 25, DateTimeKind.Utc);

			return JsonConvert.SerializeObject(bd, new IsoDateTimeConverter());
		}

		private string CreateUnixDateJson()
		{
			var doc = new JObject();
			doc["Value"] = 1309421746;

			return doc.ToString();
		}

		private string CreateJson()
		{
			var doc = new JObject();
			doc["Name"] = "John Sheehan";
			doc["StartDate"] = new DateTime(2009, 9, 25, 0, 6, 1, DateTimeKind.Utc);
			doc["Age"] = 28;
			doc["Percent"] = 99.9999m;
			doc["BigNumber"] = long.MaxValue;
			doc["IsCool"] = false;
			doc["Ignore"] = "dummy";
			doc["ReadOnly"] = "dummy";
			doc["Url"] = "http://example.com";
			doc["UrlPath"] = "/foo/bar";
			doc["Order"] = "third";
			doc["Disposition"] = "so_so";

			doc["Guid"] = new Guid(GuidString).ToString();
			doc["EmptyGuid"] = "";

			doc["BestFriend"] = new JObject(
									new JProperty("Name", "The Fonz"),
									new JProperty("Since", 1952)
								);

			var friendsArray = new JArray();
			for (int i = 0; i < 10; i++)
			{
				friendsArray.Add(new JObject(
									new JProperty("Name", "Friend" + i),
									new JProperty("Since", DateTime.Now.Year - i)
								));
			}

			doc["Friends"] = friendsArray;

			var foesArray = new JObject(
								new JProperty("dict1", new JObject(new JProperty("Nickname", "Foe 1"))),
								new JProperty("dict2", new JObject(new JProperty("Nickname", "Foe 2")))
							);

			doc["Foes"] = foesArray;

			return doc.ToString();
		}

		private string CreateJsonWithNullValues()
		{
			var doc = new JObject();
			doc["Id"] = null;
			doc["StartDate"] = null;
			doc["UniqueId"] = null;

			return doc.ToString();
		}

		private string CreateJsonWithoutEmptyValues()
		{
			var doc = new JObject();
			doc["Id"] = 123;
			doc["StartDate"] = new DateTime(2010, 2, 21, 9, 35, 00, DateTimeKind.Utc);
			doc["UniqueId"] = new Guid(GuidString).ToString();

			return doc.ToString();
		}

		public string CreateJsonStringDictionary()
		{
			var doc = new JObject();
			doc["Thing1"] = "Thing1";
			doc["Thing2"] = "Thing2";
			doc["ThingRed"] = "ThingRed";
			doc["ThingBlue"] = "ThingBlue";
			return doc.ToString();
		}

		public string CreateDynamicJsonStringDictionary ()
		{
			var doc = new JObject ();
			doc["Thing1"] = new JArray () { "Value1", "Value2" };
			doc["Thing2"] = "Thing2";
			doc["ThingRed"] = new JObject (new JProperty ("Name", "ThingRed"), new JProperty ("Color", "Red"));
			doc["ThingBlue"] = new JObject (new JProperty("Name", "ThingBlue"), new JProperty ("Color", "Blue"));
			return doc.ToString ();
		}

        #endregion

        private async Task<T> GetPayLoad<T>(string fileName)
		{
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(Path.Combine("SampleData", fileName));
            var doc = await FileIO.ReadTextAsync(file);

			var response = new RestResponse { Content = doc };
			var d = new JsonDeserializer();
			return (T)d.Deserialize(response, typeof(T));
		}
	}
}
