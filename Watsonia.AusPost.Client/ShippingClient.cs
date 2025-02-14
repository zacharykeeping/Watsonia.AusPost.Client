﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Watsonia.AusPost.Client
{
	public class ShippingClient
	{
		private const string TestingUrl = "https://digitalapi.auspost.com.au/test/shipping/v1/";
		private const string LiveUrl = "https://digitalapi.auspost.com.au/shipping/v1/";

		private readonly string _accountNumber;
		private readonly string _username;
		private readonly string _password;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is making requests against the testbed.
		/// </summary>
		/// <value>
		///   <c>true</c> if in testing; otherwise, <c>false</c>.
		/// </value>
		public bool Testing { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ShippingClient" /> class.
		/// </summary>
		/// <param name="accountNumber">The Australia Post account number.</param>
		/// <param name="username">The Australia Post API _username.</param>
		/// <param name="password">The Australia Post API _password.</param>
		public ShippingClient(string accountNumber, string username, string password, bool testing = false)
		{
			_accountNumber = accountNumber;
			_username = username;
			_password = password;
            Testing = testing;
        }

		/// <summary>
		/// This interface retrieves information regarding the requestor’s charge account and the postage products that the charge account is able to use.
		/// </summary>
		/// <returns></returns>
		public async Task<GetAccountsResponse> GetAccountsAsync()
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
			string apiUrl = apiUrlPrefix + "accounts/" + _accountNumber;

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// GET the request
				HttpResponseMessage response = await client.GetAsync(apiUrl);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				string responseJson = await response.Content.ReadAsStringAsync();
				var result = GetAccountsResponse.FromJson(responseJson);
				result.Succeeded = response.StatusCode == System.Net.HttpStatusCode.OK;

				return result;
			}
		}

		/// <summary>
		/// This interface creates a shipment with items and returns a summary of the pricing for the items.
		/// </summary>
		/// <param name="shipments">The shipments.</param>
		/// <returns></returns>
		public async Task<CreateShipmentsResponse> CreateShipmentsAsync(CreateShipmentsRequest shipments)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
			string apiUrl = apiUrlPrefix + "shipments";

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// Build the JSON content from the shipment
				var json = shipments.ToJson();
				HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

				// POST the request
				HttpResponseMessage response = await client.PostAsync(apiUrl, content);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				string responseJson = await response.Content.ReadAsStringAsync();
				var result = CreateShipmentsResponse.FromJson(responseJson);
				result.Succeeded = response.StatusCode == System.Net.HttpStatusCode.Created;

				return result;
			}
		}

        /// <summary>
        /// This service can retrieve shipment-level pricing for all products on a billing account by omitting the product_id field.
        /// </summary>
        /// <param name="shipments">The shipments.</param>
        /// <returns></returns>
        public async Task<CreateShipmentsResponse> GetShipmentPriceAsync(CreateShipmentsRequest shipments)
        {
            string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
            string apiUrl = apiUrlPrefix + "prices/shipments";

            using (var client = new HttpClient())
            {
                // Add authentication headers
                byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
                client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

                // Build the JSON content from the shipment
                var json = shipments.ToJson();
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                // POST the request
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                // Read the response
                // TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
                string responseJson = await response.Content.ReadAsStringAsync();
                var result = CreateShipmentsResponse.FromJson(responseJson);
                result.Succeeded = response.StatusCode == System.Net.HttpStatusCode.Created;

                return result;
            }
        }

        /// <summary>
        /// This service updates an existing shipment that has previously been created using the Create Shipment interface.
        /// </summary>
        /// <param name="shipmentID">The identifier for the shipment generated by Australia Post.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public async Task<UpdateItemsResponse> UpdateItemsAsync(string shipmentID, UpdateItemsRequest items)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
			string apiUrl = apiUrlPrefix + "shipments/" + shipmentID + "/items";

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// Build the JSON content from the items
				var json = items.ToJson();
				HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

				// PUT the request
				HttpResponseMessage response = await client.PutAsync(apiUrl, content);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				if (response.StatusCode == System.Net.HttpStatusCode.Created)
				{
					var result = new UpdateItemsResponse();
					result.Succeeded = true;
					return result;
				}
				else
				{
					// Read JSON errors
					string responseJson = await response.Content.ReadAsStringAsync();
					var result = UpdateItemsResponse.FromJson(responseJson);
					result.Succeeded = false;
					return result;
				}
			}
		}

		/// <summary>
		/// This service deletes an item in a shipment for shipments that have not been included in an order.
		/// </summary>
		/// <param name="shipmentID">The identifier for the shipment generated by Australia Post.</param>
		/// <param name="itemID">The identifier for the item generated by Australia Post.</param>
		/// <returns></returns>
		public async Task<DeleteItemsResponse> DeleteItemAsync(string shipmentID, string itemID)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
			string apiUrl = apiUrlPrefix + "shipments/" + shipmentID + "/items/" + itemID;

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// DELETE the item
				HttpResponseMessage response = await client.DeleteAsync(apiUrl);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					var result = new DeleteItemsResponse();
					result.Succeeded = true;
					return result;
				}
				else
				{
					// Read JSON errors
					string responseJson = await response.Content.ReadAsStringAsync();
					var result = DeleteItemsResponse.FromJson(responseJson);
					result.Succeeded = false;
					return result;
				}
			}
		}

		/// <summary>
		/// This service deletes an item in a shipment for shipments that have not been included in an order.
		/// </summary>
		/// <param name="shipmentID">The identifier for the shipment generated by Australia Post.</param>
		/// <returns></returns>
		public async Task<DeleteShipmentResponse> DeleteShipmentAsync(string shipmentID)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
			string apiUrl = apiUrlPrefix + "shipments/" + shipmentID;

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// DELETE the item
				HttpResponseMessage response = await client.DeleteAsync(apiUrl);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					var result = new DeleteShipmentResponse();
					result.Succeeded = true;
					return result;
				}
				else
				{
					// Read JSON errors
					string responseJson = await response.Content.ReadAsStringAsync();
					var result = DeleteShipmentResponse.FromJson(responseJson);
					result.Succeeded = false;
					return result;
				}
			}
		}

		/// <summary>
		/// This service creates an order for the referenced shipments that have previously been created using the Create Shipments service.
		/// </summary>
		/// <param name="shipments">The shipments.</param>
		/// <returns></returns>
		public async Task<CreateOrderFromShipmentsResponse> CreateOrderFromShipmentsAsync(CreateOrderFromShipmentsRequest shipments)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
			string apiUrl = apiUrlPrefix + "orders";

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// Build the JSON content from the items
				var json = shipments.ToJson();
				HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

				// PUT the request
				HttpResponseMessage response = await client.PutAsync(apiUrl, content);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				string responseJson = await response.Content.ReadAsStringAsync();
				var result = CreateOrderFromShipmentsResponse.FromJson(responseJson);
				result.Succeeded = response.StatusCode == System.Net.HttpStatusCode.Created;

				return result;
			}
		}

		/// <summary>
		/// This service creates an order for the contained shipments and items.
		/// </summary>
		/// <param name="shipments">The shipments.</param>
		/// <returns></returns>
		public async Task<CreateOrderIncludingShipmentsResponse> CreateOrderIncludingShipmentsAsync(CreateOrderIncludingShipmentsRequest shipments)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
			string apiUrl = apiUrlPrefix + "orders";

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// Build the JSON content from the items
				var json = shipments.ToJson();
				HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

				// POST the request
				HttpResponseMessage response = await client.PostAsync(apiUrl, content);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				string responseJson = await response.Content.ReadAsStringAsync();
				var result = CreateOrderIncludingShipmentsResponse.FromJson(responseJson);
				result.Succeeded = response.StatusCode == System.Net.HttpStatusCode.Created;

				return result;
			}
		}

		/// <summary>
		/// This service initiates the generation of labels for the requested shipments that have been previously created using the Create Shipments service.
		/// </summary>
		/// <param name="shipments">The shipments.</param>
		/// <returns></returns>
		public async Task<CreateLabelsResponse> CreateLabelsAsync(CreateLabelsRequest shipments)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
			string apiUrl = apiUrlPrefix + "labels";

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// Build the JSON content from the items
				var json = shipments.ToJson();
				HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

				// POST the request
				HttpResponseMessage response = await client.PostAsync(apiUrl, content);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				string responseJson = await response.Content.ReadAsStringAsync();
				var result = CreateLabelsResponse.FromJson(responseJson);
				result.Succeeded = response.StatusCode == System.Net.HttpStatusCode.OK;

				return result;
			}
		}

		/// <summary>
		/// Downloads the labels for a shipment.
		/// </summary>
		/// <param name="labelUrl">The label URL.</param>
		/// <returns></returns>
		public async Task<DownloadLabelsResponse> DownloadLabelsAsync(string labelUrl)
		{
			using (var client = new HttpClient())
			{
				// GET the request
				HttpResponseMessage response = await client.GetAsync(labelUrl);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				var result = new DownloadLabelsResponse();
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					result.Stream = await response.Content.ReadAsStreamAsync();
				}
				// TODO: Need to build the errors manually?
				//else
				//{
				//	// Read JSON errors
				//	string responseJson = await response.Content.ReadAsStringAsync();
				//	result.FromJson(responseJson);
				//}
				result.Succeeded = response.StatusCode == System.Net.HttpStatusCode.OK;
				return result;
			}
		}

		/// <summary>
		/// This interface retrieves information for orders, and the items contained within orders created using the Create Orders interface.
		/// </summary>
		/// <param name="orders">The orders.</param>
		/// <returns></returns>
		public async Task<GetOrdersResponse> GetOrdersAsync(GetOrdersRequest orders)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;

			// Build the query string
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			// HACK: I don't think this works, but I'm not 100% sure:
			//if (orders.OrderIDs.Any())
			//{
			//	queryParams.Add("order_ids", string.Join(",", orders.OrderIDs));
			//}
			if (orders.Offset > 0)
			{
				queryParams.Add("offset", orders.Offset.ToString());
			}
			if (orders.NumberOfOrders > 0)
			{
				queryParams.Add("number_of_orders", orders.NumberOfOrders.ToString());
			}
			string apiUrl = apiUrlPrefix + "orders?" + string.Join("&", queryParams.Select(p => $"{p.Key}={p.Value}"));

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// GET the request
				HttpResponseMessage response = await client.GetAsync(apiUrl);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				string responseJson = await response.Content.ReadAsStringAsync();
				var result = GetOrdersResponse.FromJson(responseJson);
				result.Succeeded = response.StatusCode == System.Net.HttpStatusCode.OK;

				return result;
			}
		}

		/// <summary>
		/// This interface retrieves information for shipments, and the items contained within shipments created using the Create Shipments interface.
		/// </summary>
		/// <param name="shipments">The shipments.</param>
		/// <returns></returns>
		public async Task<GetShipmentsResponse> GetShipmentsAsync(GetShipmentsRequest shipments)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;

			// Build the query string
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			if (shipments.ShipmentIDs.Any())
			{
				queryParams.Add("shipment_ids", string.Join(",", shipments.ShipmentIDs));
			}
			if (shipments.Offset > 0)
			{
				queryParams.Add("offset", shipments.Offset.ToString());
			}
			if (shipments.NumberOfShipments > 0)
			{
				queryParams.Add("number_of_shipments", shipments.NumberOfShipments.ToString());
			}
			if (!string.IsNullOrEmpty(shipments.Status))
			{
				queryParams.Add("status", shipments.Status);
			}
			if (!string.IsNullOrEmpty(shipments.DespatchDate))
			{
				queryParams.Add("despatch_date", shipments.DespatchDate);
			}
			if (!string.IsNullOrEmpty(shipments.SenderReference))
			{
				queryParams.Add("sender_reference", shipments.SenderReference);
			}
			string apiUrl = apiUrlPrefix + "shipments?" + string.Join("&", queryParams.Select(p => $"{p.Key}={p.Value}"));

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// GET the request
				HttpResponseMessage response = await client.GetAsync(apiUrl);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				string responseJson = await response.Content.ReadAsStringAsync();
				var result = GetShipmentsResponse.FromJson(responseJson);
				result.Succeeded = response.StatusCode == System.Net.HttpStatusCode.OK;

				return result;
			}
		}

		/// <summary>
		/// This service returns the PDF order summary that contains a charges breakdown of the articles in the order.
		/// </summary>
		/// <param name="orderID">The order id you wish to retrieve the summary for.</param>
		/// <returns></returns>
		public async Task<GetOrderSummaryResponse> GetOrderSummaryAsync(GetOrderSummaryRequest request)
		{
			string apiUrlPrefix = this.Testing ? ShippingClient.TestingUrl : ShippingClient.LiveUrl;
			string apiUrl = apiUrlPrefix + "accounts/" + _accountNumber + "/orders/" + request.OrderID + "/summary";

			using (var client = new HttpClient())
			{
				// Add authentication headers
				byte[] apiKey = Encoding.UTF8.GetBytes(_username + ":" + _password);
				client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(apiKey));
				client.DefaultRequestHeaders.Add("Account-Number", _accountNumber);

				// Add the account number into the request
				request.AccountNumber = _accountNumber;

				// GET the request
				HttpResponseMessage response = await client.GetAsync(apiUrl);

				// Read the response
				// TODO: I may actually need to do different things if the status code comes back as e.g. NotFound?
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					var result = new GetOrderSummaryResponse();
					result.Succeeded = true;
					result.Stream = await response.Content.ReadAsStreamAsync();
					return result;
				}
				else
				{
					// Read JSON errors
					string responseJson = await response.Content.ReadAsStringAsync();
					var result = GetOrderSummaryResponse.FromJson(responseJson);
					return result;
				}
			}
		}
	}
}
