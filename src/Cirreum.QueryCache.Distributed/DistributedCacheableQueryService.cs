namespace Cirreum.QueryCache.Distributed;

using Cirreum.Conductor.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Text.Json;

sealed class DistributedCacheableQueryService(
	IDistributedCache cache,
	JsonSerializerOptions? serializerOptions = null
) : ICacheableQueryService {

	private readonly JsonSerializerOptions _serializerOptions =
		serializerOptions ?? new JsonSerializerOptions();

	public async ValueTask<TResponse> GetOrCreateAsync<TResponse>(
		string cacheKey,
		Func<CancellationToken, ValueTask<TResponse>> factory,
		QueryCacheSettings settings,
		string[]? tags = null,
		CancellationToken cancellationToken = default) {

		var cached = await cache.GetAsync(cacheKey, cancellationToken);
		if (cached is not null) {
			return JsonSerializer.Deserialize<TResponse>(cached, _serializerOptions)!;
		}

		var value = await factory(cancellationToken);

		var useFailureExpiration = value is IResult { IsSuccess: false }
			&& settings.FailureExpiration.HasValue;

		var options = CreateOptions(settings, useFailureExpiration);
		var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _serializerOptions);

		await cache.SetAsync(cacheKey, bytes, options, cancellationToken);

		return value;
	}

	public async ValueTask RemoveAsync(string cacheKey, CancellationToken cancellationToken) {
		await cache.RemoveAsync(cacheKey, cancellationToken);
	}

	public ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default) {
		throw new NotSupportedException("IDistributedCache does not support tag-based eviction.");
	}

	public ValueTask RemoveByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default) {
		throw new NotSupportedException("IDistributedCache does not support tag-based eviction.");
	}

	private static DistributedCacheEntryOptions CreateOptions(
		QueryCacheSettings settings,
		bool useFailureExpiration = false) {
		var expiration = useFailureExpiration && settings.FailureExpiration.HasValue
			? settings.FailureExpiration.Value
			: settings.Expiration;

		return new DistributedCacheEntryOptions {
			AbsoluteExpirationRelativeToNow = expiration
		};
	}

}