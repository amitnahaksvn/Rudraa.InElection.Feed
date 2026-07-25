import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { fetchApiProviders, fetchRssProviders } from '../../api/providers';
import { getDomainFromUrl } from '../../utils/providerVisuals';
import type { ApiProviderSummary, RssProviderSummary } from '../../api/providerTypes';

export interface ProviderCatalogEntry {
  name: string;
  // True if enabled in at least one country - a provider only counts as "disabled" (hidden by
  // default in the filter) once every one of its country schedules is off.
  enabled: boolean;
  domain?: string;
}

function toEntries(pipeline: 'Rss' | 'Api', data: (RssProviderSummary | ApiProviderSummary)[]): ProviderCatalogEntry[] {
  const byName = new Map<string, ProviderCatalogEntry>();

  for (const p of data) {
    const items = pipeline === 'Api' ? (p as ApiProviderSummary).endpoints : (p as RssProviderSummary).feeds;
    const representative = items.find((i) => i.enabled) ?? items[0];
    const domain = getDomainFromUrl(representative?.url);

    const existing = byName.get(p.name);
    if (existing) {
      existing.enabled = existing.enabled || p.enabled;
      if (!existing.domain && domain) existing.domain = domain;
    } else {
      byName.set(p.name, { name: p.name, enabled: p.enabled, domain });
    }
  }

  return Array.from(byName.values()).sort((a, b) => a.name.localeCompare(b.name));
}

/// Provider list for the crawl-report page's filter selector - deduplicated by bare provider name
/// (a provider scheduled under more than one country appears once here, matching how the filter
/// itself selects "by provider name" rather than by (provider, country) pair).
export function useProviderCatalog(pipeline: 'Rss' | 'Api') {
  const query = useQuery({
    queryKey: ['providerCatalog', pipeline],
    queryFn: (): Promise<(RssProviderSummary | ApiProviderSummary)[]> =>
      pipeline === 'Api' ? fetchApiProviders() : fetchRssProviders(),
    staleTime: 5 * 60 * 1000,
  });

  const providers = useMemo(() => (query.data ? toEntries(pipeline, query.data) : []), [pipeline, query.data]);

  return { providers, isLoading: query.isLoading };
}
