﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watsonia.AusPostInterface
{
	/// <summary>
	/// Additional information about an item feature. Please note that attributes depend on the feature.
	/// </summary>
	/// <seealso cref="Watsonia.AusPostInterface.ApiItem" />
	public sealed class ItemFeatureAttribute : ApiItem
	{
		/// <summary>
		/// Cover amount.
		/// </summary>
		/// <value>
		/// The cover amount.
		/// </value>
		public decimal CoverAmount { get; set; }
	}
}
