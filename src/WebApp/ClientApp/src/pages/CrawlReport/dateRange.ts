import type { DateRangePreset } from './DateRangeControl';

function toDateInputValue(date: Date): string {
  return date.toISOString().slice(0, 10);
}

export function defaultCustomRange(): { from: string; to: string } {
  const to = new Date();
  const from = new Date(to.getTime() - 6 * 24 * 60 * 60 * 1000);
  return { from: toDateInputValue(from), to: toDateInputValue(to) };
}

/** Resolves a preset (or an explicit custom from/to pair) into the ISO datetime range the API expects. */
export function resolveDateRange(preset: DateRangePreset, customFrom: string, customTo: string): { from: string; to: string } {
  if (preset === 'custom') {
    const from = customFrom ? new Date(`${customFrom}T00:00:00.000Z`) : new Date();
    const to = customTo ? new Date(`${customTo}T23:59:59.999Z`) : new Date();
    return { from: from.toISOString(), to: to.toISOString() };
  }

  const days = parseInt(preset, 10);
  const to = new Date();
  const from = new Date(to);
  from.setUTCDate(from.getUTCDate() - (days - 1));
  from.setUTCHours(0, 0, 0, 0);
  return { from: from.toISOString(), to: to.toISOString() };
}

/**
 * Every ISO "YYYY-MM-DD" UTC day from `from` to `to` inclusive - mirrors
 * GetArticleVolumeReportQuery's own day-range loop. Needed when the backend response is sparse
 * (only real (day, provider) counts, no explicit zero rows) but the chart still needs the full,
 * contiguous day axis to plot against.
 */
export function enumerateDates(from: string, to: string): string[] {
  const start = new Date(from);
  const end = new Date(to);
  const cursor = new Date(Date.UTC(start.getUTCFullYear(), start.getUTCMonth(), start.getUTCDate()));
  const endDay = new Date(Date.UTC(end.getUTCFullYear(), end.getUTCMonth(), end.getUTCDate()));

  const dates: string[] = [];
  while (cursor <= endDay) {
    dates.push(cursor.toISOString().slice(0, 10));
    cursor.setUTCDate(cursor.getUTCDate() + 1);
  }
  return dates;
}
