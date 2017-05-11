﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watsonia.AusPostInterface
{
	public sealed class CreateOrderIncludingShipmentsRequest : ApiItem
	{
		/// <summary>
		/// A unique reference for the order, generated by the merchant.
		/// </summary>
		/// <value>
		/// The order reference.
		/// </value>
		[StringLength(50)]
		public string OrderReference { get; set; }

		/// <summary>
		/// The intended method of payment, the only valid value is: CHARGE_TO_ACCOUNT.
		/// </summary>
		/// <value>
		/// The payment method.
		/// </value>
		[StringLength(50)]
		public string PaymentMethod { get; set; } = "CHARGE_TO_ACCOUNT";

		/// <summary>
		///  The name or ID of the consignor for this order. If provided this information is included in the delivery documentation to assist traceability. 
		/// </summary>
		/// <value>
		/// The consignor.
		/// </value>
		[StringLength(50)]
		public string Consignor { get; set; }

		/// <summary>
		/// Shipment information must be supplied for each shipment to be included in the order.
		/// </summary>
		/// <value>
		/// The shipments.
		/// </value>
		public List<Shipment> Shipments { get; set; } = new List<Shipment>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CreateOrderFromShipmentsRequest"/> class.
		/// </summary>
		/// <param name="shipments">The shipments.</param>
		public CreateOrderIncludingShipmentsRequest(params Shipment[] shipments)
		{
			this.Shipments.AddRange(shipments);
		}
	}
}
