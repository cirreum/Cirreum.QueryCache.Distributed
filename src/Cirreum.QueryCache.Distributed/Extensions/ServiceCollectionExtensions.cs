namespace Cirreum.QueryCache.Distributed.Extensions;

using Cirreum.Conductor.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for configuring distributed query caching services.
/// </summary>
public static class ServiceCollectionExtensions {
	/// <summary>
	/// Registers the <see cref="DistributedCacheableQueryService"/> as the implementation for
	/// <see cref="ICacheableQueryService"/>, enabling caching support for <c>ICacheableQuery&lt;T&gt;</c> requests.
	/// </summary>
	/// <remarks>
	/// This method requires an <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/> 
	/// implementation to be registered separately (e.g., <c>AddStackExchangeRedisCache()</c>, 
	/// <c>AddSqlServerCache()</c>, or <c>AddDistributedMemoryCache()</c>).
	/// <para>
	/// Note: <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/> does not support 
	/// tag-based eviction. Calls to <see cref="ICacheableQueryService.RemoveByTagAsync"/> and 
	/// <see cref="ICacheableQueryService.RemoveByTagsAsync"/> will throw <see cref="System.NotSupportedException"/>.
	/// </para>
	/// </remarks>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddDistributedQueryCaching(this IServiceCollection services) {
		//
		// Conductor Cacheable Query Service (Distributed)
		//
		services
			.TryAddSingleton<ICacheableQueryService, DistributedCacheableQueryService>();
		return services;
	}
}