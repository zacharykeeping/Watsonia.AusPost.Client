﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watsonia.AusPostInterface
{
	public sealed class OrderInformation : ApiItem
	{
		/// <summary>
		/// The identifier for the order generated by Australia Post.
		/// </summary>
		/// <value>
		/// The order identifier.
		/// </value>
		[StringLength(50)]
		public string OrderID { get; set; }

		/// <summary>
		/// The reference for the order that was passed in the request.
		/// </summary>
		/// <value>
		/// The order reference.
		/// </value>
		[StringLength(50)]
		public string OrderReference { get; set; }

		/// <summary>
		/// The date and time that the order was created.
		/// </summary>
		/// <value>
		/// The order creation date.
		/// </value>
		public DateTime OrderCreationDate { get; set; }

		public OrderSummary OrderSummary { get; set; } = new OrderSummary();

		public List<ShipmentInformation> Shipments { get; set; } = new List<ShipmentInformation>();

		/// <summary>
		/// Details of the payment method used to pay for the order.
		/// </summary>
		/// <value>
		/// The payment method.
		/// </value>
		public string PaymentMethod { get; set; }

		/// <summary>
		/// Loads a OrderInformation from a JSON string.
		/// </summary>
		/// <param name="json">The json.</param>
		public static OrderInformation FromJson(string json)
		{
			var serializer = new ApiSerializer();
			return serializer.FromJson<OrderInformation>(json);
		}
	}
}
