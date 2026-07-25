// Mirrors Application/Crawl/Dtos/*.cs - keep these in sync by hand (no shared schema generation
// in this project yet).

export type CrawlPipelineName = 'Rss' | 'Api' | 'Social';

export interface CrawlReportSummary {
  totalRuns: number;
  successfulRuns: number;
  runsWithErrors: number;
  failedRuns: number;
  skippedRuns: number;
  successRatePercent: number;
  newArticles: number;
  failedFeeds: number;
}

export interface CrawlReportDailyPoint {
  date: string; // "2026-07-01"
  totalRuns: number;
  successfulRuns: number;
  runsWithErrors: number;
  failedRuns: number;
  skippedRuns: number;
  newArticles: number;
  failedFeeds: number;
}

export interface CrawlReportProviderRow {
  country: string;
  provider: string;
  hasRun: boolean;
  cron: string | null;
  timeZone: string | null;
  nextExecution: string | null;
  lastExecution: string | null;
  lastJobState: string | null;
  lastErrorMessage: string | null;
  totalRuns: number;
  successfulRuns: number;
  runsWithErrors: number;
  failedRuns: number;
  skippedRuns: number;
  successRatePercent: number;
  newArticles: number;
  failedFeeds: number;
}

export interface CrawlReport {
  pipeline: CrawlPipelineName;
  from: string;
  to: string;
  summary: CrawlReportSummary;
  timeSeries: CrawlReportDailyPoint[];
  providers: CrawlReportProviderRow[];
}

export interface CrawlHistoryRun {
  id: string;
  pipeline: CrawlPipelineName;
  providers: string[];
  startTime: string;
  endTime: string | null;
  feedCount: number;
  newArticles: number;
  failedFeeds: string[];
  status: string;
  error: string | null;
}

// Sparse - a (day, provider) combination with zero articles simply has no entry, rather than an
// explicit zero-count row. Zero-fill against a known date range when charting (see
// pages/CrawlReport/dateRange.ts's enumerateDates).
export interface ArticleVolumeProviderDailyPoint {
  date: string; // "2026-07-01"
  provider: string;
  count: number;
}

export interface ArticleVolumeProviderRow {
  provider: string;
  count: number;
}

export interface ArticleVolumeReport {
  pipeline: CrawlPipelineName;
  from: string;
  to: string;
  totalArticles: number;
  providerTimeSeries: ArticleVolumeProviderDailyPoint[];
  providers: ArticleVolumeProviderRow[];
}
