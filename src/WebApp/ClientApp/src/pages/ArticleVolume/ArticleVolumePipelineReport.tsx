import { useEffect, useMemo, useState } from 'react';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import { useArticleVolumeReport } from './useArticleVolumeReport';
import { ArticleVolumeProviderTable } from './ArticleVolumeProviderTable';
import { DateRangeControl, type DateRangePreset } from '../CrawlReport/DateRangeControl';
import { ProviderFilterControl } from '../CrawlReport/ProviderFilterControl';
import { useProviderCatalog } from '../CrawlReport/useProviderCatalog';
import { defaultCustomRange, enumerateDates, resolveDateRange } from '../CrawlReport/dateRange';
import { StatTile } from '../CrawlReport/StatTile';
import { DailyStackedChart, type ChartSeries } from '../CrawlReport/charts/DailyStackedChart';
import { useChartColors } from '../CrawlReport/useChartColorMode';
import { getAvatarColor } from '../../utils/providerVisuals';

// Deliberately reuses the crawl-report page's own building blocks (date range control, provider
// filter, stat tiles, daily chart) rather than parallel copies - same "look and concept", just a
// different data source (ArticleFingerprints via useArticleVolumeReport, not CrawlHistory/Hangfire
// schedule state via useCrawlReport) and a simpler Provider+Count breakdown table.
export function ArticleVolumePipelineReport({ pipeline }: { pipeline: 'Rss' | 'Api' }) {
  const [preset, setPreset] = useState<DateRangePreset>('7');
  const initialCustom = useMemo(defaultCustomRange, []);
  const [customFrom, setCustomFrom] = useState(initialCustom.from);
  const [customTo, setCustomTo] = useState(initialCustom.to);
  const [selectedProviders, setSelectedProviders] = useState<string[]>([]);
  const [defaultsApplied, setDefaultsApplied] = useState(false);

  const { from, to } = useMemo(() => resolveDateRange(preset, customFrom, customTo), [preset, customFrom, customTo]);

  const { providers: catalog } = useProviderCatalog(pipeline);

  // By default, every currently-active (enabled) provider is selected and its data loaded - not
  // just an empty filter that happens to have the same effect server-side. Applied once, the
  // first time the catalog loads, so it never overwrites a selection the reader has since made.
  useEffect(() => {
    if (!defaultsApplied && catalog.length > 0) {
      setSelectedProviders(catalog.filter((p) => p.enabled).map((p) => p.name));
      setDefaultsApplied(true);
    }
  }, [catalog, defaultsApplied]);

  const { data: report, isLoading, isError, isFetching } = useArticleVolumeReport(pipeline, from, to, selectedProviders);
  const colors = useChartColors();

  const dates = useMemo(() => enumerateDates(from, to), [from, to]);
  const activeProviders = report?.providers.filter((p) => p.count > 0).length ?? 0;

  // Pivots the sparse (day, provider) rows into one chart series per provider that had any
  // activity in range - every provider gets its own series, not just the busiest few, per
  // explicit request. Colors come from the same deterministic per-provider palette already used
  // for logos/avatars elsewhere, so a provider's chart color always matches its avatar color.
  const { series, values } = useMemo(() => {
    if (!report) {
      return { series: [] as ChartSeries[], values: {} as Record<string, number[]> };
    }

    const activeProviderNames = report.providers.filter((p) => p.count > 0).map((p) => p.provider);
    const seriesList: ChartSeries[] = activeProviderNames.map((name) => ({
      key: name,
      label: name,
      color: getAvatarColor(name),
    }));

    const dateIndex = new Map(dates.map((d, i) => [d, i]));
    const valueMap: Record<string, number[]> = {};
    for (const s of seriesList) {
      valueMap[s.key] = dates.map(() => 0);
    }

    for (const point of report.providerTimeSeries) {
      const dayIndex = dateIndex.get(point.date);
      if (dayIndex === undefined || !valueMap[point.provider]) continue;
      valueMap[point.provider][dayIndex] += point.count;
    }

    return { series: seriesList, values: valueMap };
  }, [report, dates]);

  return (
    <Stack gap={2}>
      <DateRangeControl
        preset={preset}
        onPresetChange={setPreset}
        customFrom={customFrom}
        customTo={customTo}
        onCustomFromChange={setCustomFrom}
        onCustomToChange={setCustomTo}
      />

      <ProviderFilterControl pipeline={pipeline} selected={selectedProviders} onChange={setSelectedProviders} />

      {isLoading && (
        <Stack alignItems="center" sx={{ py: 6 }}>
          <CircularProgress />
        </Stack>
      )}

      {isError && <Alert severity="error">Failed to load the {pipeline} article volume report.</Alert>}

      {report && (
        <Box sx={{ opacity: isFetching ? 0.7 : 1, transition: 'opacity 0.15s' }}>
          <Stack gap={2}>
            <Stack direction="row" flexWrap="wrap" gap={1.5}>
              <StatTile label="Total articles" value={report.totalArticles} color={colors.seriesNew} />
              <StatTile label="Providers with activity" value={activeProviders} caption={`of ${report.providers.length} listed`} />
            </Stack>

            <Card variant="outlined">
              <CardContent>
                <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>
                  Articles by day, by provider
                </Typography>
                <DailyStackedChart
                  variant="bar"
                  ariaLabel={`${pipeline} articles ingested by day, broken down by provider`}
                  dates={dates}
                  series={series}
                  values={values}
                />
              </CardContent>
            </Card>

            <Card variant="outlined">
              <CardContent>
                <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>
                  Providers ({report.providers.length})
                </Typography>
                <ArticleVolumeProviderTable rows={report.providers} catalog={catalog} />
              </CardContent>
            </Card>
          </Stack>
        </Box>
      )}
    </Stack>
  );
}
