using System;

namespace LinqToRest.OData
{
	public class ODataOrdering : ODataQueryPart
	{
		public string Field { get; private set; }

		public ODataOrderingDirection Direction { get; private set; }

		public override ODataQueryPartType QueryPartType { get { return ODataQueryPartType.Ordering; } }

		public ODataOrdering(string fieldName, ODataOrderingDirection direction)
		{
			if (String.IsNullOrWhiteSpace(fieldName))
			{
				throw new ArgumentException(String.Format("Invalid field name '{0}'.", fieldName));
			}

			Field = fieldName;
			Direction = direction;
		}

		public override string ToString()
		{
			return String.Format("{0} {1}", Field, Direction.ToString().ToLowerInvariant());
		}
	}
}
