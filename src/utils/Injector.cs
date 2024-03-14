using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SVAssistant.Api;
using SVAssistant.Http.Routes;

public interface IServerRegistration
{
	void RegisterServices(IServiceCollection services);
}

public class HttpServiceRegistration : IServerRegistration
{
	public void RegisterServices(IServiceCollection services)
	{
		services.AddSingleton<IRouteHandler, Routes>();
		services.AddSingleton<IJwtHelper, JwtHelper>();
		services.AddSingleton<IHttpResponseService, HttpResponseService>();
		services.AddSingleton<IHttpRequestService, HttpRequestService>();

		var controllers = Assembly
			.GetExecutingAssembly()
			.GetTypes()
			.Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract);

		foreach (var type in controllers)
		{
			services.AddTransient(typeof(Controller), type);
		}

		services.AddSingleton<HttpServer>();
	}
}

public static class DependencyInjectionConfig
{
	public static ServiceProvider ConfigureServices()
	{
		var serviceCollection = new ServiceCollection();

		var registrations = new List<IServerRegistration> { new HttpServiceRegistration() };

		foreach (var registration in registrations)
		{
			registration.RegisterServices(serviceCollection);
		}

		return serviceCollection.BuildServiceProvider();
	}
}
