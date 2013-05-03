using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestRT.Tests.SampleClasses
{
	public class TwilioCallList : List<Call>
	{
		public int Page { get; set; }
		public int NumPages { get; set; }
	}

	public class Call
	{
		public string Sid { get; set; }
	}
}
