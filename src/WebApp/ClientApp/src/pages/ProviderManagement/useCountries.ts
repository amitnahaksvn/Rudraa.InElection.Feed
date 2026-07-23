import { useQuery } from '@tanstack/react-query';
import { fetchCountries } from '../../api/providers';
import type { CrawlPipelineName } from '../../api/providerTypes';

export function useCountries(pipeline: CrawlPipelineName) {
  return useQuery({
    queryKey: ['countries', pipeline],
    queryFn: () => fetchCountries(pipeline),
  });
}
