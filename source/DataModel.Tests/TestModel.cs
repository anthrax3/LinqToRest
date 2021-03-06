using System;

namespace DataModel.Tests
{
	public class TestModel
	{
		public string TestString { get; set; }
		public int TestInt { get; set; }
		public decimal TestDecimal { get; set; }
		public DateTime TestDateTime { get; set; }
		public DateTimeOffset TestDateTimeOffset { get; set; }
		public TimeSpan TestTime { get; set; }
		public bool TestBoolean { get; set; }
		public Guid TestGuid { get; set; }
		public object TestObject { get; set; }
		public TestModel TestChild { get; set; }
	}
}
