using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HttpServer.Listener
{
	public class ServerListenerContext
	{
		private static readonly AsyncLocal<HttpListenerResponse> _responseContext =
			new AsyncLocal<HttpListenerResponse>();
		public static HttpListenerResponse CurrentResponse
		{
			get => _responseContext.Value;
			set => _responseContext.Value = value;
		}

		private static AsyncLocal<HttpListenerRequest> _requestContext =
			new AsyncLocal<HttpListenerRequest>();
		public static HttpListenerRequest CurrentRequest
		{
			get => _requestContext.Value;
			set => _requestContext.Value = value;
		}

		public class ServiceListenerRequestService
		{
			private static NameValueCollection? Header => CurrentRequest.Headers;
			public static string? HeaderAuthorization => Header["Authorization"];

			public async Task<T> ReadAsyncJsonBody<T>()
			{
				using (
					var reader = new StreamReader(
						CurrentRequest.InputStream,
						CurrentRequest.ContentEncoding
					)
				)
				{
					var data = JsonSerializer.Deserialize<T>(await reader.ReadToEndAsync());
					return data;
				}
			}
		}

		public class ServiceListenerResponseService
		{
			private async Task ResponseAsync(
				string data,
				string contentType,
				HttpStatusCode statusCode = HttpStatusCode.OK
			)
			{
				var response = CurrentResponse;

				response.ContentType = contentType;
				response.StatusCode = (int)statusCode;
				var buffer = Encoding.UTF8.GetBytes(data);
				response.ContentLength64 = buffer.Length;
				await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
				response.Close();
			}

			public async Task Error(string message, HttpStatusCode statusCode)
			{
				var errorData = JsonSerializer.Serialize(new { message, code = statusCode });
				await ResponseAsync(errorData, "application/json", statusCode);
			}

			public async Task Json(object data)
			{
				var jsonData = JsonSerializer.Serialize(data);
				await ResponseAsync(jsonData, "application/json");
			}
		}
	}
}
